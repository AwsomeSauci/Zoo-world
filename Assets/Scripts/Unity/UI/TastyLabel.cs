using TMPro;
using UnityEngine;

namespace ZooWorld.Unity.UI
{
    public sealed class TastyLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private RectTransform rect;

        public void Render(Vector2 position, float remaining)
        {
            rect.anchoredPosition = position;
            text.alpha = Mathf.Clamp01(remaining * 3);
        }
    }
}
