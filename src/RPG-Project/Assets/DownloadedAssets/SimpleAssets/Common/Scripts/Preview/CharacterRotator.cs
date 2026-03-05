using UnityEngine;

namespace SimpleAssets.Common.Preview
{
    public class CharacterRotator: MonoBehaviour
    {
        [SerializeField] private float RotateSpeed = 5f;

        private CharacterLoader Loader { get; set; }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            Loader = GetComponentInParent<CharacterLoader>();
        }

        private void Update()
        {
            Rotate();
        }

        private void Rotate()
        {
            if (Loader.CharacterTranform == null)
                return;
        }
    }
}