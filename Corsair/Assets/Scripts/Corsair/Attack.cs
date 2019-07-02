using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public interface IAttack
    {
        void Hurt(AttackInfo a);
    }
    [Serializable]
    public struct AttackInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public int Value;
    }
    public abstract class Attack : MonoBehaviour
    {
        public AttackInfo Info { get { return info; } }
        [SerializeField]
        private AttackInfo info;
        protected Rigidbody rig;
        protected virtual void Awake()
        {
            rig = GetComponent<Rigidbody>();
        }
        protected void OnCollisionEnter(Collision collision)
        {
            OnCollision(collision.gameObject);
        }
        public void OnCollision(GameObject g)
        {
            info.Position = transform.position;
            info.Rotation = transform.rotation;
            OnEnter(g);
        }
        protected virtual void OnEnter(GameObject g)
        {
            IAttack[] a = g.GetComponents<IAttack>();
            if (a != null && a.Length > 0)
            {
                if (g.tag != gameObject.tag)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        a[i].Hurt(info);
                    }
                }
                Destroy(gameObject);
            }
        }
        public void Launch(Vector3 impulse)
        {
            rig.AddForce(impulse, ForceMode.Impulse);
        }
        public void LaunchTo(Vector3 pos)
        {
            StartCoroutine(LaunchToCor(pos));
        }
        private IEnumerator LaunchToCor(Vector3 pos)
        {
            Vector3[] p = Tools.GetPoints(transform, pos);
            float l = Tools.GetBezierLength(p);
            float o = 0.0f;
            do
            {
                transform.position = Tools.CatmullBezier(p, o / l);
                yield return new WaitForEndOfFrame();
            } while ((o += 40f * (1 + o / 60f) * Time.deltaTime) < l);

            rig.AddForce(Tools.CatmullBezier(p, 1.0f) - transform.position, ForceMode.Force);
            yield return new WaitForEndOfFrame();
        }
    }
}