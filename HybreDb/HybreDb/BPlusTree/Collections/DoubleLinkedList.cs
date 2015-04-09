using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree.Collections {
    public class DoubleLinkedList<T> {
        public LinkedNode<T> Head { get; private set; }
        public LinkedNode<T> Tail { get; private set; }

        public LinkedNode<T> AddHead(T dat) {
            return AddHead(new LinkedNode<T>(dat));
        }

        public LinkedNode<T> AddHead(LinkedNode<T> l) {
            l.InsertBefore(Head);
            Head = l;

            return l;
        } 

        public LinkedNode<T> AddTail(T dat) {
            return AddTail(new LinkedNode<T>(dat));
        }

        public LinkedNode<T> AddTail(LinkedNode<T> l) {
            l.InsertAfter(Tail);
            Tail = l;

            return l;
        } 

        public LinkedNode<T> RemoveTail() {
            var l = Tail;

            if (l == null) return l;

            Tail = Tail.Prev;
            l.Prev = null;

            return l;
        }

        public LinkedNode<T> RemoveHead() {
            var l = Head;

            if (l == null) return l;

            Head = l.Next;
            l.Next = null;

            return l;
        } 
    }
    public class LinkedNode<T> {
        public LinkedNode<T> Next;
        public LinkedNode<T> Prev;
        public T Data;

        public LinkedNode(T data) {
            Data = data;
        }

        public void InsertBefore(LinkedNode<T> n) {
            Next = n;
            Prev = n.Prev;
            n.Prev = this;
        }

        public void InsertAfter(LinkedNode<T> n) {
            Prev = n;
            Next = n.Next;
            n.Next = this;
        }

        public void Unlink() {
            if (Next != null)
                Next.Prev = Prev;
            
            if (Prev != null)
                Prev.Next = Next;

            Next = null;
            Prev = null;

        }
    }
}
