using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
namespace Corsair
{
    public class Cannon : MonoBehaviour
    {
        [SerializeField]
        protected Part[] parts;
        [SerializeField]
        protected Transform origin;
        [SerializeField]
        protected Attack bullet;
        protected float launchTime;
        [SerializeField]
        protected float fillTime;
        public UnityEvent LaunchEvent;
        public Scrollbar.ScrollEvent FillEvent;

        public LineRenderer hint;
        [ContextMenu("Awake")]
        protected virtual void Awake()
        {
            foreach (Part p in parts)
                p.Init();
        }
        public void ShowHint(Vector3 pos)
        {
            ShowHint(pos, Quaternion.identity);
        }
        public void ShowHint(Vector3 pos, Quaternion rota)
        {
            ShowHint(pos, Quaternion.identity, Color.red);
        }
        public void ShowHint(Vector3 pos, Quaternion rota, Color col)
        {
            Vector3[] p = Tools.GetPoints(origin, pos);
            List<Vector3> _p = new List<Vector3>();

            float l = Tools.GetBezierLength(p);
            _p.Add(p[0]);
            float o = 0.0f;
            while ((o += 2f) < l)
            {
                _p.Add(Tools.CatmullBezier(p, o / l));
            }
            _p.Add(Tools.CatmullBezier(p, o / l));
            if (!hint.gameObject.activeSelf)
                hint.gameObject.SetActive(true);
            hint.startColor = hint.endColor= col;
            hint.GetComponent<MeshRenderer>().material.color = col;
            hint.positionCount = _p.Count;
            hint.SetPositions(_p.ToArray());
            hint.transform.position = pos;
            hint.transform.rotation = rota;
            hint.transform.localScale = Vector3.Distance(hint.transform.position, Player_Vive.Main.Camera.transform.position) * Vector3.one * 0.1f;
        }
        public void CloseHint()
        {
            hint.positionCount = 0;
            hint.gameObject.SetActive(false);
        }

        public void LookAt(Vector3 pos)
        {
            foreach (Part p in parts)
                p.AimAt(pos);
        }
        public void Launch(Vector3 impulse)
        {
            Launch((a) => { a.Launch(impulse); });
        }
        public void LaunchTo(Vector3 pos)
        {
            Launch((a) => { LookAt(pos); a.LaunchTo(pos); });
        }
        private void Launch(Action<Attack> func)
        {
            if (Time.time - launchTime > fillTime)
            {
                launchTime = Time.time;
                func(Instantiate(bullet, origin.position, origin.rotation));
                LaunchEvent.Invoke();
                StartCoroutine(FillCor());
            }
        }
        private IEnumerator FillCor()
        {
            while (Time.time - launchTime < fillTime)
            {
                FillEvent.Invoke((Time.time - launchTime) / fillTime);
                yield return new WaitForEndOfFrame();
            }
            FillEvent.Invoke(Mathf.Clamp01((Time.time - launchTime) / fillTime));
        }
        public void CreatePoint(GameObject t)
        {
            GameObject.Instantiate(t, origin.position, origin.rotation);
        }
        [Serializable]
        public class Part
        {
            public Transform target;
            public float min, max;
            public AxisSingle axis;
            public PartLink[] links;
            private Matrix4x4 mat;
            private float euler;
            public void Init()
            {
                mat = target.worldToLocalMatrix;
                euler = target.localEulerAngles[(int)axis];
                foreach (PartLink l in links)
                    l.Init();
            }
            public void AimAt(Vector3 p)
            {
                Vector3 d1 = mat.MultiplyPoint(p).normalized;
                switch (axis)
                {
                    case AxisSingle.X:
                        float x = Mathf.Clamp(CalculateAngle(Vector3.forward, new Vector3(0.0f, d1.y, d1.z).normalized, AxisSingle.X), min, max);
                        target.localEulerAngles = new Vector3(euler + x, target.localEulerAngles.y, target.localEulerAngles.z);
                        foreach (PartLink l in links)
                            l.Link(x);
                        break;
                    case AxisSingle.Y:
                        float y = Mathf.Clamp(CalculateAngle(Vector3.forward, new Vector3(d1.x, 0.0f, d1.z).normalized, AxisSingle.Y), min, max);
                        target.localEulerAngles = new Vector3(target.localEulerAngles.x, euler + y, target.localEulerAngles.z);
                        foreach (PartLink l in links)
                            l.Link(y);
                        break;
                    case AxisSingle.Z:
                        float z = Mathf.Clamp(CalculateAngle(Vector3.forward, new Vector3(d1.x, d1.y, 0.0f).normalized, AxisSingle.Z), min, max);
                        target.localEulerAngles = new Vector3(target.localEulerAngles.x, target.localEulerAngles.y, euler + z);
                        foreach (PartLink l in links)
                            l.Link(z);
                        break;
                }
            }
            private float CalculateAngle(Vector3 d0, Vector3 d1, AxisSingle a)
            {
                Vector3 u = Vector3.Cross(d0, d1);
                float r = Mathf.Acos(Vector3.Dot(d0, d1));
                float v = (u[(int)a] > 0 ? r : -r) * Mathf.Rad2Deg;
                //Debug.Log("Axis:" + a + "  d0:" + d0 + "  d1:" + d1 + " v:" + v);
                return v;
            }
            public void AimAt(Vector3 p, float max)
            {
            }
        }
        [Serializable]
        public class PartLink
        {
            public Transform target;
            public AxisSingle axis;
            public float rate;
            private Vector3 eular;
            private float last;
            public void Init()
            {
                eular = target.localEulerAngles;
            }
            public void Link(float v)
            {
                float _v = v - last;
                switch (axis)
                {
                    case AxisSingle.X:
                        target.Rotate(new Vector3(_v * rate, 0.0f, 0.0f), Space.Self);
                        break;
                    case AxisSingle.Y:
                        target.Rotate(new Vector3(0.0f, _v * rate, 0.0f), Space.Self);
                        break;
                    case AxisSingle.Z:
                        target.Rotate(new Vector3(0.0f, 0.0f, _v * rate), Space.Self);
                        break;
                }
                last = v;
            }
        }
    }
}