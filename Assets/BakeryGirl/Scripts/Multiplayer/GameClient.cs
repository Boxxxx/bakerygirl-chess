using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using System;

namespace BakeryGirl.Chess {
    using Random = UnityEngine.Random;
    using Hashtable = ExitGames.Client.Photon.Hashtable;

    public class GameClient : LoadBalancingClient {
        public class GameInfo {
            public string roomName = string.Empty;
            public int playerIdToMakeThisTurn = 0;
            public int[] playerIds = { 0, 0 };
            public int turnNum = 0;
        }

        private readonly static string[] kRoomPropsInLobby = new string[] { Consts.PropNames.RoomPwd };
        private const byte kPlayersNum = 2;
        private const int kEmptyRoomTtl = 5000;
        private const int kPlayerTtl = int.MaxValue;

        public Action<DisconnectCause> onDisconnect;
        public Action<TypedLobby> onJoinLobby;
        public Action<Room> onJoinRoom;
        public Action onCreateRoom;
        public Action onCloseRoom;
        public Action onGameStart;

        public int ActivePlayerCnt {
            get {
                int cnt = 0;
                foreach (var player in CurrentRoom.Players) {
                    cnt += player.Value.IsInactive ? 0 : 1;
                }
                return cnt;
            }
        }

        public bool IsGameStart {
            get { return Info != null; }
        }

        public Player Me {
            get { return LocalPlayer; }
        }
        public Player Opponent {
            get { return LocalPlayer.GetNext(); }
        }
        public Player CurrentPlayer {
            get {
                return Info.playerIdToMakeThisTurn == null ? null : CurrentRoom.GetPlayer(Info.playerIdToMakeThisTurn);
            }
        }
        public Player NextPlayer {
            get {
                return Info.playerIdToMakeThisTurn == null ? null : LocalPlayer.GetNextFor(Info.playerIdToMakeThisTurn);
            }
        }

        public bool IsMyTurn {
            get { return Info.playerIdToMakeThisTurn == LocalPlayer.ID; }
        }

        public GameClient() {
            OnStateChangeAction += clientState => {
                if (clientState == ClientState.Disconnected) {
                    OnDisconnect();
                }
            };
        }

        public GameInfo Info {
            get;
            private set;
        }

        #region Events Receiver
        public override void OnStatusChanged(StatusCode statusCode) {
            base.OnStatusChanged(statusCode);

            switch (statusCode) {
                case StatusCode.Exception:
                case StatusCode.ExceptionOnConnect:
                    Debug.LogWarning("Exception on connection level. Is the server running? Is the address (" + this.MasterServerAddress + ") reachable?");
                    break;
                case StatusCode.Disconnect:
                    break;
            }
        }

        public override void OnOperationResponse(OperationResponse operationResponse) {
            base.OnOperationResponse(operationResponse);

            switch (operationResponse.OperationCode) {
                case (byte)OperationCode.WebRpc:
                    Debug.Log("WebRpc-Response: " + operationResponse.ToStringFull());
                    if (operationResponse.ReturnCode == 0) {
                        this.OnWebRpcResponse(new WebRpcResponse(operationResponse));
                    }
                    break;
                case (byte)OperationCode.JoinLobby:
                    if (onJoinLobby != null) {
                        onJoinLobby(CurrentLobby);
                    }
                    break;
                case (byte)OperationCode.JoinGame:
                case (byte)OperationCode.CreateGame:
                    if (this.Server == ServerConnection.GameServer) {
                        if (operationResponse.ReturnCode == 0) {
                            if (onJoinRoom != null) {
                                onJoinRoom(CurrentRoom);
                            }
                        }
                    }
                    break;
                case (byte)OperationCode.JoinRandomGame:
                    if (operationResponse.ReturnCode == ErrorCode.NoRandomMatchFound) {
                        // no room found: we create one!
                        if (onCreateRoom != null) {
                            onCreateRoom();
                        }
                    }
                    break;
            }
        }

