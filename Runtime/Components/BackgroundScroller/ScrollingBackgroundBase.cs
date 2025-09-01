using UnityEngine;

namespace NekoLib.Components
{
    public abstract class ScrollingBackgroundBase : MonoBehaviour
    {
        [SerializeField] private Vector2 _speed;
        [SerializeField] private bool _autoPlay = true;
        [SerializeField] private bool _useUnscaledTime;

        private Vector2 _offset;
        private bool _isPlaying;

        protected virtual void Start()
        {
            if (_autoPlay) Play();
        }

        private void Update()
        {
            if (!_isPlaying) return;

            var deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _offset += _speed * deltaTime;
            _offset = new Vector2(_offset.x % 1f, _offset.y % 1f);

            ApplyOffset(_offset);
        }

        protected abstract void ApplyOffset(Vector2 offset);
        protected abstract void ResetOffset();

        /// <summary>
        /// Starts the scrolling.
        /// </summary>
        public void Play()
        {
            _offset = Vector2.zero;
            ResetOffset();
            _isPlaying = true;
        }

        /// <summary>
        /// Pauses the scrolling.
        /// </summary>
        public void Pause() => _isPlaying = false;

        /// <summary>
        /// Resumes the scrolling.
        /// </summary>
        public void Resume() => _isPlaying = true;

        /// <summary>
        /// Stops the scrolling.
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            _offset = Vector2.zero;
            ResetOffset();
        }

        /// <summary>
        /// Sets the scrolling speed.
        /// </summary>
        public void SetSpeed(Vector2 newSpeed) => _speed = newSpeed;

        /// <summary>
        /// Sets the scrolling speed.
        /// </summary>
        public void SetSpeed(float x, float y) => _speed = new Vector2(x, y);

        /// <summary>
        /// Sets the scrolling speed.
        /// </summary>
        public void SetSpeedX(float x) => _speed = new Vector2(x, _speed.y);

        /// <summary>
        /// Sets the scrolling speed.
        /// </summary>
        public void SetSpeedY(float y) => _speed = new Vector2(_speed.x, y);

        public Vector2 Speed => _speed;
        public bool IsPlaying => _isPlaying;
    }
}