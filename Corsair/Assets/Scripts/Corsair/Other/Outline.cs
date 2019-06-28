using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public abstract class Outline : MonoBehaviour
    {
        public Material mat;

        [ContextMenu("Show")]
        public void Show()
        {
            Show(0.5f);
        }
        public void Show(float t)
        {
            StartCoroutine(ShowCor(t));
        }
        private IEnumerator ShowCor(float t)
        {
            float _t = Time.time;
            MeshRenderer mr = GetRender();
            while (Time.time - _t < t)
            {
                Color c = mr.material.color;
                mr.material.color = new Color(c.r, c.g, c.b, (1.0f - (Time.time - _t) / t));
                yield return new WaitForEndOfFrame();
            }
            Destroy(mr.gameObject);
        }
        protected abstract MeshRenderer GetRender();
    }
}