
using CANBUSViewerInterface.Common;
using CANBUSViewerInterface.Model;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CANBUSViewerInterface.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        System.Timers.Timer TsendRandom;
        Random random;

        ICanBusModel model;

        public ObservableCollection<string> DeviceItems { get; set; }
        public ObservableCollection<string> LogMsgItems { get; set; }
        public ObservableCollection<int> DataCountItems { get; set; }
        public ObservableCollection<CanMsg> CanMessagesItems { get; set; }

        public DelegateCommand ConnectCmd { get; set; }
        public DelegateCommand DisconnectCmd { get; set; }
        public DelegateCommand ClearTraceCmd { get; set; }
        public DelegateCommand SendCmd { get; set; }
        public DelegateCommand SaveTofileCmd { get; set; }
        public DelegateCommand StartSendRandomCmd { get; set; }
        public DelegateCommand StopSendRandomCmd { get; set; }



        public DelegateCommand ClearLogCmd { get; set; }

        public MainViewModel()
        {
            //initialize 
            isConnected = false;
            DeviceItems = new ObservableCollection<string>();
            LogMsgItems = new ObservableCollection<string>();
            CanMessagesItems = new ObservableCollection<CanMsg>();
            DataCountItems = new ObservableCollection<int> { 0, 1, 2, 3, 4, 5, 6, 7,8 };

            model = new UsbTinModel();
            model.EventNewDevice += model_EventNewDevice;
            model.MessageEvent += model_MessageEvent;
            model.CanMsgEvent += model_CanMsgEvent;

            model.GetDevices().ForEach(i => DeviceItems.Add(i));

            random = new Random();
            TsendRandom = new System.Timers.Timer(500);
            TsendRandom.Elapsed += TsendRandom_Elapsed;

            ConnectCmd = new DelegateCommand(c => ExecuteConnectCmd(), c => !isConnected);
            DisconnectCmd = new DelegateCommand(c => ExecuteDisconnectCmd(), c => isConnected);
            ClearTraceCmd = new DelegateCommand(c => ExecuteClearTraceCmd(), c => true);
            SendCmd = new DelegateCommand(c => ExecuteSendCmd(), c => true);
            SaveTofileCmd = new DelegateCommand(c => ExecuteSaveTofileCmd(c), c => true);
            StartSendRandomCmd = new DelegateCommand(c => ExecuteStartSendRandomCmd(), c => IsConnected && !TsendRandom.Enabled);
            StopSendRandomCmd = new DelegateCommand(c => ExecuteStopSendRandomCmd(), c => IsConnected && TsendRandom.Enabled);
            ClearLogCmd = new DelegateCommand(c => ExecuteClearLogCmd(), c => true);


            //Init values
            selectedDevice = DeviceItems.First();
            selectedBitRate = BaudRateItems[4];
            selectedOpenMode = OpenModeItems[0];

            Id = 0x01;
            D0 = 0x00; D1 = 0x11; D2 = 0x22; D3 = 0x33; D4 = 0x44; D5 = 0x55; D6 = 0x66; D7 = 0x77;
        }

        #region delegates
        void TsendRandom_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool ex = random.Next(1) > 0;
            bool r = random.Next(1) > 0;
            int i;

            if (ex)
                i = random.Next(0xFFFFF);
            else
                i = random.Next(0x1FF);


            int c = random.Next(8);

            byte[] d = new byte[c];

            random.NextBytes(d);

            model.SendMsg(i, ex, r, c, d);
        }


        void model_CanMsgEvent(CanMsg msg)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
               CanMessagesItems.Add(msg);
            })); 
        }

        void model_MessageEvent(string message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                LogMsgItems.Add(message.Replace("\r", "[CR]").Replace("\a", "[BEL]"));
            }));       
        }

        void model_EventNewDevice()
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                DeviceItems.Clear();
                model.GetDevices().ForEach(i => DeviceItems.Add(i));
                LogMsgItems.Add("New device founded");
            }));
        }
        #endregion delegates

        void ExecuteStartSendRandomCmd()
        {
            TsendRandom.Start();
        }

        void ExecuteStopSendRandomCmd()
        {
            TsendRandom.Stop();
        }

        private void ExecuteClearTraceCmd()
        {
            CanMessagesItems.Clear();
        }

        void ExecuteClearLogCmd()
        {
            LogMsgItems.Clear();
        }

        private void ExecuteConnectCmd()
        {
            IsConnected = model.Connect(selectedDevice, selectedBitRate, (Common.OpenMode)Enum.Parse(typeof(Common.OpenMode), selectedOpenMode));
        }

        private void ExecuteDisconnectCmd()
        {
            model.Disconnect();
            IsConnected = false;
        }

        private void ExecuteSendCmd()
        {
            byte[] data = new byte[] { d0, d1, d2, d3, d4, d5, d6, d7 };
            model.SendMsg(id, ext, rtr, dataLen, data);
        }

        private void ExecuteSaveTofileCmd(object set)
        {
            string AppDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            model.LogTofile(true, System.IO.Path.Combine(AppDir, "Log.Log"));
        }

        #region bindingMembers

        bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                OnPropertyChanged("IsConnected");
            }
        }

        private bool ext;
        public bool Ext
        {
            get { return ext; }
            set
            {
                ext = value;
                OnPropertyChanged("Ext");
            }
        }

        private bool rtr;
        public bool RTR
        {
            get { return rtr; }
            set
            {
                if (value) DataLen = 0;
                rtr = value;
                OnPropertyChanged("RTR");
            }
        }
        
        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        private int dataLen;
        public int DataLen
        {
            get { return dataLen; }
            set
            {
                dataLen = value;
                OnPropertyChanged("DataLen");
            }
        }

        private byte d0;
        public byte D0
        {
            get { return d0; }
            set
            {
                d0 = value;
                OnPropertyChanged("D0");
            }
        }

        private byte d1;
        public byte D1
        {
            get { return d1; }
            set
            {
                d1 = value;
                OnPropertyChanged("D1");
            }
        }

        private byte d2;
        public byte D2
        {
            get { return d2; }
            set
            {
                d2 = value;
                OnPropertyChanged("D2");
            }
        }

        private byte d3;
        public byte D3
        {
            get { return d3; }
            set
            {
                d3 = value;
                OnPropertyChanged("D3");
            }
        }

        private byte d4;
        public byte D4
        {
            get { return d4; }
            set
            {
                d4 = value;
                OnPropertyChanged("D4");
            }
        }

        private byte d5;
        public byte D5
        {
            get { return d5; }
            set
            {
                d5 = value;
                OnPropertyChanged("D5");
            }
        }

        private byte d6;
        public byte D6
        {
            get { return d6; }
            set
            {
                d6 = value;
                OnPropertyChanged("D6");
            }
        }

        private byte d7;
        public byte D7
        {
            get { return d7; }
            set
            {
                d7 = value;
                OnPropertyChanged("D7");
            }
        }
        private string selectedDevice;
        public string SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                selectedDevice = value;
                OnPropertyChanged("SelectedDevice");
            }
        }

        private int selectedBitRate;
        public int SelectedBitRate
        {
            get { return selectedBitRate; }
            set
            {
                selectedBitRate = value;
                OnPropertyChanged("SelectedBitRate");
            }
        }

        private string selectedOpenMode;
        public string SelectedOpenMode
        {
            get { return selectedOpenMode; }
            set
            {
                selectedOpenMode = value;
                OnPropertyChanged("SelectedOpenMode");
            }
        }

        public List<string> OpenModeItems
        {
            get { return Enum.GetNames(typeof(OpenMode)).ToList(); }
        }

        public List<int> BaudRateItems
        {
            get
            {
                return new List<int> { 10000, 20000, 50000, 100000, 125000, 250000, 500000, 800000, 1000000 };
            }
        }
        #endregion bindingMembers
    }
}
