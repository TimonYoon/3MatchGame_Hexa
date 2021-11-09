using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MatchManager : MonoBehaviour
{
    public List<Block> blocks => BlockManager.instance.blocks;
    public List<DirectionType[]> bunchDirections = new List<DirectionType[]>();

    private static MatchManager _instance;
    public static MatchManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MatchManager>();
                if (_instance == null)
                {
                    var obj = new GameObject(nameof(MatchManager));
                    _instance = obj.AddComponent<MatchManager>();
                }
            }
            return _instance;
        }
    }
    private void Awake()
    {
        foreach (var it in Global.NEIGHBOR_DIRECTIONS)
        {
            DirectionType[] bunchDirArray = new DirectionType[3];
            for (int i = 0; i < 3; i++)
            {
                bunchDirArray[i] = (DirectionType)(((int)it + i) % Global.NEIGHBOR_DIRECTIONS.Length + 1);
            }
            bunchDirections.Add(bunchDirArray);
        }
    }

    /// <summary>
    /// �ش� ���� ��� ����� ��ġ�� �˻��մϴ�.
    /// </summary>
    /// <param name="block">���� ���</param>
    /// <returns>��ġ��� </returns>
    public List<MatchInfo> CheckAll()
    {
        var result = new List<MatchInfo>();
        foreach (var block in BlockManager.instance.blocks)
        {
            if (IsOverlapCheck(result,block))
			{
                continue;
			}
            var blockResult = Check(block);
            if (blockResult != null)
            {
                result = result.Union(blockResult).ToList();
            }
        }
        result = MatchUtil.Distinct(result);
        return result;
    }
    
    private bool IsOverlapCheck(List<MatchInfo> matchInfoList, Block block)
	{
        bool isOverlap = false;

        foreach (var matchInfo in matchInfoList)
		{
			foreach (var coords in matchInfo.coords)
			{
                if(block.coords == coords)
				{
                    isOverlap = true;
                }
            }
		}

        return isOverlap;
	}

    /// <summary>
    /// �ش� ����� ��� ��ġ�� �˻��մϴ�.
    /// </summary>
    /// <param name="block">���� ���</param>
    /// <returns>��ġ��� </returns>
    public List<MatchInfo> Check(Block block)
    {
        var originType = block.type;
        if (Global.OBSTACLE_BLOCKS.Exists(x => x == originType))
        {
            return new List<MatchInfo>();//��ֹ� ��� �˻����
        }
        var checkInfos = CheckStraightAll(block);
        var bunchInfos = CheckBunchAll(block);
        var matchInfos = checkInfos.Union(bunchInfos).Distinct(new ListOfMatchInfoComparer()).ToList();
        return matchInfos;
    }


    //���� ��� �˻�

    /// <summary>
    /// �ش� ����� ��� ���� �˻縦 �մϴ�. (������� �� List)
    /// </summary>
    /// <param name="block">���� ���</param>
    /// <returns>��ġ��� </returns>
    private List<MatchInfo> CheckStraightAll(Block block)
    {
        var matchInfos = new List<MatchInfo>();
        BlockType originType = block.type;

        foreach (MatchDirectionType matchDir in Enum.GetValues(typeof(MatchDirectionType)))
        {
            if (matchDir == MatchDirectionType.None)
            {
                continue;
            }
            var matchInfo = CheckStraight(block, matchDir);
            if (matchInfo != null)
            {
                matchInfos = matchInfos.Union(matchInfo).ToList();
            }
        }
        return matchInfos;
    }

    /// <summary>
    /// ���� �˻縦 �ؼ� ����� ��ȯ�մϴ�. (������� Null)
    /// </summary>
    /// <param name="block">���� ���</param>
    /// <param name="matchDir">üũ ����</param>
    /// <returns> ��ġ��� </returns>
    private List<MatchInfo> CheckStraight(Block block, MatchDirectionType matchDir)
    {
        var matchInfos = new List<MatchInfo>();
        var originType = block.type;
        var firstBlock = GetFirstBlock(block, matchDir);
        var dir = MatchUtil.LookAtEnd(matchDir);
        var result = CheckDir(firstBlock, dir);
        if (result.Count == 0)
        {
            return null;
        }

        foreach (var checkBlock in result)
        {
            if(checkBlock.specialBlockType != SpecialBlockType.None)
            {
                var specialBlockDir = MatchUtil.ChangeMatchDirectionType(checkBlock.specialBlockType);
                var specialMatchInfo = CheckStraightAllType(checkBlock, specialBlockDir);
                matchInfos.Add(specialMatchInfo);
            }
        }

        if(result.Count >= 4)
		{
            foreach (var checkBlock in result)
            {
                if (block.coords == checkBlock.coords)
                {
                    result.Remove(checkBlock);
                    var specialBlockType = Global.STRAIGHT_TYPES[UnityEngine.Random.Range(0, Global.STRAIGHT_TYPES.Length)];
                    block.SetChangeSpecialBlock(specialBlockType);
                    Debug.Log($"Ư�� �� ���� specialBlockType : {specialBlockType}");
                    break;
                }
            }
        }

        var coords = new List<Vector2Int>();
        foreach (var checkBlock in result)
        {
            coords.Add(checkBlock.coords);
        }
        
        var matchInfo = new MatchInfo(originType, MatchType.Straight, matchDir, coords);
        matchInfos.Add(matchInfo);
        return matchInfos;
    }
    
    private MatchInfo CheckStraightAllType(Block block, MatchDirectionType matchDir)
    {
        var originType = block.type;
        var firstBlock = GetFirstBlock(block, matchDir, true);
        DirectionType dir = MatchUtil.LookAtEnd(matchDir);
        var result = CheckDir(firstBlock, dir,true);
        if (result.Count == 0)
        {
            return null;
        }

        List<Vector2Int> coords = new List<Vector2Int>();
        foreach (var checkBlock in result)
        {
            coords.Add(checkBlock.coords);
        }
        
        var matchInfo = new MatchInfo(originType, MatchType.Straight, matchDir, coords);
        return matchInfo;
    }


    /// <summary>
    /// ���� ��Ϻ��� �ش�������� 3�� �̻����� Ȯ���մϴ�. (������� �� List)
    /// </summary>
    /// <param name="block">���� ���</param>
    /// <param name="matchDir">����</param>
    /// <param name="isAllType">��� ��� Ÿ�� ����</param>
    /// <returns>��� List</returns>
    private List<Block> CheckDir(Block block, DirectionType dir, bool isAllType = false)
    {
        if ((int) dir > 10)
        {
            throw new Exception("�Ķ������ ��ȿ������ �ƴմϴ�.");
        }
        var result = new List<Block>();
        var originType = block.type;
        var match = 1;
        var nextBlock = block;
        result.Add(block);
        while (true)
        {
            nextBlock = BlockManager.instance.GetNeighbor(nextBlock, dir);
            if (nextBlock == null)
            {
                break;
            }
            var isMatchType = nextBlock.type == block.type || isAllType;
            if (isMatchType == false)
            {
                break;
            }
            match++;
            result.Add(nextBlock);
        }
        var isMatch = (3 <= match);
        if (isMatch == false)
        {
            result.Clear();
        }
        return result;
    }

    /// <summary>
    /// ��ġ �˻縦 ������ ù��° ����� �����ɴϴ�.
    /// </summary>
    /// <param name="block">���� ���</param>
    /// <param name="matchDir">üũ ����</param>
    /// <param name="isAllType">��� ��� Ÿ�� ����</param>
    /// <returns>ù��° ���</returns>
    private Block GetFirstBlock(Block block, MatchDirectionType matchDir, bool isAllType = false)
    {
        Block firstBlock = block;
        Block nextBlock = block;
        DirectionType dir = MatchUtil.LookAtStart(matchDir);
        while (true)
        {
            nextBlock = BlockManager.instance.GetNeighbor(nextBlock, dir);
            if (nextBlock == null)
            {
                break;
            }
            var isMatchType = nextBlock.type == block.type || isAllType;
            if (isMatchType == false)
            {
                break;
            }
            firstBlock = nextBlock;
        }
        return firstBlock;
    }
    


    //���ִ� ���(Bunch) �˻�

    /// <summary>
    /// �ش� ����� ��� ���ִ� ��� �˻縦 �մϴ�. (������� �� List)
    /// </summary>
    /// <param name="block">���� ���</param>
    /// <returns>��ġ��� </returns>
    private List<MatchInfo> CheckBunchAll(Block block)
    {
        List<MatchInfo> matchInfos = new List<MatchInfo>();
        BlockType originType = block.type;

        var neighborBlocks = BlockManager.instance.GetNeighborAll(block);
        foreach (var checkBlock in neighborBlocks)
        {
            foreach (var it in bunchDirections)
            {
                var matchBlocks = CheckBunch(originType, checkBlock, it);
                if (matchBlocks.Count != 0)
                {
                    var matchInfo = new MatchInfo(originType, MatchType.Bunch, MatchDirectionType.None, matchBlocks);
                    matchInfos.Add(matchInfo);
                }
            }
        }
        return matchInfos;
    }

    private List<Vector2Int> CheckBunch(BlockType originType, Block block, DirectionType[] dirArray)
    {
        var matchBlocks = new List<Vector2Int>();
        if (block.type != originType) return matchBlocks;
        matchBlocks.Add(block.coords);
        var nextBlock = block;
        for (int i = 0; i < dirArray.Length; i++)
        {
            nextBlock = BlockManager.instance.GetNeighbor(block, dirArray[i]);
            if (nextBlock == null || nextBlock.type != originType)
            {
                matchBlocks.Clear();
                return matchBlocks;
            }
            matchBlocks.Add(nextBlock.coords);
        }
        return matchBlocks;
    }



}
