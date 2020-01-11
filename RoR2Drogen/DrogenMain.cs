using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using R2API.AssetPlus;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace RoR2Drogen
{
	[BepInDependency("com.bepis.r2api")]
	[BepInPlugin("com.respeak.drogen", "DrogenJedenTag", "1.0.0")]
	[R2APISubmoduleDependency(nameof(AssetPlus))]
	public class DrogenMain : BaseUnityPlugin
	{
		private const string BankName = "drogen_soundbank.bnk";

		private const uint DrogenStart = 1821358973;
		private const uint DrogenStop = 3106969855;
		private const uint DrogenPause = 83071095;
		private const uint DrogenResume = 3040584550;
		private const uint DrogenRehabilitation = 452547817;

		public void Awake()
		{
			AddSoundBank();

			On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
			On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
			On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
			On.RoR2.CharacterBody.OnDestroy += CharacterBody_OnDestroy;
		}

		private void CharacterBody_OnDestroy(On.RoR2.CharacterBody.orig_OnDestroy orig, CharacterBody self)
		{
			//Debug.LogWarning($"CharacterBody_OnDestroy {self}");
			Drogenentzug(self);
			orig(self);
		}

		private void CharacterBody_OnDeathStart(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
		{
			//Debug.LogWarning($"CharacterBody_OnDeathStart {self}");
			Drogenentzug(self);
			orig(self);
		}

		private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
		{
			//Debug.LogWarning($"CharacterBody_OnBuffFinalStackLost {self}");
			if (self.isPlayerControlled && buffDef.buffIndex == BuffIndex.TonicBuff)
			{
				Drogenentzug(self);
			}
			orig(self, buffDef);
		}

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
		{
			//Debug.LogWarning($"CharacterBody_OnBuffFirstStackGained {self}");
			if (self.isPlayerControlled && buffDef.buffIndex == BuffIndex.TonicBuff)
			{
				AkSoundEngine.PostEvent(DrogenStart, self.gameObject);
			}
			orig(self, buffDef);
		}

		private void Drogenentzug(CharacterBody self)
		{
			if (self == null || self.gameObject == null || !self.isPlayerControlled)
				return;
			AkSoundEngine.PostEvent(DrogenStop, self.gameObject);
		}

		public static void AddSoundBank()
		{
			var soundbank = LoadEmbeddedResource(BankName);
			if (soundbank != null)
			{
				var sbId = SoundBanks.Add(soundbank);
				Debug.LogWarning($"Load BankId {sbId}");
			}
			else
			{
				Debug.LogError("SoundBank Fetching Failed");
			}
		}

		private static byte[] LoadEmbeddedResource(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			resourceName = assembly.GetManifestResourceNames()
				.First(str => str.EndsWith(resourceName));

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				var data = new byte[stream.Length];
				stream.Read(data, 0, data.Length);
				return data;
			}
		}
	}
}
