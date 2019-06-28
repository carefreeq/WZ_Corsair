using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Corsair
{
    public class Emission : MonoBehaviour
    {
        public string NetID { get { return name; } }

        public int total = 20;
        public int max = 5;
        public float spand = 3f;
        public float range = 10f;
        public int prefabId;
        private int index = 0;

        public Scrollbar.ScrollEvent RemainEvent;
        public UnityEngine.Events.UnityEvent EndEvent;

        private void Start()
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    StartCoroutine(BeginCor());
                    break;
                case Corsair.NetStatus.Client:
                    break;
                case Corsair.NetStatus.Null:
                    StartCoroutine(BeginCor());
                    break;
            }
            Manager.NetDataManager.Add(NetID, NetDataManager);
        }
        private void OnDestroy()
        {
            Manager.NetDataManager.Remove(NetID);
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(transform.position + transform.right * range, transform.position - transform.right * range);
        }

        public IEnumerator BeginCor()
        {
            float t = 0.0f;
            while (index < total || Ship_Corsair.Corsairs.Count > 0)
            {
                if (Ship_Corsair.Corsairs.Count < max && (Time.time - t) > spand && index < total)
                {
                    Vector3 p = transform.TransformPoint(new Vector3(Random.Range(-range, range), 0f, 0f));
                    Quaternion r = transform.rotation;
                    Manager.CreateGameObject(prefabId, p, r);
                    index++;
                    t = Time.time;
                }
                Remain(1-index / (float)total);
                yield return new WaitForEndOfFrame();
            }
            Remain(0.0f);
            End();
        }
        public void End()
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetServer.Send(Manager.CreateNetData(NetID, (byte)NetStatus.End));
                    EndEvent.Invoke();
                    break;
                case Corsair.NetStatus.Client:
                    EndEvent.Invoke();
                    break;
                case Corsair.NetStatus.Null:
                    EndEvent.Invoke();
                    break;
            }
        }
        public void Remain(float y)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.Remain);
                    n.Write(y);
                    NetServer.Send(n);
                    RemainEvent.Invoke(y);
                    break;
                case Corsair.NetStatus.Client:
                    RemainEvent.Invoke(y);
                    break;
                case Corsair.NetStatus.Null:
                    RemainEvent.Invoke(y);
                    break;
            }
        }
        public enum NetStatus : byte
        {
            GetStatus,
            SetStatus,
            Create,
            Remain,
            End,
        }
        public void NetDataManager(NetData data)
        {
            switch ((NetStatus)data.ReadByte())
            {
                case NetStatus.Create:
                    string n = data.ReadString();
                    Vector3 p = data.ReadVector3();
                    Quaternion r = data.ReadQuaternion();
                    Manager.CreateGameObject(prefabId, p, r);
                    break;
                case NetStatus.Remain:
                    Remain(data.ReadFloat());
                    break;
                case NetStatus.End:
                    End();
                    break;
            }
        }
    }
}