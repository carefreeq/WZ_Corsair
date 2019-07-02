using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
namespace Corsair
{
    public class Cannons : MonoBehaviour
    {
        public Cannon Main { get; private set; }
        public string NetID { get { return name; } }

        public Player player;
        [SerializeField]
        private int index = 0;
        public bool hideOther = true;
        private Cannon[] cannons;
        private PointInfo lastPoint;
        private void Awake()
        {
            cannons = gameObject.GetComponentsInChildren<Cannon>();
            Select(0);
        }
        private void Start()
        {
            Manager.NetDataManager.Add(NetID, NetDataManager);
        }
        private void OnDestroy()
        {
            Manager.NetDataManager.Remove(NetID);
        }

        public void Update()
        {
            if (player.OnTouchPadUp())
            {
                Select((index + 1) % cannons.Length);
            }
            if (player.OnTrigger())
            {
                PointInfo p = player.GetPoint();
                LookAt(p.point);
                //Cannon c = cannons[index];
                //Vector3 d = (p.point - c.origin.position);
                //if (Vector3.Dot(d.normalized, c.origin.forward) < 0.2f)
                //{
                //    Vector3 p0 = Vector3.Dot(d, Vector3.up) * Vector3.up;
                //    Vector3 p1 = p0 + c.origin.position;
                //    Vector3 p2 = p1 - p.point;
                //    Vector3 p3 = p2 + p2.normalized * Mathf.Asin(0.2f);

                //    p.GetCast(c.origin.position, c.origin.forward, p1);
                //    p.color = new Color(1.0f, 0f, 0f, 0.5f);
                //}
                lastPoint = p;
                Player_Vive.Main.ShowHint(lastPoint);
            }
            if (player.OnTriggerUp())
            {
                Player_Vive.Main.CloseHint();
                LaunchTo(lastPoint.point);
            }
#if UNITY_EDITOR
            if (point)
            {
                LookAt(point.position);
            }
#endif
        }

        public void Select(Cannon can)
        {
            Main = can;
        }
        public void Select(int i, IPEndPoint ignore = null)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData ns = Manager.CreateNetData(NetID, (byte)NetStatus.Select);
                    ns.Write(i);
                    NetServer.Send(ns, NetType.TCP, ignore);

                    _Select(i);
                    break;
                case Corsair.NetStatus.Client:
                    NetData nc = Manager.CreateNetData(NetID, (byte)NetStatus.Select);
                    nc.Write(i);
                    NetClient.Send(nc);

                    _Select(i);
                    break;
                case Corsair.NetStatus.Null:
                    _Select(i);
                    break;
            }
            index = i;
        }
        private void _Select(int i)
        {

            for (int _i = 0; _i < cannons.Length; _i++)
            {
                if (_i == i)
                {
                    Main = cannons[i];
                    Main.gameObject.SetActive(true);
                }
                else
                {
                    if (hideOther)
                        cannons[_i].gameObject.SetActive(false);
                }
            }
        }
        public void ShowHint(Vector3 point, Quaternion rota, Color col)
        {
            Main.ShowHint(point, rota, col);
        }
        public void CloseHint()
        {
            Main.CloseHint();
        }
        public void LookAt(Vector3 point, IPEndPoint ignore = null)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData ns = Manager.CreateNetData(NetID, (byte)NetStatus.LookAt);
                    ns.Write(point);
                    NetServer.Send(ns, NetType.TCP, ignore);

                    Main.LookAt(point);
                    break;
                case Corsair.NetStatus.Client:
                    NetData nc = Manager.CreateNetData(NetID, (byte)NetStatus.LookAt);
                    nc.Write(point);
                    NetClient.Send(nc);

                    Main.LookAt(point);
                    break;
                case Corsair.NetStatus.Null:
                    Main.LookAt(point);
                    break;
            }
        }
        public void LaunchTo(Vector3 point, IPEndPoint ignore = null)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData ns = Manager.CreateNetData(NetID, (byte)NetStatus.LaunchTo);
                    ns.Write(point);
                    NetServer.Send(ns, NetType.TCP, ignore);

                    Main.LaunchTo(point);
                    break;
                case Corsair.NetStatus.Client:
                    NetData nc = Manager.CreateNetData(NetID, (byte)NetStatus.LaunchTo);
                    nc.Write(point);
                    NetClient.Send(nc);

                    Main.LaunchTo(point);
                    break;
                case Corsair.NetStatus.Null:
                    Main.LaunchTo(point);
                    break;
            }
        }
        public enum NetStatus : byte
        {
            Select,
            LaunchTo,
            LookAt,
        }
        public void NetDataManager(NetData data)
        {
            switch ((NetStatus)data.ReadByte())
            {
                case NetStatus.Select:
                    Select(data.ReadInt(), data.RemoteIP);
                    break;
                case NetStatus.LookAt:
                    LookAt(data.ReadVector3(), data.RemoteIP);
                    break;
                case NetStatus.LaunchTo:
                    LaunchTo(data.ReadVector3(), data.RemoteIP);
                    break;
            }
        }
#if UNITY_EDITOR
        public Transform point;
        [ContextMenu("LaunchTo")]
        public void Launch()
        {
            if (point)
                LaunchTo(point.position);
        }
        [ContextMenu("Select")]
        public void Select()
        {
            Select(index);
        }
#endif
    }
}