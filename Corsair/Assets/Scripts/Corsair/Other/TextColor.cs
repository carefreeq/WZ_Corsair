using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    [RequireComponent(typeof(TextMesh))]
    public class TextColor : MonoBehaviour
    {
        public Color from;
        public Color to;
        private TextMesh t;
        private void Awake()
        {
            t = GetComponent<TextMesh>();
        }
        public void SetColor(float i)
        {
            t.color = Color.Lerp(from, to, i);
        }
    }
}