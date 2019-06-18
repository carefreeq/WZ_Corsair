using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public enum MessageType : byte
    {
        AddPlayer,
        RemovePlayer,
        SendPlayerStatus,

    }
    public class Manager
    {
        public static GameObject GameObject { get; private set; }
        public static int Score { get; private set; }
        static Manager()
        {
            GameObject = new GameObject("Manager");
            GameObject.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(GameObject);
            Score = 0;
        }
        public static void AddScore(int score)
        {
            Score += score;
            if (Score > 500)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(1, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
        private static void NetManager(NetData n)
        {
            switch ((MessageType)n.ReadByte())
            {
                case MessageType.AddPlayer:
                    Player.AddPlayer(n);
                    break;
                case MessageType.RemovePlayer:
                    Player.RemovePlayer(n);
                    break;
                case MessageType.SendPlayerStatus:
                    Player.ReceivePlayerStatus(n);
                    break;
            }
        }
        public static void Connect()
        {
            //NetClient.Connect();
        }
    }
}