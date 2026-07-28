using System;
using System.Collections.Generic;
using UnityEngine;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.Presence;

namespace Game.Player
{
    // Renders specific characters as a 3D GLB avatar (CoreSystems' GlbCharacterAnimator) instead of
    // WorldPresence's default 2D billboard sprite -- for both the local player's own avatar and any
    // remote player wearing the same character, so everyone sees the same 3D model. Hooks
    // WorldPresence.avatarViewOverride; the GLB is loaded from the same characters-static host that
    // already serves the character's 2D sheets (<charactersBaseUrl>/characters-static/<name>/model3d/
    // <name>-rigged-textured.glb -- server.js already serves the whole CoreSystems/Characters/<name>/
    // tree, not just spritesheets/).
    //
    // Lobby2d_local-local only, not a CoreSystems component -- per the factory convention of building
    // a game-specific candidate first and promoting it later if a second game wants the same hook.
    [RequireComponent(typeof(WorldPresence))]
    public class Glb3DAvatarOverride : MonoBehaviour
    {
        [Tooltip("Character names (case-insensitive) that render as a 3D GLB avatar instead of the " +
                 "default 2D billboard sprite.")]
        [SerializeField] private List<string> glbCharacterNames = new List<string> { "zeke" };

        [SerializeField] private string idleClip = "Idle";
        [SerializeField] private string moveClip = "Run";

        [Tooltip("Yaw (degrees) applied to the loaded model so its authored forward faces travel " +
                 "direction. Tune live in Play mode if the character appears to strafe/sidestep or " +
                 "face backwards -- see GlbCharacterAnimator.modelYawOffset.")]
        [SerializeField] private float modelYawOffset = 0f;

        private WorldPresence _presence;

        private void Awake()
        {
            _presence = GetComponent<WorldPresence>();
            _presence.avatarViewOverride = TrySpawnGlbAvatar;
        }

        private bool TrySpawnGlbAvatar(string characterName, GameObject avatarRoot, bool isLocal)
        {
            if (string.IsNullOrEmpty(characterName) || !WantsGlbAvatar(characterName)) return false;
            if (string.IsNullOrEmpty(_presence.charactersBaseUrl)) return false;

            var glb = avatarRoot.AddComponent<GlbCharacterAnimator>();
            glb.GlbUrl = BuildGlbUrl(_presence.charactersBaseUrl, characterName);
            glb.IdleClip = idleClip;
            glb.MoveClip = moveClip;
            glb.ModelYawOffset = modelYawOffset;
            avatarRoot.AddComponent<GlbAvatarMotion>();
            return true;
        }

        private bool WantsGlbAvatar(string characterName)
        {
            foreach (var n in glbCharacterNames)
                if (string.Equals(n, characterName, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        // Pure/testable: matches the server.js /characters-static/<name>/model3d/<name>-rigged-textured.glb
        // convention already used for every character's rigged+textured mesh.
        public static string BuildGlbUrl(string charactersBaseUrl, string characterName)
        {
            return $"{charactersBaseUrl.TrimEnd('/')}/characters-static/{characterName}/model3d/{characterName}-rigged-textured.glb";
        }
    }
}
