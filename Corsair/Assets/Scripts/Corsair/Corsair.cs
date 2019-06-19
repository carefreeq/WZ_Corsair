using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Corsair : Ship, IAttack
    {
        public static List<Corsair> Corsairs { get; private set; }
        static Corsair()
        {
            Corsairs = new List<Corsair>();
        }
        protected Rigidbody rig;
        [SerializeField]
        private CorsairCannons cannons;
        private float launchTime = 0.0f;
        protected override void Awake()
        {
            base.Awake();
            Corsairs.Add(this);
            rig = gameObject.GetComponent<Rigidbody>();
        }
        private void Start()
        {
            StartCoroutine(MoveCor());
        }
        protected override void Death()
        {
            Corsairs.Remove(this);
            base.Death();
        }
        private IEnumerator MoveCor()
        {
            Transform ta = Rampart.Ramparts[0].transform;

            while (Vector3.Distance(ta.position, transform.position) > 150f)
            {
                transform.LookAt(new Vector3(ta.position.x, transform.position.y, ta.position.z), Vector3.up);
#if UNITY_EDITOR
                Debug.DrawLine(transform.position, ta.position, Color.green, 0.02f);
#endif
                yield return new WaitForEndOfFrame();
            }
            Status = ShipStatus.Attack;

            Vector3 d = new Vector3(transform.position.x < 0 ? -1f : 1f, 0.0f, -Random.Range(0f, 1.0f)).normalized;
            Vector3 f = transform.forward;
            float m = Random.Range(5f, 10f);
            float t = Time.time;
            while (Time.time - t < m)
            {
#if UNITY_EDITOR
                Debug.DrawLine(transform.position, ta.position, Color.red, 0.02f);
#endif
                transform.Translate(transform.forward * speed * (1.0f - Mathf.Clamp01((Time.time - t) / m)) * Time.deltaTime, Space.World);
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, d, 0.6f * Time.deltaTime, 0.3f * Time.deltaTime));
                //transform.LookAt(transform.position + Vector3.Lerp(f, d, (Time.time - t) / m));
                yield return new WaitForEndOfFrame();
            }
            while (true)
            {
                if (Rampart.Ramparts.Count > 0)
                {
                    switch (Status)
                    {
                        case ShipStatus.Attack:
                            if (Time.time - launchTime > 5f)
                            {
                                cannons.Launch(Rampart.Ramparts[0]);
                                launchTime = Time.time;
                            }
                            break;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }
}