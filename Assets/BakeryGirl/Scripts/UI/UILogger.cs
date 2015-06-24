using UnityEngine;
using System.Collections;

/// <summary>
/// To show info of gameplay (mainly used by AI player)
/// </summary>
public class UILogger : MonoBehaviour {
    // Normal state is to simplely show text itself
    // Dot state is to show some ... after text and change its number over deltatime
    public enum StateEnum{Normal, Dot};

    UILabel label;
    StateEnum state = StateEnum.Normal;
    public float deltaTime;
    public int dotNum = 3;
    private float nowTime;
    private int nowDotNum;
    private string text;

    public StateEnum State
    {
        get { return state; }
        set { state = value; }
    }
    public string Text
    {
        get { return text; }
        set { text = value; }
    }

    void Start () {
        label = GetComponent<UILabel>();
        nowTime = 0;
        nowDotNum = 0;
	}
	
	void Update () {
        string append = "";
        if (state == StateEnum.Dot)
        {
            nowTime += Time.deltaTime;
            if (nowTime > deltaTime)
            {
                nowTime = 0;
                nowDotNum = (nowDotNum + 1) % (dotNum+1);
            }
            for (int i = 0; i < nowDotNum; i++)
                append += '.';
        }
        label.text = text + append;
	}
}
