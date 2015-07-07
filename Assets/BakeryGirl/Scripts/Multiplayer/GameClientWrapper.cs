using System;
using UnityEngine;

namespace BakeryGirl.Chess {
    public class GameClientWrapper : PlayerAgent {
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

        private StateEnum _state = StateEnum.Idle;

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

        #region Agent Interfaces
        public override StateEnum State { get { return _state; } }
        public override void Think(Board board, Action<PlayerAction[], float> complete = null)
        {
            throw new NotImplementedException();
        }
        public override void Initialize()
        {
        }
        public override bool SwitchTurn(Unit.OwnerEnum nowTurn)
        {
            if (Client.IsMyTurn)
            {
                Client.EndTurn();
            }
            return !Client.IsMyTurn;
        }
        public override float GetCostTime()
        {
            return 0;
        }
        public override PlayerAction NextAction()
        {
            return new PlayerAction();
        }
        #endregion
    }
}