using LogicAPI.Interfaces;
using LogicWorld.Rendering.Components;
using UnityEngine;

namespace Locomotion
{
	public class Banner : ComponentClientCode, IComponentInfo
	{
		Texture2D texture;
		Transform visual;

		public string TextID => throw new System.NotImplementedException();

		public System.Type LogicCodeType => throw new System.NotImplementedException();

		public string LogicScript => throw new System.NotImplementedException();

		public System.Type ClientCodeType => throw new System.NotImplementedException();

		int[] IComponentInfo.CodeInfoInts => throw new System.NotImplementedException();

		float[] IComponentInfo.CodeInfoFloats => throw new System.NotImplementedException();

		string[] IComponentInfo.CodeInfoStrings => throw new System.NotImplementedException();

		bool[] IComponentInfo.CodeInfoBools => throw new System.NotImplementedException();

		protected override void InitializeInWorld()
		{
			texture = new Texture2D(256, 256);
			texture = Main.LoadImage(CodeInfoStrings[0]);

			Material material = new Material(Shader.Find("Unlit/Texture"));
			material.mainTexture = texture;

			visual = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
			visual.localScale = GetBlockEntity(0).Scale/10f;
			visual.GetComponent<Renderer>().material = material;
			visual.position = GetBlockEntity(0).WorldPosition + GetBlockEntity(0).up * (GetBlockEntity(0).Scale.y + 0.01f);
			visual.rotation = GetBlockEntity(0).WorldRotation;

			Mesh mesh = GetBlockEntity(0).Mesh;
			Vector3[] vertices = mesh.vertices;
			Vector2[] uvs = new Vector2[vertices.Length];

			for (int i = 0; i < uvs.Length; i++)
			{
				uvs[i] = new Vector2(vertices[i].x + 0.5f, vertices[i].z + 0.5f);
			}
			mesh.uv = uvs;
		}
		protected override void OnComponentDestroyed()
		{
			if (visual)
				Object.Destroy(visual.gameObject);
		}
	}
}
