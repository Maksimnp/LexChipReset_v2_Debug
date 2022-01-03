using LexChipReset_v2.NewFolder;
using Microsoft.Win32;
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
        private string SendCommandStr;
        private int _readAll;
        private int _next;
        readonly Box MessageBox = new();
        private readonly string[] Hex;
        private string Tag1 { get; set; }
        private string Tag2 { get; set; }
        private string Tag3 { get; set; }
        private string Tag4 { get; set; }
        private string Tag5 { get; set; }
        private string Tag6 { get; set; }
        private string Tag7 { get; set; }
        private string Tag8 { get; set; }
        private string Tag9 { get; set; }
        private string Tag10 { get; set; }
        private string Tag11 { get; set; }
        private string Tag12 { get; set; }
        private string Tag13 { get; set; }
        private string Tag14 { get; set; }
        private string Tag15 { get; set; }
        private string Tag16 { get; set; }
        private string Tag17 { get; set; }
        private string Type { get; set; }
       
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

            //START SET ARRAY DATA 
            string[] strArray = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            Hex = strArray;
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

        //START TITLEBAR BUTTON EVENT
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
        //END TITLEBAR BUTTON EVENT

        //START SAVE CONSOLE LOG EVENT 
        private void SaveLog_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (MainListBox.Items.Count is 0)
                {
                    MessageBox.SetText("Nothing to Save, Console is Empty");
                    MessageBox.LabelEvent.Content = "Information";
                    MessageBox.Show();
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
                        Debug_ListBox.Items.Add("Console log saved");
                    }
                    var logger = new Logger();
                    logger.Log("User saves console log");
                }
            }
            catch (Exception ex)
            {
                MessageBox.SetText(ex.Message);
                MessageBox.LabelEvent.Content = "ERROR";
                MessageBox.Show();
                var logger = new Logger();
                logger.Log("Exeption: " + ex);
            }
        }
        //END SAVE CONSOLE LOG EVENT

        //START HELP BUTTON
        private void Help_Button_Click(object sender, EventArgs e)
        {
            MessageBox.SetText("Example: 1501400111223344556677889911223344556677\n15 - command type(15 write command, 14 command for reading data)\n01 - type of cartridge toner or imaging drum(01 toner cartridge, 05 imaging drum)\n4001 - address for recording\n11223344556677889911223344556677 - data 16 of 2 in hex");
            MessageBox.LabelEvent.Content = "HELP";
            MessageBox.Show();
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
                MessageBox.SetText("First you need to connect the com port!\n" + ex.Message);
                MessageBox.LabelEvent.Content = "ERROR";
                MessageBox.Show();
                Debug_ListBox.Items.Add("First you need\nto connect the com port!");
                var logger = new Logger();
                logger.Log("Exeption: " + ex);
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
                    MessageBox.SetText("Nothing to clean!");
                    MessageBox.LabelEvent.Content = "Information";
                    MessageBox.Show();
                }
                else
                {
                    MainListBox.Items.Clear();
                    Debug_ListBox.Items.Add("Console cleared");
                }
            }
            catch (Exception ex)
            {
                MessageBox.SetText(ex.Message);
                MessageBox.LabelEvent.Content = "ERROR";
                MessageBox.Show();
                var logger = new Logger();
                logger.Log("Exeption: " + ex);
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
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.SetText(@$"COM port not selected!");
                MessageBox.LabelEvent.Content = "ERROR";
                MessageBox.Show();
                Debug_ListBox.Items.Add(@$"COM port not selected!");
                var logger = new Logger();
                logger.Log("User dont select COM Port!" + ex);
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
                Debug_ListBox.Items.Add("PROGRAM INITIALIZATION");
            }
            catch (Exception ex)
            {
                MessageBox.SetText(ex.Message);
                MessageBox.LabelEvent.Content = "ERROR";
                MessageBox.Show();
                var logger = new Logger();
                logger.Log("Exeption: " + ex);
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
                Debug_ListBox.Items.Add("COM ports updated");
            }
            catch (Exception ex)
            {
                MessageBox.SetText(ex.Message);
                MessageBox.LabelEvent.Content = "ERROR";
                MessageBox.Show();
                var logger = new Logger();
                logger.Log("Exeption: " + ex);
            }
        }
        //END CONNECT TO COM PORT EVENT

        //START PROGRESSBAR EVENT
        private async Task<ProgressBar> ReadStatus_ProgressBar_OnLoaded(object sender, RoutedEventArgs e)
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
                return ReadStatus_ProgressBar;
            }
        }
        //END PROGRESSBAR EVENT

        //START DATA RECEIVED EVENT
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
                MessageBox.SetText(ex.Message);
                MessageBox.LabelEvent.Content = "ERROR";
                MessageBox.Show();
                Debug_ListBox.Items.Add("Data recived error");
                var logger = new Logger();
                logger.Log("Exeption: " + ex);
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
            
            if (Type == "05")
            {
                serialport.WriteLine("15" + Type + Tag10);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag11);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag12);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag13);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag14);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag15);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag16);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag17);
            }
            else 
            {
                serialport.WriteLine("15" + Type + Tag1);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag2);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag3);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag4);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag5);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag6);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag7);
                Thread.Sleep(1000);
                serialport.WriteLine("15" + Type + Tag8);
            }
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
                ReadChip_Button.IsEnabled = true;
                ProgramChip_Button.IsEnabled = true;
                ClearConsole_Button.IsEnabled = true;
                SaveLog_Button.IsEnabled = true;
                Debug_ListBox.Items.Add("OK!");
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
                    serialport.WriteLine("14" + Type + Hex[_next] + "06");
                    ReadStatus_ProgressBar.Value = 55;
                }
                else
                {
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
                    serialport.WriteLine("14" + Type + Hex[_next] + "00");
                    ReadStatus_ProgressBar.Value = 75;
                }
                else
                {
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
                    MessageBox.SetText("Reading complete! To start programming the chip,\npress button 'Program Chip' !");
                    MessageBox.LabelEvent.Content = "INFORMATION";
                    MessageBox.Show();
                    TextBlockProgrBar.Visibility = Visibility.Collapsed;
                    ReadStatus_ProgressBar.Value = 0;
                }
            }
            //END READ CHIP EVENT
        }
    }
}