using System;
using UnityEngine;

namespace UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ChildrenAspectRatioScaleChanger : MonoBehaviour
    {
        [SerializeField] private float aspectRatioA = 16f / 9f;
        [SerializeField] private float aspectRatioB = 21f / 9f;
        
        [SerializeField] private Vector2 scaleA = new Vector2(1, 1);
        [SerializeField] private Vector2 scaleB = new Vector2(0.75f, 0.75f);
        
        private RectTransform[] _rectTransforms;

        private void Start()
        {
            var rectTransform = GetComponent<RectTransform>();
            _rectTransforms = new RectTransform[rectTransform.childCount];
            for (var i = 0; i < rectTransform.childCount; i++)
            {
                _rectTransforms[i] = rectTransform.GetChild(i).GetComponent<RectTransform>();
            }
        }

        private void Update()
        {
            var aspectRatio = (float) Screen.width / Screen.height;
            var t = Mathf.InverseLerp(aspectRatioA, aspectRatioB, aspectRatio);
            
            foreach (var rectTransform in _rectTransforms)
            {
                rectTransform.localScale = Vector2.Lerp(scaleA, scaleB, t);
            }
        }
    }
}