using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Corsair : Enemy, IAttack
    {
        public static List<Corsair> Corsairs { get; private set; }
        static Corsair()
        {
            Corsairs = new List<Corsair>();
        }
        protected Rigidbody rig;
        [SerializeField]
        private float speed = 10f;
        private bool isReadly = false;
        private float time = 0.0f;
        [SerializeField]
        private Cannon_auto[] cannons;
        private int index;
        private float launchTime;
        protected void Awake()
        {
            Corsairs.Add(this);
            rig = gameObject.GetComponent<Rigidbody>();
            time = Time.time;
        }

        private void Update()
        {
            transform.position = new Vector3(transform.position.x, Mathf.Cos(Time.time - time), transform.position.z);
            if (Rampart.Ramparts.Count > 0)
            {
                if (!isReadly)
                {
                    Rampart r = Rampart.Ramparts[0];
                    float d = Vector3.Distance(r.transform.position, transform.position);
                    if (d > 100f)
                    {
                        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
                        transform.LookAt(new Vector3(r.transform.position.x, transform.position.y, r.transform.position.z), Vector3.up);
#if UNITY_EDITOR
                        Debug.DrawLine(transform.position, r.transform.position, Color.green, 0.02f);
#endif
                    }
                    else
                    {
                        StartCoroutine(ReadlyCor());
                        isReadly = true;
                    }
                }
                else
                {
                    if (Time.time - launchTime > 5f)
                    {
                        cannons[index].Launch(Rampart.Ramparts[0].GetPosition());
                        index = (index + 1) % cannons.Length;
                        launchTime = Time.time;
                    }
                }
            }
            else
            {
                transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Corsairs.Remove(this);
        }
        private void OnCollisionEnter(Collision collision)
        {
            rig.AddForce(collision.impulse, ForceMode.Impulse);
        }
        private IEnumerator ReadlyCor()
        {
            Vector3 d = transform.position.x < 0 ? Vector3.left : Vector3.right;
            Vector3 f = transform.forward;
            float m = 5f;
            float t = Time.time;
            while (Time.time - t < m)
            {
#if UNITY_EDITOR
                Debug.DrawLine(transform.position, Rampart.Ramparts[0].transform.position, Color.red, 0.02f);
#endif
                transform.Translate(transform.forward * speed * (1.0f - Mathf.Clamp01((Time.time - t) / m)) * Time.deltaTime, Space.World);
                transform.LookAt(transform.position + Vector3.Lerp(f, d, (Time.time - t) / m / 2f));
                yield return new WaitForEndOfFrame();
            }
        }

        public override void Hit(AttackInfo a)
        {
            if (a.Faction == AttackFaction.Player)
            {
                base.Hit(a);
            }
        }
    }
}