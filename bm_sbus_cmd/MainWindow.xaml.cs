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
        SerialPort serial = new SerialPort();

        public MainWindow()
        {
            InitializeComponent();
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
            openSerial(cmbPorts.SelectedValue as string);
        }

        private void writeFrame(int cam, int[] data)
        {
            if (data.Length != 17)
            {
                throw new Exception("data size != 17");
            }
            byte[] buf = new byte[25] { 0x0F, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            buf[1] = (byte)cam;
            // Set 11-bit values
            for (int i = 0; i < 15; i++)
            {
                packData(data[i], 8 + 11 + (i * 11), buf);
            }
            // Set 1-bit values
            if (data[15] != 0)
            {
                buf[23] |= 0x80;
            }
            else
            {
                buf[23] &= 0x7F;
            }
            if (data[16] != 0)
            {
                buf[23] |= 0x40;
            }
            else
            {
                buf[23] &= 0xBF;
            }
            // Write data
            if (serial.IsOpen)
            {
                serial.Write(buf, 0, 25);
            }
        }

        private void packData(int data, int offset, byte[] buffer)
        {
            int byteOffset = offset / 8;
            int bitOffset = offset % 8;
            uint value = ((uint)data);
            
            buffer[byteOffset + 0] |= (byte)((value << bitOffset) & 0xFF);
            buffer[byteOffset + 1] |= (byte)((value >> (8 - bitOffset)) & 0xFF);
            buffer[byteOffset + 2] |= (byte)((value >> (16 - bitOffset)) & 0xFF);
        }

        private void updateValues()
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int[] data = new int[17] { (int)V0_00.Value, (int)V0_01.Value, (int)V0_02.Value, (int)V0_03.Value, (int)V0_04.Value, (int)V0_05.Value, (int)V0_06.Value, (int)V0_07.Value, (int)V0_08.Value, (int)V0_09.Value, (int)V0_10.Value, (int)V0_11.Value, (int)V0_12.Value, (int)V0_13.Value, (int)V0_14.Value, (V0_15.IsChecked == true) ? 1 : 0, (V0_16.IsChecked == true) ? 1 : 0 };
            //data[0] = (int)V0_00.Value;
            data[1] = (int)V0_01.Value;
            writeFrame(0, data);
            outp.Text = serial.ReadExisting();
        }
    }
}
