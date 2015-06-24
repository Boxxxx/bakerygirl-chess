using UnityEngine;
using System.Collections;

public class ResultSprite : MonoBehaviour
{
    public void OnGameOver(Ruler.GameResult result)
    {
        if (result == Ruler.GameResult.Black_Win)
            GetComponent<UISprite>().spriteName = "black_win";
        else if (result == Ruler.GameResult.White_Win)
            GetComponent<UISprite>().spriteName = "white_win";
        else
            GetComponent<UISprite>().spriteName = "draw";
    }
}
