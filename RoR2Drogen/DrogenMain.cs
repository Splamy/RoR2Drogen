using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using static RoR2.RoR2Content;

namespace RoR2Drogen
{
	[BepInDependency("com.bepis.r2api")]
	[BepInPlugin("com.respeak.drogen", "DrogenJedenTag", "1.2.0")]
	[NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
	public class DrogenMain : BaseUnityPlugin
	{
		private const string BankName = "drogen_soundbank.bnk";

		public const uint DrogenStart = 1821358973;
		public const uint DrogenStop = 3106969855;
		public const uint DrogenPause = 83071095;
		public const uint DrogenResume = 3040584550;
		public const uint DrogenRehabilitation = 452547817;

		public const uint GnomeGnomedPlay = 3563009708;
		public const uint GnomeHuhPlay = 247409129;

		public const uint SteamworksStart = 2660522436;
		public const uint SteamworksStop = 1138002366;

		public const uint LazerStart = 2729798857;
		public const uint LazerCharge = 758272295;
		public const uint LazerFire = 2040934607;
		public const uint LazerFireEnd = 2649077457;
		public const uint LazerStop = 182908003;

		public void Awake()
		{
			AddSoundBank();

			On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
			On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
			On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
			On.RoR2.CharacterBody.OnDestroy += CharacterBody_OnDestroy;
			On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
			On.RoR2.PurchaseInteraction.OnEnable += PurchaseInteraction_OnEnable;
			On.RoR2.PurchaseInteraction.OnDisable += PurchaseInteraction_OnDisable;
			On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
			On.RoR2.Inventory.RpcItemAdded += Inventory_RpcItemAdded;
			On.RoR2.MoneyPickup.Start += MoneyPickup_Start;
			On.EntityStates.TitanMonster.FireMegaLaser.OnEnter += FireMegaLaser_OnEnter;
			On.EntityStates.TitanMonster.ChargeMegaLaser.OnEnter += ChargeMegaLaser_OnEnter;
			On.EntityStates.TitanMonster.FireMegaLaser.OnExit += FireMegaLaser_OnExit;
		}

		#region Lazermaster

		private void ChargeMegaLaser_OnEnter(On.EntityStates.TitanMonster.ChargeMegaLaser.orig_OnEnter orig, EntityStates.TitanMonster.ChargeMegaLaser self)
		{
			var isGold = self is EntityStates.TitanMonster.ChargeGoldMegaLaser;

			var gameObj = self.outer.gameObject;
			var lm = gameObj.GetComponent<Lazermaster>();
			if (lm == null)
			{
				lm = gameObj.AddComponent<Lazermaster>();
				AkSoundEngine.PostEvent(LazerStart, gameObj);
			}

			//Debug.LogError($"Chaaarge {Time.realtimeSinceStartup} {isGold}");
			AkSoundEngine.PostEvent(LazerCharge, gameObj);
			orig(self);
		}

		private void FireMegaLaser_OnEnter(On.EntityStates.TitanMonster.FireMegaLaser.orig_OnEnter orig, EntityStates.TitanMonster.FireMegaLaser self)
		{
			var isGold = self is EntityStates.TitanMonster.FireGoldMegaLaser;
			//Debug.LogError($"LAZERRRRRRR {Time.realtimeSinceStartup} {isGold}");
			AkSoundEngine.PostEvent(LazerFire, self.outer.gameObject);
			orig(self);
		}

		private void FireMegaLaser_OnExit(On.EntityStates.TitanMonster.FireMegaLaser.orig_OnExit orig, EntityStates.TitanMonster.FireMegaLaser self)
		{
			var isGold = self is EntityStates.TitanMonster.FireGoldMegaLaser;
			//Debug.LogError($"NO lazer {Time.realtimeSinceStartup} {isGold}");
			AkSoundEngine.PostEvent(LazerFireEnd, self.outer.gameObject);
			orig(self);
		}

		#endregion

		#region Gnoomed

		private void Inventory_RpcItemAdded(On.RoR2.Inventory.orig_RpcItemAdded orig, Inventory self, ItemIndex itemIndex)
		{
			try
			{
				Debug.LogWarning($"Inventory_RpcItemAdded {self} {itemIndex}");
				CheckGnooomed(self, itemIndex);
			}
			catch { Debug.LogWarning($"ERRRRR: Inventory_RpcItemAdded"); }
			orig(self, itemIndex);
		}

		private void CheckGnooomed(Inventory self, ItemIndex itemIndex)
		{
			if (itemIndex != Items.BonusGoldPackOnKill.itemIndex)
				return;

			var cm = self.GetComponent<CharacterMaster>();
			if (cm == null)
			{
				Debug.Log("No character master, skipping");
				return;
			}
			var body = cm.GetBodyObject();
			if (body == null)
			{
				Debug.Log("No character body, skipping");
				return;
			}

			Debug.LogError("You got gnomed");

			AkSoundEngine.PostEvent(GnomeGnomedPlay, body);
		}

		private void MoneyPickup_Start(On.RoR2.MoneyPickup.orig_Start orig, MoneyPickup self)
		{
			self.gameObject.AddComponent<GoldDestroyer>();
			orig(self);
		}

		#endregion

		#region Gambling

		private void PurchaseInteraction_OnDisable(On.RoR2.PurchaseInteraction.orig_OnDisable orig, PurchaseInteraction self)
		{
			//Debug.LogWarning($"PurchaseInteraction_OnDisable");
			AkSoundEngine.PostEvent(SteamworksStop, self.gameObject);
			orig(self);
		}

		private void PurchaseInteraction_SetAvailable(On.RoR2.PurchaseInteraction.orig_SetAvailable orig, PurchaseInteraction self, bool newAvailable)
		{
			//Debug.LogWarning($"PurchaseInteraction_SetAvailable {newAvailable}");
			if (self.gameObject.GetComponent<ShrineChanceBehavior>() != null)
			{
				if (newAvailable)
				{
					AkSoundEngine.PostEvent(SteamworksStart, self.gameObject);
				}
				else
				{
					AkSoundEngine.PostEvent(SteamworksStop, self.gameObject);
				}
			}
			orig(self, newAvailable);
		}

		private void PurchaseInteraction_OnEnable(On.RoR2.PurchaseInteraction.orig_OnEnable orig, PurchaseInteraction self)
		{
			//Debug.LogWarning($"PurchaseInteraction_OnEnable");
			if (self.gameObject.GetComponent<ShrineChanceBehavior>() != null)
			{
				AkSoundEngine.PostEvent(SteamworksStart, self.gameObject);
			}
			orig(self);
		}

		private void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator)
		{
			//Debug.LogWarning($"ShrineChanceBehavior_AddShrineStack");
			orig(self, activator);
		}

		#endregion

		#region Drogen

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
			if (self.isPlayerControlled && buffDef.buffIndex == Buffs.TonicBuff.buffIndex)
			{
				Drogenentzug(self);
			}
			orig(self, buffDef);
		}

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
		{
			//Debug.LogWarning($"CharacterBody_OnBuffFirstStackGained {self} {buffDef}");
			if (self.isPlayerControlled && buffDef.buffIndex == Buffs.TonicBuff.buffIndex)
			{
				Drogenrausch(self);
			}
			orig(self, buffDef);
		}

