using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Logger = NLog.Logger;

namespace SaveRoomCP
{
    public class SerialPortManager
    {
        private readonly int _baudRate;
        private readonly int _photoResitorThreshold;
        private readonly string _arduinoSerialPort;
        private readonly Logger _logger;

        public SerialPortManager(IConfiguration config, Logger logger)
        {
            _baudRate = config.GetValue<int>("BaudRate");

            if (_baudRate <= 0)
            {
                _baudRate = 9600;
            }

            _photoResitorThreshold = config.GetValue<int>("PlaySoundThreshold");

            if (_photoResitorThreshold <= 0)
            {
                _photoResitorThreshold = 600;
            }

            _arduinoSerialPort = config.GetValue<string>("ArduinoSerialPort");

            _logger = logger;
        }

        /// <summary>
        /// Detect available serial ports and news up a connection to parse data
        /// </summary>
        /// <returns>SerialPort</returns>
        public SerialPort EstablishSerialPortCommunication(out bool quitProgram)
        {
            SerialPort serialPort = new SerialPort();
            string[] targetPorts = FilterPorts(SerialPort.GetPortNames());

            _logger.Info("The following serial ports were found:");

            foreach (var port in targetPorts)
            {
                _logger.Info(port);
            }

            if (targetPorts.Length < 1 || !targetPorts.Any(x => x.Contains(_arduinoSerialPort)))
            {
                quitProgram = true;
                return null;
            }
            else
            {
                // Create a new SerialPort on port COM? or /dev/ttyACM?
                Console.WriteLine();
                var arduinoPortIndex = Array.IndexOf(targetPorts, _arduinoSerialPort);
                serialPort = new SerialPort(targetPorts[arduinoPortIndex], _baudRate);

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
                targetPorts = ports.Where(p => p != "COM1").ToArray();
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
            return dataString.ToLower().Contains("Analog value:".ToLower());
        }

        /// <summary>
        /// Reaches analog input from photocell sensor to determine is light is being detected If
        /// light is above expected threshold, return true
        /// </summary>
        /// <param name="serialPort"></param>
        /// <returns>bool</returns>
        public bool IsTheLightOn(SerialPort serialPort)
        {
            var serialInput = serialPort.ReadLine();

            if (serialInput != string.Empty)
            {
                _logger.Debug(serialInput);
            }

            var photoResistorVal = 0;

            if (serialInput.Length > 14)
            {
                int.TryParse(serialInput.Substring(13), out photoResistorVal);
            }

            return photoResistorVal >= _photoResitorThreshold;
        }
    }
}