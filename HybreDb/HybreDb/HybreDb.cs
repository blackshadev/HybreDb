using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions;

namespace HybreDb {
    public class HybreDb {
        public Database Database;
        public Queue<IHybreAction> Queue;
    }
}
