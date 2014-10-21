using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapTheremin.game {
	abstract class GameLoop {
		public abstract void Setup();
		public abstract void Calculate();
		public abstract void Update();
		public abstract void Exit();

		public abstract bool IsEnd();
	}
}
