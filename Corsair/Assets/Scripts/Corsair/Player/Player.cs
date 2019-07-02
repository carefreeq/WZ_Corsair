using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;
namespace Corsair
{
    public class Player : Life
    {
        public enum PlayerStatus
        {
            Alive,
            Death,
        }
        public class PlayerBody : MonoBehaviour, IAttack
        {
            public Player Player { get; set; }
            public void Hurt(AttackInfo a)
            {
                Player.Hurt(a);
            }
        }
        public static Dictionary<int, Player> Players { get; private set; }
        public static event Action PlayerAllDeathEvent;
        static Player()
        {
            Players = new Dictionary<int, Player>();
        }

        public Transform Head
        {
            get
            {
                if (isMain)
                    return Player_Vive.Main.Camera.transform;
                return head;
            }
        }
        public string NetID { get { return name; } }
        public PlayerStatus status = PlayerStatus.Alive;
        public int playerId = 0;
        public bool isMain = false;
        public Transform head;
        public Transform left;
        public Transform right;
        protected override void Awake()
        {
            base.Awake();
            Players.Add(playerId, this);
            Manager.PlayerIndexEvent += PlayerIdManager;
        }
        private void Start()
        {
            Manager.NetDataManager.Add(NetID, NetDataManager);

            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    break;
                case Corsair.NetStatus.Client:
                    NetClient.Send(Manager.CreateNetData(NetID, (byte)NetStatus.GetStatus));
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
        }
        private void OnDestroy()
        {
            Players.Remove(playerId);
            Manager.PlayerIndexEvent -= PlayerIdManager;

            Manager.NetDataManager.Remove(NetID);
        }
        private void PlayerIdManager(int id)
        {
            if (playerId == id)
            {
                if (!isMain)
                {
                    isMain = true;

                    Player_Vive.Main.transform.SetParent(transform);
                    Player_Vive.Main.transform.localPosition = Vector3.zero;

                    PlayerBody[] p = Player_Vive.Main.GetComponentsInChildren<PlayerBody>();
                    if (p.Length == 0)
                    {
                        Collider[] c = Player_Vive.Main.GetComponentsInChildren<Collider>();
                        foreach (Collider _c in c)
                            _c.gameObject.AddComponent<PlayerBody>();
                        p = Player_Vive.Main.GetComponentsInChildren<PlayerBody>();
                    }
                    foreach (PlayerBody _p in p)
                        _p.Player = this;

                    head.gameObject.SetActive(false);
                    left.gameObject.SetActive(false);
                    right.gameObject.SetActive(false);
                }
            }
            else
            {
                isMain = false;
                head.gameObject.SetActive(true);
                left.gameObject.SetActive(true);
                right.gameObject.SetActive(true);
            }
        }
        private void FixedUpdate()
        {
            if (isMain)
                switch (Net.Status)
                {
                    case Corsair.NetStatus.Server:
                        NetServer.Send(GetAvatar(), NetType.UDP);
                        break;
                    case Corsair.NetStatus.Client:
                        NetClient.Send(GetAvatar(), NetType.UDP);
                        break;
                    case Corsair.NetStatus.Null:
                        break;
                }
        }
        private NetData GetAvatar()
        {
            NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.Avatar);
            n.Write(Player_Vive.Main.Camera.transform.position);
            n.Write(Player_Vive.Main.Camera.transform.rotation);

            n.Write(Player_Vive.Main.Left.transform.position);
            n.Write(Player_Vive.Main.Left.transform.rotation);

