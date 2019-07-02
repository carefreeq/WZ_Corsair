using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Ship_Corsair : Ship
    {
        public static List<Ship_Corsair> Corsairs { get; private set; }
        static Ship_Corsair()
        {
            Corsairs = new List<Ship_Corsair>();
        }

        protected override void Awake()
        {
            base.Awake();
            Corsairs.Add(this);
            Manager.GameObjects.Add(gameObject);

            if (Rampart.Ramparts.Count > 0)
                target = Rampart.Ramparts[0];
        }
        protected override void Start()
        {
            base.Start();
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    StartCoroutine(MoveCor());
                    break;
                case Corsair.NetStatus.Client:
                    break;
                case Corsair.NetStatus.Null:
                    StartCoroutine(MoveCor());
                    break;
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Corsairs.Contains(this))
                Corsairs.Remove(this);
            Manager.GameObjects.Remove(gameObject);
        }
        public override void Death()
        {
            base.Death();
            Corsairs.Remove(this);
        }
        private IEnumerator MoveCor()
        {
            if (Rampart.Ramparts.Count > 0)
            {
                Transform ta = Rampart.Ramparts[0].transform;
                float dis = Random.Range(250f, 400f);
                while (Vector3.Distance(ta.position, transform.position) > dis)
                {
                    transform.LookAt(new Vector3(ta.position.x, transform.position.y, ta.position.z), Vector3.up);
#if UNITY_EDITOR
                    Debug.DrawLine(transform.position, ta.position, Color.green, 0.02f);
#endif
                    yield return new WaitForEndOfFrame();
                }
                SetStatus(ShipStatus.Attack);

                Vector3 d = new Vector3(-Random.Range(0f, 1.0f), 0.0f, transform.position.z < 0f ? -1f : 1f).normalized;
                Vector3 f = transform.forward;
                float m = Random.Range(2f, 6f);
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
            }
        }
        public void SetStatus(ShipStatus s)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.ShipStatus);
                    n.Write((byte)s);
                    NetServer.Send(n);

                    Status = s;
                    break;
                case Corsair.NetStatus.Client:
                    break;
                case Corsair.NetStatus.Null:
                    Status = s;
                    break;
            }
        }
    }
}