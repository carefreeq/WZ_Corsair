using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Corsair
{
    public static class Tools
    {
        public static Vector3 MoveLerp(float i)
        {
            return Vector3.zero;
        }
        public static Vector3 MoveForward(float d)
        {
            return Vector3.zero;
        }
        public static Vector3 GetPosition(this Transform t, Vector3 center, Vector3 size)
        {
            size = size * 0.5f;
            return t.TransformPoint(new Vector3(UnityEngine.Random.Range(-size.x, size.x) / t.localScale.x, UnityEngine.Random.Range(-size.y, size.y) / t.localScale.y, UnityEngine.Random.Range(-size.z, size.z) / t.localScale.z) + new Vector3(center.x / t.localScale.x, center.y / t.localScale.y, center.z / t.localScale.z));
        }
        public static void DrawCubeGizmos(Transform t, Vector3 center, Vector3 size)
        {
            size = size * 0.5f;
            Vector3 c = new Vector3(center.x / t.localScale.x, center.y / t.localScale.y, center.z / t.localScale.z);
            Vector3[] p = new Vector3[8];
            p[0] = t.TransformPoint(new Vector3(-size.x / t.localScale.x, -size.y / t.localScale.y, -size.z / t.localScale.z) + c);
            p[1] = t.TransformPoint(new Vector3(size.x / t.localScale.x, -size.y / t.localScale.y, -size.z / t.localScale.z) + c);
            p[2] = t.TransformPoint(new Vector3(size.x / t.localScale.x, size.y / t.localScale.y, -size.z / t.localScale.z) + c);
            p[3] = t.TransformPoint(new Vector3(-size.x / t.localScale.x, size.y / t.localScale.y, -size.z / t.localScale.z) + c);

            p[4] = t.TransformPoint(new Vector3(-size.x / t.localScale.x, -size.y / t.localScale.y, size.z / t.localScale.z) + c);
            p[5] = t.TransformPoint(new Vector3(size.x / t.localScale.x, -size.y / t.localScale.y, size.z / t.localScale.z) + c);
            p[6] = t.TransformPoint(new Vector3(size.x / t.localScale.x, size.y / t.localScale.y, size.z / t.localScale.z) + c);
            p[7] = t.TransformPoint(new Vector3(-size.x / t.localScale.x, size.y / t.localScale.y, size.z / t.localScale.z) + c);

            Gizmos.color = Color.yellow;
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(p[i], p[(i + 1) % 4]);
                Gizmos.DrawLine(p[i + 4], p[(i + 1) % 4 + 4]);
                Gizmos.DrawLine(p[i], p[i + 4]);
            }
        }
        public static float GetBezierLength(Vector3[] p)
        {
            float l = 0.0f;
            Vector3 t0 = p[0];
            Vector3 t1;
            for (int i = 1; i < 10; i++)
            {
                t1 = Tools.CatmullBezier(p, i / 10f);
                l += Vector3.Distance(t0, t1);
                t0 = t1;
            }
            return l;
        }
        public static Vector3[] GetPoints(Transform origin, Vector3 to)
        {
            return GetPoints(origin.position, origin.forward, to);
        }
        public static Vector3[] GetPoints(Vector3 form, Vector3 dir, Vector3 to)
        {
            return new Vector3[4] { form, form + Vector3.Dot(to - form, dir) * dir, to, to };
        }
        public static Vector3 CatmullBezier(Vector3[] p, float i)
        {
            if (p.Length == 4)
            {
                return CatmullBezier(p[0], p[1], p[2], p[3], i);
            }
            return Vector3.zero;
        }
        public static Vector3 CatmullBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
        {
            return p0 * (1f - i) * (1f - i) * (1f - i) + 3f * p1 * i * (1 - i) * (1 - i) + 3f * p2 * i * i * (1f - i) + p3 * i * i * i;
        }
        public static bool IsRandom(this GameObject[] g)
        {
            return g.Length > 0;
        }
        public static GameObject GetRandom(this GameObject[] g)
        {
            return g[UnityEngine.Random.Range(0, g.Length)];
        }
    }
    public interface IGetPosition
    {
        Vector3 GetPosition();
    }
    public enum AxisSingle : int
    {
        X, Y, Z
    }
    public abstract class Life : MonoBehaviour, IAttack
    {
        public int Heart { get { return heart; } }
        [SerializeField]
        protected int heart = 3;
        private int heartMax = 3;
        public event Action<AttackInfo> HurtStatusEvent, DeathStatusEvent;
        public UnityEngine.UI.Scrollbar.ScrollEvent HeartPercentEvent;
        public GameObjects[] hurtGameObjects = new GameObjects[0];
        public UnityEngine.Events.UnityEvent HurtEvent;
        public GameObjects[] deathGameObjects = new GameObjects[0];
        public UnityEngine.Events.UnityEvent DeathEvent;
        protected virtual void Awake()
        {
            heartMax = heart;
            HurtStatusEvent += (a) => Create(a, hurtGameObjects);
            DeathStatusEvent += (a) => Create(a, deathGameObjects);
        }
        public virtual void Hurt(AttackInfo a)
        {
            heart -= a.Value;
            if (heart > 0)
            {
                HurtEvent.Invoke();
                if (HurtStatusEvent != null)
                    HurtStatusEvent(a);
            }
            else
                Death();
            HeartPercentEvent.Invoke(heart / (float)heartMax);
        }
        public virtual void Death()
        {
            DeathEvent.Invoke();
        }

        private void Create(AttackInfo a, GameObjects[] go)
        {
            if (go != null)
                for (int i = 0; i < go.Length; i++)
                {
                    if (go[i].gameObjects.IsRandom())
                        Instantiate(go[i].gameObjects.GetRandom(), a.Position, a.Rotation);
                }
        }
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
    [Serializable]
    public class GameObjects
    {
        public GameObject[] gameObjects = new GameObject[0];
    }
    public struct PointInfo
    {
        public Vector3[] path;
        public Vector3 point;
        public Vector3 nomral;
        public Color color;
        public RaycastHit hit;
        public void Get(Vector3 fromPos, Vector3 fromDir, Vector3 toPos, Vector3 toDir, Color col)
        {
            path = Tools.GetPoints(fromPos, fromDir, toPos);
            point = toPos;
            nomral = toDir;
            color = col;
        }
        public bool GetCast(Vector3 fromPos, Vector3 fromDir, Vector3 toPos, int layer = ~0)
        {
            Vector3 d = new Vector3(fromDir.x, -0.5f, fromDir.z).normalized;
            Vector3[] p = Tools.GetPoints(fromPos, fromDir, toPos);

            List<Vector3> _p = new List<Vector3>();

            float l = Tools.GetBezierLength(p);
            _p.Add(p[0]);
            float o = 0.0f;
            RaycastHit h;
            while ((o += 2f) < l)
            {
                _p.Add(Tools.CatmullBezier(p, o / l));
                Vector3 p0 = _p[_p.Count - 2];
                Vector3 p1 = _p[_p.Count - 1];
                if (Physics.Raycast(p0, (p1 - p0).normalized, out h, (p1 - p0).magnitude, layer))
                {
                    _p[_p.Count - 1] = h.point;
                    nomral = h.normal;
                    point = _p[_p.Count - 1];
                    path = _p.ToArray();
                    color = new Color(0f, 1f, 0f, 0.5f);
                    return true;
                }
            }
            point = _p[_p.Count - 1];
            path = _p.ToArray();
            nomral = Vector3.up;
            color = new Color(1f, 0f, 0f, 0.5f);
            return false;
        }
    }
}