            n.Write(Player_Vive.Main.Right.transform.position);
            n.Write(Player_Vive.Main.Right.transform.rotation);
            return n;
        }
        private PointInfo point;
        public bool OnTrigger()
        {
            switch (status)
            {
                case PlayerStatus.Alive:
                    if (isMain)
                    {
                        if (Player_Vive.Main.OnTrigger())
                        {
                            point = Player_Vive.Main.GetPoint(Player_Vive.Main.OnTrigger(Valve.VR.SteamVR_Input_Sources.LeftHand) ? Player_Vive.Main.Left.transform : Player_Vive.Main.Right.transform);
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }
        public bool OnTriggerDown()
        {
            switch (status)
            {
                case PlayerStatus.Alive:
                    if (isMain)
                        return Player_Vive.Main.OnTriggerDown();
                    break;
            }
            return false;
        }
        public bool OnTriggerUp()
        {
            switch (status)
            {
                case PlayerStatus.Alive:
                    if (isMain)
                    {
                        if (Player_Vive.Main.OnTriggerUp())
                        {
                            point = Player_Vive.Main.GetPoint(Player_Vive.Main.OnTriggerUp(Valve.VR.SteamVR_Input_Sources.LeftHand) ? Player_Vive.Main.Left.transform : Player_Vive.Main.Right.transform);
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }
        public bool OnTouchPad()
        {
            switch (status)
            {
                case PlayerStatus.Alive:
                    if (isMain)
                    {
                        if (Player_Vive.Main.OnTouchPad())
                        {
                            point = Player_Vive.Main.GetPoint(Player_Vive.Main.OnTouchPad(Valve.VR.SteamVR_Input_Sources.LeftHand) ? Player_Vive.Main.Left.transform : Player_Vive.Main.Right.transform);
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }
        public bool OnTouchPadDown()
        {
            switch (status)
            {
                case PlayerStatus.Alive:
                    if (isMain)
                        return Player_Vive.Main.OnTouchPadDown();
                    break;
            }
            return false;
        }
        public bool OnTouchPadUp()
        {
            switch (status)
            {
                case PlayerStatus.Alive:
                    if (isMain)
                    {
                        if (Player_Vive.Main.OnTouchPadUp())
                        {
                            point = Player_Vive.Main.GetPoint(Player_Vive.Main.OnTouchPadUp(Valve.VR.SteamVR_Input_Sources.LeftHand) ? Player_Vive.Main.Left.transform : Player_Vive.Main.Right.transform);
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }
        public PointInfo GetPoint()
        {
            switch (status)
            {
                case PlayerStatus.Alive:
                    if (isMain)
                    {
                        return point;
                    }
                    break;
            }
            return new PointInfo();
        }

        public void Hurt(AttackInfo a, IPEndPoint ignore)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData ns = Manager.CreateNetData(NetID, (byte)NetStatus.Hurt);
                    ns.Write(a.Position);
                    ns.Write(a.Rotation);
                    ns.Write(a.Value);
                    NetServer.Send(ns, NetType.TCP, ignore);
                    break;
                case Corsair.NetStatus.Client:
                    NetData nc = Manager.CreateNetData(NetID, (byte)NetStatus.Hurt);
                    nc.Write(a.Position);
                    nc.Write(a.Rotation);
                    nc.Write(a.Value);
                    NetClient.Send(nc);
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            _Hurt(a);
        }
        public override void Hurt(AttackInfo a)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData ns = Manager.CreateNetData(NetID, (byte)NetStatus.Hurt);
                    ns.Write(a.Position);
                    ns.Write(a.Rotation);
                    ns.Write(a.Value);
                    NetServer.Send(ns);
                    break;
                case Corsair.NetStatus.Client:
                    NetData nc = Manager.CreateNetData(NetID, (byte)NetStatus.Hurt);
                    nc.Write(a.Position);
                    nc.Write(a.Rotation);
                    nc.Write(a.Value);
                    NetClient.Send(nc);
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            _Hurt(a);
        }
        private void _Hurt(AttackInfo a)
        {
            base.Hurt(a);
            if (isMain)
            {
                ScreenControl.Main.Hurt(1.0f);
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
            _Death();
        }
        public override void Death()
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData ns = Manager.CreateNetData(NetID, (byte)NetStatus.Death);
                    NetServer.Send(ns);
                    break;
                case Corsair.NetStatus.Client:
                    NetData nc = Manager.CreateNetData(NetID, (byte)NetStatus.Death);
                    NetClient.Send(nc);
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            _Death();
        }
        private void _Death()
        {
            base.Death();
            status = PlayerStatus.Death;
            if (!isMain)
            {
                head.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f, 0.5f);
                left.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f, 0.5f);
                right.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f, 0.5f);
            }
            foreach (PlayerInfo p in Manager.Players)
                if (Player.Players[p.ID].Heart > 0)
                    return;
            if (PlayerAllDeathEvent != null)
                PlayerAllDeathEvent();
        }

        public enum NetStatus : byte
        {
            GetStatus,
            SetStatus,
            Avatar,
            Hurt,
            Death,
        }

        public void NetDataManager(NetData data)
        {
            if (!isMain)
            {
                switch ((NetStatus)data.ReadByte())
                {
                    case NetStatus.GetStatus:
                        NetData ngs = Manager.CreateNetData(NetID, (byte)NetStatus.SetStatus);
                        ngs.Write(Heart);
                        ngs.Write((byte)status);
                        NetServer.SendTo(ngs, data.RemoteIP);
                        break;
                    case NetStatus.SetStatus:
                        heart = data.ReadInt();
                        status = (PlayerStatus)data.ReadByte();
                        break;
                    case NetStatus.Avatar:
                        head.transform.position = data.ReadVector3();
                        head.transform.rotation = data.ReadQuaternion();

                        left.transform.position = data.ReadVector3();
                        left.transform.rotation = data.ReadQuaternion();

                        right.transform.position = data.ReadVector3();
                        right.transform.rotation = data.ReadQuaternion();
                        break;
                    case NetStatus.Death:
                        Death(data.RemoteIP);
                        break;
                    case NetStatus.Hurt:
                        AttackInfo a = new AttackInfo();
                        a.Position = data.ReadVector3();
                        a.Rotation = data.ReadQuaternion();
                        a.Value = data.ReadInt();
                        Hurt(a, data.RemoteIP);
                        break;
                }
            }
        }


        [ContextMenu("Test")]
        public void Test()
        {
            Debug.Log("PlayerCount:" + Manager.Players.Count);
            Debug.Log("Player 1 Status:" + Player.Players[0].status);
            Debug.Log("Player 2 Status:" + Player.Players[1].status);
        }
    }
}