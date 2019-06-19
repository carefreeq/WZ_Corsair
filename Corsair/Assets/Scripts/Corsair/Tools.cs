using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Corsair
{
    public class Tools : MonoBehaviour
    {
        public List<Vector3> poss = new List<Vector3>();
        public static Vector3 MoveLerp(float i)
        {
            return Vector3.zero;
        }
        public static Vector3 MoveForward(float d)
        {
            return Vector3.zero;
        }
        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
        {
            return 0.5f * ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i + (-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
        }

    }
    public abstract class Life : MonoBehaviour, IAttack
    {
        [SerializeField]
        protected int heart = 3;
        [SerializeField]
        private Vector3 range;
        [SerializeField]
        private Vector3 center;
        public virtual void Hit(AttackInfo a)
        {
            heart -= a.Value;
            if (heart <= 0)
                Death();
        }
        public Vector3 GetPosition()
        {
            return transform.TransformPoint(new Vector3(UnityEngine.Random.Range(-range.x, range.x) / transform.localScale.x, UnityEngine.Random.Range(-range.y, range.y) / transform.localScale.y, UnityEngine.Random.Range(-range.z, range.z) / transform.localScale.z) * 0.5f + new Vector3(center.x / transform.localScale.x, center.y / transform.localScale.y, center.z / transform.localScale.z));
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 0, 0.5f);
            Gizmos.DrawCube(center + transform.position, range);
        }
        protected abstract void Death();
    }

    public class Resource<T> where T : UnityEngine.Object
    {
        private string path;
        private T target;
        public T Target
        {
            get
            {
                if (target == null)
                    target = Resources.Load<T>(path);
                return target;
            }
        }
        public Resource(string p)
        {
            path = p;
        }
        public static implicit operator T(Resource<T> r)
        {
            return r.target;
        }
    }
}