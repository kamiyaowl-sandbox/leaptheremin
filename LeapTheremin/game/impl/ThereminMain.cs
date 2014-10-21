using DxLibDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapTheremin.lib;
using Leap;

namespace LeapTheremin.game.impl {
	class ThereminMain : GameLoop {
		private int screenWidth = 1024;
		private int screenHeight = 768;
		private float cameraDistance = 300;
		private float cameraDistanceDiff = 20;
		private float cameraX;
		private float cameraY;
		private float cameraZ;

		private Controller leap;
		private Frame frame;
		private int isFill = DX.TRUE;

		private int volumeWidth = 160;
		private int volumeHeight = 20;
		private Leap.Vector volCenter = new Leap.Vector(-80, 80, 0);
		private LeapAntenna volumeAntenna;

		private int freqWidth = 200;
		private int freqHeight = 20;
		private Leap.Vector freqCenter = new Leap.Vector(120, 100 + 30, 0);
		private LeapAntenna freqAntenna;

		private int volSend = 0;
		private int freqSend = 0;
		private ArduinoWaveGen waveGen;
		private bool isDebugMode;

		public override void Setup() {

			Console.WriteLine("========================================");
			Console.WriteLine("// LeapTheremin v0.1 for ShimaLab");
			Console.WriteLine("//                     2014 Fumiya Katoh");
			Console.WriteLine("========================================");
			Console.WriteLine("Setup LeapMotion. Please Connect LeapMotion.");
			Console.Write("Press AnyKey...");
			Console.ReadLine();
			leap = new Controller();

			Console.WriteLine("LeapMotion Connected.");
			Console.WriteLine("Arduino Wave Generator Initialize.");
			waveGen = new ArduinoWaveGen();
			Console.Write("PortName (\"s\" is skip) >");
			var portname = Console.ReadLine();
			if (portname != "s") {
				waveGen.PortName = portname;
				waveGen.Open();

				waveGen.Update(440, 0, 1);
			}
			Console.Write("Use DebugMode? (True or False)");
			isDebugMode = bool.Parse(Console.ReadLine());

			cameraX = -200;
			cameraY = 200;
			cameraZ = -200;

			DX.SetWindowSize(screenWidth, screenHeight);
			DX.SetFullScreenScalingMode(DX.DX_SCREEN_FRONT);
			DX.SetCameraNearFar(100.0f, 2000.0f);
			//DX.SetUseLighting(DX.FALSE);
			DX.SetUseZBuffer3D(DX.TRUE);
			DX.SetWriteZBuffer3D(DX.TRUE);
			DX.ChangeLightTypeDir(DX.VGet(0.0f, 0.0f, 1.0f));

			volumeAntenna = new LeapAntenna() {
				Pos1 = new Leap.Vector(-volumeWidth / 2.0f + volCenter.x, volCenter.y, -volumeHeight / 2.0f + volCenter.z),
				Pos2 = new Leap.Vector(volumeWidth / 2.0f + volCenter.x, volCenter.y, volumeHeight / 2.0f + volCenter.z),
			};

			freqAntenna = new LeapAntenna() {
				Pos1 = new Leap.Vector(freqCenter.x, -freqWidth / 2.0f + freqCenter.y, -freqHeight / 2.0f + freqCenter.z),
				Pos2 = new Leap.Vector(freqCenter.x, freqWidth / 2.0f + freqCenter.y, freqHeight / 2.0f + freqCenter.z),
			};
		}

		public override void Calculate() {
			frame = leap.Frame();

			if (!frame.Hands.IsEmpty) {
				var volDistance = frame.Hands.Min(h => h.SphereCenter.DistanceTo(volumeAntenna.Center));
				var freqDistance = frame.Hands.Min(h => h.SphereCenter.DistanceTo(freqAntenna.Center));

				volSend = mapVolume(volDistance);
				freqSend = mapFreq(freqDistance);

				if (waveGen.IsOpen) {
					waveGen.UpdateFreq(freqSend);
					waveGen.UpdateVol(volSend);
				}

				if (isDebugMode) {
					DX.DrawString(10, 50, string.Format("vol = {0}, freq = {1}", volDistance, freqDistance), DX.GetColor(0xff, 0xff, 0xff));
					DX.DrawString(10, 70, string.Format("vol = {0}, freq = {1}", volSend, freqSend), DX.GetColor(0xff, 0xff, 0xff));
				}
			}

			moveCamera();
		}

		private int mapVolume(float volDistance) {
			const int volMin = 50;
			const int volMax = 255;
			const int thresh = 120;
			if (volDistance > thresh) return 0;
			else return (int)((volMax - volMin) * (1.0 - volDistance / (double)thresh)) + volMin;
		}

		private int mapFreq(float freqDistance) {
			const int freqMin = 100;//31
			const int freqMax = 2000;//16383;
			const int thresh = 150;
			if (freqDistance > thresh) return freqMax;
			else return (int)((freqMax - freqMin) * (freqDistance / (double)thresh)) + freqMin;
		}

