using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;
namespace Corsair
{
    public class DelayOnClick : Delay
    {
        public SteamVR_Action_Boolean teleportAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");

        public Scrollbar.ScrollEvent TimeEvent;
        public UnityEngine.Events.UnityEvent OnClickEvent;

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(UpdateCor());
        }
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        private IEnumerator UpdateCor()
        {
            float t = 0.0f;
            while ((t += Time.deltaTime) < time)
            {
                TimeEvent.Invoke(t / time);
                yield return new WaitForEndOfFrame();
            }
            TimeEvent.Invoke(1.0f);
            yield return new WaitUntil(() => teleportAction.GetStateUp(SteamVR_Input_Sources.Any));
            yield return new WaitUntil(() => teleportAction.GetStateUp(SteamVR_Input_Sources.Any));
            OnClickEvent.Invoke();
        }
    }
}