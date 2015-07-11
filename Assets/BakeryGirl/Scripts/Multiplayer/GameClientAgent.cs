using System;
using UnityEngine;

namespace BakeryGirl.Chess {
    public class GameClientAgent : PlayerAgent {
        private static GameClientAgent _instance = null;
        public static GameClientAgent Instance {
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType<GameClientAgent>();
                    if (_instance == null) {
                        var obj = new GameObject();
                        obj.name = "GameClient";
                        _instance = obj.AddComponent<GameClientAgent>();
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
        public override Unit.OwnerEnum MyTurn {
            get {
                return Client.MyTurn;
            }
        }

        #region Unity Callbacks
        void Awake() {
            Client = new GameClient();
            Client.AppId = appId;
            Client.AppVersion = appVersion;
            Client.onReceiveMove += OnReceiveMove;
        }

        void OnGUI() {
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
            if (Client != null && Client.loadBalancingPeer != null)
            {
                Client.Disconnect();
                Client.loadBalancingPeer.StopThread();
            }
            Client = null;
        }

        #endregion

        #region Public Interfaces
        public void JoinGame(string nickname) {
            Client.ConnectWithNameAndRegion(nickname, region);
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
                Client.VerifyBoardFromProperties();
            }
        }
        #endregion
    }
}