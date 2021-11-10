using JimmysUnityUtilities;
using LogicAPI.Data;
using LogicAPI.Data.BuildingRequests;
using LogicAPI.Interfaces;
using LogicAPI.WorldDataMutations;
using LogicUI.ColorChoosing;
using LogicUI.MenuParts;
using LogicWorld;
using LogicWorld.BuildingManagement;
using LogicWorld.ClientCode;
using LogicWorld.Interfaces.Building;
using LogicWorld.Rendering.Components;
using LogicWorld.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Locomotion
{
	public class EditPistonMenu : EditComponentMenu<Piston.IData>
	{
		[SerializeField]
		private ColorChooser PistonColorChooser;

		[SerializeField]
		private InputSlider PistonHeight;

		[SerializeField]
		private InputSlider PistonSpeed;

		public override void Initialize()
		{
			base.Initialize();

			PistonHeight.Min = 1f;
			PistonHeight.Max = 10f;
			PistonHeight.OnValueChanged += PistonHeightSlider_OnValueChanged;

			PistonSpeed.Min = 0.01f;
			PistonSpeed.Max = 10f;
			PistonSpeed.OnValueChanged += PistonSpeedSlider_OnValueChanged;
		}

		protected override void OnStartEditing()
		{
			Piston.IData data = ComponentsBeingEdited.First().Data;
			PistonColorChooser.SetColorWithoutNotify(data.PistonColor);
			PistonHeight.SetValueWithoutNotify(data.PistonDistance);
			PistonSpeed.SetValueWithoutNotify(data.PistonSpeed);
		}

		private void PistonHeightSlider_OnValueChanged(float newHeight)
		{
			foreach (TypedEditingComponentInfo<Piston.IData> item in base.ComponentsBeingEdited)
			{
				if ((_ = item.ClientCode as Piston) != null)
				{
					item.Data.PistonDistance = newHeight;
				}
			}
		}

		private void PistonSpeedSlider_OnValueChanged(float newSpeed)
		{
			foreach (TypedEditingComponentInfo<Piston.IData> item in base.ComponentsBeingEdited)
			{
				if ((_ = item.ClientCode as Piston) != null)
				{
					item.Data.PistonSpeed = newSpeed;
				}
			}
		}
	}

	public class Piston : ComponentClientCode<Piston.IData>, IComponentInfo, IColorableClientCode
	{
		public interface IData
		{
			float PistonSpeed { get; set; }
			float PistonDistance { get; set; }
			float Timer { get; set; }
			bool PistonActive { get; set; }
			Color24 PistonColor { get; set; }
		}

		protected struct AddyPos
		{
			public ComponentAddress address;
			public Vector3 localPos;
		}

		float timer = 0;
		bool previousActive;

		public string TextID => throw new NotImplementedException();

		public Type LogicCodeType => throw new NotImplementedException();

		public string LogicScript => throw new NotImplementedException();

		public Type ClientCodeType => throw new NotImplementedException();

		int[] IComponentInfo.CodeInfoInts => throw new NotImplementedException();

		float[] IComponentInfo.CodeInfoFloats => throw new NotImplementedException();

		string[] IComponentInfo.CodeInfoStrings => throw new NotImplementedException();

		bool[] IComponentInfo.CodeInfoBools => throw new NotImplementedException();

		Color24 IColorableClientCode.Color
		{
			get
			{
				return Data.PistonColor;
			}
			set
			{
				Data.PistonColor = value;
			}
		}

		public string ColorsFileKey => "Interactables";

		public float MinColorValue => 0f;

		private Vector3 up;

		protected override void SetDataDefaultValues()
		{
			Data.PistonSpeed = CodeInfoFloats[0];
			Data.PistonDistance = CodeInfoFloats[1];
			Data.PistonActive = false;
			Data.PistonColor = Color24.Purple;
		}

		protected override void InitializeInWorld()
		{
			Main.TryLoadSoundEffects();
			MarkChildPlacementInfoDirty();
		}

		protected override void DataUpdate()
		{
			if (!PlacedInMainWorld)
				return;
			up = new Vector3(0,1,0);
			SetBlockColor(Data.PistonColor);
			if (previousActive != Data.PistonActive)
			{
				if(Data.PistonActive)
					Main.PistonUp.PlayAudioAt(GetBlockEntity(0).WorldPosition);
				else
					Main.PistonDown.PlayAudioAt(GetBlockEntity(0).WorldPosition);
				previousActive = Data.PistonActive;
				QueueFrameUpdate();
			}
		}

		protected override void FrameUpdate()
		{
			AddyPos childLocation = StoreAddressesAndPositions();
			Data.PistonActive = GetInputState();
			if (GetInputState() && timer < 1)
			{
				timer += Time.deltaTime / Data.PistonSpeed;
				ContinueUpdatingForAnotherFrame();
			}
			else if (!GetInputState() && timer > 0)
			{
				timer -= Time.deltaTime / Data.PistonSpeed;
				ContinueUpdatingForAnotherFrame();
			}
			else
			{
				if (childLocation.localPos.x != float.MinValue)
				{
					FinalizeMoveOnServer(childLocation);
				}
			}

			SetBlockPosition(0, Vector3.Lerp(up * 1.5f, up * 1.5f + up * Data.PistonDistance, timer));
			SetBlockPosition(1, Vector3.Lerp(up * 0.5f, up * 0.5f + up, timer));
			SetBlockScale(1, Vector3.Lerp(new Vector3(0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0.5f) + up * Data.PistonDistance, timer));

			if (childLocation.localPos.x != float.MinValue) {
				MoveAttachedCircuit(childLocation);
			}

			MarkChildPlacementInfoDirty();
		}

		protected override ChildPlacementInfo GenerateChildPlacementInfo()
		{
			ChildPlacementInfo childPlacementInfo = new ChildPlacementInfo
			{
				Points = new FixedPlacingPoint[1]
				{
					new FixedPlacingPoint
					{
						Position = Vector3.Lerp(up * 2f, up * 2f + up * Data.PistonDistance, timer),
						UpDirection = up
					}
				}
			};
			return childPlacementInfo;
		}

		AddyPos StoreAddressesAndPositions ()
		{
			IEnumerable<ComponentAddress> addresses = Component.EnumerateChildren();
			Vector3 position = GetBlockEntity(0).WorldPosition - Component.WorldPosition;
			foreach (ComponentAddress addy in addresses)
			{
				if (SceneAndNetworkManager.MainWorld.Data.Lookup(addy).Parent == Address)
				{
					AddyPos tempAddyPos = new AddyPos
					{
						address = addy,
						localPos = SceneAndNetworkManager.MainWorld.Data.Lookup(addy).LocalPosition - position
					};
					return tempAddyPos;
				}
			}
			AddyPos noChild = new AddyPos
			{
				localPos = new Vector3(float.MinValue, float.MinValue, float.MinValue)
			};
			return noChild;
		}

		void MoveAttachedCircuit(AddyPos addyPos)
		{
			Vector3 position = GetBlockEntity(0).WorldPosition - Component.WorldPosition;
			WorldMutation_UpdateComponentPositionRotationParent mutation = new WorldMutation_UpdateComponentPositionRotationParent
			{
				AddressOfTargetComponent = addyPos.address,
				NewLocalPosition = addyPos.localPos + position,
				NewParent = Address
			};
			SceneAndNetworkManager.MainWorld.ApplyWorldMutationFromServer(mutation);
		}

		void FinalizeMoveOnServer (AddyPos addyPos)
		{
			Vector3 position = GetBlockEntity(0).WorldPosition - Component.WorldPosition;
			BuildRequest_UpdateComponentPositionRotationParent movement = new BuildRequest_UpdateComponentPositionRotationParent(addyPos.address, addyPos.localPos + position, SceneAndNetworkManager.MainWorld.Data.Lookup(addyPos.address).LocalRotation, Address);
			BuildRequestManager.SendBuildRequestWithoutAddingToUndoStack(movement);
		}
	}
}
