using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A utility class to show number with several single number sprites
/// </summary>
public class NumberBar : MonoBehaviour
{
    #region Variables
    public string[] spriteIdList;
    public int defaultLength = 1;
    public GameObject emptySprite;

    private int length = 1;
    private int number = 0;
    private List<int> numberList = new List<int>();
    private List<tk2dSprite> sprites = new List<tk2dSprite>();
    #endregion

    /// <summary>
    /// The number to show
    /// </summary>
    public int Number
    {
        get {return number;}
        set {
            number = value;
            length = 0;
            Clear();
            while (number > 0)
            {
                numberList.Add(number % 10);
                number /= 10;
                length++;
            }
            while (length < defaultLength)
            {
                numberList.Add(0);
                length++;
            }
            numberList.Reverse();
            float offset = 0;
            for (int i = 0; i < length; i++)
            {
                GameObject single = GameObject.Instantiate(emptySprite) as GameObject;
                tk2dSprite sprite = single.GetComponent<tk2dSprite>();
                sprites.Add(sprite);

                single.transform.parent = transform;
                single.GetComponent<tk2dSprite>().spriteId = sprite.GetSpriteIdByName(spriteIdList[numberList[i]]);
                single.transform.localPosition = new Vector3(offset, 0, 0);
                offset += single.collider.bounds.size.x;
            }
        }
    }
    void Clear()
    {
        foreach (tk2dSprite sprite in sprites)
            GameObject.Destroy(sprite);
        sprites.Clear();
        numberList.Clear();
    }

    #region Unity Callback Functions
    void Start()
    {
        Number = 0;
    }
    #endregion
}
