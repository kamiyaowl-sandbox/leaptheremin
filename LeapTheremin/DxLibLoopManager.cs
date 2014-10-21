using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using LeapTheremin.game;
using DxLibDLL;
using LeapTheremin.game.impl;


namespace ConsoleApplication1 {
	static class DxLibLoopManager {
		static void Main(string[] args) {
			//==============================
			// 起動するゲームループを可変にした
			GameLoop game = new ThereminMain();
			//==============================

			DX.ChangeWindowMode(DX.TRUE);
			if (DX.DxLib_Init() == -1) {
				Console.WriteLine("Initialize failed.");
				return;
			}
			DX.SetDrawScreen(DX.DX_SCREEN_BACK);
			DX.SetAlwaysRunFlag(DX.TRUE);
			game.Setup();

			const int fps = 60;
			const int duration = 1000 / fps;//ms

			int lastTick = 0;
			int idealNextTick = 0;

			int fpsCalcTick = 0;
			int fpsCount = 0;
			float currentFps = 0;

			while (DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0 && !game.IsEnd()) {
				#region Calclate
				if (++fpsCount > fps) {
					currentFps = fps * 1000 / (float)(DX.GetNowCount() - fpsCalcTick);
					fpsCalcTick = DX.GetNowCount();
					fpsCount = 0;
				}

				game.Calculate();
				#endregion

				#region Drawing

				DX.DrawString(10, 10, string.Format("FPS = {0}", currentFps), DX.GetColor(0xff, 0xff, 0xff));
				game.Update();

				#endregion

				DX.ScreenFlip();

				#region FPS fix wait
				if (DX.GetNowCount() < idealNextTick) {
					DX.WaitTimer(idealNextTick - DX.GetNowCount());
				}
				idealNextTick = DX.GetNowCount() + duration;
				#endregion
			}
			game.Exit();
			DX.DxLib_End();
		}
	}
}
