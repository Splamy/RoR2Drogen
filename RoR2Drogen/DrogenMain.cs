using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using R2API.AssetPlus;
using R2API.Utils;
using RoR2;
using UnityEngine;
using BossGroup = On.RoR2.BossGroup;

namespace RoR2Drogen
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.respeak.drogen", "DrogenJedenTag", "1.0.2")]
    [R2APISubmoduleDependency(nameof(AssetPlus))]
    public class DrogenMain : BaseUnityPlugin
    {
        private const string BankName = "drogen_soundbank.bnk";

        private const uint DrogenStart = 1821358973;
        private const uint DrogenStop = 3106969855;
        private const uint DrogenPause = 83071095;
        private const uint DrogenResume = 3040584550;
        private const uint DrogenRehabilitation = 452547817;

        private const uint SteamworksStart = 2660522436;
        private const uint SteamworksStop = 1138002366;

        public void Awake()
        {
            AddSoundBank();

            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
            On.RoR2.CharacterBody.OnDestroy += CharacterBody_OnDestroy;
            On.RoR2.ShrineChanceBehavior.Start += ShrineChanceBehavior_Start;
            On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.PurchaseInteraction.OnEnable += PurchaseInteraction_OnEnable;
            On.RoR2.PurchaseInteraction.OnDisable += PurchaseInteraction_OnDisable;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            On.RoR2.PurchaseInteraction.SetAvailableTrue += PurchaseInteraction_SetAvailableTrue;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBodyOnOnSkillActivated;
        }

        private void CharacterBodyOnOnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            Debug.LogWarning("killActivated!!!");
            if (self != null) Debug.LogWarning($"self: ${self.name}");
            if (skill != null) Debug.LogWarning($"skill: ${skill.skillName}");
            orig(self, skill);
        }

        private void PurchaseInteraction_SetAvailableTrue(On.RoR2.PurchaseInteraction.orig_SetAvailableTrue orig, PurchaseInteraction self)
        {
            Debug.LogWarning($"PurchaseInteraction_SetAvailableTrue");
            orig(self);
        }

        private void PurchaseInteraction_OnDisable(On.RoR2.PurchaseInteraction.orig_OnDisable orig, PurchaseInteraction self)
        {
            Debug.LogWarning($"PurchaseInteraction_OnDisable");
            AkSoundEngine.PostEvent(SteamworksStop, self.gameObject);
            orig(self);
        }

        private void PurchaseInteraction_SetAvailable(On.RoR2.PurchaseInteraction.orig_SetAvailable orig, PurchaseInteraction self, bool newAvailable)
        {
            Debug.LogWarning($"PurchaseInteraction_SetAvailable {newAvailable}");
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
            Debug.LogWarning($"PurchaseInteraction_OnEnable");
            if (self.gameObject.GetComponent<ShrineChanceBehavior>() != null)
            {
                AkSoundEngine.PostEvent(SteamworksStart, self.gameObject);
            }
            orig(self);
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            Debug.LogWarning($"PurchaseInteraction_OnInteractionBegin");
            orig(self, activator);
        }

        private void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator)
        {
            Debug.LogWarning($"ShrineChanceBehavior_AddShrineStack");
            orig(self, activator);
        }

        private void ShrineChanceBehavior_Start(On.RoR2.ShrineChanceBehavior.orig_Start orig, ShrineChanceBehavior self)
        {
            Debug.LogWarning($"ShrineChanceBehavior_Start");
            orig(self);
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

        private static void PrintGameObjectTree(GameObject start)
        {
            Debug.LogWarning("---");
            GameObject cur = start;
            List<GameObject> elements = new List<GameObject>();
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
