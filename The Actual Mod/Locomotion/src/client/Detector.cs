using LogicWorld.Rendering.Components;
using UnityEngine;

namespace Locomotion
{
	class Detector : ComponentClientCode<Detector.IData>
	{
		public interface IData
		{
			bool Detecting { get; set; }
		}

		Color onColor = new Color(0, 1, 0);
		Color offColor = new Color(1, 0, 0);

		protected override void InitializeInWorld()
		{
			GetBlockEntity(1).SetColor(offColor);
			QueueFrameUpdate();
		}

		protected override void FrameUpdate()
		{
			Ray ray = new Ray()
			{
				origin = Component.WorldPosition,
				direction = Component.localUp
			};
			Data.Detecting = false;
			if (Physics.SphereCast(ray, 1, 1))
			{
				Data.Detecting = true;
			}

			GetBlockEntity(1).SetColor(Data.Detecting ? onColor : offColor);
			ContinueUpdatingForAnotherFrame();
		}

		public override byte[] SerializeCustomData()
		{
			return new byte[1] { Data.Detecting ? (byte)1 : (byte)0 };
		}

		protected override void SetDataDefaultValues()
		{
			Data.Detecting = false;
		}
	}
}
