using Flexalon;
using TMPro;
using UnityEngine;

namespace FlexalonCopilot
{
    [ExecuteAlways]
    internal class EditModeLerpAnimator : MonoBehaviour, TransformUpdater
    {
        private FlexalonNode _node;

        private AnimatedPropertyOrField _localPosition;
        private AnimatedPropertyOrField _localRotation;
        private AnimatedPropertyOrField _localScale;
        private AnimatedPropertyOrField _rectSizeX;
        private AnimatedPropertyOrField _rectSizeY;

        void OnEnable()
        {
            _node = Flexalon.Flexalon.GetOrCreateNode(gameObject);
            _node.SetTransformUpdater(this);
            hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;

            _localPosition = new AnimatedPropertyOrField(typeof(Transform), "localPosition");
            _localRotation = new AnimatedPropertyOrField(typeof(Transform), "localRotation");
            _localScale = new AnimatedPropertyOrField(typeof(Transform), "localScale");
            _rectSizeX = new AnimatedPropertyOrField(typeof(RectTransform), "sizeDelta.x");
            _rectSizeY = new AnimatedPropertyOrField(typeof(RectTransform), "sizeDelta.y");
        }

        void OnDisable()
        {
            _node?.SetTransformUpdater(null);
            _node = null;
        }

        public void PreUpdate(FlexalonNode node)
        {
        }

        private void RecordEdit(FlexalonNode node)
        {
#if UNITY_EDITOR
            if (Flexalon.Flexalon.RecordFrameChanges)
            {
                UnityEditor.Undo.RecordObject(node.GameObject.transform, "Flexalon transform change");
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(node.GameObject.transform);
            }
#endif
        }

        public bool UpdatePosition(FlexalonNode node, Vector3 position)
        {
            _localPosition.SetValue(node.GameObject.transform, position);
            return _localPosition.Done;
        }

        public bool UpdateRotation(FlexalonNode node, Quaternion rotation)
        {
            _localRotation.SetValue(node.GameObject.transform, rotation);
            return _localRotation.Done;
        }

        public bool UpdateScale(FlexalonNode node, Vector3 scale)
        {
            _localScale.SetValue(node.GameObject.transform, scale);
            return _localScale.Done;
        }

        public bool UpdateRectSize(FlexalonNode node, Vector2 size)
        {
            // There's basically two things to consider:
            //  - If we're in a layout, Flexalon is going to try to set the anchor/offset to 0.5
            //    remove any animations that interfere with that, then animate the size.
            //  - If we're not in a layout, force any ongoing anchor/offset animations to finish
            //    so that we can assign the size, if it's not set by the anchors.
            //  Also skip animation for text because the text itself is animated.

            var rectTransform = node.GameObject.transform as RectTransform;

            if (node.SkipLayout || rectTransform.gameObject.GetComponent<TMP_Text>() != null)
            {
                AnimationUpdater.Instance.ForceFinish(rectTransform, "offsetMin.x");
                AnimationUpdater.Instance.ForceFinish(rectTransform, "offsetMax.x");
                AnimationUpdater.Instance.ForceFinish(rectTransform, "anchorMin.x");
                AnimationUpdater.Instance.ForceFinish(rectTransform, "anchorMax.x");

                AnimationUpdater.Instance.ForceFinish(rectTransform, "offsetMin.y");
                AnimationUpdater.Instance.ForceFinish(rectTransform, "offsetMax.y");
                AnimationUpdater.Instance.ForceFinish(rectTransform, "anchorMin.y");
                AnimationUpdater.Instance.ForceFinish(rectTransform, "anchorMax.y");

                if (rectTransform.anchorMin.x == rectTransform.anchorMax.x)
                {
                    RecordEdit(node);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                }

                if (rectTransform.anchorMin.y == rectTransform.anchorMax.y)
                {
                    RecordEdit(node);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                }

                return true;
            }
            else
            {
                AnimationUpdater.Instance.Remove(rectTransform, "offsetMin.x");
                AnimationUpdater.Instance.Remove(rectTransform, "offsetMax.x");
                AnimationUpdater.Instance.Remove(rectTransform, "anchorMin.x");
                AnimationUpdater.Instance.Remove(rectTransform, "anchorMax.x");

                AnimationUpdater.Instance.Remove(rectTransform, "offsetMin.y");
                AnimationUpdater.Instance.Remove(rectTransform, "offsetMax.y");
                AnimationUpdater.Instance.Remove(rectTransform, "anchorMin.y");
                AnimationUpdater.Instance.Remove(rectTransform, "anchorMax.y");

                _rectSizeX.SetValue(rectTransform, size.x);
                _rectSizeY.SetValue(rectTransform, size.y);
                return _rectSizeX.Done && _rectSizeY.Done;
            }
        }
    }
}