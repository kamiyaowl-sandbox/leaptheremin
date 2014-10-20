using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1 {
	class Program {
		static void Main(string[] args) {
			Console.Write("Com PortName>");
			var serial = new SerialPort() {
				BaudRate = 115200,
				PortName = Console.ReadLine()
			};
			serial.DataReceived += serial_DataReceived;

			serial.Open();
			while (true) {
				Console.Write("Freq>");
				var freq = int.Parse(Console.ReadLine());
				Console.Write("Vol>");
				var vol = int.Parse(Console.ReadLine());

				var src = new byte[] {
					 0x81, (byte)((freq >> 7) & 0x7f), (byte)(freq & 0x7f),//31~16383
					 0x82, 0x0, (byte)((vol >> 1) & 0x7f)
				 };
				serial.Write(src, 0, src.Length);
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
}
