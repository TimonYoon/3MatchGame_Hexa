using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// -Todo. �ؾߵɰ�-
/// 1.Gravity ����
/// 2.���� Map ������ Match���� �� ���� 
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Block selectBlock;
    [SerializeField]
    private List<MatchInfo> curMatchInfos = new List<MatchInfo>();
    [SerializeField]
    private bool isReady = true;

    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    var obj = new GameObject(nameof(GameManager));
                    _instance = obj.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    private void Update()
    {
        if (!isReady)
        {
            return;
        }

        if (selectBlock != null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var mousePos = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hitBlock = Physics2D.Raycast(mousePos, Vector2.zero);


            if (hitBlock.collider != null && hitBlock.collider.gameObject.GetComponent<Block>() != null)
            {
                selectBlock = hitBlock.collider.gameObject.GetComponent<Block>();
                StartCoroutine(CoWaitDrag(selectBlock.transform.position));
            }
        }
    }

    IEnumerator CoWaitDrag(Vector3 initPos)
    {
        isReady = false;
        while (true)
        {
            if (Input.GetMouseButtonUp(0))
            {
                break;
            }
            var mousePos = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var distance = Vector3.Distance(initPos, mousePos);
            if (distance > Global.DRAG_DISTANCE)
            {
                //�巡�� �Ϸ�
                var dir = BoardUtil.GetDirection(initPos, mousePos);
                Block changeBlock = BlockManager.instance.GetNeighbor(selectBlock, dir);
                if (changeBlock == null)
                {
                    break;
                }
                //��� ����
                yield return StartCoroutine(BlockManager.instance.CoSwapBlock(selectBlock, changeBlock));
                var selectInfos = MatchManager.instance.Check(selectBlock);
                var targetInfos = MatchManager.instance.Check(changeBlock);
                curMatchInfos = MatchUtil.Distinct(selectInfos.Union(targetInfos).ToList());
                //���� ���� (Undo)
                if (curMatchInfos.Count == 0)
                {
                    Debug.Log("Swap Fail");
                    yield return StartCoroutine(BlockManager.instance.CoUndoSwap());
                    break;
                }
                //�߷� ���� �� �� ���� (�߰� Match�� ���������� �ݺ�)
                while (true)
                {
                    if (curMatchInfos.Count == 0)
                    {
                        break;
                    }

                    if (curMatchInfos.Count >= 1)
                    {
                        foreach (var MatchInfo in curMatchInfos)
                        {
                            var coordsText = MatchInfo.blockType.ToString();
                            foreach (var coord in MatchInfo.coords)
                            {
                                coordsText += $"{coord} / ";
                            }
                            Debug.Log(coordsText);
                        }
                    }

                    var destoryBlockCoords = MatchUtil.GetCoordsAll(curMatchInfos);
                    foreach (var coords in destoryBlockCoords)
                    {
                        var block = BlockManager.instance.GetBlock(coords);
                        block.SetDestroyCheck();
                    }
                    yield return new WaitForSeconds(0.5f);
                    BlockManager.instance.DestoryBlocks(MatchUtil.GetCoordsAll(curMatchInfos));
                    yield return new WaitForSeconds(0.5f);
                    yield return StartCoroutine(BlockManager.instance.CoApplyGravityAndGenerateMap());
                    curMatchInfos = MatchManager.instance.CheckAll();
                }
                break;
            }
            yield return null;
        }
        if (BlockManager.instance.totalTopCount == 0)
        {
            Debug.Log("Win!");
        }

        //�ʱ�ȭ
        selectBlock = null;
        isReady = true;
    }
}
