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

        [Tooltip("The GLB mesh's own real (bind-pose) height in its authored units -- NOT the same " +
                 "thing as WorldPresence.avatarScale, which is calibrated for the 2D sprite pipeline " +
                 "where a character is exactly 1 unit tall. zeke's rigged-textured.glb measures 2.0 " +
                 "units tall bind-pose (confirmed in Blender, independent of Unity/glTFast) -- reusing " +
                 "avatarScale unmodified as this GameObject's localScale doubled the intended size " +
                 "(avatarScale=5 -> ~10 units tall instead of 5). The correction below rescales so the " +
                 "GLB avatar ends up the SAME target height (avatarScale) as a 2D avatar would.")]
        [SerializeField] private float glbNativeHeight = 2.0f;

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

            // Overwrite (not multiply) -- avatarRoot.transform.localScale was already set to
            // Vector3.one * avatarScale by WorldPresence before this callback ran, which assumes a
            // 1-unit-tall source (true for 2D sprites, not true for this GLB).
            var scale = ComputeAvatarLocalScale(_presence.avatarScale, glbNativeHeight);
            avatarRoot.transform.localScale = Vector3.one * scale;
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

        // Pure/testable: the uniform localScale that renders a GLB avatar (whose own bind-pose is
        // glbNativeHeight units tall) at the SAME target height a 2D sprite avatar would reach at
        // WorldPresence.avatarScale (2D sprites are exactly 1 unit tall, so avatarScale IS their
        // target height directly).
        public static float ComputeAvatarLocalScale(float avatarScale, float glbNativeHeight)
        {
            if (glbNativeHeight <= 0f) return avatarScale;
            return avatarScale / glbNativeHeight;
        }
    }
}
