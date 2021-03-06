using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// A utility class to make sprite spark over time
/// </summary>
public class sprite_spark : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Reset();
	}

    bool isUp = false;
    public float sparkBegin = 0.6f;
    public float sparkEnd = 1f;
    public float resetAlpha = 1;
    public float nowAlpha = 1;
    public float speed = 1f;

    public bool isSparkAlpha = false;

    public bool working = true;

    public enum WorkingType
    {
        UISprite, Sprite
    }

    public WorkingType workType = WorkingType.Sprite;

    bool initialized = false;
    public Color initColor;

    public void ResetInitColor()
    {
        if (workType == WorkingType.UISprite)
            initColor = GetComponent<Image>().color;
        else
            initColor = GetComponent<SpriteRenderer>().color;
        initialized = true;
    }

    public void Reset()
    {
        if (!initialized)
            ResetInitColor();
        nowAlpha = resetAlpha;
        UpdateColor();

        if (nowAlpha > sparkBegin)
            isUp = false;
        else
            isUp = true;

    }

    void UpdateColor()
    {
        if (workType == WorkingType.UISprite)
        {
            Button button = GetComponent<Button>();
            Image sprite = GetComponent<Image>();
            if (!button.interactable) {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1.0f);
            }
            else {
                if (isSparkAlpha)
                    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, nowAlpha);
                else
                    sprite.color = new Color(initColor.r * nowAlpha, initColor.g * nowAlpha, initColor.b * nowAlpha, initColor.a);
            }
        }
        else
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (isSparkAlpha)
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, nowAlpha);
            else
                sprite.color = new Color(initColor.r * nowAlpha, initColor.g * nowAlpha, initColor.b * nowAlpha, initColor.a);
        }
        
    }

	// Update is called once per frame
	void Update () {
        if (!working) return;

        if (isUp)
        {
            nowAlpha += Time.deltaTime * speed;
            if (nowAlpha > sparkEnd)
            {
                nowAlpha = sparkEnd;
                isUp = false;
            }
        }
        else if (!isUp)
        {
            nowAlpha -= Time.deltaTime * speed;
            if (nowAlpha < sparkBegin)
            {
                nowAlpha = sparkBegin;
                isUp = true;
            }
        }
        UpdateColor();
	}

}
