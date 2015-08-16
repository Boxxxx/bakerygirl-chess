using UnityEngine;
using FullInspector;
using System.Collections.Generic;

public class ArtManager : BaseBehavior {
    private static ArtManager _instance;
    public static ArtManager Instance {
        get { return _instance; }
    }

    public Sprite blackFlag;
    public Sprite whiteFlag;
    public Dictionary<string, Card> battleCards;
    public Dictionary<string, Sprite> storageCards;
    public Dictionary<string, Sprite> characterImages;

    public ArtManager() {
        _instance = this;
    }

    public Card GetBattleCardGraphics(Unit.TypeEnum type, Unit.OwnerEnum owner) {
        string spriteName = GetCardName(type, owner);
        return battleCards.ContainsKey(spriteName) ? battleCards[spriteName] : null;
    }

    public Card GetBattleCardGraphicsByName(string name) {
        return battleCards.ContainsKey(name) ? battleCards[name] : null;
    }

    public Sprite GetCharacterImage(string name) {
        return characterImages.ContainsKey(name) ? characterImages[name] : null;
    }

    public Sprite GetStorageCardSprite(Unit.TypeEnum type, Unit.OwnerEnum owner) {
        var name = GetCardName(type, owner);
        return storageCards.ContainsKey(name) ? storageCards[name] : null;
    }

    public Sprite GetFlagSprite(Unit.OwnerEnum owner) {
        if (owner == Unit.OwnerEnum.Black) {
            return blackFlag;
        }
        else if (owner == Unit.OwnerEnum.White) {
            return whiteFlag;
        }
        else {
            return null;
        }
    }

    public static string GetCardName(Unit.TypeEnum type, Unit.OwnerEnum owner) {
        string spriteName;
        if (type == Unit.TypeEnum.Bread)
            spriteName = "bread";
        else if (type == Unit.TypeEnum.Void)
            spriteName = "void";
        else if (type == Unit.TypeEnum.Tile)
            spriteName = "tile";
        else
            spriteName = type.ToString().ToLower() + ((int)owner).ToString();
        return spriteName;
    }
}
