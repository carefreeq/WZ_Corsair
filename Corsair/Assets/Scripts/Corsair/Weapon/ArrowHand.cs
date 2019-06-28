//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The object attached to the player's hand that spawns and fires the
//			arrow
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Valve.VR.InteractionSystem;

namespace Corsair
{
    //-------------------------------------------------------------------------
    public class ArrowHand : Valve.VR.InteractionSystem.ArrowHand
    {
        public string NetID { get; private set; }


        private void Start()
        {
            Player p = GetComponentInParent<Player>();
            if(p)
            {
                NetID = p.NetID +"_"+ name;
            }
        }
        protected void OnDestroy()
        {
            Debug.Log("Destroy:"+name);
        }
        protected override void FireArrow()
        {
            base.FireArrow();
            Debug.Log("FireArrow:" + name);
        }
        public enum NetStatus : byte
        {
            Launch,
        }

        public void NetDataManager(NetData data)
        {
                switch ((NetStatus)data.ReadByte())
                {
                    case NetStatus.Launch:
                        break;
                }
        }
    }
}
