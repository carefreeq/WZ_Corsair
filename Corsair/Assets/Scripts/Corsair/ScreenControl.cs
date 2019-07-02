using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class ScreenControl : MonoBehaviour
    {
        public static ScreenControl Main { get; private set; }
        public Material mat;
        private void Awake()
        {
            Main = this;
            FromBlack(1.0f);
        }
        private void OnDestroy()
        {
            mat.color = new Color(0f, 0f, 0f, 0f);
        }
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, mat);
        }
        public void Hurt(float t)
        {
            StartCoroutine(HurtCor(new Color(1f, 0f, 0f, 0.5f), t));
        }
        public void FromBlack(float t)
        {
            StartCoroutine(FromCor(Color.black, t));
        }
        public void ToBlack(float t)
        {
            StartCoroutine(ToCor(Color.black, t));
        }
        private IEnumerator HurtCor(Color c, float t)
        {
                float _t = t;
                while ((_t -= Time.deltaTime) > 0)
                {
                    mat.color = new Color(c.r, c.g, c.b, _t / t * c.a);
                    yield return new WaitForEndOfFrame();
            }
            mat.color = new Color();
        }
        private IEnumerator FromCor(Color c, float t)
        {
            float _t = t;
            while ((_t -= Time.deltaTime) > 0)
            {
                mat.color = new Color(c.r, c.g, c.b, _t / t * c.a);
                yield return new WaitForEndOfFrame();
            }
            mat.color = new Color();
        }
        private IEnumerator ToCor(Color c, float t)
        {
            float _t = 0;
            while ((_t += Time.deltaTime) < t)
            {
                mat.color = new Color(c.r, c.g, c.b, _t / t * c.a);
                yield return new WaitForEndOfFrame();
            }
            mat.color = c;
        }
    }
}