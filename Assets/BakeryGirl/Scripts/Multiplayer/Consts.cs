using UnityEngine;
using System.Collections;

namespace BakeryGirl.Chess {
    public static class Consts {
        public static class PropNames {
            public const string RoomPwd = "pwd";
            public const string PlayerIdToMakeThisTurn = "pt";
            public const string TurnNum = "t#";
            public const string Players = "players";
            public const string IsStart = "start";

            public static string GetPlayerResourceKey(int id) {
                return string.Format("res{0}", id);
            }
            public static string GetBoardSlotTypeKey(int r, int c) {
                return string.Format("bt{0},{1}", r, c);
            }
            public static string GetBoardSlotOwnerKey(int r, int c) {
                return string.Format("bo{0},{1}", r, c);
            }
        }

        public static class EventCode {
            public const byte OnMove = 1;
        }

        public enum ErrorCode {
            NotCompatible
        }
    }
}