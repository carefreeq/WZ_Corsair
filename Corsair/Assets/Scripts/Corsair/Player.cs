using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Valve.VR;

namespace Corsair
{
    public class Player : Life
    {
        public static Player Main { get; protected set; }
        public static List<Player> Players { get; private set; }
        public readonly static Resource<Player> Prefab = new Resource<Player>("Player");
        public static Dictionary<string, Player> OtherPlayer = new Dictionary<string, Player>();
        public static Transform Parent { get; private set; }
        public static SteamVR_Action_Boolean TeleprotAction { get; private set; }

        static Player()
        {
            TeleprotAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");
            Players = new List<Player>();
        }
        public static void AddPlayer(NetData n)
        {
            string k = n.ReadString();
            if (!OtherPlayer.ContainsKey(k))
            {
                Player p = GameObject.Instantiate(Prefab.Target);
                OtherPlayer.Add(k, p);
                p.name = n.ReadString();
            }
        }
        public static void RemovePlayer(NetData n)
        {
            string k = n.ReadString();
            if (OtherPlayer.ContainsKey(k))
            {
                GameObject.Destroy(OtherPlayer[k].gameObject);
                OtherPlayer.Remove(k);
            }
        }
        public static void ReceivePlayerStatus(NetData n)
        {
            while (n.IsRead())
            {
                string k = n.ReadString();
                if (k != Main.GUID)
                    if (OtherPlayer.ContainsKey(k))
                    {
                        Player p = OtherPlayer[k];
                        p.head.position = n.ReadVector3();
                        p.head.rotation = n.ReadQuaternion();
                        p.left.position = n.ReadVector3();
                        p.left.rotation = n.ReadQuaternion();
                        p.right.position = n.ReadVector3();
                        p.right.rotation = n.ReadQuaternion();
                    }
            }
        }
        public string GUID { get; private set; }
        [SerializeField]
        private Transform head, left, right;
        [SerializeField]
        private Transform target;
        public Cannon_manual cannon;
        private bool isActive = false;
        protected void Awake()
        {
            Players.Add(this);

            if (!Parent)
                Parent = new GameObject("Players").transform;
            if (!Main)
                Main = this;
            transform.SetParent(Parent.transform);
            GUID = System.Guid.NewGuid().ToString().Substring(0, 8);

            Body[] bs = gameObject.GetComponentsInChildren<Body>();
            foreach (Body b in bs)
                b.Player = this;
        }
        private void Update()
        {
            if (TeleprotAction.GetState(SteamVR_Input_Sources.Any))
            {
                if (target.gameObject.activeSelf)
                {
                    cannon.AimAt(target.position);
                }
            }
            if (TeleprotAction.GetStateUp(SteamVR_Input_Sources.Any))
            {
                cannon.Launch(target.position);
            }
        }
        private void FixedUpdate()
        {
            if (Net.Status != NetStatus.Null)
            {
                NetData n = new NetData(NetMessageType.Data);
                n.Write((byte)MessageType.SendPlayerStatus);
                switch (Net.Status)
                {
                    case NetStatus.Server:
                        foreach (Player p in OtherPlayer.Values)
                            p.SendStatus(ref n);
                        break;
                    case NetStatus.Client:
                        SendStatus(ref n);
                        break;
                    default:
                        break;
                }
                NetClient.Send(n);
            }
        }
        private void SendStatus(ref NetData n)
        {
            n.Write(GUID);
            n.Write(head.position);
            n.Write(head.rotation);
            n.Write(left.position);
            n.Write(left.rotation);
            n.Write(right.position);
            n.Write(right.rotation);
        }
        protected void OnDestroy()
        {
            switch (Net.Status)
            {
                case NetStatus.Server:
                    break;
                case NetStatus.Client:
                    NetData n = new NetData(NetMessageType.Data);
                    n.Write((byte)MessageType.RemovePlayer);
                    n.Write(GUID);
                    break;
                default:
                    break;
            }
        }

    }
}