using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Rampart : Life, IGetPosition
    {
        public string NetID { get { return name; } }
        public static List<Rampart> Ramparts { get; private set; }

        static Rampart()
        {
            Ramparts = new List<Rampart>();
        }
        public UnityEngine.UI.Scrollbar.ScrollEvent LifeEvent;
        private int max;
        [SerializeField]
        private Vector3 center;
        [SerializeField]
        private Vector3 size;
        protected override void Awake()
        {
            base.Awake();
            max = Heart;
            Ramparts.Add(this);
        }
        private void Start()
        {
            Manager.NetDataManager.Add(NetID, NetManager);
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
            Manager.NetDataManager.Remove(NetID);
        }
        public Vector3 GetPosition()
        {
            return transform.GetPosition(center, size);
        }
        public void OnDrawGizmosSelected()
        {
            Tools.DrawCubeGizmos(transform, center, size);
        }
        public override void Hurt(AttackInfo a)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.Hurt);
                    n.Write(a.Position);
                    n.Write(a.Rotation);
                    n.Write(a.Value);
                    NetServer.Send(n);

                    base.Hurt(a);
                    LifeEvent.Invoke(heart / (float)max);
                    break;
                case Corsair.NetStatus.Client:
                    break;
                case Corsair.NetStatus.Null:
                    base.Hurt(a);
                    LifeEvent.Invoke(heart / (float)max);
                    break;
            }
        }
        public override void Death()
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.Death);
                    NetServer.Send(n);

                    base.Death();
                    Ramparts.Remove(this);
                    break;
                case Corsair.NetStatus.Client:
                    break;
                case Corsair.NetStatus.Null:
                    base.Death();
                    Ramparts.Remove(this);
                    break;
            }
        }

        public enum NetStatus
        {
            GetStatus,
            SetStatus,
            Hurt,
            Death,
        }
        public void NetManager(NetData data)
        {
            switch ((NetStatus)data.ReadByte())
            {
                case NetStatus.GetStatus:
                    NetData ngs = Manager.CreateNetData(NetID, (byte)NetStatus.SetStatus);
                    ngs.Write(heart);
                    NetServer.Send(ngs);
                    break;
                case NetStatus.SetStatus:
                    heart = data.ReadInt();
                    if (heart > 0)
                    {
                        LifeEvent.Invoke(heart / (float)max);
                    }
                    else
                    {
                        base.Death();
                        Ramparts.Remove(this);
                    }
                    break;
                case NetStatus.Hurt:
                    AttackInfo a = new AttackInfo();
                    a.Position = data.ReadVector3();
                    a.Rotation = data.ReadQuaternion();
                    a.Value = data.ReadInt();
                    base.Hurt(a);
                    LifeEvent.Invoke(heart / (float)max);
                    break;
                case NetStatus.Death:
                    base.Death();
                    Ramparts.Remove(this);
                    break;
            }
        }
    }
}