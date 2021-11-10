using GameDataAccess;
using LogicAPI.Client;
using System.IO;
using UnityEngine;

namespace Locomotion
{
    class Main : ClientMod
    {
        public static CustomSoundEffect PistonUp { get; private set; }
        public static CustomSoundEffect PistonDown { get; private set; }
		public static CustomSoundClipDatabase clipDatabase { get; private set; }

		public static void TryLoadSoundEffects ()
		{
			if (PistonUp != null)
				return;

			clipDatabase = new CustomSoundClipDatabase();

			PistonUp = new CustomSoundEffect();
			PistonDown = new CustomSoundEffect();

			CustomSoundEffectData data = new CustomSoundEffectData() { VolumeScale = 10f, ClipRelativePaths = new string[1] { "Locomotion/sounds/clips/PistonUp.wav" } };
			PistonUp.Initialize(data, "Interaction", clipDatabase);
			data = new CustomSoundEffectData() { VolumeScale = 10f, ClipRelativePaths = new string[1] { "Locomotion/sounds/clips/PistonDown.wav" } };
			PistonDown.Initialize(data, "Interaction", clipDatabase);
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
    }
}
