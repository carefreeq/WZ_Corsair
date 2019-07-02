using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace Corsair
{
    public class Player_Vive : MonoBehaviour
    {
        public static Player_Vive Main { get; protected set; }
        public static SteamVR_Action_Boolean Teleprot { get; private set; }
        public static SteamVR_Action_Boolean GrabPinch { get; private set; }
        static Player_Vive()
        {
            Teleprot = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");
            GrabPinch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
        }
        public Camera Camera { get { return head; } }
        public Hand Left { get { return left; } }
        public Hand Right { get { return right; } }
        public bool TeleprotLock, GrabPinchLock;
        [SerializeField]
        private Camera head;
        [SerializeField]
        private Hand left, right;
        private void Awake()
        {
            if (Main == null)
                Main = this;
        }
        public float max = 100f;
        public LayerMask layer = ~0;
        public LineRenderer hint;
        public void ShowHint(PointInfo info)
        {
            if (!hint.gameObject.activeSelf)
                hint.gameObject.SetActive(true);
            hint.startColor = hint.endColor = info.color;
            hint.GetComponent<MeshRenderer>().material.color = info.color;
            hint.positionCount = info.path.Length;
            hint.SetPositions(info.path);
            hint.transform.localScale = Mathf.Clamp(Vector3.Distance(hint.transform.position, Player_Vive.Main.Camera.transform.position), 5f, 50f) * Vector3.one * 0.1f;
            hint.transform.position = info.path[info.path.Length - 1];
            hint.transform.rotation = Quaternion.LookRotation(info.nomral);
        }
        public void CloseHint()
        {
            hint.positionCount = 0;
            hint.gameObject.SetActive(false);
        }
        public PointInfo GetPoint(Transform t)
        {
            PointInfo info = new PointInfo();
            Vector3 d = new Vector3(t.forward.x, -0.5f, t.forward.z).normalized;
            Vector3[] p = Tools.GetPoints(t, d * Vector3.Dot(t.forward, d) * max);

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
                    info.nomral = h.normal;
                    if (h.transform.gameObject.layer == 10)
                        info.color = new Color(0f, 1f, 0f, 0.5f);
                    else
                        info.color = new Color(1f, 0f, 0f, 0.5f);
                    break;
                }
            }
            if (o >= l)
            {
                info.nomral = Vector3.up;
                info.color = new Color(1f, 0f, 0f, 0.5f);
            }
            info.point = _p[_p.Count - 1];
            info.path = _p.ToArray();
            return info;
        }
        public bool OnTrigger(SteamVR_Input_Sources input = SteamVR_Input_Sources.Any)
        {
            if (GrabPinchLock)
                return false;
            return GrabPinch.GetState(input);
        }
        public bool OnTriggerUp(SteamVR_Input_Sources input = SteamVR_Input_Sources.Any)
        {
            if (GrabPinchLock)
                return false;
            return GrabPinch.GetStateUp(input);
        }
        public bool OnTriggerDown(SteamVR_Input_Sources input = SteamVR_Input_Sources.Any)
        {
            if (GrabPinchLock)
                return false;
            return GrabPinch.GetStateDown(input);
        }
        public bool OnTouchPad(SteamVR_Input_Sources input = SteamVR_Input_Sources.Any)
        {
            if (TeleprotLock)
                return false;
            return Teleprot.GetState(input);
        }
        public bool OnTouchPadUp(SteamVR_Input_Sources input = SteamVR_Input_Sources.Any)
        {
            if (TeleprotLock)
                return false;
            return Teleprot.GetStateUp(input);
        }
        public bool OnTouchPadDown(SteamVR_Input_Sources input = SteamVR_Input_Sources.Any)
        {
            if (TeleprotLock)
                return false;
            return Teleprot.GetStateDown(input);
        }
    }
    public static class HandExtension
    {
        public static bool OnTrigger(this Hand hand)
        {
            return hand.grabPinchAction.GetState(hand.handType);
        }
        public static bool OnTriggerDown(this Hand hand)
        {
            return hand.grabPinchAction.GetStateDown(hand.handType);
        }
        public static bool OnTriggerUp(this Hand hand)
        {
            return hand.grabPinchAction.GetStateUp(hand.handType);
        }
    }
}