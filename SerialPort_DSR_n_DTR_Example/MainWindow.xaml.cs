using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialPort_DSR_n_DTR_Example
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        SerialPort m_Port;
        string m_SelectedPortName;
        int m_SelectedBaudRate;
        private string m_ReceivedMessage;
        private string m_SentMessage;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string pName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(pName));
            }
        }

        public ObservableCollection<string> PortNames { get; set; }
        public ObservableCollection<int> BaudRates { get; set; }

        public string SendingMessage { get; set; }
        public string SentMessage
        {
            get { return m_SentMessage; }
            set
            {
                m_SentMessage = value;
                RaisePropertyChanged("SentMessage");
            }
        }
        public string ReceivedMessage
        {
            get { return m_ReceivedMessage; }
            set
            {
                m_ReceivedMessage = value;
                RaisePropertyChanged("ReceivedMessage");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            PortNames = new ObservableCollection<string>(GetConnectComDevice());
            BaudRates = new ObservableCollection<int>
            {
                1200,
                2400,
                4800,
                9600,
                19200,
                38400,
                57600,
                115200,
                230400,
                460800,
                921600
            };
        }

        public static List<string> GetConnectComDevice()
        {
            List<string> lstPorts = new List<string>();
            RegistryKey rkRoot = Registry.LocalMachine.OpenSubKey("HARDWARE");
            RegistryKey rkSubKey = rkRoot.OpenSubKey("DEVICEMAP\\SERIALCOMM");

            if (rkSubKey == null || rkSubKey.ValueCount == 0)
            {
                lstPorts.Add("none");
            }
            else
            {
                string[] tmpCom = rkSubKey.GetValueNames();
                for (int i = 0; i < rkSubKey.ValueCount; i++)
                {
                    lstPorts.Insert(0, (rkSubKey.GetValue(tmpCom[i]).ToString()));
                }
            }
            return lstPorts;
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_Port != null) m_Port.Close();
                m_Port = new SerialPort(m_SelectedPortName, m_SelectedBaudRate);
                m_Port.Open();
                if (m_Port.IsOpen)
                {
                    Thread t = new Thread(new ThreadStart(() =>
                    {
                        while (m_Port.IsOpen)
                        {
                            if(m_Port.BytesToRead > 0)
                            {
                                ReceivedMessage += m_Port.ReadExisting() + Environment.NewLine;
                            }
                            Thread.Sleep(10);
                        }
                    }));
                    t.Start();
                    m_Port.DtrEnable = true;
                    MessageBox.Show("오픈 성공.");
                }
                else
                {
                    throw new Exception("포트 오픈 실패.");
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                while (!m_Port.DsrHolding) Thread.Sleep(10);
                m_Port.Write(SendingMessage);
                SentMessage += SendingMessage + Environment.NewLine;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void PortName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                m_SelectedPortName = (string)e.AddedItems[0];
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                m_SelectedBaudRate = (int)e.AddedItems[0];
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(m_Port != null)
            {
                m_Port.Close();
            }
        }
    }
}
