using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BakeryGirl.Chess.Multiplayer {
    using Random = UnityEngine.Random;
    using Hashtable = ExitGames.Client.Photon.Hashtable;

    public class GameClient : LoadBalancingClient {
        private readonly static string[] kRoomPropsInLobby = new string[] { "pwd" };
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
            get { return CurrentRoom != null && CurrentRoom.Players.Count == kPlayersNum; }
        }

        public Player Me {
            get { return LocalPlayer; }
        }
        public Player Opponent {
            get { return LocalPlayer.GetNext(); }
        }

        public GameClient() {
            OnStateChangeAction += clientState => {
                if (clientState == ClientState.Disconnected) {
                    OnDisconnect();
                }
            };
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
                    break;
                case EventCode.Join:
                    if (this.CurrentRoom.Players.Count == kPlayersNum && this.CurrentRoom.IsOpen) {
                        this.CurrentRoom.IsOpen = false;
                        this.CurrentRoom.IsVisible = false;
                        //this.SavePlayersInProps();
                        this.CurrentRoom.CustomProperties.Add("start", true);
                        if (onGameStart != null) {
                            onGameStart();
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
        #endregion

        #region Public Interfaces
        public void Connect(string playerName, string region) {
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

            var customRoomProperties = new Hashtable() { { "pwd", roomPwd } };

            RoomOptions roomOptions = new RoomOptions() {
                MaxPlayers = kPlayersNum,
                CustomRoomProperties = customRoomProperties,
                CustomRoomPropertiesForLobby = kRoomPropsInLobby,
                EmptyRoomTtl = kEmptyRoomTtl,
                PlayerTtl = kPlayerTtl
            };
            this.OpCreateRoom(newRoomName, roomOptions, TypedLobby.Default);
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
            string interestingProps = "";
            foreach (var prop in customRoomProps) {
                interestingProps += prop.Key + ":" + prop.Value;
            }
            return interestingProps;
        }

        public void Update() {
            Service();
        }
        #endregion
    }
}