using GifImporter;
using UnityEngine;
using UnityEngine.UI;

namespace GifImporter
{
    [ExecuteAlways]
    public class GifPlayer : MonoBehaviour
    {
        public Gif Gif;

        private int   _index;
        private float _flip;
        private Gif   _setGif;

        private void OnEnable()
        {
            if (Gif == null) return;
            var frames = Gif.Frames;
            if (frames == null || frames.Count == 0) return;

            if (_index > frames.Count - 1)
            {
                _index = _index % frames.Count;
            }

            var frame = frames[_index];
            Apply(frame);
        }

        private void Update()
        {
            if (Gif == null) return;
            var frames = Gif.Frames;
            if (frames == null || frames.Count == 0) return;

            int index = _index;

            if (Application.isPlaying && _flip < Time.time)
            {
                index++;
            }

            if (index > frames.Count - 1)
            {
                index %= frames.Count;
            }

            if (index != _index || _setGif != Gif)
            {
                _index = index;
                var frame = frames[_index];
                Apply(frame);
            }
        }

        private void Apply(GifFrame frame)
        {
            Image image = null;
            if (TryGetComponent<SpriteRenderer>(out var spriteRenderer) || TryGetComponent(out image))
            {
                _flip = Time.time + frame.DelayInMs * 0.001f;

                if (spriteRenderer != null) spriteRenderer.sprite = frame.Sprite;
                else if (image != null) image.sprite              = frame.Sprite;

                _setGif = Gif;
            }
        }
    }
}

