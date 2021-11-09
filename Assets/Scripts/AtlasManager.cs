using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasManager : MonoBehaviour
{
    private SpriteAtlas blockAtlas;
    
    private static AtlasManager _instance;
    public static AtlasManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AtlasManager>();
                if (_instance == null)
                {
                    var obj = new GameObject(nameof(AtlasManager));
                    _instance = obj.AddComponent<AtlasManager>();
                }
            }
            return _instance;
        }
    }

    public Sprite GetBlockSprite(string spriteName)
    {
        if (blockAtlas == null)
        {
            blockAtlas = Resources.Load<SpriteAtlas>("Block_Atlas");
        }
        return blockAtlas.GetSprite(spriteName);
    }
}
