using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BlockManager : MonoBehaviour
{
    public int totalTopCount => remainTopCount + blocks.FindAll(x => x.type == BlockType.Top).Count;
    public int remainTopCount = 10;
    public List<Block> prefabs = new List<Block>();
    public List<Block> blocks = new List<Block>();

    private Block[] backUpBlock = new Block[2];

    private static BlockManager _instance;

    public static BlockManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BlockManager>();
                if (_instance == null)
                {
                    var obj = new GameObject(nameof(BlockManager));
                    _instance = obj.AddComponent<BlockManager>();
                }
            }

            return _instance;
        }
    }

    private void Start()
    {
        while (true)
        {
            CreateRandomMap();
            var matchInfo = MatchManager.instance.CheckAll();
            if (matchInfo.Count == 0)
            {
                break;
            }

            ClearMap();
        }
    }

    public void Restart()
    {
        ClearMap();
        Start();
    }

    public void CreateRandomMap()
    {
        Debug.Log("�ʻ���");
        remainTopCount = 10;
        foreach (var it in BoardManager.instance.boards)
        {
            CreateRandomBlock(it.coords);
        }
    }

    public void ClearMap()
    {
        Debug.Log("�ʻ���");
        var objs = blocks.ConvertAll(x => x.gameObject);
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }

        blocks.Clear();
    }

    public bool ExistsBlock(Vector2Int coords) { return blocks.Exists(x => x.coords == coords); }
    public Block GetBlock(Vector2Int coords) { return blocks.Find(x => x.coords == coords); }
    public Block GetBlock(int x, int y) { return blocks.Find(xx => xx.coords == new Vector2Int(x, y)); }

    public Block GetNeighbor(Vector2Int coords, DirectionType dir)
    {
        return blocks.Find(x => x.coords == BoardUtil.GetNeighbor(coords, dir));
    }

    public Block GetNeighbor(Block origin, DirectionType dir)
    {
        return blocks.Find(x => x.coords == BoardUtil.GetNeighbor(origin.coords, dir));
    }

    public IEnumerable<Block> GetNeighborAll(Block origin)
    {
        return BoardUtil.GetNeighborAll(origin.coords).Select(x => GetBlock(x)).Where(x => x != null);
    }

    public void DestoryBlocks(List<Vector2Int> targetCoords)
    {
        Debug.Log($"Pang!");
        TryDestroyObstacle(targetCoords);

        //Debug.Log($"block count {blocks.Count}  targetCoords.Count {targetCoords.Count}");
        for (int i = 0; i < targetCoords.Count; i++)
        {
            Vector2Int coords = targetCoords[i];
            var targetBlock = GetBlock(coords);
            if (targetBlock == null)
            {
                continue;
            }
            targetBlock.TryDestroy();
        }
    }


    public IEnumerator CoSwapBlock(Block blockA, Block blockB)
    {
        if (blockA.coords == blockB.coords)
        {
            throw new Exception("���� ��� ���� �Ұ�");
        }

        Debug.Log($"Swap ({blockA.coords} , {blockB.coords})");
        backUpBlock[0] = blockA;
        backUpBlock[1] = blockB;
        //�̵�
        blockA.ToGoal(BoardUtil.GetPosition(blockB.coords), Global.BLOCK_SWAP_SPEED);
        blockB.ToGoal(BoardUtil.GetPosition(blockA.coords), Global.BLOCK_SWAP_SPEED);
        yield return new WaitUntil(() => !blockA.IsMoving() && !blockB.IsMoving());
        //���� ��ǥ����
        Vector2Int temp = blockA.coords;
        blockA.SetCoords(blockB.coords);
        blockB.SetCoords(temp);
    }

    public IEnumerator CoUndoSwap()
    {
        if (backUpBlock[0] == null || backUpBlock[1] == null)
        {
            yield break;
        }
        Debug.Log("Swap Undo");
        yield return StartCoroutine(CoSwapBlock(backUpBlock[0], backUpBlock[1]));
        backUpBlock[0] = null;
        backUpBlock[1] = null;
    }

    public IEnumerator CoApplyGravityAndGenerateMap()
    {
        while (true)
        {
            //��ü �߷�����
            var emptyBoards = BoardManager.instance.boards.FindAll(x => x.isEmpty());
            if (emptyBoards.Count == 0)
            {
                break;
            }

            bool isFillExists = false;

            foreach (var emptyBoard in emptyBoards)
            {
                var fillBlock = emptyBoard.FindFillBlock();
                if (fillBlock != null)
                {
                    isFillExists = true;
                    fillBlock.ChangeCoordsAndMove(emptyBoard.coords, Global.BLOCK_DROP_SPEED);
                }
            }

            if (!isFillExists)
            {
                break;
            }
        }

        while (true)
        {
            //��� ������ ��ġ
            var spawnBlock = SpawnRandomBlock();
            if (blocks.Count == Global.TOTAL_COUNT)
            {
                break;
            }

            var emptyCoords = FindEmpty();
            spawnBlock.ChangeCoordsAndMove(emptyCoords, Global.BLOCK_DROP_SPEED);
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// ���� ��ġ���� ����� �����մϴ�.
    /// </summary>
    /// <param name="blockPrefab">������ ������</param>
    /// <param name="coords">��ǥ</param>
    private Block SpawnRandomBlock()
    {
        var randPrefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
        if (randPrefab.type == BlockType.Top)
        {
            if (remainTopCount > 0)
            {
                remainTopCount--;
            }
            else
            {
                return SpawnRandomBlock();
            }
        }

        Vector2Int spawnCoords = new Vector2Int(Global.MAX_SIZE.x / 2, Global.MAX_SIZE.y - 1);
        if (blocks.Exists(x => x.coords == spawnCoords))
        {
            throw new Exception("���� ��ġ�� ����� ����");
        }

        var block = Instantiate(randPrefab, transform, false);
        block.SetCoords(spawnCoords);
        block.transform.position = BoardUtil.GetPosition(spawnCoords) + new Vector3(0f, 3f, 0f);
        block.ToGoal(BoardUtil.GetPosition(spawnCoords), Global.BLOCK_DROP_SPEED);
        blocks.Add(block);
        return block;
    }

    /// <summary>
    /// �ش� ��ǥ�� ����� ��� �����մϴ�.
    /// </summary>
    /// <param name="blockPrefab">������ ������</param>
    /// <param name="coords">��ǥ</param>
    private Block CreateBlock(Block blockPrefab, Vector2Int coords)
    {
        if (!BoardManager.instance.IsEnable(coords))
        {
            throw new Exception("��ȿ���� �ʰ�");
        }

        if (blocks.Exists(x => x.coords == coords))
        {
            throw new Exception("���� ��ġ�� ����� ����");
        }

        var block = Instantiate(blockPrefab, transform, true);
        block.SetCoords(coords);
        block.transform.position = BoardUtil.GetPosition(block.coords);
        blocks.Add(block);
        return block;
    }

    /// <summary>
    /// �ش� ��ǥ�� ����� �������� �����մϴ�.
    /// </summary>
    /// <param name="coords">��ǥ</param>
    private Block CreateRandomBlock(Vector2Int coords)
    {
        var randPrefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
        if (randPrefab.type == BlockType.Top)
        {
            if (remainTopCount > 0)
            {
                remainTopCount--;
            }
            else
            {
                return CreateRandomBlock(coords);
            }
        }

        return CreateBlock(randPrefab, coords);
    }

    /// <summary>
    /// ��ֹ��� �����մϴ�.
    /// </summary>
    private void TryDestroyObstacle(List<Vector2Int> targetCoords)
    {
        var obstacles = new List<IObstacle>();
        foreach (var targetCoord in targetCoords)
        {
            foreach (var coords in BoardUtil.GetNeighborAll(targetCoord))
            {
                var block = GetBlock(coords);
                if (block != null && block is IObstacle)
                {
                    var obstacle = block as IObstacle;
                    if (!obstacles.Exists(x => x == obstacle))
                    {
                        obstacles.Add(obstacle);
                    }
                }
            }
        }

        foreach (var obstacle in obstacles)
        {
            (obstacle as Block)?.TryDestroy();
        }
    }

    /// <summary>
    /// ����ִ� �� ��ǥ ã�� (Y�� �������� �켱����,Y�� ������� X�� Center�� �������� �켱����)
    /// </summary>
    /// <returns></returns>
    private Vector2Int FindEmpty()
    {
        var coordList = BoardManager.instance.boards.Where(x => GetBlock(x.coords) == null).Select(x => x.coords).ToList();
        coordList.Sort((c1, c2) => (10 * c1.y + (Mathf.Abs(Global.MAX_SIZE.x / 2) - c1.x)) -
                                   (10 * c2.y + (Mathf.Abs(Global.MAX_SIZE.x / 2) - c2.x)));
        return coordList[0];
    }
}