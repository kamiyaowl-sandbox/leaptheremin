using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapTheremin.lib {
	class LeapAntenna {
		public Leap.Vector Pos1 { get; set; }
		public Leap.Vector Pos2 { get; set; }

		public Leap.Vector Center {
			get {
				return (Pos2 + Pos1) / 2;
			}
		}
	}
}
