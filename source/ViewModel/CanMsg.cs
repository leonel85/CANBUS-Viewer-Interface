using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CANBUSViewerInterface.ViewModel
{
    public class CanMsg
    {
        public int ID { get; set; }
        public bool Ext { get; set; }
        public bool RTR { get; set; }       
        public int DLC { get; set; }
        public byte[] Data { get; set; }
    }
}
