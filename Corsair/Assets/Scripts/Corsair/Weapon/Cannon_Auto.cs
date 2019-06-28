using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Cannon_Auto : Cannon
    {
        private Vector3 dir;
        protected override void Awake()
        {
            base.Awake();
            dir = transform.parent.InverseTransformDirection(transform.forward);
        }
        public void AutoLaunch(Vector3 pos)
        {
            StopAllCoroutines();
            StartCoroutine(AutoLaunchCor(pos));
        }
        private IEnumerator AutoLaunchCor(Vector3 p)
        {
            Vector3 d0 = transform.parent.TransformDirection(dir);
            Vector3 p0 = p - transform.position;
            Vector3 d1 = p0.normalized;
            float d = Vector3.Dot(d0, d1);
            if (d > 0.8f)
            {
                Vector3 a = d0 * p0.magnitude * d;
                float t = Time.time;
                float s = Random.Range(1.0f, 2.0f);
                while ((Time.time - t) < s)
                {
                    LookAt(Vector3.Lerp(a, p, (Time.time - t) / s));
#if UNITY_EDITOR
                    Debug.DrawLine(transform.position, p, Color.yellow, 0.02f);
#endif
                    yield return new WaitForEndOfFrame();
                }
                Launch((p - origin.position).normalized * Random.Range(70f, 120f));
#if UNITY_EDITOR
                Debug.DrawRay(origin.position, p - origin.position, Color.red, 1.0f);
#endif
            }
        }
    }
}