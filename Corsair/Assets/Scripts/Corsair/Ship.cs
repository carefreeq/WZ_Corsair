using Hydroform;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
namespace Corsair
{
    public enum ShipStatus : byte
    {
        Idle,
        Move,
        Attack,
        Death,
    }
    public class Ship : Life, IGetPosition
    {
        public string NetID { get { return name; } }

        public Vector3 center;
        public Vector3 size;
        [SerializeField]
        protected float speed = 10f;
        [SerializeField]
        private Cannon_Auto[] cannons;
        public ShipStatus Status { get; set; }

        public GameObject[] death;
        private float yt = 0.0f;
        private float lt = 0.0f;
        private Collider col;
        public IGetPosition target;
        protected override void Awake()
        {
            base.Awake();
            col = GetComponent<Collider>();
            cannons = GetComponentsInChildren<Cannon_Auto>();
            yt = Random.Range(0f, 3.1514926f);
            Status = ShipStatus.Move;
        }
        protected virtual void Start()
        {
            Manager.NetDataManager.Add(NetID, NetDataManager);
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    StartCoroutine(UpdateCor());
                    break;
                case Corsair.NetStatus.Client:
                    NetClient.Send(Manager.CreateNetData(NetID, (byte)NetStatus.GetStatus));
                    break;
                case Corsair.NetStatus.Null:
                    StartCoroutine(UpdateCor());
                    break;
            }
        }
        protected virtual void OnDestroy()
        {
            Manager.NetDataManager.Remove(NetID);
        }
        protected virtual void Update()
        {
            switch (Manager.GameStatus)
            {
                case GameStatus.Playing:
                    switch (Status)
                    {
                        case ShipStatus.Attack:
                            if (target != null)
                                if (Time.time - lt > 5f)
                                {
                                    foreach (Cannon_Auto c in cannons)
                                        c.AutoLaunch(target.GetPosition());
                                    lt = Time.time;
                                }
                            break;
                    }
                    break;
            }
        }
        protected void FixedUpdate()
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.Avatar);
                    n.Write(transform.position);
                    n.Write(transform.rotation);
                    NetServer.Send(n, NetType.UDP);
                    break;
                case Corsair.NetStatus.Client:
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
        }
        public void Death(IPEndPoint ignore)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData ns = Manager.CreateNetData(NetID, (byte)NetStatus.Death);
                    NetServer.Send(ns, NetType.TCP, ignore);
                    break;
                case Corsair.NetStatus.Client:
                    NetData nc = Manager.CreateNetData(NetID, (byte)NetStatus.Death);
                    NetClient.Send(nc);
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            Death();
        }
        public override void Death()
        {
            StopAllCoroutines();
            StartCoroutine(DeathCor());
            Status = ShipStatus.Death;
            base.Death();
        }
        private IEnumerator UpdateCor()
        {
            HydroformComponent w = null;
            HydroformComponent[] compList = FindObjectsOfType(typeof(HydroformComponent)) as HydroformComponent[];
            if (compList.Length > 0 && compList[0] != null)
            {
                w = compList[0];
            }

            float yp = transform.position.y;
            float xe = transform.eulerAngles.x;
            while (true)
            {
                if (w != null)
                {
                    float h0 = w.GetHeightAtPoint(transform.position);
                    transform.position = new Vector3(transform.position.x, h0, transform.position.z);
                    //float h1 = w.GetHeightAtPoint(transform.position+transform.forward);
                    //transform.eulerAngles = new Vector3( Quaternion.LookRotation( h1-h0, transform.eulerAngles.y, transform.eulerAngles.z);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, yp + Mathf.Cos(Time.time - yt), transform.position.z);
                }
                transform.eulerAngles = new Vector3(xe + Mathf.Cos(Time.time - yt) * 4f, transform.eulerAngles.y, transform.eulerAngles.z);
                switch (Status)
                {
                    case ShipStatus.Move:
                        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
                        break;
                    default:
                        break;
                }
                yield return new WaitForEndOfFrame();
            }
        }
        private IEnumerator DeathCor()
        {
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<Collider>().enabled = false;

            float t = Time.time;
            float s = t;
            while (Time.time - t < 12f)
            {
                if (transform.eulerAngles.x > -60f)
                    transform.eulerAngles += new Vector3(-10f * Time.deltaTime, 0.0f, 0.0f);
                transform.position += new Vector3(0.0f, -3.2f * Time.deltaTime, 0.0f);
                if (death.Length > 0)
                {
                    if (Time.time - s > 0)
                    {
                        GameObject.Instantiate(death[Random.Range(0, death.Length)], transform.GetPosition(center, size), Quaternion.identity);
                        s = Time.time + Random.Range(0f, 1f);
                    }
                }
                yield return new WaitForEndOfFrame();
            }
            Destroy(gameObject);
        }

        public enum NetStatus
        {
            GetStatus,
            SetStatus,
            ShipStatus,
            Avatar,
            Death,
        }
        public void NetDataManager(NetData data)
        {
            switch ((NetStatus)data.ReadByte())
            {
                case NetStatus.GetStatus:
                    NetData ngs = Manager.CreateNetData(NetID, (byte)NetStatus.SetStatus);
                    ngs.Write(heart);
                    ngs.Write((byte)Status);
                    NetServer.SendTo(ngs, data.RemoteIP);
                    break;
                case NetStatus.SetStatus:
                    heart = data.ReadInt();
                    Status = (ShipStatus)data.ReadInt();
                    break;
                case NetStatus.ShipStatus:
                    Status = (ShipStatus)data.ReadByte();
                    break;
                case NetStatus.Avatar:
                    transform.position = data.ReadVector3();
                    transform.rotation = data.ReadQuaternion();
                    break;
                case NetStatus.Death:
                    Death(data.RemoteIP);
                    break;
            }
        }

        public Vector3 GetPosition()
        {
            return transform.GetPosition(center, size);
        }
        private void OnDrawGizmosSelected()
        {
            Tools.DrawCubeGizmos(transform, center, size);
        }
    }
}