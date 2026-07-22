using UnityEngine;

namespace Game.Environment
{
    // Plays a packed sprite sheet as a seamless looping animation on a SpriteRenderer. Slices the
    // sheet into frames at runtime (row-major, top-left = frame 0, matching the factory's
    // animated-prop packer) so no Editor slicing / AnimationClip authoring is needed -- a light,
    // self-contained alternative to an Animator for ambient looping effects (torches, fire, etc.).
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSheetLoopAnimator : MonoBehaviour
    {
        [Tooltip("Packed sprite sheet (frames laid out left-to-right, top-to-bottom).")]
        public Texture2D sheet;
        public int columns = 5;
        public int rows = 5;
        [Tooltip("Number of real frames (a sheet grid may have empty trailing cells).")]
        public int frameCount = 25;
        public float fps = 12f;
        public float pixelsPerUnit = 100f;

        private SpriteRenderer _sr;
        private Sprite[] _frames;
        private float _timer;
        private int _index;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            Build();
        }

        private void OnEnable() => Build();

        private void Build()
        {
            if (sheet == null || columns <= 0 || rows <= 0) return;
            int cw = sheet.width / columns;
            int ch = sheet.height / rows;
            int n = Mathf.Clamp(frameCount, 1, columns * rows);
            _frames = new Sprite[n];
            for (int i = 0; i < n; i++)
            {
                int col = i % columns;
                int row = i / columns;
                // Texture space is bottom-up; frame 0 is the TOP-left cell.
                var rect = new Rect(col * cw, sheet.height - (row + 1) * ch, cw, ch);
                _frames[i] = Sprite.Create(sheet, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            if (_sr != null && _frames.Length > 0) _sr.sprite = _frames[0];
        }

        private void Update()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_frames == null) Build();   // lazy: sheet may be assigned after AddComponent
            if (_frames == null || _frames.Length == 0 || _sr == null || fps <= 0f) return;
            _timer += Time.deltaTime;
            float step = 1f / fps;
            while (_timer >= step)
            {
                _timer -= step;
                _index = (_index + 1) % _frames.Length;
                _sr.sprite = _frames[_index];
            }
        }
    }
}
