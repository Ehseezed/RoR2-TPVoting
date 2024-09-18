using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using RoR2;
using UnityEngine.Networking;

namespace TPVoting
{
    public class TPLocker : NetworkBehaviour
    {
        public bool IsTPUnlocked = true;

        public delegate bool LockedTPInteractibility(NetworkUser user);
        public LockedTPInteractibility IsLockedTPInteractable { get; set; } = (NetworkUser user) => { return true; };

        public delegate void LockedTPInteractionAttempt(NetworkUser interactingUser);
        public event LockedTPInteractionAttempt OnLockedTPInteractionAttempt;

        public delegate Interactability orig_GetInteractability(GenericInteraction self, Interactor activator);
        public Hook hook_GetInteractability;

        public delegate void orig_OnInteractionBegin(GenericInteraction self, Interactor activator);
        public Hook hook_OnInteractionBegin;

        public void Awake()
        {
            On.RoR2.TeleporterInteraction.GetInteractability += TeleporterInteraction_GetInteractability;
            On.RoR2.TeleporterInteraction.OnInteractionBegin += TeleporterInteraction_OnInteractionBegin;

            hook_GetInteractability = new Hook(typeof(GenericInteraction).GetMethod("RoR2.IInteractable.GetInteractability", BindingFlags.NonPublic | BindingFlags.Instance), typeof(TPLocker).GetMethod("GenericInteraction_GetInteractability"), this, new HookConfig());
            hook_OnInteractionBegin = new Hook(typeof(GenericInteraction).GetMethod("RoR2.IInteractable.OnInteractionBegin", BindingFlags.NonPublic | BindingFlags.Instance), typeof(TPLocker).GetMethod("GenericInteraction_OnInteractionBegin"), this, new HookConfig());
        }

        public void OnDestroy()
        {
            On.RoR2.TeleporterInteraction.GetInteractability -= TeleporterInteraction_GetInteractability;
            On.RoR2.TeleporterInteraction.OnInteractionBegin -= TeleporterInteraction_OnInteractionBegin;

            hook_GetInteractability.Dispose();
            hook_OnInteractionBegin.Dispose();
        }

        private Interactability TeleporterInteraction_GetInteractability(On.RoR2.TeleporterInteraction.orig_GetInteractability orig, TeleporterInteraction self, Interactor activator)
        {
            return GetInteractability(orig, self, activator);
        }

        // Called only if TeleporterInteraction_GetInteractability returns Interactability.Available
        private void TeleporterInteraction_OnInteractionBegin(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, TeleporterInteraction self, Interactor activator)
        {
            OnInteractionBegin(orig, self, activator);
        }

        public Interactability GenericInteraction_GetInteractability(orig_GetInteractability orig, GenericInteraction self, Interactor activator)
        {
            if (!self.name.ToLower().Contains("portal"))
            {
                orig(self, activator);
            }

            return GetInteractability(orig, self, activator);
        }

        // Called only if GenericInteraction_GetInteractability returns Interactability.Available
        public void GenericInteraction_OnInteractionBegin(orig_OnInteractionBegin orig, GenericInteraction self, Interactor activator)
        {
            if (!self.name.ToLower().Contains("portal"))
            {
                orig(self, activator);
            }

            OnInteractionBegin(orig, self, activator);
        }

        private Interactability GetInteractability<ORIG, SELF>(ORIG orig, SELF self, Interactor activator) where ORIG : Delegate
        {
            var user = UsersHelper.GetUser(activator);
            if (IsTPUnlocked || (user && IsLockedTPInteractable(user)))
            {
                return (Interactability)orig.DynamicInvoke(self, activator);
            }
            else
            {
                return Interactability.ConditionsNotMet;
            }
        }

        private void OnInteractionBegin<ORIG, SELF>(ORIG orig, SELF self, Interactor activator) where ORIG : Delegate
        {
            if (IsTPUnlocked)
            {
                orig.DynamicInvoke(self, activator);
            }
            else
            {
                var user = UsersHelper.GetUser(activator);
                if (user)
                {
                    OnLockedTPInteractionAttempt?.Invoke(user);
                }
            }
        }
    }
}
