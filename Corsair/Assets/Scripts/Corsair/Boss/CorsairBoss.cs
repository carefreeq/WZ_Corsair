using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;

namespace Corsair
{
    public class CorsairBoss : Life
    {
        public enum RoleStatus : int
        {
            Idle = 0,
            Moving,
            Attacking,
            Hurt,
            Death,
            Vectory
        }
        [Serializable]
        public class AttackInfo
        {
            public Attack_Arrow attack;
            public ParticleSystem prelude;
            public ParticleSystem launch;
        }
        public string NetID { get { return name; } }
        private Animator animator;
        public RoleStatus status;
        public Transform leftHand;
        public Transform rightHand;
        public Transform point;
        public AttackInfo attacks;

        public Transform target;
        private int index = 0;
        public bool isLookAt;
        private void Awake()
        {
            animator = gameObject.GetComponent<Animator>();
            status = RoleStatus.Idle;
        }
        private void Start()
        {
            SetTarget(UnityEngine.Random.Range(0, Player.Players.Count));

            Manager.NetDataManager.Add(NetID, NetDataManager);
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    StartCoroutine(AICor());
                    break;
                case Corsair.NetStatus.Client:
                    NetClient.Send(Manager.CreateNetData(NetID, (byte)NetStatus.GetStatus));
                    break;
                case Corsair.NetStatus.Null:
                    StartCoroutine(AICor());
                    break;
            }
        }
        public void OnAnimatorIK(int layerIndex)
        {
            if (isLookAt)
            {
                animator.SetLookAtWeight(0.4f);
                animator.SetLookAtPosition(target.position);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.1f);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, target.position);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.1f);
                animator.SetIKPosition(AvatarIKGoal.RightHand, target.position);
            }
        }
        private void OnDestroy()
        {
            Manager.NetDataManager.Remove(NetID);
        }



        public int NextTarget()
        {
            for (int i = 0; i < Player.Players.Count; i++)
            {
                index = (index + 1) % Manager.Players.Count;
                Player p = Player.Players[Manager.Players[index].ID];
                if (p.status == Player.PlayerStatus.Alive)
                    return index;
            }
            return index;
        }
        public void SetTarget(int i, IPEndPoint ignore = null)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData ns = Manager.CreateNetData(NetID, (byte)NetStatus.SetTarget);
                    ns.Write(i);
                    NetServer.Send(ns, NetType.TCP, ignore);
                    break;
                case Corsair.NetStatus.Client:
                    NetData nc = Manager.CreateNetData(NetID, (byte)NetStatus.SetTarget);
                    nc.Write(i);
                    NetClient.Send(nc);
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            index = i;
            target = Player.Players[i].Head;
        }

        public void SetRoleStatus(int i)
        {
            status = (RoleStatus)i;
        }

        public void PlayIdle(IPEndPoint ignore = null)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetServer.Send(Manager.CreateNetData(NetID, (byte)NetStatus.Idle), NetType.TCP, ignore);
                    break;
                case Corsair.NetStatus.Client:
                    NetClient.Send(Manager.CreateNetData(NetID, (byte)NetStatus.Idle));
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            animator.Play("Idle");
            attacks.prelude.gameObject.SetActive(false);
        }

        private string[] attackClip = new string[] { "Attack0", "Attack1", "Attack2" };
        private float[] attackTime = new float[] { 1.5f, 3.0f, 3.0f };
        private float[] attackCD = new float[] { 1f, 10f, 5f };
        private float[] attackLast = new float[] { 0.0f, 0.0f, 0.0f };

        public bool PlayAttack(int i, IPEndPoint ignore = null)
        {
            if (Time.time - attackLast[i] < attackCD[i])
                return false;
            attackLast[i] = Time.time;
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData ns = Manager.CreateNetData(NetID, (byte)NetStatus.Attacking);
                    ns.Write(i);
                    NetServer.Send(ns, NetType.TCP, ignore);
                    break;
                case Corsair.NetStatus.Client:
                    NetData nc = Manager.CreateNetData(NetID, (byte)NetStatus.Attacking);
                    nc.Write(i);
                    NetClient.Send(nc);
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            switch (i)
            {
                case 0:
                    isLookAt = true;
                    animator.Play(attackClip[0]);
                    transform.LookAt(target, Vector3.up);
                    break;
                case 1:
                    isLookAt = false;
                    animator.Play(attackClip[1]);
                    break;
                case 2:
                    isLookAt = true;
                    animator.Play(attackClip[2]);
                    transform.LookAt(target, Vector3.up);
                    break;
            }
            return true;
        }
        public void AttackEvent(int i)
        {
            switch (i)
            {
                case 0:
                    Attack_Arrow a0 = GameObject.Instantiate(attacks.attack, point.position, point.rotation);
                    a0.Launch((target.GetPosition(Vector3.zero, Vector3.one) - point.position).normalized * UnityEngine.Random.Range(3f, 7f));
                    break;
                case 1:
                    StartCoroutine(Attack2Cor(target));
                    attacks.prelude.gameObject.SetActive(false);
                    attacks.launch.gameObject.SetActive(true);
                    attacks.launch.Play();
                    break;
                case 2:
                    Attack_Arrow a2 = GameObject.Instantiate(attacks.attack, point.position, point.rotation);
                    a2.GetComponent<Rigidbody>().useGravity = false;
                    a2.Launch((target.position - a2.transform.position).normalized * 7f);
                    attacks.prelude.gameObject.SetActive(false);
                    attacks.launch.gameObject.SetActive(true);
                    attacks.launch.Play();
                    break;
            }
        }
        private IEnumerator Attack2Cor(Transform t)
        {
            for (int i = 0; i < 5f; i++)
            {
                int r = UnityEngine.Random.Range(8, 16);
                for (int _i = 0; _i < r; _i++)
                {
                    Attack_Arrow a = GameObject.Instantiate(attacks.attack, transform.position + new Vector3(UnityEngine.Random.Range(-3f, 3f), 10f, UnityEngine.Random.Range(-3f, 3f)), Quaternion.identity);
                    a.transform.LookAt(t.GetPosition(Vector3.zero, Vector3.one * 8f));
                    a.Launch(a.transform.forward * UnityEngine.Random.Range(4f, 7f));
                }
                yield return new WaitForSeconds(0.3f);
            }
        }

        public void PlayHurt(Corsair.AttackInfo a, IPEndPoint ignore)
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
            base.Hurt(a);
            //animator.Play("Hurt");
            //attacks.prelude.gameObject.SetActive(false);
        }

        public override void Hurt(Corsair.AttackInfo a)
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
            base.Hurt(a);
            //animator.Play("Hurt");
            //attacks.prelude.gameObject.SetActive(false);
        }


        public void PlayDeath(IPEndPoint ignore = null)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetServer.Send(Manager.CreateNetData(NetID, (byte)NetStatus.Death), NetType.TCP, ignore);
                    break;
                case Corsair.NetStatus.Client:
                    NetClient.Send(Manager.CreateNetData(NetID, (byte)NetStatus.Death));
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            animator.Play("Death");
            attacks.prelude.gameObject.SetActive(false);
            StopAllCoroutines();
            base.Death();
        }
        public override void Death()
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetServer.Send(Manager.CreateNetData(NetID, (byte)NetStatus.Death));
                    break;
                case Corsair.NetStatus.Client:
                    NetClient.Send(Manager.CreateNetData(NetID, (byte)NetStatus.Death));
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            animator.Play("Death");
            attacks.prelude.gameObject.SetActive(false);
            StopAllCoroutines();
            base.Death();
        }

        public void PlayVictory(IPEndPoint ignore = null)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetServer.Send(Manager.CreateNetData(NetID, (byte)NetStatus.Victory), NetType.TCP, ignore);
                    break;
                case Corsair.NetStatus.Client:
                    NetClient.Send(Manager.CreateNetData(NetID, (byte)NetStatus.Victory));
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            animator.Play("Victory");
            attacks.prelude.gameObject.SetActive(false);
            StopAllCoroutines();
            gameObject.GetComponent<Collider>().enabled = false;
        }

        private IEnumerator AICor()
        {
            while (status != RoleStatus.Death)
            {
                SetTarget(NextTarget());
                float s = UnityEngine.Random.Range(8f, 12f);
                float t = Time.time;
                while (Time.time - t < s)
                {
                    float w = 0.0f;
                    while (true)
                    {
                        int i = UnityEngine.Random.Range(0, 3);
                        if (PlayAttack(i))
                        {
                            w = attackTime[i];
                            break;
                        }
                        yield return new WaitForEndOfFrame();
                    }
                    w += UnityEngine.Random.Range(1f, 3f);
                    yield return new WaitForSeconds(w);
                }

            }
        }

        public enum NetStatus : byte
        {
            GetStatus,
            SetStatus,
            SetTarget,
            Idle,
            Attacking,
            Moving,
            Hurt,
            Death,
            Victory,
        }
        public void NetDataManager(NetData data)
        {
            switch ((NetStatus)data.ReadByte())
            {
                case NetStatus.GetStatus:
                    NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.SetStatus);
                    n.Write(Heart);
                    n.Write((byte)status);
                    n.Write(index);
                    NetServer.Send(n);
                    break;
                case NetStatus.SetStatus:
                    heart = data.ReadInt();
                    status = (RoleStatus)data.ReadByte();
                    SetTarget(data.ReadInt(), data.RemoteIP);
                    switch (status)
                    {
                        case RoleStatus.Death:
                            PlayDeath(data.RemoteIP);
                            break;
                        case RoleStatus.Vectory:
                            PlayVictory(data.RemoteIP);
                            break;
                    }
                    break;
                case NetStatus.SetTarget:
                    SetTarget(data.ReadInt(), data.RemoteIP);
                    break;
                case NetStatus.Idle:
                    PlayIdle(data.RemoteIP);
                    break;
                case NetStatus.Attacking:
                    PlayAttack(data.ReadInt(),data.RemoteIP);
                    break;
                case NetStatus.Moving:
                    break;
                case NetStatus.Hurt:
                    Corsair.AttackInfo a = new Corsair.AttackInfo();
                    a.Position = data.ReadVector3();
                    a.Rotation = data.ReadQuaternion();
                    a.Value = data.ReadInt();
                    base.Hurt(a);
                    break;
                case NetStatus.Death:
                    PlayDeath(data.RemoteIP);
                    break;
                case NetStatus.Victory:
                    PlayVictory(data.RemoteIP);
                    break;
            }
        }
    }
}