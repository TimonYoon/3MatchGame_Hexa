using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global
{
    public static readonly int TOTAL_COUNT = 30;
    public static readonly Vector2Int MAX_SIZE = new Vector2Int(7, 11);
    public static readonly DirectionType[] NEIGHBOR_DIRECTIONS = new DirectionType[6] { DirectionType.Up, DirectionType.RightUp, DirectionType.RightDown, DirectionType.Down, DirectionType.LeftDown, DirectionType.LeftUp };
    public static readonly List<BlockType> OBSTACLE_BLOCKS = new List<BlockType>() { BlockType.Top };
    public static readonly SpecialBlockType[] STRAIGHT_TYPES = new SpecialBlockType[3] { SpecialBlockType.Straight_Vertical, SpecialBlockType.Straight_ForwardSlash, SpecialBlockType.Straight_BackSlash };
    public const float DRAG_DISTANCE = 0.5f;
    public const float BLOCK_SWAP_SPEED = 8f;
    public const float BLOCK_DROP_SPEED = 12f;
    public const float TOP_BLOCK_DESTROY_SPEED = 100f;
    public const float MATCH_CHECK_DELAY = 1.5f;
}


public enum BlockType
{
    None = 0,
    Blue = 1,
    Green = 2,
    Orange = 3,
    Purple = 4,
    Red = 5,
    Yellow = 6,
    Top = 7,
}

public enum SpecialBlockType
{
    None = 0,
    Straight_Vertical = 1,
    Straight_ForwardSlash = 2,
    Straight_BackSlash = 3,
    Boomerang = 4,
}

public enum DirectionType
{
    None = 0,
    Up = 1,
    RightUp = 2,
    RightDown = 3,
    Down = 4,
    LeftDown = 5,
    LeftUp = 6,
    //이웃보다 한칸더 떨어진 방향 (각 요소를 제곱해서 더한후 곱하기10)
    RightUpOffset = 50,
    RightOffset = 130,
    RightDownOffset = 250,
    LeftDownOffset = 410,
    LeftOffset = 610,
    LeftUpOffset = 370,
}
public enum MatchType
{
    None = 0,
    Straight = 1,
    Bunch = 2,
}
public enum MatchDirectionType
{
    None = 0,
    Vertical = 1,
    /// <summary>
    /// 밑에서 위로 (/)
    /// </summary>
    ForwardSlash = 2,
    /// <summary>
    /// 위에서 밑으로 (\)
    /// </summary>
    BackSlash = 3,
}

