using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI topCountText;
    [SerializeField] private GameObject clearPanel;

    public Transform topCountTextPos => topCountText.transform;
    
    private static UIManager _instance;
    public static UIManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
                if (_instance == null)
                {
                    var obj = new GameObject(nameof(UIManager));
                    _instance = obj.AddComponent<UIManager>();
                }
            }
            return _instance;
        }
    }

    void Update()
    {
        int topCount = BlockManager.instance.totalTopCount;
        topCountText.text = topCount.ToString();
        clearPanel.SetActive(topCount == 0);
    }

    public void OnClickResetButton()
    {
        BlockManager.instance.Restart();
    }

}
