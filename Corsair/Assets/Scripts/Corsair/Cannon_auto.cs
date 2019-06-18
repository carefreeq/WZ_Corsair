using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Cannon_auto : Cannon
    {
        [SerializeField]
        private float rate = 15.0f;
        public void Launch(Vector3 p)
        {
            StopAllCoroutines();
            StartCoroutine(LaunchCor(p));
        }
        private IEnumerator LaunchCor(Vector3 p)
        {
            Vector3 d = p - transform.position;
            Quaternion ft = transform.rotation;
            Quaternion dt = Quaternion.LookRotation(new Vector3(d.x, 0.0f, d.z));
            float t = Time.time;
            while ((Time.time - t) < 2.0f)
            {
                transform.rotation = Quaternion.Lerp(ft, dt, (Time.time - t) / 2.0f);
#if UNITY_EDITOR
                Debug.DrawLine(transform.position, p, Color.red, 0.02f);
#endif
                yield return new WaitForEndOfFrame();
            }
            Quaternion fb = barrel.transform.rotation;
            Quaternion db = Quaternion.LookRotation(new Vector3(0.0f, d.y, 0.0f));
            while ((Time.time - t) < 2.0f)
            {
                barrel.transform.rotation = Quaternion.Lerp(fb, db, (Time.time - t) / 2.0f);
#if UNITY_EDITOR
                Debug.DrawLine(transform.position, p, Color.red, 0.02f);
#endif
                yield return new WaitForEndOfFrame();
            }
            Attack b = GameObject.Instantiate(bullet,point.position,point.rotation);
            b.Launch(d.normalized * 50);
        }
    }
}