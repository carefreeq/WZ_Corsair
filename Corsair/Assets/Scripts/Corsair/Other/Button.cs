using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Corsair
{
    public class Button : MonoBehaviour
    {
        public UnityEngine.Events.UnityEvent OnDrag;
        public UnityEngine.Events.UnityEvent OnClick;
        private void OnHandHoverBegin(Hand hand)
        {

        }
        private void HandHoverUpdate(Hand hand)
        {
            if (hand.OnTriggerDown())
            {
                OnClick.Invoke();
            }
            OnDrag.Invoke();

        }

        private void OnHandHoverEnd(Hand hand)
        {
        }
        [ContextMenu("OnClick")]
        public void OnClickFunc()
        {
            OnClick.Invoke();
        }
    }
}