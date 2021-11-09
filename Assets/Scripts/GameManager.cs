using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// -Todo. 해야될것-
/// 1.Gravity 적용
/// 2.최초 Map 생성시 Match없는 맵 생성 
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
                //드래그 완료
                var dir = BoardUtil.GetDirection(initPos, mousePos);
                Block changeBlock = BlockManager.instance.GetNeighbor(selectBlock, dir);
                if (changeBlock == null)
                {
                    break;
                }
                //블록 스왑
                yield return StartCoroutine(BlockManager.instance.CoSwapBlock(selectBlock, changeBlock));
                var selectInfos = MatchManager.instance.Check(selectBlock);
                var targetInfos = MatchManager.instance.Check(changeBlock);
                curMatchInfos = MatchUtil.Distinct(selectInfos.Union(targetInfos).ToList());
                //스왑 실패 (Undo)
                if (curMatchInfos.Count == 0)
                {
                    Debug.Log("Swap Fail");
                    yield return StartCoroutine(BlockManager.instance.CoUndoSwap());
                    break;
                }
                //중력 적용 후 맵 생성 (추가 Match가 없을때까지 반복)
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

        //초기화
        selectBlock = null;
        isReady = true;
    }
}
