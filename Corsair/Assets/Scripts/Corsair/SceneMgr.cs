using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class SceneMgr : MonoBehaviour
    {
#if UNITY_EDITOR
        public int levelId = 0;
#endif
        public UnityEngine.Events.UnityEvent VictoryEvent, FailEvent;
        private void Start()
        {
            Manager.SetPlayerIndex(Manager.Info.ID);

            Manager.SetGameStatus(GameStatus.Playing);

            switch (Net.Status)
            {
                case NetStatus.Server:
                    break;
                case NetStatus.Client:
                    NetData n = Manager.CreateNetData(Manager.NetID, (byte)Manager.NetStatus.GetGameObjects);
                    NetClient.Send(n);
                    break;
                case NetStatus.Null:
                    break;
            }

            Player.PlayerAllDeathEvent += Fail;
        }
        private void OnDestroy()
        {
            Player.PlayerAllDeathEvent -= Fail;
        }
        public void SetLevel(int i)
        {
            Manager.SetLevel(i);
        }
        public void ResetLevel(int i)
        {
            Manager.ResetLevel(i);
        }
        [ContextMenu("AutoNet")]
        public void AutoNet()
        {
            Manager.AutoNet();
        }
        public void Victory()
        {
            switch (Manager.GameStatus)
            {
                case GameStatus.Playing:
                    Manager.SetGameStatus(GameStatus.Victory);
                    VictoryEvent.Invoke();
                    break;
            }
        }
        public void Fail()
        {
            switch (Manager.GameStatus)
            {
                case GameStatus.Playing:
                    Manager.SetGameStatus(GameStatus.Fail);
                    FailEvent.Invoke();
                    break;

            }
        }
    }
}