﻿using HarmonyLib;

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    [HarmonyPatch(typeof(PauseController), nameof(PauseController.HandleHMDUnmounted))]
    internal class PatchHandleHMDUnmounted {
        internal static bool Prefix() {

            return !Plugin.ReplayState.IsPlaybackEnabled;
        }
    }
}