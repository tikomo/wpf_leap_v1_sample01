/*
 * 
 * 
 * 
 * 
 *  Bluetooth ZEAL-C02 pass code "0123"
 */

using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Leap;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace LeapWPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort _port;
        int _countBak;

        Controller leap = new Controller();
        float windowWidth = 1400;
        float windowHeight = 800;
        DrawingAttributes touchIndicator = new DrawingAttributes();

        //  C#1.1 & C#2.0+匿名メソッドで必要な、デリゲート宣言
        //delegate void SerialPortDeligate1(string recieve);
        //delegate void SerialPortDeligate2();
        public delegate void NoArgDelegate();

        public MainWindow()
        {
            InitializeComponent();

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            CompositionTarget.Rendering += Update;
            touchIndicator.Width = 20;
            touchIndicator.Height = 20;
            touchIndicator.StylusTip = StylusTip.Ellipse;


            //
            // シリアルポートを列挙する
            //
            List<string> ports = GetAllPorts();
            ports.Sort();

            foreach (string port in ports)
            {
                Debug.WriteLine(port);
            }

            this.comboBoxPorts.ItemsSource = ports;
            this.comboBoxPorts.SelectedIndex = 0;


            _countBak = 0;

        }


        //
        //
        //
        //
        private void ioControl(int aCount)
        {
            labelFingerCount.Content = "指の数 = " + aCount.ToString();

            if (aCount != _countBak)
            {

                _countBak = aCount;
                SendCommand("-z");

                if (aCount == 1)
                {
                    SendCommand("+0");
                }
                else if (aCount == 2)
                {
                    SendCommand("+1");
                }
                else if (aCount == 3)
                {
                    SendCommand("+2");
                }
                else if (aCount == 4)
                {
                    SendCommand("+3");
                }
                else if (aCount == 5)
                {
                    SendCommand("+4");
                }
                else if (aCount == 6)
                {
                    SendCommand("+5");
                }
                else if (aCount == 7)
                {
                    SendCommand("+6");
                }
                else if (aCount == 8)
                {
                    SendCommand("+7");
                }
            }


        }

        protected void Update( object sender, EventArgs e )
        {
            paintCanvas.Strokes.Clear();
            windowWidth = (float)this.Width;
            windowHeight = (float)this.Height;

            Frame frame = leap.Frame();
            InteractionBox interactionBox = leap.Frame().InteractionBox;


            Debug.WriteLine("Count = " + leap.Frame().Pointables.Count);
            ioControl(leap.Frame().Pointables.Count);

            //String str = "";
            //foreach (Pointable pointable in leap.Frame().Pointables)
            //{
            //    str = str + " " + pointable.Id;
            //}
            //Debug.WriteLine(str);

            foreach ( Pointable pointable in leap.Frame().Pointables ) {
                Leap.Vector normalizedPosition = interactionBox.NormalizePoint( pointable.StabilizedTipPosition );
                float tx = normalizedPosition.x * windowWidth;
                float ty = windowHeight - normalizedPosition.y * windowHeight;

                int alpha = 255;
                // ホバー状態
                if ( pointable.TouchDistance > 0 && pointable.TouchZone != Pointable.Zone.ZONENONE ) {
                    alpha = 255 - (int)(255 * pointable.TouchDistance);
                    touchIndicator.Color = Color.FromArgb( (byte)alpha, 0x0, 0xff, 0x0 );
                }
                // タッチ状態
                else if ( pointable.TouchDistance <= 0 ) {

                    alpha = -(int)(255 * pointable.TouchDistance);
                    touchIndicator.Color = Color.FromArgb( (byte)alpha, 0xff, 0x0, 0x0 );
                }
                // タッチ対象外
                else {
                    alpha = 50;
                    touchIndicator.Color = Color.FromArgb( (byte)alpha, 0x0, 0x0, 0xff );
                }

                StylusPoint touchPoint = new StylusPoint( tx, ty );
                StylusPointCollection tips = new StylusPointCollection( new StylusPoint[] { touchPoint } );
                Stroke touchStroke = new Stroke( tips, touchIndicator );
                paintCanvas.Strokes.Add( touchStroke );
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        //
        // シリアルポートを列挙する
        //
        private List<string> GetAllPorts()
        {
            List<String> allPorts = new List<String>();
            foreach (String portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                allPorts.Add(portName);
            }
            return allPorts;
        }

        private void buttonDummy_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("+4");
        }

        private void port_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = sender as SerialPort;
            string rxdata = sp.ReadLine();

            base.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (NoArgDelegate)delegate
            {
                //data = Sp.ReadExisting();
                labelLog.Content = rxdata;
            }
            );
            
            /*
            string InputData = _port.ReadExisting();
            if (InputData != String.Empty)
            {
                Debug.WriteLine(InputData);
                //this.BeginInvoke(new SetTextCallback(SetText), new object[] { InputData });
                // label1.Text = label1.Text + "I got some data";

            }
             * */
        }

        private void SetText(string text)
        {
            //this.rtbIncoming.Text += text;
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            // 接続ボタン
            string buttonContent = (string)buttonConnect.Content;
            // if (buttonConnect.Content == "接続")
            if (buttonContent == "接続")
            {
                // 接続する
                buttonConnect.Content = "切断";

                // 【ポートのオープン】
                // 「COM1」というCOMポートがシステムに存在しているものとする
                string comPortName = comboBoxPorts.Text;
                _port = new SerialPort(comPortName, 9600, Parity.None, 8, StopBits.One);
                _port.Encoding = Encoding.GetEncoding(20127);
                _port.NewLine = "\r";

                _port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived_1); 



                try
                {
                    //開いていれば一旦閉じる
                    if (_port.IsOpen == true)
                    {
                        _port.Close();
                    }

                    //オープン
                    _port.Open();
                    // this.toolStripStatusLabel1.Text = "PORT OPEN";
                    Debug.WriteLine("PORT OPEN");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    //MessageBox.Show(ex.Message, "Title", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);

                    MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
                //_port.Handshake = false;
                //_port.DtrEnable = true;
                //_port.RtsEnable = true;
            }
            else if ((string)(buttonConnect.Content) == "切断")
            {
                // 切断する
                buttonConnect.Content = "接続";               
                if (this._port.IsOpen == true)
                {
                    this._port.Close();
                    Debug.WriteLine("PORT CLOSE");
                }

            }
        }

        //
        //
        //
        private void SendCommand(string aText)
        {
            if (_port != null)
            {
                string str = aText.Trim();
                if (str == "")
                {
                    //this.toolStripStatusLabel1.Text = "Error : None Data!";
                    Debug.WriteLine("Error : None Data!");
                    this.labelLog.Content = "Error : None Data!";
                    return;
                }

                if (true == _port.IsOpen)
                {
                    // データの送信
                    _port.WriteLine(aText);
                    Debug.WriteLine("Send Tx = " + aText);
                    this.labelLog.Content = "Send Tx = " + aText;
                    //this.toolStripStatusLabel1.Text = "Send Tx = " + aText;
                }
                else
                {
                    // ポートがオープンされていません
                    Debug.WriteLine("Error : Port Closed!");
                    this.labelLog.Content = "Error : Port Closed!";
                    //this.toolStripStatusLabel1.Text = "Error : Port Closed!";
                }
            }
        }

        private void buttonTest1_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("+z");
        }

        private void buttonTest2_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("-z");
        }
    }
}