        public override void OnEvent(EventData photonEvent) {
            base.OnEvent(photonEvent);

            switch (photonEvent.Code) {
                case EventCode.PropertiesChanged:
                    Debug.Log("Got Properties via Event. Update board by room props.");
                    LoadGameFromProperties(true);
                    break;
                case EventCode.Join:
                    if (this.CurrentRoom.Players.Count == kPlayersNum && this.CurrentRoom.IsOpen) {
                        this.CurrentRoom.IsOpen = false;
                        this.CurrentRoom.IsVisible = false;

                        if (LocalPlayer.ID == CurrentRoom.MasterClientId) {
                            OnPlayerReady();
                        }
                    }
                    break;
                case EventCode.Leave:
                    if (ActivePlayerCnt < kPlayersNum) {
                        OpLeaveRoom();
                        if (onCloseRoom != null) {
                            onCloseRoom();
                        }
                    }
                    break;
            }
        }

        private void OnWebRpcResponse(WebRpcResponse response) {
            if (response.ReturnCode != 0) {
                Debug.Log(response.ToStringFull());     // in an error case, it's often helpful to see the full response
                return;
            }
        }

        private void OnDisconnect() {
            if (onDisconnect != null) {
                onDisconnect(DisconnectedCause);
            }
        }

        private void OnPlayerReady() {
            if (LocalPlayer.ID != CurrentRoom.MasterClientId) {
                return;
            }

            DealGameStart(false);

            Hashtable hashtable = new Hashtable();
            hashtable[Consts.PropNames.IsStart] = true;
            SavePlayersInProps(hashtable);
            SaveGameToProperties(hashtable);
            OpSetCustomPropertiesOfRoom(hashtable);

            if (onGameStart != null) {
                onGameStart();
            }
        }

        public void OnUpdate() {
            Service();
        }
        #endregion

        #region Public Interfaces
        public void Connect(string playerName, string region) {
            Clear();

            if (IsConnected) {
                Disconnect();
            }
            PlayerName = playerName;
            ConnectToRegionMaster(region);
        }

        public void JoinRandomRoom(string pwd = "") {
            if (string.IsNullOrEmpty(pwd)) {
                this.OpJoinRandomRoom(null, 0);
            }
            else {
                var customRoomProperties = new Hashtable() { { "pwd", pwd } };
                this.OpJoinRandomRoom(customRoomProperties, 0);
            }
        }

        /// <summary>
        /// Demo method that makes up a roomname and sets up the room in general.
        /// </summary>
        public void CreateTurnbasedRoom(string roomPwd) {
            string newRoomName = this.PlayerName + "-" + Random.Range(0, 1000).ToString("D4");    // for int, Random.Range is max-exclusive!
            Debug.Log("CreateTurnbasedRoom() will create: " + newRoomName);

            var customRoomProperties = new Hashtable() { 
                { "pwd", roomPwd }  // the password of this room
            };

            RoomOptions roomOptions = new RoomOptions() {
                MaxPlayers = kPlayersNum,
                CustomRoomProperties = customRoomProperties,
                CustomRoomPropertiesForLobby = kRoomPropsInLobby,
                EmptyRoomTtl = kEmptyRoomTtl,
                PlayerTtl = kPlayerTtl
            };
            this.OpCreateRoom(newRoomName, roomOptions, TypedLobby.Default);
        }

        public void EndTurn() {
            if (!IsMyTurn) {
                Debug.LogWarning("[Client] this is not you turn!");
                return;
            }

            HandoverTurnToNextPlayer();
            SaveGameToProperties();
        }

        public string FormatRoomPropsInLobby() {
            Hashtable customRoomProps = CurrentRoom.CustomProperties;
            string interestingProps = "";
            foreach (string propName in kRoomPropsInLobby) {
                if (customRoomProps.ContainsKey(propName)) {
                    if (!string.IsNullOrEmpty(interestingProps)) interestingProps += " ";
                    interestingProps += propName + ":" + customRoomProps[propName];
                }
            }
            return interestingProps;
        }

