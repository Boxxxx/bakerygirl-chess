using UnityEngine;
using UnityEngine.UI;

public class UIBattleStart : MonoBehaviour {
    public Text countdownLabel;
    public float countdownTime = 3f;

    private float m_countdown;
    private int m_lasttick;

	public void Show() {
        m_countdown = countdownTime;
        // Set lastTick to countDown + 1, so that it will immediately play countdown sound effect in Update.
        m_lasttick = (int)Mathf.Ceil(m_lasttick) + 1;
        gameObject.SetActive(true);
    }
	
	void Update () {
        m_countdown -= Time.deltaTime;
        int currenttick = (int)Mathf.Ceil(m_countdown);
        countdownLabel.text = currenttick.ToString();

        if (m_countdown <= 0) {
            gameObject.SetActive(false);
        }
        else if (currenttick != m_lasttick) {
            if (currenttick > 1) {
                SoundManager.Instance.PlaySound("countdown_normal");
            }
            else {
                SoundManager.Instance.PlaySound("countdown_timeup");
            }
        }
        m_lasttick = currenttick;
	}
}
