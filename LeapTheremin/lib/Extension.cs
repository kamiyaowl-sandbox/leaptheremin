using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapTheremin.lib {
	static class Extension {
		public static bool ToBool(this int src) {
			return src == DxLibDLL.DX.TRUE;
		}

		public static int Not(this int src) {
			return src == 0 ? 1 : 0;
		}


		private static IEnumerable<int> infRandomArr() {
			var r = new Random();
			while (true) {
				yield return r.Next();
			}
		}
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> src) {
			return src.Zip(infRandomArr(), (x, i) => new { Value = x, Index = i })
				.OrderBy(x => x.Index)
				.Select(x => x.Value);
		}

		public static DxLibDLL.DX.VECTOR ToDX(this Leap.Vector vec) {
			return DxLibDLL.DX.VGet(vec.x * 1, vec.y * 1, -vec.z * 1);
		}
	}
}