        public string FormatRoomProps() {
            Hashtable customRoomProps = CurrentRoom.CustomProperties;
            string interestingProps = "[";
            bool isFirst = true;
            foreach (var prop in customRoomProps) {
                if (isFirst) {
                    isFirst = false;
                }
                else {
                    interestingProps += ", ";
                }
                interestingProps += prop.Key + ":" + prop.Value;
            }
            return interestingProps + "]";
        }
        #endregion

        #region Utility
        private void LoadGameFromProperties(bool calledByEvent) {
            Hashtable roomProps = CurrentRoom.CustomProperties;
            Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));

            if (IsGameStart || roomProps.ContainsKey(Consts.PropNames.IsStart)) {
                if (Info == null) {
                    DealGameStart();
                }

                // we set properties "pt" (player turn) and "t#" (turn number). those props might have changed
                // it's easier to use a variable in gui, so read the latter property now
                if (this.CurrentRoom.CustomProperties.ContainsKey(Consts.PropNames.TurnNum)) {
                    Info.turnNum = (int)this.CurrentRoom.CustomProperties[Consts.PropNames.TurnNum];
                }
                else {
                    Info.turnNum = 0;
                }

                if (this.CurrentRoom.CustomProperties.ContainsKey(Consts.PropNames.PlayerIdToMakeThisTurn)) {
                    Info.playerIdToMakeThisTurn = (int)this.CurrentRoom.CustomProperties[Consts.PropNames.PlayerIdToMakeThisTurn];
                }
                else {
                    Info.playerIdToMakeThisTurn = 0;
                }

                // if the game didn't save a player's turn yet (it is 0): use master
                if (Info.playerIdToMakeThisTurn == 0) {
                    Info.playerIdToMakeThisTurn = this.CurrentRoom.MasterClientId;
                }
            }
        }

        private void SaveGameToProperties(Hashtable hashtable) {
            Info.turnNum = Info.turnNum + 1;

            hashtable.Add(Consts.PropNames.PlayerIdToMakeThisTurn, Info.playerIdToMakeThisTurn);  // "pt" is for "player turn" and contains the ID/actorNumber of the player who's turn it is
            hashtable.Add(Consts.PropNames.TurnNum, Info.turnNum);
        }

        private void SaveGameToProperties() {
            Hashtable hashtable = new Hashtable();
            SaveGameToProperties(hashtable);
            OpSetCustomPropertiesOfRoom(hashtable);
        }

        private void SavePlayersInProps(Hashtable hashtable) {
            if (this.CurrentRoom == null || this.CurrentRoom.CustomProperties == null || this.CurrentRoom.CustomProperties.ContainsKey(Consts.PropNames.Players)) {
                Debug.Log("Skipped saving names. They are already saved.");
                return;
            }

            Debug.Log("Saving names.");
            hashtable[Consts.PropNames.Players] = string.Format("{0};{1}", this.LocalPlayer.Name, this.Opponent.Name);
        }

        private void DealGameStart(bool callEvent = true) {
            if (IsGameStart) {
                return;
            }
            Info = new GameInfo() {
                roomName = CurrentRoom.Name,
                playerIdToMakeThisTurn = CurrentRoom.MasterClientId,
                turnNum = 0
            };
            if (callEvent && onGameStart != null) {
                onGameStart();
            }
        }

        private void HandoverTurnToNextPlayer() {
            if (this.LocalPlayer != null) {
                Player nextPlayer = this.LocalPlayer.GetNextFor(Info.playerIdToMakeThisTurn);
                if (nextPlayer != null) {
                    Info.playerIdToMakeThisTurn = nextPlayer.ID;
                    return;
                }
            }

            Info.playerIdToMakeThisTurn = 0;
            Debug.LogWarning("[Client] no localPlayer");
        }

        private void Clear() {
            Info = null;
        }
        #endregion
    }
}