using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaveRoomCP
{
    public class SerialPortManager
    {
        private static readonly int baudRate = 9600;
        private static readonly int photoResitorThreshold = 300;

        /// <summary>
        /// Detect available serial ports and news up a connection to parse data
        /// </summary>
        /// <returns>SerialPort</returns>
        public SerialPort EstablishSerialPortCommunication(out bool quitProgram)
        {
            SerialPort serialPort = new SerialPort();
            string[] targetPorts = FilterPorts(SerialPort.GetPortNames());

            Console.WriteLine("The following serial ports were found:");
            foreach (var port in targetPorts)
            {
                Console.WriteLine(port);
            }

            if (targetPorts.Length < 1)
            {
                quitProgram = true;
                return null;
            }
            else
            {
                // Create a new SerialPort on port COM? or /dev/ttyACM?
                Console.WriteLine();
                serialPort = new SerialPort(targetPorts[0], baudRate);

                serialPort.ReadTimeout = 1500;
                serialPort.WriteTimeout = 1500;
                serialPort.Open();
                Console.WriteLine();

                quitProgram = !PortIsReceivingData(serialPort);

                return serialPort;
            }
        }
        /// <summary>
        /// Filter the desired serial ports based on the Operating System in use
        /// </summary>
        private string[] FilterPorts(string[] ports)
        {
            string[] targetPorts = { };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                targetPorts = ports.Where(p => !p.Contains("COM1")).ToArray();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                targetPorts = ports.Where(p => p.Contains("ttyACM")).ToArray();
            }

            return targetPorts;
        }

        /// <summary>
        /// Parses the byte stream data of the open serial port to determine if data is being received
        /// </summary>
        /// <param name="serialPort"></param>
        /// <returns>bool</returns>
        private bool PortIsReceivingData(SerialPort serialPort)
        {
            Thread.Sleep(500);
            byte[] data = new byte[serialPort.BytesToRead];
            Stream portStream = serialPort.BaseStream;
            portStream.Read(data, 0, data.Length);
            string dataString = Encoding.UTF8.GetString(data);
            return dataString.Contains("Analog value:");
        }

        /// <summary>
        /// Reaches analog input from photocell sensor to determine is light is being detected
        /// If light is above expected threshold, return true
        /// </summary>
        /// <param name="serialPort"></param>
        /// <returns>bool</returns>
        public bool IsTheLightOn(SerialPort serialPort)
        {
            var serialInput = serialPort.ReadLine();

            if (serialInput != string.Empty)
            {
                Console.WriteLine(serialInput);
            }

            var photoResistorVal = 0;

            if (serialInput.Length > 14)
            {
                int.TryParse(serialInput.Substring(13), out photoResistorVal);
            }

            return photoResistorVal >= photoResitorThreshold;
        }
    }
}