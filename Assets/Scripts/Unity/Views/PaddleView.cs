using System;
using Arknoid.Core.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Arknoid.Unity.Views
{
    /// <summary>Vista de la pala: traduce el Slider de la UI a un evento y coloca la pala donde le digan.</summary>
    public class PaddleView : MonoBehaviour, IPaddleView
    {
        private const float CenteredSliderValue = 0.5f;

        [SerializeField] private Slider movementSlider;

        private Vector3 startPosition;

        public event Action<float> SliderMoved;

        private void Awake()
        {
            startPosition = transform.position;
        }

        private void OnEnable()
        {
            if (movementSlider != null) movementSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnDisable()
        {
            if (movementSlider != null) movementSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        public void SetPositionX(float x)
        {
            var position = transform.position;
            transform.position = new Vector3(x, position.y, position.z);
        }

        public void ResetToStart()
        {
            transform.position = startPosition;
            if (movementSlider != null) movementSlider.value = CenteredSliderValue;
        }

        private void OnSliderValueChanged(float value) => SliderMoved?.Invoke(value);
    }
}
