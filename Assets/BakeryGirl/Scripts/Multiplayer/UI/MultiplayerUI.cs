using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;

namespace BakeryGirl.Chess {
    public class MultiplayerUI : MonoBehaviour {
        private const string kUserNamePlayerPref = "NamePickUserName";
        private const string kDefaultUserName = "Jefuty";

        public enum EntryState {
            None, MainEntry, Connecting, WaitingForOpponent, GamePlay
        };

        public UIPanel mainEntryPanel;
        public UIInput nicknameLabel;
        public UIInput roomPwdLabel;
        public NGUIButtonHandler randomMatchButton;
        public NGUIButtonHandler pwdMatchButton;

        public UIPanel connectingPanel;
        public UILabel connectingLabel;
        public NGUIButtonHandler exitConnectingBUtton;

        public UIPanel waitingPanel;
        public NGUIButtonHandler exitWaitingButton;

        public GameObject[] mainGameComponents = {};

        private EntryState _state = EntryState.None;
        private GameClient _client;

        private string Nickname {
            get {
                return nicknameLabel.text;
            }
            set {
                nicknameLabel.text = value;
            }
        }

        private string RoomPwd {
            get { return roomPwdLabel.text; }
        }

        public EntryState CurrentState {
            get { return _state; }
            private set {
                if (_state != value) {
                    var oldState = _state;
                    _state = value;
                    connectingPanel.gameObject.SetActive(false);
                    mainEntryPanel.gameObject.SetActive(false);
                    waitingPanel.gameObject.SetActive(false);
                    foreach (var obj in mainGameComponents) {
                        obj.gameObject.SetActive(false);
                    }
                    switch (value) {
                        case EntryState.MainEntry:
                            GameClientWrapper.ClientInstance.Disconnect();
                            mainEntryPanel.gameObject.SetActive(true);
                            break;
                        case EntryState.Connecting:
                            connectingPanel.gameObject.SetActive(true);
                            break;
                        case EntryState.WaitingForOpponent:
                            waitingPanel.gameObject.SetActive(true);
                            break;
                        case EntryState.GamePlay:
                            foreach (var obj in mainGameComponents) {
                                obj.gameObject.SetActive(true);
                            }
                            break;
                    }
                    OnSwitchState(value, oldState);
                }
            }
        }

        void Awake() {
            // load stored nickname in playerPrefs
            string prefsName = PlayerPrefs.GetString(kUserNamePlayerPref);
            if (!string.IsNullOrEmpty(prefsName)) {
                Nickname = prefsName;
            }
            else {
                Nickname = kDefaultUserName;
            }

            RegisterButtonCBs();
            RegisterClientCBs();

            CurrentState = EntryState.MainEntry;
        }

        void Update() {
            if (_state == EntryState.Connecting) {
                connectingLabel.text = GameClientWrapper.Instance.Client.State.ToString();
            }
        }

        void RegisterButtonCBs() {
            randomMatchButton.clickedHandle += (data, index) => {
                PlayerPrefs.SetString(kUserNamePlayerPref, Nickname);
                GameClientWrapper.Instance.Connect(Nickname);
                CurrentState = EntryState.Connecting;
            };
            pwdMatchButton.clickedHandle += (data, index) => {
                PlayerPrefs.SetString(kUserNamePlayerPref, Nickname);
                GameClientWrapper.Instance.Connect(Nickname);
                CurrentState = EntryState.Connecting;
            };
            exitWaitingButton.clickedHandle += (data, index) => {
                CurrentState = EntryState.MainEntry;
            };
            exitConnectingBUtton.clickedHandle += (data, index) => {
                CurrentState = EntryState.MainEntry;
            };
        } 

        void RegisterClientCBs() {
            GameClientWrapper.ClientInstance.onDisconnect += OnDisconnect;
            GameClientWrapper.ClientInstance.onJoinLobby += OnJoinLobby;
            GameClientWrapper.ClientInstance.onJoinRoom += OnJoinRoom;
            GameClientWrapper.ClientInstance.onCreateRoom += OnCreateRoom;
            GameClientWrapper.ClientInstance.onCloseRoom += OnCloseRoom;
            GameClientWrapper.ClientInstance.onGameStart += OnGameStart;
        }

        #region Events
        void OnSwitchState(EntryState newState, EntryState oldState) { }

        void OnDisconnect(DisconnectCause disconnectCause) {
            if (disconnectCause == DisconnectCause.None) {
                return;
            }
            ErrorPanel.Show("Disonnected", () => {
                CurrentState = EntryState.MainEntry;
            });
        }

        void OnJoinLobby(TypedLobby lobby) {
            if (CurrentState != EntryState.Connecting) {
                // If it is not in connecting state, return
                return;
            }
            OutputRoomList();
            GameClientWrapper.ClientInstance.JoinRandomRoom(RoomPwd);
        }

        void OnJoinRoom(Room room) {
            Debug.Log("joined game " + room.Name);
            CurrentState = EntryState.WaitingForOpponent;
        }

        void OnCreateRoom() {
            GameClientWrapper.ClientInstance.CreateTurnbasedRoom(RoomPwd);
        }

        void OnCloseRoom() {
            ErrorPanel.Show("Player has left", () => {
                CurrentState = EntryState.MainEntry;
            });
        }

        void OnGameStart() {
            Debug.Log("game start");
            CurrentState = EntryState.GamePlay;
        }
        #endregion

        #region Debug
        void OutputRoomList() {
            string roomList = "";
            bool isFirst = true;
            foreach (var roomInfo in GameClientWrapper.ClientInstance.RoomInfoList) {
                if (isFirst) {
                    isFirst = false;
                }
                else {
                    roomList += ", ";
                }
                roomList += roomInfo.Value.Name;
            }
            Debug.Log("RoomList: [" + roomList + "]");
        }
        #endregion
    }
}