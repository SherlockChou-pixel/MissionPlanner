using MissionPlanner.Utilities;
using MissionPlanner.Comms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace MissionPlanner
{

    public partial class NMEA_GPS_Connection : UserControl
    {
        private const string PORT_TCP_HOST_14551_CN = "TCP主机 - 14551";
        private const string PORT_TCP_CLIENT_CN = "TCP客户端";
        private const string PORT_UDP_HOST_14551_CN = "UDP主机 - 14551";
        private const string PORT_UDP_CLIENT_CN = "UDP客户端";

        private string NormalizePortName(string port)
        {
            if (string.IsNullOrEmpty(port))
                return port;

            switch (port)
            {
                case "TCP Host - 14551":
                case "TCP Host":
                    return PORT_TCP_HOST_14551_CN;
                case "TCP Client":
                    return PORT_TCP_CLIENT_CN;
                case "UDP Host - 14551":
                    return PORT_UDP_HOST_14551_CN;
                case "UDP Client":
                    return PORT_UDP_CLIENT_CN;
                default:
                    return port;
            }
        }

        public class PointNMEA
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
            public double Alt { get; set; }
            public double Alt_WGS84 { get; set; }
            public float hdop { get; set; }
            public int sats { get; set; }
            public int fix_type { get; set; }
            public bool IsManualOverride { get; set; }
        }

        static TcpListener listener;
        static ICommsSerial comPort = null;
        //static internal PointLatLngAlt lastgotolocation = new PointLatLngAlt(0, 0, 0, "Goto last");
        //static internal PointLatLngAlt gotolocation = new PointLatLngAlt(0, 0, 0, "Goto");

        private PointNMEA _thisData { get; set; } = new PointNMEA();

        private bool _manualOverrideEnabled;
        private PointNMEA _manualOverrideData { get; set; } = new PointNMEA();

        public Stopwatch last_gps_msg = new Stopwatch();
        private bool portsAreLoaded = false;
        static NMEA_GPS_Connection Instance;

        private NMEA_Viewer _nmeaViewer;

        System.Threading.Thread _thread;
        static bool threadrun = false;
        DateTime _last_time_1 = DateTime.Now;
        DateTime _last_time_2 = DateTime.Now;
        DateTime _startup_time = DateTime.Now;
        float _update_rate_hz_1 = 10.0f; // 10 hz
        float _update_rate_hz_2 = 1.0f; // 1 hz 

        public NMEA_GPS_Connection()
        {
            InitializeComponent();
            Instance = this;

            try
            {
                _manualOverrideEnabled = Settings.Instance.GetBoolean("ODID_ManualOperatorLocationEnabled", false);
                _manualOverrideData.Lat = Settings.Instance.GetDouble("ODID_ManualOperatorLat", 0);
                _manualOverrideData.Lng = Settings.Instance.GetDouble("ODID_ManualOperatorLng", 0);
                _manualOverrideData.Alt = Settings.Instance.GetDouble("ODID_ManualOperatorAlt", 0);
                _manualOverrideData.Alt_WGS84 = _manualOverrideData.Alt;

                UpdateStatusLabel();
            }
            catch
            {
                // ignore settings load failures
            }

            try
            {
                CHK_use_home_operator_location.Checked = Settings.Instance.GetBoolean("ODID_UseHomeOperatorLocation", false);
                UpdateControlsState();
            }
            catch
            {
                // ignore
            }

            try
            {
                init_com_port_list();
            }
            catch
            {
                Console.WriteLine("初始化 NMEA 界面失败");
            }
            //timer2.Start();

            if ((LicenseManager.UsageMode != LicenseUsageMode.Designtime) && (!String.IsNullOrEmpty(Settings.Instance["moving_gps_com"])))
                start();

        }

        private void start()
        {
/*            try
            {
                if (_thread != null)
                {
                    _thread.Abort();

                }
            }
            catch { }*/

            threadrun = true;
            _thread = new System.Threading.Thread(new System.Threading.ThreadStart(mainloop))
            {
                IsBackground = true,
                Name = "NMEA_Thread"
            };
            _thread.Start();
        }

        public PointNMEA getPointNMEA()
        {
            if (_manualOverrideEnabled)
                return _manualOverrideData;

            return _thisData;
        }

        private void UpdateStatusLabel()
        {
            if (LBL_gpsStatus == null)
                return;

            if (_manualOverrideEnabled)
            {
                LBL_gpsStatus.Text = string.Format(CultureInfo.InvariantCulture,
                    "手动覆盖: {0:0.00000} {1:0.00000} {2:0.002} m", _manualOverrideData.Lat, _manualOverrideData.Lng, _manualOverrideData.Alt);
                return;
            }

            if (comPort == null || !comPort.IsOpen)
            {
                LBL_gpsStatus.Text = "尚未启动\r\n右键可手动输入经纬度/高度";
            }
        }

        private void LBL_gpsStatus_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            try
            {
                double lat = _manualOverrideData.Lat;
                double lng = _manualOverrideData.Lng;
                double alt = _manualOverrideData.Alt;

                if (MissionPlanner.Controls.InputBox.Show("地面站定位", "请输入纬度(°)", ref lat) != DialogResult.OK)
                    return;
                if (MissionPlanner.Controls.InputBox.Show("地面站定位", "请输入经度(°)", ref lng) != DialogResult.OK)
                    return;
                if (MissionPlanner.Controls.InputBox.Show("地面站定位", "请输入高度(m)", ref alt) != DialogResult.OK)
                    return;

                _manualOverrideData.Lat = lat;
                _manualOverrideData.Lng = lng;
                _manualOverrideData.Alt = alt;
                _manualOverrideData.Alt_WGS84 = alt;
                _manualOverrideEnabled = true;

                Settings.Instance["ODID_ManualOperatorLocationEnabled"] = "True";
                Settings.Instance["ODID_ManualOperatorLat"] = lat.ToString(CultureInfo.InvariantCulture);
                Settings.Instance["ODID_ManualOperatorLng"] = lng.ToString(CultureInfo.InvariantCulture);
                Settings.Instance["ODID_ManualOperatorAlt"] = alt.ToString(CultureInfo.InvariantCulture);
                Settings.Instance.Save();

                last_gps_msg.Restart();
                UpdateStatusLabel();
            }
            catch
            {
                // ignore
            }
        }


        private void init_com_port_list()
        {
            CMB_serialport.Items.Clear();
            CMB_serialport.Items.AddRange(SerialPort.GetPortNames());
            CMB_serialport.Items.Add(PORT_TCP_HOST_14551_CN);
            CMB_serialport.Items.Add(PORT_TCP_CLIENT_CN);
            CMB_serialport.Items.Add(PORT_UDP_HOST_14551_CN);
            CMB_serialport.Items.Add(PORT_UDP_CLIENT_CN);
            portsAreLoaded = true;

        }


        private void CMB_serialport_Enter(object sender, EventArgs e)
        {
            init_com_port_list();
        }

        private void autoConnectGPS()
        {
            // TODO quick autoconnect after 2 minutes

            if (portsAreLoaded == false || CMB_serialport.SelectedIndex > 0) return;
            init_com_port_list();
            
            
            try
            {
                // Preload Serial port from settings
                var movingGpsCom = NormalizePortName(Settings.Instance["moving_gps_com"]);
                if (!String.IsNullOrEmpty(movingGpsCom) && CMB_serialport.Items.Contains(movingGpsCom))
                {
                    CMB_serialport.SelectedIndex = CMB_serialport.Items.IndexOf(movingGpsCom);
                    //Console.Write("COM: " + CMB_serialport.Text);
                }
                else
                    return;

                //Preload Baud Rate from Settings
                if (!String.IsNullOrEmpty(Settings.Instance["moving_gps_baud"]) && CMB_baudrate.Items.Contains(Settings.Instance["moving_gps_baud"]))
                {
                    CMB_baudrate.SelectedIndex = CMB_baudrate.Items.IndexOf(Settings.Instance["moving_gps_baud"]);
                    //Console.Write(" BAUD: " + CMB_baudrate.Text);
                }              

                //Preload Auto-Connect from Settings
                if (Settings.Instance.GetBoolean("moving_gps_auto"))
                {
                    if (CB_auto_connect.Checked == true)
                        doGPSConnect();
                    else
                        CB_auto_connect.Checked = true; // will auto try to connect
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("自动连接设置失败");
                Console.WriteLine(ex.Message);
            }

        }

        private void CB_auto_connect_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_auto_connect.Checked == true && CMB_serialport.SelectedIndex > 0) doGPSConnect();

            Settings.Instance["moving_gps_auto"] = CB_auto_connect.Checked.ToString();

        }

        private void doGPSConnect()
        {
            if (comPort != null && comPort.IsOpen)
            {
                comPort.Close();
                BUT_connect.Text = "连接基站定位";
                threadrun = false;
                LBL_gpsStatus.Text = "已断开";
                _thisData = new PointNMEA();
                _thread.Abort();
                UpdateStatusLabel();
            }
            else
            {
                LBL_gpsStatus.Text = "正在连接 " + CMB_serialport.Text;
                try
                {
                    switch (CMB_serialport.Text)
                    {
                        case "TCP Host - 14551":
                        case "TCP Host":
                        case PORT_TCP_HOST_14551_CN:
                            comPort = new TcpSerial();
                            CMB_baudrate.SelectedIndex = 0;
                            listener = new TcpListener(System.Net.IPAddress.Any, 14551);
                            listener.Start(0);
                            listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), listener);
                            BUT_connect.Text = "停止";
                            break;
                        case "TCP Client":
                        case PORT_TCP_CLIENT_CN:
                            comPort = new TcpSerial() { retrys = 999999, autoReconnect = true, ConfigRef = "OpenDroneIDTCP" };
                            CMB_baudrate.SelectedIndex = 0;
                            break;
                        case "UDP Host - 14551":
                        case PORT_UDP_HOST_14551_CN:
                            comPort = new UdpSerial();
                            CMB_baudrate.SelectedIndex = 0;
                            break;
                        case "UDP Client":
                        case PORT_UDP_CLIENT_CN:
                            comPort = new UdpSerialConnect();
                            CMB_baudrate.SelectedIndex = 0;
                            break;
                        default:
                            comPort = new SerialPort();
                            comPort.PortName = CMB_serialport.Text;
                            break;
                    }
                }
                catch
                {
                    CustomMessageBox.Show("端口名称无效");
                    return;
                }
                try
                {
                    comPort.BaudRate = int.Parse(CMB_baudrate.Text);
                }
                catch
                {
                    CustomMessageBox.Show("波特率无效", "错误");
                    return;
                }
                try
                {
                    if (listener == null)
                        comPort.Open();
                }
                catch (Exception ex)
                {
                    //CustomMessageBox.Show(Strings.ErrorConnecting + "\n" + ex.ToString(), Strings.ERROR);
                    LBL_gpsStatus.Text = "连接失败：" + CMB_serialport.Text + "，请重试。";
                    return;
                }

                if (comPort != null && comPort.IsOpen)
                {
                    Console.WriteLine("移动基站端口已打开：" + comPort.PortName);
                    LBL_gpsStatus.Text = "已连接 " + comPort.PortName + "，等待定位...";

                    start();

                }

                Settings.Instance["moving_gps_com"] = CMB_serialport.Text;
                Settings.Instance["moving_gps_baud"] = CMB_baudrate.Text;
                Settings.Instance["moving_gps_auto"] = CB_auto_connect.Checked.ToString();

                last_gps_msg.Start();
                BUT_connect.Text = "停止";
            }
        }

        void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

            try
            {
                // End the operation and display the received data on  
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(ar);

                ((TcpSerial)comPort).client = client;

                listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), listener);
            }
            catch { }

        }

        private void readNMEAGPS()
        {
            try // Process Comport Data
            {
                if (comPort != null && comPort.IsOpen)
                {

                    while (comPort.BytesToRead > 0)
                    {
                        string line = comPort.ReadLine();
                        //Console.WriteLine(line); // for debug
                        

                        //string line = string.Format("$GP{0},{1:HHmmss},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},", "GGA", DateTime.Now.ToUniversalTime(), Math.Abs(lat * 100), MainV2.comPort.MAV.cs.lat < 0 ? "S" : "N", Math.Abs(lng * 100), MainV2.comPort.MAV.cs.lng < 0 ? "W" : "E", MainV2.comPort.MAV.cs.gpsstatus, MainV2.comPort.MAV.cs.satcount, MainV2.comPort.MAV.cs.gpshdop, MainV2.comPort.MAV.cs.alt, "M", 0, "M", "");
                        if (line.StartsWith("$GPGGA") || line.StartsWith("$GNGGA")) // 
                        {
                            string[] items = line.Trim().Split(',', '*');

                            if (items[items.Length - 1] != GetChecksum(line.Trim()))
                            {
                                Console.WriteLine("校验失败 " + items[15] + " vs " + GetChecksum(line.Trim()));
                                continue;
                            }

                            if (items[6] == "0")
                            {
                                LBL_gpsStatus.Text = "已连接，无定位";
                                continue;
                            }

                            _thisData.Lat = double.Parse(items[2], CultureInfo.InvariantCulture) / 100.0;

                            _thisData.Lat = (int)_thisData.Lat + ((_thisData.Lat - (int)_thisData.Lat) / 0.60);

                            if (items[3] == "S")
                                _thisData.Lat *= -1;

                            _thisData.Lng = double.Parse(items[4], CultureInfo.InvariantCulture) / 100.0;

                            _thisData.Lng = (int)_thisData.Lng + ((_thisData.Lng - (int)_thisData.Lng) / 0.60);

                            if (items[5] == "W")
                                _thisData.Lng *= -1;

                            _thisData.Alt = double.Parse(items[9], CultureInfo.InvariantCulture);

                            if (!String.IsNullOrEmpty(items[11]))
                            {
                                _thisData.Alt_WGS84 = _thisData.Alt + double.Parse(items[11], CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                _thisData.Alt_WGS84 = -1.0;
                            }


                            _thisData.fix_type = (int.Parse(items[6]));

                            _thisData.sats = (int.Parse(items[7]));
                            _thisData.hdop = (float.Parse(items[8]));

                            last_gps_msg.Restart();
                            udpate_gps_text();
                            updateNMEAViewer(true, line);
                        } else
                        {
                            updateNMEAViewer(false, line);
                        }



                    }
                }
                else
                {
                    BUT_connect.Text = "连接基站定位";
                }
            }
            catch
            {
                Console.WriteLine("处理移动基站 NMEA 数据失败");
            }
        }

        private void updateNMEAViewer(bool inUse, string line)
        {
            try
            {
                if (_nmeaViewer != null)
                {
                    _nmeaViewer.update_NMEA_String(((inUse==true)?"> ":"X ") + line);
                }
            }
            catch { }
        }

        private void udpate_gps_text()
            {
                if (!Instance.IsDisposed)
                {
                    Instance.BeginInvoke(
                        (MethodInvoker)
                            delegate
                            {
                                if (_manualOverrideEnabled)
                                {
                                    UpdateStatusLabel();
                                    return;
                                }
                                Instance.LBL_gpsStatus.Text = String.Format("{0:0.00000}", _thisData.Lat) + " " + String.Format("{0:0.00000}", _thisData.Lng) + " " +
                                                             String.Format("{0:0.002} m", _thisData.Alt) + Environment.NewLine + "WGS84: " + String.Format("{0:0.002} m", _thisData.Alt_WGS84) + 
                                                             " Sats: " + _thisData.sats + " HDOP: " + String.Format("{0:0.02}", _thisData.hdop) + " DGPS: " + ((_thisData.fix_type > 1) ? "Yes":"No");
                            }
                        );
                }
            }



        private void LBL_gpsStatus_DoubleClick(object sender, EventArgs e)
        {
            _nmeaViewer = new NMEA_Viewer(); 
            _nmeaViewer.Show();
            _nmeaViewer.setLabel("Showing Feed from: " + comPort.PortName);
        }

        private void CHK_use_home_operator_location_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance["ODID_UseHomeOperatorLocation"] = CHK_use_home_operator_location.Checked.ToString();
            Settings.Instance.Save();
            UpdateControlsState();
        }

        private void UpdateControlsState()
        {
            bool useHome = CHK_use_home_operator_location.Checked;
            CMB_serialport.Enabled = !useHome;
            CMB_baudrate.Enabled = !useHome;
            BUT_connect.Enabled = !useHome;
            CB_auto_connect.Enabled = !useHome;
        }

        private void menuItem_ManualInput_Click(object sender, EventArgs e)
        {
            using (var inputForm = new Form())
            {
                inputForm.Text = "手动输入地面站坐标";
                inputForm.Width = 350;
                inputForm.Height = 200;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var lblLat = new Label() { Left = 20, Top = 20, Width = 80, Text = "纬度 (°):" };
                var txtLat = new TextBox() { Left = 110, Top = 20, Width = 200, Text = _thisData.Lat.ToString("F6") };

                var lblLng = new Label() { Left = 20, Top = 50, Width = 80, Text = "经度 (°):" };
                var txtLng = new TextBox() { Left = 110, Top = 50, Width = 200, Text = _thisData.Lng.ToString("F6") };

                var lblAlt = new Label() { Left = 20, Top = 80, Width = 80, Text = "高度 (m):" };
                var txtAlt = new TextBox() { Left = 110, Top = 80, Width = 200, Text = _thisData.Alt.ToString("F2") };

                var btnOK = new Button() { Text = "确定", Left = 150, Width = 75, Top = 120, DialogResult = DialogResult.OK };
                var btnCancel = new Button() { Text = "取消", Left = 235, Width = 75, Top = 120, DialogResult = DialogResult.Cancel };

                inputForm.Controls.Add(lblLat);
                inputForm.Controls.Add(txtLat);
                inputForm.Controls.Add(lblLng);
                inputForm.Controls.Add(txtLng);
                inputForm.Controls.Add(lblAlt);
                inputForm.Controls.Add(txtAlt);
                inputForm.Controls.Add(btnOK);
                inputForm.Controls.Add(btnCancel);

                inputForm.AcceptButton = btnOK;
                inputForm.CancelButton = btnCancel;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        double lat = double.Parse(txtLat.Text, CultureInfo.InvariantCulture);
                        double lng = double.Parse(txtLng.Text, CultureInfo.InvariantCulture);
                        double alt = double.Parse(txtAlt.Text, CultureInfo.InvariantCulture);

                        if (lat < -90 || lat > 90)
                        {
                            CustomMessageBox.Show("纬度必须在 -90 到 90 之间", "输入错误");
                            return;
                        }

                        if (lng < -180 || lng > 180)
                        {
                            CustomMessageBox.Show("经度必须在 -180 到 180 之间", "输入错误");
                            return;
                        }

                        _thisData.Lat = lat;
                        _thisData.Lng = lng;
                        _thisData.Alt = alt;
                        _thisData.Alt_WGS84 = alt;
                        _thisData.IsManualOverride = true;
                        _thisData.fix_type = 1;
                        _thisData.sats = 0;
                        _thisData.hdop = 0;

                        last_gps_msg.Restart();
                        LBL_gpsStatus.Text = String.Format("手动输入: {0:0.00000}, {1:0.00000}, {2:0.00} m", lat, lng, alt);

                        Console.WriteLine("手动设置地面站坐标: " + lat + ", " + lng + ", " + alt);
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("输入格式错误: " + ex.Message, "错误");
                    }
                }
            }
        }

        // Calculates the checksum for a sentence
        string GetChecksum(string sentence)
        {
            // Loop through all chars to get a checksum
            int Checksum = 0;
            foreach (char Character in sentence.ToCharArray())
            {
                switch (Character)
                {
                    case '$':
                        // Ignore the dollar sign
                        break;
                    case '*':
                        // Stop processing before the asterisk
                        return Checksum.ToString("X2");
                    default:
                        // Is this the first value for the checksum?
                        if (Checksum == 0)
                        {
                            // Yes. Set the checksum to the value
                            Checksum = Convert.ToByte(Character);
                        }
                        else
                        {
                            // No. XOR the checksum with this character's value
                            Checksum = Checksum ^ Convert.ToByte(Character);
                        }
                        break;
                }
            }
            // Return the checksum formatted as a two-character hexadecimal
            return Checksum.ToString("X2");
        }

        private void BUT_connect_Click(object sender, EventArgs e)
        {
            doGPSConnect();

        }

        private void mainloop()
        {
            threadrun = true;
            while (threadrun)
            {
                DateTime _now = DateTime.Now;
                if ((comPort != null && comPort.IsOpen))
                {
                    try
                    {
                        if (_now > _last_time_1.AddSeconds(1.0 / _update_rate_hz_1))
                        {
                            // Check GPS
                            readNMEAGPS();
                            _last_time_1 = DateTime.Now;
                        }
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep((int)(1000 / _update_rate_hz_1));
                    }
                }
                else
                {
                    try
                    {
                        if (_now.AddSeconds(-300) < _startup_time && _now > _last_time_2.AddSeconds(1.0 / _update_rate_hz_2))
                        {
                            // Check GPS
                            autoConnectGPS();
                            _last_time_2 = DateTime.Now;
                        }
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep((int)(1000 / _update_rate_hz_2));
                    }


                }
                System.Threading.Thread.Sleep((int)(10));
            }
        }

    }

}
