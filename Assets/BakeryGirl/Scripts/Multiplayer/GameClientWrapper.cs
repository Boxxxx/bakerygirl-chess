using UnityEngine;
using System;
using System.Collections;

namespace BakeryGirl.Chess.Multiplayer {
    using Hashtable = ExitGames.Client.Photon.Hashtable;

    public class GameClientWrapper : MonoBehaviour {
        private static GameClientWrapper _instance = null;
        public static GameClientWrapper Instance {
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType<GameClientWrapper>();
                    if (_instance == null) {
                        var obj = new GameObject();
                        obj.name = "GameClient";
                        _instance = obj.AddComponent<GameClientWrapper>();
                    }
                }
                return _instance;
            }
        }
        public static GameClient ClientInstance {
            get { return Instance.Client; }
        }

        public string appId = string.Empty;
        public string appVersion = "v1.0.0";
        public string region = "EU";

        public GameClient Client {
            get;
            private set;
        }

        #region Unity Callbacks
        void Awake() {
            Client = new GameClient();
            Client.AppId = appId;
            Client.AppVersion = appVersion;
        }

        void OnGUI() {
            GUI.skin.button.stretchWidth = true;
            GUI.skin.button.fixedWidth = 0;

            GUILayout.Label("name: " + Client.PlayerName +" state: " + Client.State);
            if (Client.State == ExitGames.Client.Photon.LoadBalancing.ClientState.Joined) {
                string interestingPropsAsString = Client.FormatRoomProps();
                GUILayout.Label("room: " + Client.CurrentRoom.Name + " props: " + interestingPropsAsString);
            }
        }

        void Update() {
            Client.OnUpdate();
        }

        void OnApplicationQuit() {
            if (Client != null && Client.loadBalancingPeer != null) {
                Client.Disconnect();
                Client.loadBalancingPeer.StopThread();
            }
            Client = null;
        }
        #endregion

        #region Public Interfaces
        public void Connect(string nickname) {
            Client.Connect(nickname, region);
        }
        #endregion
    }
}