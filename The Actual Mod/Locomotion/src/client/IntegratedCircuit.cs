using LogicAPI.Data;
using LogicWorld;
using LogicWorld.Rendering.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using LogicAPI;
using LogicWorld.Rendering.Chunks;
using LogicWorld.ClientCode;
using JimmysUnityUtilities;

namespace Locomotion
{
	class IntegratedCircuit : ComponentClientCode<IntegratedCircuit.IData>, IColorableClientCode
	{
		public interface IData
		{
			Color24 Color { get; set; }
		}

		protected struct PhysicalComponent
		{
			public List<Collider> colliders;
			public ComponentAddress addy;
			public List<Decoration> decorations;
			public List<RenderedEntity> entities;
			public List<Mesh> meshes;
		}

		List<PhysicalComponent> physicalComponents;

		Color24 IColorableClientCode.Color
		{
			get
			{
				return Data.Color;
			}
			set
			{
				Data.Color = value;
			}
		}

		public string ColorsFileKey => "Interactables";

		public float MinColorValue => 0f;

		protected override void SetDataDefaultValues()
		{
			Data.Color = Color24.Purple;
		}

		protected override void DataUpdate()
		{
			SetBlockColor(Data.Color);
		}

		protected override void InitializeInGhost()
		{
			try
			{
				if (Component != null && Component.Parent != null)
				{
					if (!WorldRenderer.Entities.GetClientCode(Component.Parent).ToString().Contains("CircuitBoard"))
					{
						return;
					}
					physicalComponents = new List<PhysicalComponent>();

					FindAndSetScale();

					SetActive(false);
				}
			}
			catch (Exception e)
			{
				Debug.Log("IntegratedCircuit.InitializeInWorld: It broke. Idk mang. " + e);
			}
		}

		protected override void InitializeInWorld()
		{
			try
			{
				if (Component != null && Component.Parent != null)
				{
					if (!WorldRenderer.Entities.GetClientCode(Component.Parent).ToString().Contains("CircuitBoard"))
					{
						return;
					}
					physicalComponents = new List<PhysicalComponent>();

					FindAndSetScale();

					SetActive(false);
				}
			}
			catch (Exception e)
			{
				Debug.Log("IntegratedCircuit.InitializeInWorld: It broke. Idk mang. " + e);
			}
		}

		void FindAndSetScale()
		{
			try
			{
				ComponentAddress pAddress = Component.Parent;
				RecursivelyFindComponents(pAddress);
				Bounds bounds = new Bounds(Component.WorldPosition, Vector3.zero);
				foreach (PhysicalComponent component in physicalComponents)
				{
					foreach (Collider c in component.colliders)
					{
						if (c != WorldRenderer.EntityColliders.GetCollidersOfComponent(Component.Parent)[0])
							bounds.Encapsulate(c.bounds);
					}
				}

				GetBlockEntity(0).WorldPosition = bounds.center - new Vector3(SceneAndNetworkManager.MainWorld.Data.Lookup(pAddress).up.x * bounds.size.x, SceneAndNetworkManager.MainWorld.Data.Lookup(pAddress).up.y * bounds.size.y, SceneAndNetworkManager.MainWorld.Data.Lookup(pAddress).up.z * bounds.size.z) / 2f;
				GetBlockEntity(0).Scale = bounds.size - new Vector3(0.01f, 0.01f, 0.01f);

			}
			catch (Exception e)
			{
				Debug.Log("IntegratedCircuit.FindAndSetScale: It broke. Idk mang. " + e);
			}
		}

		void RecursivelyFindComponents(ComponentAddress address)
		{
			try
			{
				PhysicalComponent tempComponent = new PhysicalComponent()
				{
					colliders = new List<Collider>(),
					addy = address,
					decorations = new List<Decoration>(),
					entities = new List<RenderedEntity>(),
					meshes = new List<Mesh>()
				};
				foreach (Collider c in WorldRenderer.EntityColliders.GetCollidersOfComponent(address))
				{
					tempComponent.colliders.Add(c);
				}
				foreach (Decoration decoration in WorldRenderer.Entities.GetDecorations(address))
				{
					tempComponent.decorations.Add(decoration);
				}
				foreach (RenderedEntity entity in WorldRenderer.Entities.GetBlockEntitiesAt(address))
				{
					tempComponent.entities.Add(entity);
					tempComponent.meshes.Add(entity.Mesh);
				}
				foreach (WireAddress wire in SceneAndNetworkManager.MainWorld.Data.GetWiresDirectlyAttachedTo(address)) {
					tempComponent.entities.Add((RenderedEntity)WorldRenderer.Entities.GetWireEntity(wire));
					tempComponent.meshes.Add(WorldRenderer.Entities.GetWireEntity(wire).Mesh);
				}
				physicalComponents.Add(tempComponent);
				foreach (ComponentAddress item in SceneAndNetworkManager.MainWorld.Data.Lookup(address).EnumerateChildren())
				{
					RecursivelyFindComponents(item);
				}
			}
			catch (Exception e)
			{
				Debug.Log("IntegratedCircuit.RecursivelyFindColliders: It broke. Idk mang. " + e);
			}
		}


		protected override void OnComponentDestroyed()
		{
			try
			{
				if (Component != null && Component.Parent != null)
					SetActive(true);
			}
			catch (Exception e)
			{
				Debug.Log("IntegratedCircuit.OnComponentDestroyed: It broke. Idk mang. " + e);
			}
		}

		void SetActive(bool state)
		{
			try
			{
				if (physicalComponents == null)
					return;
				foreach (PhysicalComponent c in physicalComponents)
				{
					if (c.addy == Component.Parent)
						continue;
					if (c.colliders[0] == WorldRenderer.EntityColliders.GetCollidersOfComponent(Address)[0])
					{
						continue;
					}
					if (WorldRenderer.Entities.GetClientCode(c.addy) != null && WorldRenderer.Entities.GetClientCode(c.addy).ToString().Contains("Socket"))
					{
						continue;
					}
					foreach (Decoration d in c.decorations)
					{
						d.DecorationObject.SetActive(state);
					}
					foreach (Collider col in c.colliders)
					{
						col.enabled = state;
					}
					if (state)
					{
						for (int i = 0; i < c.entities.Count; i++)
						{
							c.entities[i].SetMesh(c.meshes[i]);
						}
					}
					else
					{
						foreach (RenderedEntity entity in c.entities)
						{
							entity.SetMesh(new Mesh());
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.Log("IntegratedCircuit.SetActive: It broke. Idk mang. " + e);
			}
		}
	}
}