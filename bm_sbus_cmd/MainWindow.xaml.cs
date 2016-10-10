using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
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

namespace bm_sbus_cmd
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CheckBox[] Syncs;
        Slider[] Values;
        TextBox[] Comments;

        uint[,] data = new uint[4, 18];

        SerialPort serial = new SerialPort();

        public MainWindow()
        {
            InitializeComponent();

            Syncs = new CheckBox[18] { S0_00, S0_01, S0_02, S0_03, S0_04, S0_05, S0_06, S0_07, S0_08, S0_09, S0_10, S0_11, S0_12, S0_13, S0_14, S0_15, S0_16, S0_17 };
            Values = new Slider[16] { V0_00, V0_01, V0_02, V0_03, V0_04, V0_05, V0_06, V0_07, V0_08, V0_09, V0_10, V0_11, V0_12, V0_13, V0_14, V0_15 };
            Comments = new TextBox[18] { T0_00, T0_01, T0_02, T0_03, T0_04, T0_05, T0_06, T0_07, T0_08, T0_09, T0_10, T0_11, T0_12, T0_13, T0_14, T0_15, T0_16, T0_17 };
        }

        private void cmbPorts_DropDownOpened(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cmbPorts.ItemsSource = ports;
        }

        private void openSerial(string name)
        {
            if (serial.IsOpen)
            {
                serial.Close();
            }
            serial = new SerialPort(name, 9600, Parity.None, 8, StopBits.Two);
            serial.Open();
        }

        private void cmbPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPorts.SelectedIndex >= 0)
            {
                openSerial(cmbPorts.SelectedValue as string);
            }
        }

        private void writeFrame(int cam, uint[,] data)
        {
            if (data.GetLength(1) != 18)
            {
                throw new Exception("data size != 18");
            }
            byte[] buf = new byte[26] { 0x0F, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            buf[1] = (byte)cam;
            // Set 11-bit values
            for (int i = 0; i < 15; i++)
            {
                packData(data[cam, i], 8 + 8 + (i * 11), buf);
            }
            // Set 1-bit values
            if (data[cam, 16] != 0)
            {
                buf[24] |= 0x80;
            }
            else
            {
                buf[24] &= 0x7F;
            }
            if (data[cam, 17] != 0)
            {
                buf[24] |= 0x40;
            }
            else
            {
                buf[24] &= 0xBF;
            }
            // Write data
            if (serial.IsOpen)
            {
                serial.Write(buf, 0, 26);
            }
        }

        private void packData(uint data, int offset, byte[] buffer)
        {
            int byteOffset = offset / 8;
            int bitOffset = offset % 8;
            
            buffer[byteOffset + 0] |= (byte)((data << bitOffset) & 0xFF);
            buffer[byteOffset + 1] |= (byte)((data >> (8 - bitOffset)) & 0xFF);
            buffer[byteOffset + 2] |= (byte)((data >> (16 - bitOffset)) & 0xFF);
        }

        private void updateValues()
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            //data[0] = (int)V0_00.Value;
            //data[1] = (int)V0_01.Value;
            writeFrame(0, data);
            outp.Text = serial.ReadExisting();
        }

        private void Button_Click_Disconnect(object sender, RoutedEventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Close();
            }
            cmbPorts.SelectedIndex = -1;
        }
    }
}
