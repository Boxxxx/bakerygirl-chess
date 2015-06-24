using UnityEngine;
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
        UISprite, tk2dSprite
    }

    public WorkingType workType = WorkingType.tk2dSprite;

    bool initialized = false;
    public Color initColor;

    public void ResetInitColor()
    {
        if (workType == WorkingType.UISprite)
            initColor = GetComponent<UISprite>().color;
        else
            initColor = GetComponent<tk2dSprite>().color;
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
            UISprite sprite = GetComponent<UISprite>();
            if (isSparkAlpha)
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, nowAlpha);
            else
                sprite.color = new Color(initColor.r * nowAlpha, initColor.g * nowAlpha, initColor.b * nowAlpha, initColor.a);
        }
        else
        {
            tk2dSprite sprite = GetComponent<tk2dSprite>();
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
