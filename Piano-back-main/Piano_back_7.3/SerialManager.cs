using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO.Ports;
using System.Text;

namespace Piano_back_7._3
{
    public class SerialManager
    {
        private SerialPort _serialPort;
        private StringBuilder _receiveBuffer = new StringBuilder();

        // Событие, которое сработает, когда придет целая строка (команда)
        public event Action<string> OnCommandReceived;

        // Событие для статуса (подключено/отключено/ошибка)
        public event Action<string> OnStatusChanged;

        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

        // Подключение
        public void Connect(string portName, int baudRate = 115200)
        {
            try
            {
                _serialPort = new SerialPort(portName, baudRate);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();
                _receiveBuffer.Clear();
                OnStatusChanged?.Invoke($"Подключено к {portName}");
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"Ошибка: {ex.Message}");
            }
        }

        // Отключение
        public void Disconnect()
        {
            if (IsConnected)
            {
                _serialPort.Close();
                OnStatusChanged?.Invoke("Отключено");
            }
        }

        // Отправка данных на ESP32
        public void SendData(string data)
        {
            if (IsConnected)
            {
                _serialPort.WriteLine(data);
            }
        }

        // Внутренняя логика чтения (БУФЕРИЗАЦИЯ). Сюда лезть не нужно!
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string incomingData = _serialPort.ReadExisting();
                _receiveBuffer.Append(incomingData);

                string bufferContent = _receiveBuffer.ToString();
                while (bufferContent.Contains("\n"))
                {
                    int newlineIndex = bufferContent.IndexOf('\n');
                    string command = bufferContent.Substring(0, newlineIndex).Trim();

                    _receiveBuffer.Remove(0, newlineIndex + 1);
                    bufferContent = _receiveBuffer.ToString();

                    if (!string.IsNullOrEmpty(command))
                    {
                        // Выбрасываем событие с готовой командой наружу
                        OnCommandReceived?.Invoke(command);
                    }
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"Ошибка чтения: {ex.Message}");
            }
        }
    }
}
