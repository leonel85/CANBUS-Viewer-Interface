using CANBUSViewerInterface.Common;
using CANBUSViewerInterface.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CANBUSViewerInterface.Model
{
    public delegate void EventHandler();
    public delegate void MessageEventHandler(string message);
    public delegate void CanMsgEventHandler(CanMsg msg);

    public interface ICanBusModel
    {
        event EventHandler EventNewDevice;
        event MessageEventHandler MessageEvent;
        event CanMsgEventHandler CanMsgEvent;

        List<string> GetDevices();
        bool Connect(string deviceName, int bitRate, Common.OpenMode opMode);
        void Disconnect();
        
        void SendMsg(int id, bool ext, bool RTR, int dlc, byte[] data);


        void LogTofile(bool log, string filePth);
    }
}
