using CommonLib;
using CANBUSViewerInterface.Common;
using CANBUSViewerInterface.ViewModel;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CANBUSViewerInterface.Model
{
    public class UsbTinModel : ICanBusModel
    {
        CsharpUSBTinLib.UsbTin USBTinLib;
        Thread Th_SearchDevices;

        string logFilePath;
        string AppDir;
        bool logToFile;

        public UsbTinModel()
        {
            
            AppDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            logFilePath = System.IO.Path.Combine(AppDir, "Log.Log");

            USBTinLib = new CsharpUSBTinLib.UsbTin();
            Th_SearchDevices = new Thread(ThD_SearchDevices);
            Th_SearchDevices.IsBackground = true;
            Th_SearchDevices.Start();

            //register events
            USBTinLib.MessageEvent += USBTinLib_MessageEvent;
            USBTinLib.CANMessageEvent += USBTinLib_CANMessageEvent;

           
            
        }


        void USBTinLib_CANMessageEvent(CsharpUSBTinLib.CANMessage message)
        {
            byte[] data = new byte[8]; 

            if (!message.IsRTR)
            {
                for (int i = 0; i < message.DLC; i++)
                {
                    data[i] = message.Data[i];
                }
            }

            LogToFile(message.ToString());

            RaiseCanMsgEvent(new CanMsg
            {
                ID = message.Id,
                Ext = message.isExtended,
                RTR = message.IsRTR,
                DLC = message.DLC,
                Data = data
            });

            RaiseMessageEvent("Received message \t" + message.ToString());
            LogToFile("Received message:\t" + message.ToString());
        }

        void USBTinLib_MessageEvent(string message)
        {
            LogToFile(message);
            RaiseMessageEvent(message);
        }


        #region threads
        private void ThD_SearchDevices(object data)
        {
            int deviceCount = SerialPort.GetPortNames().Count();
            int newCount;
            while (true)//ciclo infinito di lettura della coda Q_TOOLSTCH
            {
                newCount = SerialPort.GetPortNames().Count();
                if (newCount != deviceCount)
                {
                    deviceCount = newCount;
                    RaiseNewDevice();
                }
                System.Threading.Thread.Sleep(5000);
            }
        }
        #endregion


        private void LogToFile(string text)
        {
             if (logToFile)
                 System.IO.File.AppendAllText(logFilePath, Environment.NewLine + text.Replace("\r", "[CR]").Replace("\a", "[BEL]"));
        }

        void ICanBusModel.LogTofile(bool log, string filePath)
        {
            logFilePath = filePath;
            logToFile = log;
        }

        void ICanBusModel.SendMsg(int id, bool ext, bool rtr, int dlc, byte[] data)
        {
            USBTinLib.Send(new CsharpUSBTinLib.CANMessage(id, ext, rtr, data, dlc));
        }

        bool ICanBusModel.Connect(string deviceName, int bitRate, OpenMode opMode)
        {
            bool retVal = USBTinLib.Connect(deviceName);

            if (retVal)
            {
                CsharpUSBTinLib.Shared.OpenMode mode;

                switch (opMode)
                {
                    case OpenMode.ACTIVE:
                        mode = CsharpUSBTinLib.Shared.OpenMode.ACTIVE;
                        break;
                    case OpenMode.LISTENONLY:
                        mode = CsharpUSBTinLib.Shared.OpenMode.LISTENONLY;
                        break;
                    case OpenMode.LOOPBACK:
                        mode = CsharpUSBTinLib.Shared.OpenMode.LOOPBACK;
                        break;
                    default:
                        mode = CsharpUSBTinLib.Shared.OpenMode.ACTIVE;
                        break;
                }

                USBTinLib.OpenCANChannel(bitRate, mode);
            }
            return retVal;
        }

        void ICanBusModel.Disconnect()
        {
            USBTinLib.CloseCANChannel();
            USBTinLib.Disconnect();
        }

        List<string> ICanBusModel.GetDevices()
        {
            return USBTinLib.GetDevives();
        }




        #region Events
        public event EventHandler EventNewDevice;
        public event MessageEventHandler MessageEvent;
        public event CanMsgEventHandler CanMsgEvent;

        private void RaiseNewDevice()
        {
            if (EventNewDevice != null) EventNewDevice();
        }

        private void RaiseMessageEvent(string msg)
        {
            if (MessageEvent != null) MessageEvent(msg);
        }

        private void RaiseCanMsgEvent(CanMsg msg)
        {
            if (CanMsgEvent != null) CanMsgEvent(msg);
        }
        #endregion Events
    }
}
