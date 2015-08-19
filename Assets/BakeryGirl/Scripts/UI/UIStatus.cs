using UnityEngine;
using UnityEngine.UI;

public class UIStatus : MonoBehaviour {
    public const int dotNum = 3;
    public enum StateEnum { DisplayText, DisplayTime, DisplayDot };

    public Text turnNum;
    public Text logLabel;
    public Text timeLabel;
    public RectTransform panelLog;
    public RectTransform panelTimecost;

    public float waitingDotTime = 0.25f;
    public string actionText = "行动中";

    private string m_displayText;
    private float m_costTime;
    private float m_nowTime;
    private int m_nowDotNum;
    
    private StateEnum m_state = StateEnum.DisplayText;

	public void SetTurn(int turn) {
        turnNum.text = turn.ToString();
    }

    public void ShowAction() {
        ShowDot(actionText);
    }

    public void ShowText(string text) {
        m_state = StateEnum.DisplayText;
        m_displayText = text;
        panelTimecost.gameObject.SetActive(false);
        panelLog.gameObject.SetActive(true);
    }

    public void ShowDot(string text) {
        m_state = StateEnum.DisplayDot;
        m_displayText = text;
        panelTimecost.gameObject.SetActive(false);
        panelLog.gameObject.SetActive(true);
    }

    public void ShowTimecost(float time) {
        m_state = StateEnum.DisplayTime;
        m_costTime = time;
        panelTimecost.gameObject.SetActive(true);
        panelLog.gameObject.SetActive(false);
    }

    void Awake() {
        m_nowTime = 0;
        m_nowDotNum = 0;
    }

    void Update() {
        if (m_state == StateEnum.DisplayDot) {
            string append = "";
            m_nowTime += Time.deltaTime;
            if (m_nowTime > waitingDotTime) {
                m_nowTime = 0;
                m_nowDotNum = (m_nowDotNum + 1) % (dotNum + 1);
            }
            for (int i = 0; i < m_nowDotNum; i++)
                append += '.';
            logLabel.text = "//" + m_displayText + append;
        }
        else if (m_state == StateEnum.DisplayTime) {
            timeLabel.text = string.Format("{0}", ((int)m_costTime).ToString("D2"));
        }
        else {
            logLabel.text = m_displayText;
        }
    }

}
