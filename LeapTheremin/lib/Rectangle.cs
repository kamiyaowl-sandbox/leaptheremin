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
				return new Leap.Vector(
					Pos1.x > Pos2.x ? Pos1.x - Pos2.x : Pos1.x - Pos2.x,
					Pos1.y > Pos2.y ? Pos1.y - Pos2.y : Pos1.y - Pos2.y,
					Pos1.z > Pos2.z ? Pos1.z - Pos2.z : Pos1.z - Pos2.z
					);
			}
		}
	}
}
