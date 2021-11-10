using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameDataAccess;
using JimmysUnityUtilities;
using UnityEngine;
using System.Linq;
using LogicSettings;
using LogicWorld.Audio;
using UnityEngine.Audio;
using SUCC;

namespace Locomotion
{
	internal class CustomSoundClipDatabase
	{
		private Dictionary<string, AudioClip> _ClipsByRelativePath = new Dictionary<string, AudioClip>();

		private HashSet<string> ClipsCurrentlyBeingLoaded = new HashSet<string>();

		public IReadOnlyDictionary<string, AudioClip> ClipsByRelativePath => _ClipsByRelativePath;

		public void UnloadEverything()
		{
			foreach (AudioClip value in ClipsByRelativePath.Values)
			{
				UnityEngine.Object.Destroy(value);
			}
			_ClipsByRelativePath.Clear();
		}

		public void GetClipByRelativePath(string relativePath, System.Action<AudioClip> onClipLoaded)
		{
			if (ClipsCurrentlyBeingLoaded.Contains(relativePath))
			{
				CoroutineUtility.Run(WaitForClipToBeLoaded());
				return;
			}
			if (ClipsByRelativePath.TryGetValue(relativePath, out var value))
			{
				onClipLoaded(value);
				return;
			}
			FileInfo fileByRelativePath = GameData.GetFileByRelativePath(relativePath);
			if (AudioLoadingUtilities.FileCanBeLoadedAsAudio(fileByRelativePath))
			{
				ClipsCurrentlyBeingLoaded.Add(relativePath);
				AudioLoadingUtilities.LoadAudioFromDisk(fileByRelativePath, delegate (AudioClip loadedClip)
				{
					loadedClip.name = relativePath;
					_ClipsByRelativePath.Add(relativePath, loadedClip);
					onClipLoaded(loadedClip);
					ClipsCurrentlyBeingLoaded.Remove(relativePath);
				});
			}
			IEnumerator WaitForClipToBeLoaded()
			{
				AudioClip value2;
				while (!ClipsByRelativePath.TryGetValue(relativePath, out value2))
				{
					yield return null;
				}
				onClipLoaded(value2);
			}
		}
	}

	class CustomSoundEffect : SoundEffect
	{
		public float MaxRandomPitchVariation { get; internal set; }

		public float MaxRandomVolumeVariation { get; internal set; }

		public float PitchScale { get; internal set; }

		public float VolumeScale { get; internal set; }

		public AudioMixerGroup Output { get; internal set; }

		public AudioClip[] Clips { get; internal set; }


		private static bool EnablePitchRandomization { get; set; } = true;


		private static bool EnableVolumeRandomization { get; set; } = true;


		private static bool EnableClipRandomization { get; set; } = true;

		internal void Initialize(CustomSoundEffectData data, string outputGroup, CustomSoundClipDatabase clipDatabase)
		{
			MaxRandomPitchVariation = data.MaxRandomPitchVariation;
			MaxRandomVolumeVariation = data.MaxRandomVolumeVariation;
			PitchScale = data.PitchScale;
			VolumeScale = data.VolumeScale;
			Output = Object.FindObjectOfType<AudioSource>().outputAudioMixerGroup.audioMixer.FindMatchingGroups(outputGroup)[0];
			Clips = new AudioClip[data.ClipRelativePaths.Length];
			for (int i = 0; i < Clips.Length; i++)
			{
				string relativePath = data.ClipRelativePaths[i];
				int index = i;
				clipDatabase.GetClipByRelativePath(relativePath, delegate (AudioClip loadedClip)
				{
					Debug.Log("Loaded clip " + loadedClip.name);
					Clips[index] = loadedClip;
				});
			}
		}

		public void AddClip(AudioClip clip)
		{
			AudioClip[] tempArray = new AudioClip[Clips.Length + 1];
			Clips.CopyTo(tempArray, 0);
			tempArray[tempArray.Length - 1] = clip;
			Clips = tempArray;
		}

		internal float GetPitch()
		{
			if (EnablePitchRandomization)
			{
				return PitchScale + Random.Range(0f - MaxRandomPitchVariation, MaxRandomPitchVariation);
			}
			return PitchScale;
		}

		internal float GetVolumeScale()
		{
			if (EnableVolumeRandomization)
			{
				return VolumeScale - Random.Range(0f, MaxRandomVolumeVariation);
			}
			return VolumeScale;
		}

		internal AudioClip GetClip()
		{
			if (Clips == null || Clips.Length == 0)
			{
				return Sounds.FailDoSomething.Clips[0];
			}
			AudioClip audioClip = ((!EnableClipRandomization) ? Clips.FirstOrDefault() : Clips.GetRandomElement());
			if (audioClip == null)
			{
				return Sounds.FailDoSomething.Clips[0];
			}
			return audioClip;
		}

		public void PlayAudioAt(Vector3 location)
		{
			AudioSource.PlayClipAtPoint(GetClip(), location, GetVolumeScale());
		}
	}

	internal class CustomSoundEffectData
	{
		public float MaxRandomPitchVariation { get; set; } = 0.035f;

		public float MaxRandomVolumeVariation { get; set; } = 0.3f;

		public float PitchScale { get; set; } = 1f;

		public float VolumeScale { get; set; } = 1f;

		[SaveThis(SaveAs = "Clips")]
		public string[] ClipRelativePaths { get; set; }
	}
}