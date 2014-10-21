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
		private float cameraX;
		private float cameraY;
		private float cameraZ;

		private Controller leap;
		private Frame frame;
		private double rad;
		private int isFill = DX.TRUE;

		public override void Setup() {
			leap = new Controller();

			cameraX = -200;
			cameraY = 200;
			cameraZ = -200;

			DX.SetWindowSize(1024, 768);
			DX.SetFullScreenScalingMode(DX.DX_SCREEN_FRONT);
			DX.SetCameraNearFar(100.0f, 2000.0f);
			//DX.SetUseLighting(DX.FALSE);
			DX.SetUseZBuffer3D(DX.TRUE);
			DX.SetWriteZBuffer3D(DX.TRUE);
			DX.ChangeLightTypeDir(DX.VGet(0.0f, 0.0f, 1.0f));
		}

		public override void Calculate() {
			frame = leap.Frame();
			DX.DrawString(10, 30, frame.Timestamp.ToString(), DX.GetColor(0xff, 0xff, 0xff));

			rad += 0.01;
			//cameraX = (float)(100 * Math.Cos(rad));
			//cameraZ = (float)(100 * Math.Sin(rad));

		}

		public override void Update() {
			DX.SetCameraPositionAndTarget_UpVecY(DX.VGet(cameraX, cameraY, cameraZ), DX.VGet(0.0f, 0.0f, 0.0f));

			drawAxis();

			drawHands();

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
					Diffuse = DX.GetColorF(0.0f, 0.0f, 0.0f, 1.0f),//拡散光
					Ambient = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//環境光
					Specular = DX.GetColorF(0.0f, 0.0f, 0.0f, 0.0f),//反射光
					Emissive = DX.GetColorF(0.0f, 0.0f, 1.0f, 0.0f),//発光色
					Power = 10.0f,//反射角度
				} : new DX.MATERIALPARAM() {
					Diffuse = DX.GetColorF(0.0f, 1.0f, 0.0f, 0.0f),//拡散光
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
