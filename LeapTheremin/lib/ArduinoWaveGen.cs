﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapTheremin.lib {
	class ArduinoWaveGen : IDisposable {
		private SerialPort serialPort;

		public string PortName { get; set; }
		public bool IsOpen { get { return serialPort != null && serialPort.IsOpen; } }
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
		IEnumerable<byte> poltUpdate(int pol) {
			return new byte[] { 0x83, (byte)(pol & 0x7f) };
		}
		public void Update(int freq, int vol, int pol) {
			var src = freqUpdate(freq).Concat(volUpdate(vol)).Concat(poltUpdate(pol)).ToArray();
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
		public void UpdatePoltament(int pol) {
			var src = poltUpdate(pol).ToArray();
			serialPort.Write(src, 0, src.Length);
		}

		public void Dispose() {
			Close();
		}
	}
}