		private void Drogenrausch(CharacterBody self)
		{
			AkSoundEngine.PostEvent(DrogenStart, self.gameObject);
		}

		private void Drogenentzug(CharacterBody self)
		{
			if (self == null || self.gameObject == null || !self.isPlayerControlled)
				return;
			AkSoundEngine.PostEvent(DrogenStop, self.gameObject);
		}

		#endregion

		public static void AddSoundBank()
		{
			var soundbank = LoadEmbeddedResource(BankName);
			if (soundbank != null)
			{
				var sbId = SoundAPI.SoundBanks.Add(soundbank);
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

		private static void PrintGameObjectTree(GameObject start)
		{
			Debug.LogWarning("---");
			GameObject cur = start;
			var elements = new List<GameObject>();
			while (cur.transform.parent != null)
			{
				cur = cur.transform.parent.gameObject;
				elements.Add(cur);
			}
			elements.Reverse();

			int indentCounter = 0;
			foreach (var e in elements)
			{
				PrintGameObject(e, indentCounter, false);
				indentCounter++;
			}
			PrintGameObjectChildren(start, indentCounter, true);
			Debug.LogWarning("---");
		}

		private static void PrintGameObject(GameObject gameObject, int indentCount, bool isStart)
		{
			string startStr = isStart ? "<-" : "";
			string indent = "";
			for (int i = 0; i < indentCount; i++)
			{
				indent += "  ";
			}
			Debug.LogWarning($"{indent}{gameObject.name} {startStr}");
			gameObject.GetComponents<MonoBehaviour>().ToList().ForEach(mb => Debug.LogWarning($"{indent}⊢{mb.GetType().Name}"));
		}

		private static void PrintGameObjectChildren(GameObject gameObject, int indentCount, bool isStart)
		{
			PrintGameObject(gameObject, indentCount, isStart);
			foreach (Transform child in gameObject.transform)
			{
				PrintGameObjectChildren(child.gameObject, indentCount + 1, false);
			}
		}
	}
}
