using UnityEngine;

namespace ZooWorld.Unity.Physics
{
    public sealed class AnimalView : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        private float elevation;
        private Quaternion rotation = Quaternion.identity;

        public void SetMotion(Vector3 velocity, float visualElevation)
        {
            elevation = visualElevation;
            if (velocity.sqrMagnitude > 0.001f) rotation = Quaternion.LookRotation(velocity);
        }

        public void ResetPose()
        {
            elevation = 0;
            rotation = Quaternion.identity;
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = rotation;
        }

        private void LateUpdate()
        {
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, Vector3.up * elevation, 1 - Mathf.Exp(-30 * Time.deltaTime));
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, rotation, 1 - Mathf.Exp(-18 * Time.deltaTime));
        }
    }
}
