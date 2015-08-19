using System;
using UnityEngine;

namespace BakeryGirl.Chess {
    public class GameClientAgent : PlayerAgent {
        private static GameClientAgent _instance = null;
        public static GameClientAgent Instance {
            get {
                //if (_instance == null) {
                //    _instance = GameObject.FindObjectOfType<GameClientAgent>();
                //    if (_instance == null) {
                //        var obj = new GameObject();
                //        obj.name = "GameClient";
                //        _instance = obj.AddComponent<GameClientAgent>();
                //    }
                //}
                return _instance;
            }
        }
        public static GameClient ClientInstance {
            get { return Instance.Client; }
        }

        public string appVersion = "v1.0.0";
        public string appId = string.Empty;
        public bool useCloud = false;
        public string region = "EU";
        public string customServerIp = "120.24.99.31:5055";

        public bool logOnScreen = true;
        public bool verifyEachTurn = true;

        public GameClient Client {
            get;
            private set;
        }
        public override Unit.OwnerEnum PlayerTurn {
            get { return Client.MyTurn; }
        }
        public string MyUsername {
            get { return Client.MyUsername; }
        }
        public string OpponentUsername {
            get { return Client.OpponentUsername; }
        }

        #region Unity Callbacks
        void Awake() {
            _instance = null;
            if (_instance == null) {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                if (_instance != this) {
                    Destroy(gameObject);
                    return;
                }
            }

            Client = new GameClient();
            Client.AppId = appId;
            Client.AppVersion = appVersion;
            Client.onReceiveMove += OnReceiveMove;
        }

        void OnGUI() {
            if (!logOnScreen) {
                return;
            }

            GUI.skin.button.stretchWidth = true;
            GUI.skin.button.fixedWidth = 0;

            GUI.color = Color.black;
            GUILayout.Label("name: " + Client.PlayerName +" state: " + Client.State + " player turn: " + (Client.IsPlayerTurn ? "true" : "false"));
            if (Client.State == ExitGames.Client.Photon.LoadBalancing.ClientState.Joined) {
                string interestingPropsAsString = Client.FormatRoomPropsInLobby();
                GUILayout.Label("room: " + Client.CurrentRoom.Name + " props: " + interestingPropsAsString);
            }
        }

        void Update() {
            Client.OnUpdate();
        }

        void OnDestroy() {
            DestroyClient();
        }

        void OnApplicationQuit() {
            DestroyClient();
        }

        public override void OnAgentDestroy() {
            Destroy(gameObject);
            _instance = null;
        }
        #endregion

        #region Public & Private Interfaces
        public void JoinGame(string username) {
            if (useCloud) {
                Client.ConnectToCloud(username, region);
            }
            else {
                Client.ConnectToCustomServer(username, customServerIp);
            }
        }

        private void DestroyClient() {
            if (Client != null && Client.loadBalancingPeer != null) {
                Client.Disconnect();
                Client.loadBalancingPeer.StopThread();
            }
            Client = null;
        }
        #endregion

        #region Agent Interfaces
        private void OnReceiveMove(PlayerAction[] actions)
        {
            if (State == StateEnum.Thinking)
            {
                TurnOver(actions);
            }
        }
        public override void Initialize()
        {
            base.Initialize();
            GameInfo.Instance.blackPlayerName = PlayerTurn == Unit.OwnerEnum.Black ? MyUsername : OpponentUsername;
            GameInfo.Instance.whitePlayerName = PlayerTurn == Unit.OwnerEnum.White ? MyUsername : OpponentUsername;
        }
        public override void Think(Board board, Action<PlayerAction[], float> complete = null)
        {
            base.Think(board, complete);
            //throw new NotImplementedException();
        }
        public override bool SwitchTurn(Unit.OwnerEnum nowTurn, bool initial)
        {
            return !Client.IsPlayerTurn;
        }
        public override void OnMove(bool isAgent, PlayerAction[] actions) {
            if (!isAgent) {
                Client.SendMove(actions);
            }
            else {
                if (verifyEachTurn) {
                    Client.VerifyBoardFromProperties();
                }
            }
        }
        #endregion
    }
}