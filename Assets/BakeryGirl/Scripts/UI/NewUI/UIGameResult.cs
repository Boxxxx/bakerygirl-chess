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

    public RectTransform infoPanel;

    public Sprite winSprite;
    public Sprite loseSprite;

    public float moveInTime = 0.5f;

    public void OnGameOver(UIStorage storage, int turn, Ruler.GameResult result) {
        player0Name.text = storage.playerDown.playerName.text;
        player1Name.text = storage.playerUp.playerName.text;
        player0Flag.sprite = storage.playerDown.flag.sprite;
        player0Flag.SetNativeSize();
        player1Flag.sprite = storage.playerUp.flag.sprite;
        player1Flag.SetNativeSize();
        turnNum.text = turn.ToString("D2");
        bool player0Highlight = 
            (result == Ruler.GameResult.Black_Win && !GameInfo.Instance.ShouldUpsidedown)
            || (result == Ruler.GameResult.White_Win && GameInfo.Instance.ShouldUpsidedown);
        bool player1Highlight = (result == Ruler.GameResult.White_Win && !GameInfo.Instance.ShouldUpsidedown)
            || (result == Ruler.GameResult.Black_Win && GameInfo.Instance.ShouldUpsidedown);
        player0Name.color = player0Highlight ? StorageInfo.Orange : Color.white;
        player0Flag.color = player0Highlight ? StorageInfo.Orange : Color.white;
        player0Result.sprite = player0Highlight ? winSprite : loseSprite;
        player0Result.SetNativeSize();
        player1Name.color = player1Highlight ? StorageInfo.Orange : Color.white;
        player1Flag.color = player1Highlight ? StorageInfo.Orange : Color.white;
        player1Result.sprite = player1Highlight ? winSprite : loseSprite;
        player1Result.SetNativeSize();

        infoPanel.localPosition = new Vector3(infoPanel.rect.width, 0, 0);
        iTween.MoveTo(infoPanel.gameObject, iTween.Hash("position", Vector3.zero, "time", moveInTime, "easetype", iTween.EaseType.easeOutBounce, "islocal", true));
    }
}
