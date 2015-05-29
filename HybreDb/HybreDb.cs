using System;
using System.Collections.Generic;
using System.Threading;
using HybreDb.Actions;
using HybreDb.Actions.Result;
using HybreDb.Communication;
using Newtonsoft.Json;

namespace HybreDb {
    public class HybreDb {
        public Thread Consumer;
        public Database Database;
        public bool IsRunning = false;
        public Queue<KeyValuePair<ClientState, IHybreAction>> Queue;
        public ThreadedSocketServer Server;

        public HybreDb(string name = "HyrbeDb", int port = 4242, bool clean = false) {
            Database = new Database(name, clean);
            Server = new ThreadedSocketServer(port);
            Queue = new Queue<KeyValuePair<ClientState, IHybreAction>>();
            Consumer = new Thread(Consume);

            Server.OnDataReceived += ClientDataCallback;
        }

        public void Start() {
            IsRunning = true;
            Server.Start();
            Consumer.Start();
        }

        public void Stop() {
            IsRunning = false;
            Server.Stop();
            Monitor.Pulse(Queue);
            Consumer.Join();
        }

        public void Consume() {
            while (IsRunning) {
                while (IsRunning && Queue.Count == 0) {
                    lock (Queue) {
                        Monitor.Wait(Queue);
                    }
                }
                if (!IsRunning) return;

                KeyValuePair<ClientState, IHybreAction> act;
                lock (Queue) {
                    act = Queue.Dequeue();
                }

                HybreResult res = HybreAction.Execute(Database, act.Value);
                string retrDat = JsonConvert.SerializeObject(res);
                act.Key.Send(retrDat);
            }
        }

        public void ClientDataCallback(object s, ClientDataReceivedEvent e) {
            IHybreAction act;
            try {
                act = HybreAction.Parse(e.Message);
            }
            catch (Exception ex) {
                var res = new HybreError(ex);
                e.State.Send(JsonConvert.SerializeObject(res));
                return;
            }
            lock (Queue) {
                Queue.Enqueue(new KeyValuePair<ClientState, IHybreAction>(e.State, act));
                Monitor.Pulse(Queue);
            }
        }
    }
}