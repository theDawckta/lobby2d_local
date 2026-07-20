using System.Collections;
using UnityEngine;
using OneTimeGames.CoreSystems;

namespace Game.Auth
{
    // Wires the game's ConfigService (Rule C) to CoreSystems' FactoryAuth so player identity is
    // resolved automatically on scene start, with no in-game login form -- FactoryAuth itself
    // handles the ?code= redeem vs guest fallback; this controller's only job is to supply it
    // charactersBaseUrl from runtime config and kick off Resolve().
    [RequireComponent(typeof(FactoryAuth))]
    public class AuthConnectController : MonoBehaviour
    {
        public FactoryAuth Auth { get; private set; }

        private void Awake()
        {
            Auth = GetComponent<FactoryAuth>();
        }

        private IEnumerator Start()
        {
            // Guard: ConfigService may not exist yet if this GameObject is instantiated outside
            // the normal scene flow (e.g. directly in a test) before its auto-create hook runs.
            if (ConfigService.Instance != null)
            {
                yield return ConfigService.Instance.EnsureLoaded();
                Auth.charactersBaseUrl = ConfigService.Instance.Get("charactersBaseUrl");
            }
            Auth.Resolve();
        }
    }
}
