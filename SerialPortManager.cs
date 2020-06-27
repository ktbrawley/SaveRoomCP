using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaveRoomCP
{
    public class SerialPortManager
    {
        private static readonly int baudRate = 9600;
        private static readonly int photoResitorThreshold = 300;
        private static readonly int photoResistorLimit = 200;

        /// <summary>
        /// Detect available serial ports and news up a connection to parse data
        /// </summary>
        /// <returns>SerialPort</returns>
        public SerialPort EstablishSerialPortCommunication(out bool quitProgram)
        {
            //Define serial port instance
            SerialPort serialPort = new SerialPort();

            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();
            Console.WriteLine("The following serial ports were found:");

            // Display each port name to the console.
            foreach (string port in ports)
            {
                Console.WriteLine(port);

                if (!port.Contains("COM1"))
                {
                    // Create a new SerialPort on port COM?
                    serialPort = new SerialPort(port, baudRate);
                    // Set the read/write timeouts
                    serialPort.ReadTimeout = 1500;
                    serialPort.WriteTimeout = 1500;
                    serialPort.Open();
                }
            }
            Console.WriteLine();

            quitProgram = !PortIsReceivingData(serialPort);

            return serialPort;
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