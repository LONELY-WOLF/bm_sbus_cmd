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
            uint mask = 2047, value = 0;
            mask <<= (32 - 8 - 11 - bitOffset);
            value = ((uint)data) << (32 - 8 - 11 - bitOffset);

            buffer[byteOffset] &= (byte)(((~mask) >> 16) & 0xFF); // Clear bits
            buffer[byteOffset] |= (byte)(((value) >> 16) & 0xFF); // Set bits

            buffer[byteOffset] &= (byte)(((~mask) >> 8) & 0xFF);
            buffer[byteOffset] |= (byte)(((value) >> 8) & 0xFF);

            buffer[byteOffset] &= (byte)((~mask) & 0xFF);
            buffer[byteOffset] |= (byte)((value) & 0xFF);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int[] data = new int[17] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            data[0] = (int)V0_00.Value;
            writeFrame(0, data);
            outp.Text = serial.ReadExisting();
        }
    }
}
