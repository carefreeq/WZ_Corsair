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
        public void SetColor(float i)
        {
            if (!t)
                t = GetComponent<TextMesh>();
            t.color = Color.Lerp(from, to, i);
        }
    }
}