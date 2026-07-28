using UnityEngine;
using OneTimeGames.CoreSystems;

namespace Game.Player
{
    // Drives a GlbCharacterAnimator's locomotion (SetSpeed) from this GameObject's OWN per-frame
    // movement -- the 3D-avatar counterpart to SpriteCharacterAnimator's velocity derivation. Works
    // unmodified whether this transform is moved directly (the local player) or lerped toward a
    // network target (RemotePlayer), since both cases move THIS transform every frame either way.
    [RequireComponent(typeof(GlbCharacterAnimator))]
    public class GlbAvatarMotion : MonoBehaviour
    {
        private GlbCharacterAnimator _animator;
        private Vector3 _lastPosition;
        private bool _hasLastPosition;

        private void Awake()
        {
            _animator = GetComponent<GlbCharacterAnimator>();
        }

        private void Update()
        {
            var pos = transform.position;
            if (!_hasLastPosition)
            {
                _lastPosition = pos;
                _hasLastPosition = true;
                return;
            }

            var speed = Time.deltaTime > 0f ? (pos - _lastPosition).magnitude / Time.deltaTime : 0f;
            _lastPosition = pos;
            _animator.SetSpeed(speed);
        }
    }
}
