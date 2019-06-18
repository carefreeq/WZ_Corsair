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
    public enum AttackFaction : byte
    {
        Player,
        Enemy
    }
    public enum AttackType : byte
    {
        Arrow,
        Cannonball,
    }
    [Serializable]
    public struct AttackInfo
    {
        public AttackFaction Faction;
        public AttackType Type;
        public Vector3 Position;
        public int Value;
    }
    public class Attack : MonoBehaviour
    {
        [SerializeField]
        private AttackInfo attack;
        [SerializeField]
        private float rate = 0.7f;
        private Rigidbody rig;
        private void Awake()
        {
            rig = GetComponent<Rigidbody>();
            StartCoroutine(DelayDestroy(5f));
        }
        protected void OnCollisionEnter(Collision collision)
        {
            IAttack a = collision.gameObject.GetComponent<IAttack>();
            if (a != null)
            {
                attack.Position = transform.position;
                a.Hit(attack);
            }
            Destroy(gameObject);
        }
        public void Launch(Vector3 impulse)
        {
            rig.AddForce(impulse, ForceMode.Impulse);
        }
        private IEnumerator DelayDestroy(float t)
        {
            yield return new WaitForSeconds(t);
            Destroy(gameObject);
        }
        public void LaunchTo(Vector3 toPos)
        {
            float d = Vector3.Distance(toPos,transform.position);
            Launch(transform.forward*d*rate);
            //StopAllCoroutines();
            //StartCoroutine(LaunchToCor(toPos, toDir));
        }
        private IEnumerator LaunchToCor(Vector3 toPos, Vector3 toDir)
        {
            Vector3 dir = transform.forward;
            Vector3 axis = new Vector3(dir.x, .0f, dir.z).normalized;

            Vector3 gravity = new Vector3(0.0f, -9.8f, 0.0f);

            float rate = Vector3.Dot(dir, axis);
            float dis = Vector3.Dot(toPos - transform.position, axis);
            dir = dir * dis / rate;
            Launch(dir);
            //float t = Time.time;
            //while (Time.time - t < 5f)
            //{
            //    float _t = Time.time - t;
            //    transform.Translate((dir + _t * gravity) * Time.deltaTime, Space.World);
                yield return new WaitForEndOfFrame();
            //}


            //Vector3 fromPos = transform.position;
            //Quaternion fromDir = transform.rotation;
            //float t = Vector3.Distance(fromPos, toPos) / 40f;
            //Vector3 dir = transform.forward * 100;
            //float _t = Time.time;
            //while (Time.time - _t < 15)
            //{
            //    float i = (Time.time - _t) / t;
            //    //Move(to, dir, (Time.time - _t) / t);
            //    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((toPos - transform.position).normalized), i);
            //    transform.Translate(Vector3.forward * Time.deltaTime * 40f, Space.Self);
            //    //transform.position = Vector3.SlerpUnclamped(fromPos, toPos, (Time.time - _t) / t);
            //    yield return new WaitForEndOfFrame();
            //}
            //Destroy(gameObject);
        }
    }
}