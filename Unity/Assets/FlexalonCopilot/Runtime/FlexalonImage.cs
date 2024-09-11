#if UNITY_UI

using UnityEngine;
using UnityEngine.UI;
using Flexalon;

namespace FlexalonCopilot
{
    [ExecuteAlways, RequireComponent(typeof(Image))]
    public class FlexalonImage : FlexalonComponent, Adapter
    {
        private Image _image;
        private Vector2 _lastImageSize;
        private bool _lastPreserveAspect;

        protected override void UpdateProperties()
        {
            _image = GetComponent<Image>();
            _node.SetAdapter(this);
        }

        protected override void ResetProperties()
        {
            _node.SetAdapter(null);
        }

        public Bounds Measure(FlexalonNode node, Vector3 size, Vector3 min, Vector3 max)
        {
            if (_image.preserveAspect)
            {
                var spriteSize = _image.sprite != null ? _image.sprite.rect.size : Vector2.one;
                return Math.MeasureComponentBounds2D(new Bounds(Vector3.zero, spriteSize), node, size, min, max);
            }

            bool componentX = node.GetSizeType(Axis.X) == SizeType.Component;
            bool componentY = node.GetSizeType(Axis.Y) == SizeType.Component;
            bool componentZ = node.GetSizeType(Axis.Z) == SizeType.Component;

            var measureSize = new Vector3(
                componentX ? _image.rectTransform.rect.size.x : size.x,
                componentY ? _image.rectTransform.rect.size.y : size.y,
                componentZ ? 0 : size.z);

            measureSize = Math.Clamp(measureSize, min, max);

            var center = new Vector3((0.5f - _image.rectTransform.pivot.x) * measureSize.x, (0.5f - _image.rectTransform.pivot.y) * measureSize.y, 0);
            return new Bounds(center, measureSize);
        }

        public bool TryGetScale(FlexalonNode node, out Vector3 scale)
        {
            scale = Vector3.one;
            return true;
        }

        public bool TryGetRectSize(FlexalonNode node, out Vector2 rectSize)
        {
            rectSize = node.Result.AdapterBounds.size;
            return true;
        }

        public override void DoUpdate()
        {
            var spriteSize = Vector2.zero;
            if (_image.sprite)
            {
                spriteSize = _image.sprite.rect.size;
            }

            if (_lastImageSize != spriteSize || _lastPreserveAspect != _image.preserveAspect)
            {
                _lastImageSize = spriteSize;
                _lastPreserveAspect = _image.preserveAspect;
                MarkDirty();
            }
        }
    }
}

#endif