		private void moveCamera() {
			int mouseX, mouseY;
			DX.GetMousePoint(out mouseX, out mouseY);

			var zRad = -(mouseX / (double)screenWidth) * 2 * Math.PI;
			var yRad = (mouseY / (double)screenWidth) * 2 * Math.PI;
			cameraX = (float)(cameraDistance * Math.Cos(zRad));
			cameraZ = (float)(cameraDistance * Math.Sin(zRad));

			cameraY = (float)(cameraDistance * Math.Cos(yRad));
			var wheel = DX.GetMouseWheelRotVol();
			if (wheel > 0) cameraDistance -= cameraDistanceDiff;
			else if (wheel < 0) cameraDistance += cameraDistanceDiff;
		}

		public override void Update() {
			DX.SetCameraPositionAndTarget_UpVecY(DX.VGet(cameraX, cameraY, cameraZ), DX.VGet(0.0f, 0.0f, 0.0f));

			drawThereminAntenna();

			drawAxis();
			drawHands();

		}

		private void drawThereminAntenna() {
			DX.SetMaterialParam(new DX.MATERIALPARAM() {
				Diffuse = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//拡散光
				Ambient = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//環境光
				Specular = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//反射光
				Emissive = DX.GetColorF(0.5f, 1.0f, 0.0f, 0.0f),//発光色
				Power = 10.0f,//反射角度
			});
			DX.DrawCube3D(volumeAntenna.Pos1.ToDX(), volumeAntenna.Pos2.ToDX(), 0, 0, DX.TRUE);

			if (isDebugMode) {
				DX.DrawSphere3D(volumeAntenna.Center.ToDX(), 10, 10, 0, 0, DX.FALSE);
			}

			DX.SetMaterialParam(new DX.MATERIALPARAM() {
				Diffuse = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//拡散光
				Ambient = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//環境光
				Specular = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//反射光
				Emissive = DX.GetColorF(1.0f, 0.5f, 0.0f, 0.0f),//発光色
				Power = 10.0f,//反射角度
			});
			DX.DrawCube3D(freqAntenna.Pos1.ToDX(), freqAntenna.Pos2.ToDX(), 0, 0, DX.TRUE);

			if (isDebugMode) {
				DX.DrawSphere3D(freqAntenna.Center.ToDX(), 10, 10, 0, 0, DX.FALSE);
			}

		}

		private static void drawAxis() {
			DX.DrawLine3D(DX.VGet(-1000, 0, 0), DX.VGet(1000, 0, 0), DX.GetColor(0xff, 0, 0));
			DX.DrawLine3D(DX.VGet(0, -1000, 0), DX.VGet(0, 1000, 0), DX.GetColor(0, 0xff, 0));
			DX.DrawLine3D(DX.VGet(0, 0, -1000), DX.VGet(0, 0, 1000), DX.GetColor(0, 0, 0xff));
		}

		private void drawHands() {
			DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 200);

			foreach (var h in frame.Hands) {
				var handColor = h.IsLeft ? DX.GetColor(0, 0, 240) : DX.GetColor(240, 0, 0);
				var material = h.IsLeft ? new DX.MATERIALPARAM() {
					Diffuse = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//拡散光
					Ambient = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//環境光
					Specular = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//反射光
					Emissive = DX.GetColorF(0.0f, 0.0f, 1.0f, 0.0f),//発光色
					Power = 10.0f,//反射角度
				} : new DX.MATERIALPARAM() {
					Diffuse = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//拡散光
					Ambient = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//環境光
					Specular = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//反射光
					Emissive = DX.GetColorF(1.0f, 0.0f, 0.0f, 0.0f),//発光色
					Power = 10.0f,//反射角度
				};
				DX.SetMaterialParam(material);

				DX.DrawSphere3D(h.PalmPosition.ToDX(), 10, 10, handColor, handColor, isFill);
				DX.DrawSphere3D(h.Arm.Center.ToDX(), 10, 10, handColor, handColor, isFill);
				DX.DrawLine3D(h.PalmPosition.ToDX(), h.Arm.Center.ToDX(), handColor);

				foreach (var f in h.Fingers) {
					DX.DrawSphere3D(f.TipPosition.ToDX(), 5, 5, handColor, handColor, isFill);
					foreach (var boneType in Enum.GetValues(typeof(Bone.BoneType))) {
						var b = f.Bone((Bone.BoneType)boneType);
						DX.DrawSphere3D(b.Center.ToDX(), 3, 3, handColor, handColor, isFill);
						DX.DrawLine3D(b.Center.ToDX(), b.NextJoint.ToDX(), handColor);
						DX.DrawLine3D(b.Center.ToDX(), b.PrevJoint.ToDX(), handColor);

					}
				}
			}
			DX.SetDrawBlendMode(DX.DX_BLENDMODE_NOBLEND, 0);
		}

		public override void Exit() {
		}

		public override bool IsEnd() {
			return DX.CheckHitKey(DX.KEY_INPUT_ESCAPE).ToBool();
		}
	}
}
