using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using R2API.AssetPlus;
using R2API.Utils;
using RoR2.WwiseUtils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

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
			On.RoR2.Stage.Start += Stage_Start;
			On.RoR2.Run.AdvanceStage += Run_AdvanceStage;
			On.RoR2.Run.EndStage += Run_EndStage;
		}

		private void Run_EndStage(On.RoR2.Run.orig_EndStage orig, Run self)
		{
			Debug.LogWarning($"Run_EndStage");
			AkSoundEngine.PostEvent(DrogenRehabilitation, null);
			orig(self);
		}

		private void Run_AdvanceStage(On.RoR2.Run.orig_AdvanceStage orig, Run self, SceneDef nextScene)
		{
			Debug.LogWarning($"Run_AdvanceStage");
			AkSoundEngine.PostEvent(DrogenRehabilitation, null);
			orig(self, nextScene);
		}

		private void Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
		{
			Debug.LogWarning($"Stage_Start");
			AkSoundEngine.PostEvent(DrogenRehabilitation, null);
			orig(self);
		}

		private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
		{
			if (buffDef.buffIndex == BuffIndex.TonicBuff)
			{
				AkSoundEngine.PostEvent(DrogenStop, self.gameObject);
			}
			orig(self, buffDef);
		}

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
		{
			Debug.LogWarning($"CharacterBody_OnBuffFirstStackGained");
			if (buffDef.buffIndex == BuffIndex.TonicBuff)
			{
				AkSoundEngine.PostEvent(DrogenStart, self.gameObject);
			}
			orig(self, buffDef);
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
