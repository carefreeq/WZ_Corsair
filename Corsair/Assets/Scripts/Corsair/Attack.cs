using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public interface IAttack
    {
        void Hit(AttackInfo a);
    }
    [Serializable]
    public struct AttackInfo
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Quaternion Rotation;
        public int Value;
    }
    public abstract class Attack : MonoBehaviour
    {
        public AttackInfo Info { get { return info; } }
        [SerializeField]
        private AttackInfo info;
        [SerializeField]
        private float rate = 0.7f;
        protected Rigidbody rig;
        protected virtual void Awake()
        {
            rig = GetComponent<Rigidbody>();
        }
        protected void OnCollisionEnter(Collision collision)
        {
            IAttack a = collision.gameObject.GetComponent<IAttack>();
            if (a != null)
            {
                info.Position = transform.position;
                info.Direction = transform.forward;
                info.Rotation = transform.rotation;

                OnAttack(a);

                if (collision.gameObject.tag != gameObject.tag)
                {
                    a.Hit(info);
                }
                Destroy(gameObject);
            }
        }
        protected abstract void OnAttack(IAttack a);

        public void Launch(Vector3 impulse)
        {
            rig.AddForce(impulse, ForceMode.Impulse);
        }
        public void LaunchTo(Vector3 toPos)
        {
            float d = Vector3.Distance(toPos, transform.position);
            Launch(transform.forward * d * rate);
        }
    }
}