using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;

namespace BakeryGirl.Chess {
    public class MultiplayerUI : MonoBehaviour {
        private const string kUserNamePlayerPref = "NamePickUserName";
        private const string kDefaultUserName = "Jefuty";

        public enum EntryState {
            None, MainEntry, Connecting, WaitingForOpponent, GamePlay
        };

        public UISprite background;

        public UIPanel mainEntryPanel;
        public UIInput nicknameLabel;
        public UIInput roomPwdLabel;
        public NGUIButtonHandler randomMatchButton;

        public UIPanel connectingPanel;
        public UILabel connectingLabel;
        public NGUIButtonHandler exitConnectingBUtton;

        public UIPanel waitingPanel;
        public NGUIButtonHandler exitWaitingButton;

        public GameObject[] mainGameComponents = {};

        private EntryState _state = EntryState.None;

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
                            GameClientAgent.ClientInstance.Disconnect();
                            mainEntryPanel.gameObject.SetActive(true);
                            background.gameObject.SetActive(true);
                            break;
                        case EntryState.Connecting:
                            connectingPanel.gameObject.SetActive(true);
                            background.gameObject.SetActive(true);
                            break;
                        case EntryState.WaitingForOpponent:
                            waitingPanel.gameObject.SetActive(true);
                            background.gameObject.SetActive(true);
                            break;
                        case EntryState.GamePlay:
                            foreach (var obj in mainGameComponents) {
                                obj.gameObject.SetActive(true);
                                background.gameObject.SetActive(false);
                            }
                            break;
                    }
                    OnSwitchState(value, oldState);
                }
            }
        }

        #region Unity Callbakcs
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
                connectingLabel.text = GameClientAgent.Instance.Client.State.ToString();
            }
        }

        void OnGUI() {
            //if (GUI.Button(new Rect(50, 300, 100, 50), "NextTurn"))
            //{
            //    Application.LoadLevel(Application.loadedLevel);
            //}
        }

        void RegisterButtonCBs() {
            randomMatchButton.clickedHandle += (data, index) => {
                PlayerPrefs.SetString(kUserNamePlayerPref, Nickname);
                GameClientAgent.Instance.JoinGame(Nickname);
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
            GameClientAgent.ClientInstance.onDisconnect += OnDisconnect;
            GameClientAgent.ClientInstance.onJoinLobby += OnJoinLobby;
            GameClientAgent.ClientInstance.onJoinRoom += OnJoinRoom;
            GameClientAgent.ClientInstance.onCreateRoom += OnCreateRoom;
            GameClientAgent.ClientInstance.onCloseRoom += OnCloseRoom;
            GameClientAgent.ClientInstance.onGameStart += OnGameStart;
            GameClientAgent.ClientInstance.onError += OnError;
        }

        void ReloadLevel() {
            Application.LoadLevel(Application.loadedLevel);
        }
        #endregion

        #region Events
        void OnSwitchState(EntryState newState, EntryState oldState) { }

        void OnDisconnect(DisconnectCause disconnectCause) {
            if (disconnectCause == DisconnectCause.None) {
                return;
            }
            ErrorPanel.Show("Disonnected", () => {
                ReloadLevel();
            });
        }

        void OnJoinLobby(TypedLobby lobby) {
            if (CurrentState != EntryState.Connecting) {
                // If it is not in connecting state, return
                return;
            }
            OutputRoomList();
            GameClientAgent.ClientInstance.JoinRandomRoom(RoomPwd);
        }

        void OnJoinRoom(Room room) {
            Debug.Log("joined game " + room.Name);
            CurrentState = EntryState.WaitingForOpponent;
        }

        void OnCreateRoom() {
            GameClientAgent.ClientInstance.CreateTurnbasedRoom(RoomPwd);
        }

        void OnCloseRoom() {
            ErrorPanel.Show("Player has left", () => {
                ReloadLevel();
            });
        }

        void OnGameStart() {
            Debug.Log("game start");
            CurrentState = EntryState.GamePlay;
        }

        void OnError(Consts.ErrorCode err) {
            switch (err) {
                case Consts.ErrorCode.NotCompatible:
                    Debug.Log("game data not compatible");
                    ErrorPanel.Show("Not compatible", () => {
                        ReloadLevel();
                    });
                    break;
            }
        }
        #endregion

        #region Debug
        void OutputRoomList() {
            string roomList = "";
            bool isFirst = true;
            foreach (var roomInfo in GameClientAgent.ClientInstance.RoomInfoList) {
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