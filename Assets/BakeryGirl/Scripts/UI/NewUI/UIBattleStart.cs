using UnityEngine;
using UnityEngine.UI;

public class UIBattleStart : MonoBehaviour {
    public Text countdownLabel;
    public float countdownTime = 3f;

    private float m_countdown;

	public void Show() {
        m_countdown = countdownTime;
        gameObject.SetActive(true);
	}
	
	void Update () {
        m_countdown -= Time.deltaTime;
        countdownLabel.text = ((int)Mathf.Ceil(m_countdown)).ToString();
        if (m_countdown <= 0) {
            gameObject.SetActive(false);
        }
	}
}
