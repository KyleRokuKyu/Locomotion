using GameDataAccess;
using LogicAPI;
using LogicAPI.Client;
using LogicAPI.Data;
using LogicWorld;
using LogicWorld.Interfaces;
using System;
using System.IO;
using UnityEngine;

namespace Locomotion
{
    class Main : ClientMod
    {
        public static CustomSoundEffect PistonUp { get; private set; }
        public static CustomSoundEffect PistonDown { get; private set; }
		public static CustomSoundClipDatabase ClipDatabase { get; private set; }

		public static void TryLoadSoundEffects ()
		{
			if (PistonUp != null)
				return;

			ClipDatabase = new CustomSoundClipDatabase();

			PistonUp = new CustomSoundEffect();
			PistonDown = new CustomSoundEffect();

			CustomSoundEffectData data = new CustomSoundEffectData() { VolumeScale = 10f, ClipRelativePaths = new string[1] { "Locomotion/sounds/clips/PistonUp.wav" } };
			PistonUp.Initialize(data, "Interaction", ClipDatabase);
			data = new CustomSoundEffectData() { VolumeScale = 10f, ClipRelativePaths = new string[1] { "Locomotion/sounds/clips/PistonDown.wav" } };
			PistonDown.Initialize(data, "Interaction", ClipDatabase);
		}

        public static Texture2D LoadImage(string filename)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(GameData.GetFileByRelativePath(filename).FullName);

                Texture2D texture = new Texture2D(4, 4);
                ImageConversion.LoadImage(texture, bytes);

                return texture;
            }
            catch
            {
                Texture2D texture = new Texture2D(4, 4);
                Color pink = new Color(1, 0.5f, 0.5f);
                Color black = Color.black;
                Color[] colors = new Color[16] { pink, black, pink, black, pink, black, pink, black, pink, black, pink, black, pink, black, pink, black };
                texture.SetPixels(colors);
                texture.Apply();
                return texture;
            }
        }

		public static void StopRenderingExceptThis(ComponentAddress cAddress, IWorldRenderer worldRenderer)
		{
			ComponentAddress pAddress = SceneAndNetworkManager.MainWorld.Data.Lookup(cAddress).Parent;

			foreach (ComponentAddress item in SceneAndNetworkManager.MainWorld.Data.Lookup(pAddress).EnumerateChildren())
			{
				if (item == cAddress)
					continue;
				try
				{
					if (worldRenderer.Entities.GetClientCode(item) != null && worldRenderer.Entities.GetClientCode(item).ToString().Contains("Socket"))
					{
						continue;
					}
				}
				catch (Exception e)
				{
					Debug.Log("StopRenderingExceptThis: It broke. Idk mang. " + e);
				}
				worldRenderer.EntityManager.StopRenderingComponentRecursive(item);
			}
			worldRenderer.EntityManager.StopRenderingComponent(pAddress);
			foreach (WireAddress item2 in SceneAndNetworkManager.MainWorld.Data.GetWiresDirectlyAttachedTo(pAddress))
			{
				worldRenderer.EntityManager.StopRenderingWire(item2);
			}
		}

		public static void StartRendering(ComponentAddress cAddress, IWorldRenderer worldRenderer)
		{
			try
			{
				if (cAddress == null)
					return;
				//worldRenderer.EntityManager.RenderComponent(cAddress);
				StartRenderingRecursively(cAddress, worldRenderer);
			}
			catch (Exception e)
			{
				Debug.Log("StartRendering: It broke. Idk mang. " + e);
			}
		}

		public static void StartRenderingRecursively(ComponentAddress cAddress, IWorldRenderer worldRenderer)
		{
			Debug.Log("1");
			try
			{
				if (cAddress != null)
				{
					Debug.Log("2");
					worldRenderer.EntityManager.RenderComponent(cAddress);
					//worldRenderer.EntityManager.ReRenderComponentsAndWiresRecursive(cAddress);
				}
				Debug.Log("3");
				foreach (ComponentAddress item in SceneAndNetworkManager.MainWorld.Data.Lookup(cAddress).EnumerateChildren())
				{
					Debug.Log("4");
					if (item == cAddress)
						continue;
					Debug.Log("5");
					StartRenderingRecursively(item, worldRenderer);
					Debug.Log("6");
					/*
					foreach (WireAddress item2 in SceneAndNetworkManager.MainWorld.Data.GetWiresDirectlyAttachedTo(item))
					{
						Debug.Log("7");
						worldRenderer.EntityManager.RenderWire(item2);
					}
					*/
					Debug.Log("8");
				}
				Debug.Log("9");
			}
			catch (Exception e)
			{
				Debug.Log("StartRenderingRecursively: It broke. Idk mang. " + e);
			}
		}

		public static void StartRenderingExceptThis(ComponentAddress cAddress, ComponentAddress oAddress, IWorldRenderer worldRenderer)
		{
			try
			{
				if (oAddress == null)
					return;
				StartRenderingRecursively(oAddress, worldRenderer);
			}
			catch (Exception e)
			{
				Debug.Log("StartRendering: It broke. Idk mang. " + e);
			}
		}

		public static void StartRenderingRecursivelyExceptThis(ComponentAddress cAddress, ComponentAddress oAddress, IWorldRenderer worldRenderer)
		{
			Debug.Log("1");
			try
			{
				if (oAddress != null)
				{
					Debug.Log("2");
					worldRenderer.EntityManager.RenderComponent(oAddress);
					//worldRenderer.EntityManager.ReRenderComponentsAndWiresRecursive(cAddress);
				}
				Debug.Log("3");
				foreach (ComponentAddress item in SceneAndNetworkManager.MainWorld.Data.Lookup(oAddress).EnumerateChildren())
				{
					Debug.Log("4");
					if (item == cAddress || item == oAddress)
						continue;
					Debug.Log("5");
					StartRenderingRecursively(item, worldRenderer);
					Debug.Log("6");
					/*
					foreach (WireAddress item2 in SceneAndNetworkManager.MainWorld.Data.GetWiresDirectlyAttachedTo(item))
					{
						Debug.Log("7");
						worldRenderer.EntityManager.RenderWire(item2);
					}
					*/
					Debug.Log("8");
				}
				Debug.Log("9");
			}
			catch (Exception e)
			{
				Debug.Log("StartRenderingRecursively: It broke. Idk mang. " + e);
			}
		}
	}
}
