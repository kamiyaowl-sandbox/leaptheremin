using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1 {
	class Program {
		static void Main(string[] args) {
			var gen = new ArduinoWaveGen();
			Console.Write("Com PortName>");
			gen.PortName = Console.ReadLine();
			gen.Open();


			while (true) {
				Console.Write("Freq>");
				var freq = int.Parse(Console.ReadLine());
				Console.Write("Vol>");
				var vol = int.Parse(Console.ReadLine());

				gen.Update(freq, vol);
			}
		}

		static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e) {
			SerialPort port = (SerialPort)sender;
			byte[] buf = new byte[1024];
			int len = port.Read(buf, 0, 1024);
			string s = new string(buf.Take(len).Select(x => (char)x).ToArray());
			Console.WriteLine(s);
		}
	}

	class ArduinoWaveGen : IDisposable {
		private SerialPort serialPort;

		public string PortName { get; set; }

		public ArduinoWaveGen() { }

		public void Open() {
			Close();
			serialPort = new SerialPort() {
				BaudRate = 115200,
				PortName = this.PortName
			};
			serialPort.Open();
		}
		public void Close() {
			if (serialPort != null) {
				if (serialPort.IsOpen) serialPort.Close();
				serialPort.Dispose();
				serialPort = null;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="freq">周波数 31~16383の間で指定</param>
		/// <returns></returns>
		IEnumerable<byte> freqUpdate(int freq) {
			return new byte[] { 0x81, (byte)((freq >> 7) & 0x7f), (byte)(freq & 0x7f) };
		}
		IEnumerable<byte> volUpdate(int vol) {
			return new byte[] { 0x82, 0x0, (byte)((vol >> 1) & 0x7f) };
		}
		public void Update(int freq, int vol) {
			var src = freqUpdate(freq).Concat(volUpdate(vol)).ToArray();
			serialPort.Write(src, 0, src.Length);
		}
		public void UpdateFreq(int freq) {
			var src = freqUpdate(freq).ToArray();
			serialPort.Write(src, 0, src.Length);
		}
		public void UpdateVol(int vol) {
			var src = volUpdate(vol).ToArray();
			serialPort.Write(src, 0, src.Length);
		}
		public void Dispose() {
			Close();
		}
	}
}
