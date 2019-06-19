using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Cannon_Auto : Cannon
    {
        [SerializeField]
        private float rate = 15.0f;
        private Vector3 dir;
        private void Awake()
        {
            dir = transform.parent.InverseTransformDirection(transform.forward);
        }
        public void Launch(Vector3 p)
        {
            StopAllCoroutines();
            StartCoroutine(LaunchCor(p));
        }
        [ContextMenu("Launch")]
        public void Launch()
        {
            Attack b = GameObject.Instantiate(bullet, point.position, point.rotation);
            b.Launch(point.transform.forward * 100);
        }
        private IEnumerator LaunchCor(Vector3 p)
        {
            Vector3 _dir = transform.parent.TransformDirection(dir);
            if (Vector3.Dot(_dir, (p - transform.position).normalized) > 0.8f)
            {
                Vector3 d = p - transform.position;
                Quaternion ft = transform.rotation;
                Quaternion dt = Quaternion.LookRotation(new Vector3(d.x, 0.0f, d.z));
                float t = Time.time;
                float st = Random.Range(1.0f, 2.0f);
                while ((Time.time - t) < st)
                {
                    transform.rotation = Quaternion.Lerp(ft, dt, (Time.time - t) / 2.0f);
#if UNITY_EDITOR
                    Debug.DrawLine(transform.position, p, Color.yellow, 0.02f);
#endif
                    yield return new WaitForEndOfFrame();
                }
                Quaternion fb = barrel.transform.rotation;
                Quaternion db = Quaternion.LookRotation(new Vector3(0.0f, d.y, 0.0f));
                //                while ((Time.time - t) < st)
                //                {
                //                    barrel.transform.rotation = Quaternion.Lerp(fb, db, (Time.time - t) / 2.0f);
                //#if UNITY_EDITOR
                //                    Debug.DrawLine(transform.position, p, Color.yellow, 0.02f);
                //#endif
                //                    yield return new WaitForEndOfFrame();
                //                }
                Attack b = GameObject.Instantiate(bullet, point.position, point.rotation);
                b.Launch(d.normalized * Random.Range(70f, 120f));
#if UNITY_EDITOR
                Debug.DrawRay(transform.position, d.normalized * 5, Color.red, 1.0f);
#endif
            }
        }
    }
}