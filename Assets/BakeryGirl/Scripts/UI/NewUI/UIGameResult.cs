using UnityEngine;
using UnityEngine.UI;
using System;

public class UIGameResult : MonoBehaviour {
    public Text player0Name;
    public Text player1Name;
    public Image player0Result;
    public Image player0Flag;
    public Image player1Flag;
    public Image player1Result;
    public Text turnNum;

    public Sprite winSprite;
    public Sprite loseSprite;

    public void OnGameOver(string player0, string player1, int turn, Ruler.GameResult result) {
        player0Name.text = player0;
        player1Name.text = player1;
        turnNum.text = turn.ToString("D2");
        bool player0Highlight = false;
        bool player1Highlight = false;
        switch (result) {
            case Ruler.GameResult.Black_Win:
                player0Highlight = true;
                break;
            case Ruler.GameResult.White_Win:
                player1Highlight = true;
                break;
            default:
                break;
        }
        player0Name.color = player0Highlight ? StorageInfo.Orange : Color.white;
        player0Flag.color = player0Highlight ? StorageInfo.Orange : Color.white;
        player0Result.sprite = player0Highlight ? winSprite : loseSprite;
        player0Result.SetNativeSize();
        player1Name.color = player1Highlight ? StorageInfo.Orange : Color.white;
        player1Flag.color = player1Highlight ? StorageInfo.Orange : Color.white;
        player1Result.sprite = player1Highlight ? winSprite : loseSprite;
        player1Result.SetNativeSize();
    }
}
