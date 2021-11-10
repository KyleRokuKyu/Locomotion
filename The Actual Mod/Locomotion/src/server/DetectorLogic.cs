using LogicAPI.Server.Components;

namespace Locomotion
{
	class DetectorLogic : LogicComponent
	{
		protected override void DeserializeData(byte[] data)
		{
			if (data == null || data.Length == 0)
				return;
			Outputs[0].On = false;
			if (data[0] == 1)
			{
				Outputs[0].On = true;
			}
		}
	}
}
