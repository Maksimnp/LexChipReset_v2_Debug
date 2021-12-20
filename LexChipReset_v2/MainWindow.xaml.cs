using Microsoft.Win32;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LexChipReset_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SerialPort serialport;
        private readonly IContainer components;
        private string _dataForSend;
        private string _writeStatus;
      
        private readonly string DataForSend1;
        private string DataForSend2;
        private string DataForSend3;
        private string DataForSend4;
        private string DataForSend5;
        private int _readAll;
        private int _next;
        private readonly string[] Hex;
        private string Tag1 { get; set; }
        private string Tag2 { get; }
        private string Tag3 { get; }
        private string Tag4 { get; }
        private string Tag5 { get; }
        private string Tag6 { get; }
        private string Tag7 { get; }
        private string Tag8 { get; }
        private string Tag9 { get; }
        private string Tag10 { get; }
        private string Tag11 { get; }
        private string Tag12 { get; }
        private string Tag13 { get; }
        private string Tag14 { get; }
        private string Tag15 { get; }
        private string Tag16 { get; }
        private string Tag17 { get; }

        private string Type;
        private string DataForSend6;
        private string DataForSend7;
        private string DataForSend8;
        private string SendCommandStr;

        public MainWindow()
        {
            InitializeComponent();
            Debug_ListBox.Items.Add("Hello there.\nTo get started, select the COM port \nand press the 'Сonnect' button.");
            components = new Container();
            ComponentResourceManager resources = new(typeof(MainWindow));
            serialport = new SerialPort(components);
            ConnectionToCom_Button = new Button();
            string[] portNames = SerialPort.GetPortNames();
            serialport.DataReceived += new SerialDataReceivedEventHandler(serialportDataReceived);
            foreach (string port in portNames) { COMPort_Combobox.Items.Add(port); }
            TextBlockProgrBar.Visibility = Visibility.Collapsed;
            ReadChip_Button.IsEnabled = false;
            ProgramChip_Button.IsEnabled = false;
            ClearConsole_Button.IsEnabled = false;
            SaveLog_Button.IsEnabled = false;

            //START LOG EVENT
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/LOG.txt", rollingInterval: RollingInterval.Hour)
            .CreateLogger();
            //END LOG EVENT

            //START SET ARRAY DATA (Создание динамического массива и инициализация строк)
            string[] strArray = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            Hex = strArray;
            _writeStatus = "";
            strArray[1] = "90";
            strArray[2] = "A0";
            strArray[3] = "B0";
            strArray[4] = "C0";
            strArray[5] = "D0";
            strArray[6] = "E0";
            strArray[7] = "F0";
            Tag1 = "000000000000000000000000";
            Tag2 = "000000000000";
            Tag3 = "00000000000000000000000000000000";
            Tag4 = "0000000000000000000000000000";
            Tag5 = "000000000000000000";
            Tag6 = "0000000000000000000000";
            Tag7 = "0000000000000000000000000000";
            Tag8 = "F00000000000000000FFFFFFFF0100000000";
            Tag9 = "400100000000000000FFFFFFFF0100000000";
            Tag10 = "C000000000000000000000000000000000FF";
            Tag11 = "D000FFFFFF7FFFFF00000000000000000000";
            Tag12 = "E0000002FC002269002269071C001DA1D400";
            Tag13 = "F0000000C1710000AD290AF9000064130000";
            Tag14 = "1001000000000000000000000000000000FF";
            Tag15 = "2001FFFFFF7FFFFF00000000000000000000";
            Tag16 = "30010002FC002269002269071C001DA1D400";
            Tag17 = "40010000C1710000AD290AF9000064130000";
            //END SET ARRAY DATA
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ButtonMinimaze_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //START SAVE CONSOLE LOG EVENT 
        private void SaveLog_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (MainListBox.Items.Count is 0)
                {
                    MessageBox.Show("Nothing to Save, Console is Empty");
                }
                else
                {
                    SaveFileDialog dialog = new()
                    {
                        Filter = "Text Files(*.txt)|*.txt|All(*.*)|*"
                    };
                    string allItems = string.Join(" ", MainListBox.Items.OfType<object>());
                    if (dialog.ShowDialog() == true)
                    {
                        File.WriteAllText(dialog.FileName, allItems, Encoding.UTF8);
                        Debug_ListBox.Items.Clear();
                        Debug_ListBox.Items.Add("CONSOLE LOG SAVED...");
                    }
                    Log.Information("CONSOLE LOG SAVED...");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $@"Exeption: {ex}");
            }
        }
        //END SAVE CONSOLE LOG EVENT

        //START HELP BUTTON
        private void Help_Button_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Example: 1501400111223344556677889911223344556677\n15 - command type(15 write command, 14 command for reading data)\n01 - type of cartridge toner or imaging drum(01 toner cartridge, 05 imaging drum)\n4001 - address for recording 11223344556677889911223344556677 - data 16 of 2 in hex");
        }
        //END HELP BUTTON

        //START INPUT TEXT FROM ENTER 
        private void InputTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SendCommandStr = InputTextBox.Text;
                    serialport.WriteLine(SendCommandStr);
                }
            }
            catch (Exception ex)
            {
                Debug_ListBox.Items.Add("First you need\nto connect the com port!");
                Log.Error(ex, $@"Exeption: {ex}");
            }
        }
        //END INPUT TEXT FROM ENTER

        //START CLEAR CONSOLE BUTTON EVENT
        private void ClearConsole_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (MainListBox.Items.Count is 0)
                {
                    MessageBox.Show("Nothing to clean!");
                }
                else
                {
                    MainListBox.Items.Clear();
                    Debug_ListBox.Items.Add("CONSOLE CLEANED!");
                    Log.Information("CONSOLE CLEANED...");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $@"Exeption: {ex}");
            }
        }
        //END CLEAN CONSOLE BUTTON EVENT

        //START CONNECT TO COM PORT EVENT
        private void ConnectionToCom_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serialport.IsOpen)
                {
                    Connect();
                    Log.Information("CONNECT TO {0}.", COMPort_Combobox.SelectedItem.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $@"CHECK WHETHER PORT IS SELECTED!");
                Log.Error(ex, $@"Exeption: {ex}");
            }
        }
        private void Connect()
        {
            try
            {
                serialport.PortName = COMPort_Combobox.SelectedItem.ToString();
                serialport.BaudRate = 115200;
                serialport.StopBits = StopBits.One;
                serialport.DataBits = 8;
                serialport.Handshake = Handshake.None;
                serialport.RtsEnable = true;
                serialport.ReadTimeout = 500;
                serialport.WriteTimeout = 500;
                serialport.Parity = Parity.None;
                serialport.DtrEnable = true;
                serialport.Open();
                Connection_Label.Content = $@"CONNECTED TO {serialport.PortName}";
                Debug_ListBox.Items.Clear();
                Debug_ListBox.Items.Add("PROGRAM INITIALIZATION...");
                Log.Information("CONNECT TO {0}.", COMPort_Combobox.SelectedItem.ToString());

            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Exception: {ex}");
                Log.Error(ex, $@"Exeption: {ex}");
                serialport.Close();
            }
        }

        private void RefreshCOMPort_Button_Click(object sender, EventArgs e)
        {
            try
            {
                COMPort_Combobox.Items.Clear();
                COMPort_Combobox.Text = " ";
                string[] portNames = SerialPort.GetPortNames();
                foreach (string port in portNames)
                {
                    serialport.Close();
                    COMPort_Combobox.Items.Add(port);
                    ReadChip_Button.IsEnabled = false;
                    ProgramChip_Button.IsEnabled = false;
                    ClearConsole_Button.IsEnabled = false;
                    SaveLog_Button.IsEnabled = false;
                }
                Log.Information("COM PORTS UPDATED...");
                Debug_ListBox.Items.Add("COM PORTS UPDATED...");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $@"Exeption: {ex}");
            }
        }
        //END CONNECT TO COM PORT EVENT

        //START PROGRESSBAR EVENT
        private async void ReadStatus_ProgressBar_OnLoaded(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                ReadStatus_ProgressBar.Minimum = 0;
                ReadStatus_ProgressBar.Maximum = 100;
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(50);
                    ReadStatus_ProgressBar.Value = i;
                }
            }
        }
        //END PROGRESSBAR EVENT

        //START DATA RECEIVED EVENT (Работа асинхронного делегата)
        private void serialportDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string? str = serialport.ReadLine();
                object[]? args = new object[] { str };
                Dispatcher.BeginInvoke(new LineReceivedEvent(LineReceived), args);
            }
            catch (Exception ex)
            {
                Debug_ListBox.Items.Add("DATA RECEIVED ERROR");
                Log.Error(ex, $@"Exeption: {ex}");
                serialport.Close();
            }
        }

        private delegate void LineReceivedEvent(string command);
        //END DATA RECEIVED EVENT

        //START READ AND PROGRAMMING CHIP EVENT
        private void ReadChip_Button_Click(object sender, EventArgs e)
        {
            Debug_ListBox.Items.Clear();
            TextBlockProgrBar.Visibility = Visibility.Visible;
            MainListBox.Items.Clear();
            if (_readAll == 0)
            {
                serialport.WriteLine("14" + Type + "0004");
            }
            _readAll = 1;
        }

        private void ProgramChip_Button_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    MainListBox.SelectedItem = 0x35;
            //    int num = 0;
            //    int num2 = 0;
            //    string source = "";
            //    string text = new string(MainListBox.ItemStringFormat.Reverse().ToArray()).Remove(0, 2);
            //    for (int i = 0; i <= text.Length; i++)
            //    {
            //        if (num >= 4)
            //        {
            //            i = text.Length;
            //        }
            //        else if (text.ElementAt<char>(i).ToString() != " ")
            //        {
            //            num2++;
            //            source += text.ElementAt(i).ToString();
            //        }
            //        else
            //        {
            //            if (num2 == 1)
            //            {
            //                source += "0";
            //            }
            //            num++;
            //            num2 = 0;
            //        }
            //    }
            //    string[] strArray = new string[] { "15", Type, "2000", Tag1, new string(source.Reverse().ToArray()) };
            //    DataForSend2 = string.Concat(strArray);
            //    _writeStatus = "4000";
            //    MainListBox.SelectedValue = 0x37;
            //    text = MainListBox.Name;
            //    num = 0;
            //    num2 = 0;
            //    string str3 = "";
            //    for (int j = 0; j <= text.Length; j++)
            //    {
            //        if (num >= 10)
            //        {
            //            j = text.Length;
            //        }
            //        else if (text.ElementAt(j).ToString() != " ")
            //        {
            //            num2++;
            //            str3 += text.ElementAt(j).ToString();
            //        }
            //        else
            //        {
            //            if (num2 == 1)
            //            {
            //                str3 = str3.Insert(str3.Length - 1, "0");
            //            }
            //            num++;
            //            num2 = 0;
            //        }
            //    }
            //    strArray = new string[] { "15", Type, "4000", str3, Tag2 };
            //    DataForSend3 = string.Concat(strArray);
            //    MainListBox.SelectedValue = 0x3e;
            //    num = 0;
            //    num2 = 0;
            //    string? str4 = "";
            //    text = new string(MainListBox.ItemStringFormat.Reverse<char>().ToArray<char>()).Remove(0, 2);
            //    for (int k = 0; k <= text.Length; k++)
            //    {
            //        if (num >= 2)
            //        {
            //            k = text.Length;
            //        }
            //        else if (text.ElementAt(k).ToString() != " ")
            //        {
            //            num2++;
            //            str4 += text.ElementAt(k).ToString();
            //        }
            //        else
            //        {
            //            if (num2 == 1)
            //            {
            //                str4 += "0";
            //            }
            //            num++;
            //            num2 = 0;
            //        }
            //    }

            //    strArray = new string[] { "15", Type, "B000", Tag4, new string(str4.Reverse().ToArray()) };
            //    DataForSend5 = string.Concat(strArray);
            //    MainListBox.SelectedValue = 0x40;
            //    num = 0;
            //    num2 = 0;
            //    string? str5 = "";
            //    text = new string(MainListBox.ItemStringFormat.Reverse<char>().ToArray<char>()).Remove(0, 2);
            //    for (int m = 0; m <= text.Length; m++)
            //    {
            //        if (num >= 7)
            //        {
            //            m = text.Length;
            //        }
            //        else if (text.ElementAt(m).ToString() != " ")
            //        {
            //            num2++;
            //            str5 += text.ElementAt(m).ToString();
            //        }
            //        else
            //        {
            //            if (num2 == 1)
            //            {
            //                str5 += "0";
            //            }
            //            num++;
            //            num2 = 0;
            //        }
            //    }

            //    DataForSend6 += new string(str5.Reverse().ToArray());
            //    MainListBox.SelectedValue = 0x41;
            //    text = MainListBox.ToString();
            //    num = 0;
            //    num2 = 0;
            //    string? str6 = "";
            //    for (int n = 0; n <= text.Length; n++)
            //    {
            //        if (num >= 5)
            //        {
            //            n = text.Length;
            //        }
            //        else if (text.ElementAt(n).ToString() != " ")
            //        {
            //            num2++;
            //            str6 += text.ElementAt(n).ToString();
            //        }
            //        else
            //        {
            //            if (num2 == 1)
            //            {
            //                str6 = str6.Insert(str6.Length - 1, "0");
            //            }
            //            num++;
            //            num2 = 0;
            //        }
            //    }

            //    text = (str6 + Tag6).Remove(0, 6);
            //    DataForSend7 = "000000" + text;
            //    MainListBox.SelectedValue = 0x44;
            //    num = 0;
            //    num2 = 0;
            //    string? str7 = "";
            //    text = new string(MainListBox.ItemStringFormat.Reverse<char>().ToArray<char>()).Remove(0, 2);
            //    for (int num8 = 0; num8 <= text.Length; num8++)
            //    {
            //        if (num >= 2)
            //        {
            //            num8 = text.Length;
            //        }
            //        else if (text.ElementAt(num8).ToString() != " ")
            //        {
            //            num2++;
            //            str7 += text.ElementAt(num8).ToString();
            //        }
            //        else
            //        {
            //            if (num2 == 1)
            //            {
            //                str7 += "0";
            //            }
            //            num++;
            //            num2 = 0;
            //        }
            //    }
            //    DataForSend8 += new string(str7.Reverse().ToArray());
            //    serialport.WriteLine(DataForSend2);
            //}
            //catch (Exception ex)
            //{
            //    Debug_ListBox.Items.Clear();
            //    Debug_ListBox.Items.Add("DATA RECEIVED ERROR!\nSEE LOG!");
            //    Log.Error(ex, $@"Exeption: {ex}");
            //}
        }
        //END READ AND PROGRAMMING CHIP EVENT

        //START PROGRAM INITIALIZATION EVENT
        private void LineReceived(string command)
        {
            MainListBox.Items.Add(command.ToUpper());
            switch (command)
            {
                case "start":
                    serialport.WriteLine("11");
                    break;
                case "1t\r":
                    Type = "01";
                    TypeLabel.Content = @"Toner Cartridge";
                    serialport.WriteLine("12" + Type + "6404");
                    break;
                case "5t\r":
                    Type = "05";
                    TypeLabel.Content = @"Imaging Unit";
                    serialport.WriteLine("12" + Type + "6404");
                    break;
            }

            if (command.Contains("serial"))
            {
                snLabel.Content = " ";
                string? source = command.Remove(0, 6);
                string? str2 = " ";
                int index = 0;
                while (true)
                {
                    if (index > 0x24)
                    {
                        serialport.WriteLine("13" + Type + "8004");
                        break;
                    }

                    string? str3 = source.ElementAt(index).ToString();
                    if (str3 != " ")
                    {
                        str2 += str3;
                    }
                    else
                    {
                        snLabel.Content += Convert.ToChar(Convert.ToInt16(str2)).ToString();
                        str2 = " ";
                    }
                    index++;
                }
                Debug_ListBox.Items.Add("OK!");
                ReadChip_Button.IsEnabled = true;
                ProgramChip_Button.IsEnabled = true;
                ClearConsole_Button.IsEnabled = true;
                SaveLog_Button.IsEnabled = true;
            }
            //END PROGRAM INITIALIZATION EVENT

            //START READ CHIP EVENT
            if ((_readAll <= 0) || (command == "14"))
            {
                return;
            }

            _next++;
            if (_readAll == 1)
            {
                if (_next <= 15)
                {
                    serialport.WriteLine("14" + Type + Hex[_next] + "04");
                    ReadStatus_ProgressBar.Value = 1;
                }
                else
                {
                    _next = 0;
                    _readAll = 2;
                    MainListBox.Items.Add("05");
                    Debug_ListBox.Items.Add("ADDRESS DATA 'x0005' READ");
                    ReadStatus_ProgressBar.Value = 15;
                }
            }

            if (_readAll == 2)
            {
                if (_next <= 15)
                {
                    serialport.WriteLine("14" + Type + Hex[_next] + "05");
                    ReadStatus_ProgressBar.Value = 30;
                }
                else
                {
                    ReadStatus_ProgressBar.Value = 35;
                    _next = 0;
                    _readAll = 3;
                    MainListBox.Items.Add("06");
                    Debug_ListBox.Items.Add("ADDRESS DATA 'x0006' READ");
                    ReadStatus_ProgressBar.Value = 45;
                }
            }

            if (_readAll == 3)
            {
                if (_next <= 15)
                {
                    ReadStatus_ProgressBar.Value = 50;
                    serialport.WriteLine("14" + Type + Hex[_next] + "06");
                    ReadStatus_ProgressBar.Value = 55;
                }
                else
                {
                    ReadStatus_ProgressBar.Value = 60;
                    _next = 0;
                    _readAll = 4;
                    MainListBox.Items.Add("00");
                    Debug_ListBox.Items.Add("ADDRESS DATA 'x0000' READ");
                    ReadStatus_ProgressBar.Value = 65;
                }
            }

            if (_readAll == 4)
            {
                if (_next <= 15)
                {
                    ReadStatus_ProgressBar.Value = 70;
                    serialport.WriteLine("14" + Type + Hex[_next] + "00");
                    ReadStatus_ProgressBar.Value = 75;
                }
                else
                {
                    ReadStatus_ProgressBar.Value = 80;
                    _next = 0;
                    _readAll = 5;
                    MainListBox.Items.Add("01");
                    Debug_ListBox.Items.Add("ADDRESS DATA 'x0001' READ");
                    ReadStatus_ProgressBar.Value = 85;
                }
            }

            if (_readAll == 5)
            {
                ReadStatus_ProgressBar.Value = 90;
                if (_next <= 15)
                {
                    serialport.WriteLine("14" + Type + Hex[_next] + "01");
                }
                else
                {
                    _next = 0;
                    _readAll = 0;
                    ReadStatus_ProgressBar.Value = 100;
                    Debug_ListBox.Items.Add("READING COMPLETE!");
                    MessageBox.Show("Reading complete!\nTo start programming the chip, press button 'Program Chip' !");
                    ReadStatus_ProgressBar.Value = 0;
                    TextBlockProgrBar.Visibility = Visibility.Collapsed;
                }
            }
            //END READ CHIP EVENT

            //START PROGRAMMING CHIP EVENT
            if (command.Contains("writeok") && (_writeStatus == "4000"))
            {
                _writeStatus = "5000";
                serialport.WriteLine(DataForSend1);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "5000"))
            {
                _dataForSend = "15" + Type + "5000" + Tag3;
                _writeStatus = "6000";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "6000"))
            {
                _dataForSend = "15" + Type + "6000" + Tag3;
                _writeStatus = "7000";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "7000"))
            {
                _dataForSend = "15" + Type + "7000" + Tag3;
                _writeStatus = "8000";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "8000"))
            {
                _dataForSend = "15" + Type + "8000" + Tag3;
                _writeStatus = "9000";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "9000"))
            {
                _dataForSend = "15" + Type + "9000" + Tag3;
                _writeStatus = "A000";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "A000"))
            {
                _dataForSend = "15" + Type + "A000" + Tag3;
                _writeStatus = "B000";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "B000"))
            {
                _writeStatus = "C000";
                serialport.WriteLine(DataForSend2);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "C000"))
            {
                _dataForSend = "15" + Type + "C000" + Tag3;
                _writeStatus = "D000";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "D000"))
            {
                _dataForSend = "15" + Type + "D000" + DataForSend3;
                _writeStatus = "E000";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "E000"))
            {
                _dataForSend = "15" + Type + "E000" + DataForSend4;
                _writeStatus = "F000";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "F000"))
            {
                _dataForSend = "15" + Type + "F000" + Tag3;
                _writeStatus = "0001";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "0001"))
            {
                _dataForSend = "15" + Type + "0001" + DataForSend5;
                _writeStatus = "1001";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "1001"))
            {
                _dataForSend = "15" + Type + "1001" + Tag3;
                _writeStatus = "2001";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "2001"))
            {
                _dataForSend = "15" + Type + "2001" + DataForSend3;
                _writeStatus = "3001";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "3001"))
            {
                _dataForSend = "15" + Type + "3001" + DataForSend4;
                _writeStatus = "4001";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "4001"))
            {
                _dataForSend = "15" + Type + "4001" + Tag3;
                _writeStatus = "5001";
                serialport.WriteLine(_dataForSend);
                Thread.Sleep(1500);
            }

            if (command.Contains("writeok") && (_writeStatus == "4001"))
            {
                _dataForSend = "15" + Type + "4001" + DataForSend5;
                _writeStatus = "";
                serialport.WriteLine(_dataForSend);
                DataForSend3 = "";
                DataForSend4 = "";
                DataForSend5 = "";
                //END PROGRAMMING CHIP EVENT
            }
        }
    }
}
