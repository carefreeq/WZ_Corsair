using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using UnityEngine.SceneManagement;
namespace Corsair
{
    public enum GameStatus : byte
    {
        Playing,
        Victory,
        Fail,
        Stop
    }
    public class PlayerInfo
    {
        public int ID;
        public string Name;
        public IPEndPoint IP;
    }
    public class Manager : MonoBehaviour
    {
        public static Manager Instance { get; private set; }
        public static string NetID { get { return "Manager"; } }
        public static PlayerInfo Info { get; private set; }
        public static event Action<int> PlayerIndexEvent;
        public static Dictionary<string, Action<NetData>> NetDataManager { get; private set; }

        public static int Score { get; private set; }
        public static int Level { get; private set; }

        public static List<PlayerInfo> Players { get; private set; }
        public static List<GameObject> GameObjects { get; private set; }
        public static List<GameObject> Prefabs { get; private set; }

        public static GameStatus GameStatus { get; private set; }
        public static event Action<GameStatus> GameStatusEvent;
        static Manager()
        {
            Score = 0;
            Info = new PlayerInfo() { ID = 0, Name = "Player", IP = Net.LocalIPEndPoint };
            Players = new List<PlayerInfo>();
            Players.Add(Info);
            Scene s = SceneManager.GetActiveScene();
            Level = GetCurrentLevel();
            GameStatus = GameStatus.Playing;

            NetServer.AddClientEvent += AddPlayer;
            NetServer.RemoveClientEvent += RemovePlayer;

            NetDataManager = new Dictionary<string, Action<NetData>>();
            NetDataManager.Add(NetID, NetManager);

            Net.NetDataEvent += (n) =>
            {
                switch (n.MessageType)
                {
                    case NetMessageType.Data:
                        string id = n.ReadString();
                        if (NetDataManager.ContainsKey(id))
                            NetDataManager[id](n);
                        break;
                }
            };

            GameObject m = new GameObject("Manager");
            Instance = m.AddComponent<Manager>();
            DontDestroyOnLoad(m);

            Prefabs = new List<GameObject>();
            Prefabs.Add(Resources.Load<GameObject>("Corsair_1"));
            GameObjects = new List<GameObject>();
        }
        public static void AddPlayer(NetClient.Info client)
        {
            PlayerInfo p = new PlayerInfo();
            p.ID = 0;
            for (int i = 0; i < Players.Count; i++)
            {
                if (p.ID == Players[i].ID)
                    p.ID++;
            }
            p.Name = client.PlayerName;
            p.IP = client.IP;
            Players.Add(p);
            Debug.Log("AddPlayer:" + p.ID + "  name:" + p.Name);
        }
        public static void RemovePlayer(NetClient.Info client)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (client.IP.ToString() == Players[i].IP.ToString())
                {
                    Players.RemoveAt(i);
                    Debug.Log("RemovePlayer:" + i);
                }
            }
        }
        public static NetData CreateNetData(string id, byte status)
        {
            NetData n = new NetData(NetMessageType.Data);
            n.Write(id);
            n.Write(status);
            return n;
        }
        public enum NetStatus : byte
        {
            GetLevel,
            SetLevel,
            ResetLevel,
            SetPlayerIndex,
            GetPlayerIndex,
            GetGameStatus,
            SetGameStatus,
            CreateGameObject,
            GetGameObjects,
        }
        private static void NetManager(NetData data)
        {
            switch ((NetStatus)data.ReadByte())
            {
                case NetStatus.GetLevel:
                    NetData ngl = Manager.CreateNetData(NetID, (byte)NetStatus.SetLevel);
                    ngl.Write(Level);
                    NetServer.SendTo(ngl, data.RemoteIP);
                    break;
                case NetStatus.SetLevel:
                    int level = data.ReadInt();
                    if (Level != level)
                        LoadLevel(level);
                    break;
                case NetStatus.ResetLevel:
                    LoadLevel(data.ReadInt());
                    break;
                case NetStatus.GetPlayerIndex:
                    NetData ngp = Manager.CreateNetData(NetID, (byte)NetStatus.SetPlayerIndex);
                    ngp.Write(1);
                    NetServer.SendTo(ngp, data.RemoteIP);
                    break;
                case NetStatus.SetPlayerIndex:
                    SetPlayerIndex(data.ReadInt());
                    break;
                case NetStatus.GetGameStatus:
                    NetData ngg = Manager.CreateNetData(NetID, (byte)NetStatus.SetGameStatus);
                    ngg.Write((byte)GameStatus);
                    NetServer.SendTo(ngg, data.RemoteIP);
                    break;
                case NetStatus.SetGameStatus:
                    SetGameStatus((GameStatus)data.ReadByte());
                    break;
                case NetStatus.CreateGameObject:
                    int id = data.ReadInt();
                    string na = data.ReadString();
                    Vector3 po = data.ReadVector3();
                    Quaternion ro = data.ReadQuaternion();
                    Instantiate(Prefabs[id], po, ro).name = na;
                    break;
                case NetStatus.GetGameObjects:
                    for (int i = 0; i < GameObjects.Count; i++)
                    {
                        NetData ng = Manager.CreateNetData(NetID, (byte)NetStatus.CreateGameObject);
                        GameObject g = GameObjects[i];
                        ng.Write(int.Parse(g.name.Substring(0, g.name.IndexOf('_'))));
                        ng.Write(g.name);
                        ng.Write(g.transform.position);
                        ng.Write(g.transform.rotation);
                        NetServer.SendTo(ng, data.RemoteIP);
                    }
                    break;
            }
        }

        public static void AutoNet()
        {
            NetUdp.Listen();
            Instance.StartCoroutine(AutoNetCor());
        }
        private static IEnumerator AutoNetCor()
        {
            NetClient.Flush();
            yield return new WaitForSeconds(2f);
            if (NetClient.Servers.Count > 0)
            {
                while (Net.Status != Corsair.NetStatus.Client)
                {
                    NetClient.ConnectToServer(NetClient.Servers[0].IP);
                    yield return new WaitForSeconds(2f);
                }
                NetData np = Manager.CreateNetData(NetID, (byte)NetStatus.GetPlayerIndex);
                NetClient.Send(np);
                NetData nl = Manager.CreateNetData(NetID, (byte)NetStatus.GetLevel);
                NetClient.Send(nl);
            }
            else
            {
                NetServer.Listen();
            }
        }

        private void OnApplicationQuit()
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetServer.ShutDown();
                    break;
                case Corsair.NetStatus.Client:
                    NetClient.ShutDown();
                    break;
                case Corsair.NetStatus.Null:
                    break;
            }
            NetUdp.ShutDown();
            Net.NetDataMgr.Enable = false;
        }
        public static int GetCurrentLevel()
        {
            Scene s = SceneManager.GetActiveScene();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                if (SceneManager.GetSceneByBuildIndex(i).name == s.name)
                    return i;
            return -1;
        }
        public static void SetPlayerIndex(int index)
        {
            Info.ID = index;
            if (PlayerIndexEvent != null)
                PlayerIndexEvent(Info.ID);
        }
        public static void SetGameStatus(GameStatus s)
        {
            if (GameStatusEvent != null)
                GameStatusEvent(s);
            GameStatus = s;
        }
        public static void AddScore(int score)
        {
            Score += score;
        }

        private static int index = 0;
        public static void CreateGameObject(int id, Vector3 pos, Quaternion rota)
        {
            if (id > -1 && id < Prefabs.Count)
            {
                string name = id + "_" + Prefabs[id].name + "_" + index;
                switch (Net.Status)
                {
                    case Corsair.NetStatus.Server:
                        NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.CreateGameObject);
                        n.Write(id);
                        n.Write(name);
                        n.Write(pos);
                        n.Write(rota);
                        NetServer.Send(n);

                        Instantiate(Prefabs[id], pos, rota).name = name;
                        break;
                    case Corsair.NetStatus.Client:
                        Instantiate(Prefabs[id], pos, rota).name = name;
                        break;
                    case Corsair.NetStatus.Null:
                        Instantiate(Prefabs[id], pos, rota).name = name;
                        break;
                }
                index++;
            }
        }
        public static void SetLevel(int i)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.SetLevel);
                    n.Write(i);
                    NetServer.Send(n);
                    if (i != Level)
                        LoadLevel(i);
                    break;
                case Corsair.NetStatus.Client:
                    break;
                case Corsair.NetStatus.Null:
                    if (i != Level)
                        LoadLevel(i);
                    break;
            }
        }
        public static void ResetLevel(int i)
        {
            switch (Net.Status)
            {
                case Corsair.NetStatus.Server:
                    NetData n = Manager.CreateNetData(NetID, (byte)NetStatus.ResetLevel);
                    n.Write(Level);
                    NetServer.Send(n);
                    LoadLevel(i);
                    break;
                case Corsair.NetStatus.Client:
                    break;
                case Corsair.NetStatus.Null:
                    LoadLevel(i);
                    break;
            }
        }
        private static IEnumerator loadlevelEnumerator;
        private static void LoadLevel(int i)
        {
            if (i < SceneManager.sceneCountInBuildSettings)
            {
                if (loadlevelEnumerator != null)
                    Instance.StopCoroutine(loadlevelEnumerator);
                loadlevelEnumerator = LoadLevelCor(i);
                Instance.StartCoroutine(LoadLevelCor(i));
            }
        }
        private static IEnumerator LoadLevelCor(int i)
        {
            Level = i;
            ScreenControl.Main.ToBlack(1.0f);
            yield return new WaitForSeconds(2.0f);
            SceneManager.LoadScene(Level, LoadSceneMode.Single);
            //异步后vive 会出BUG
            //SceneManager.LoadScene(AsyncLoad.SceneName, LoadSceneMode.Single);
        }
    }
}