using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MatchUtil
{
    public static DirectionType LookAtStart(MatchDirectionType matchDir)
    {
        DirectionType dir = DirectionType.None;
        if (matchDir == MatchDirectionType.Vertical)
        {
            dir = DirectionType.Down;
        }
        else if (matchDir == MatchDirectionType.ForwardSlash)
        {
            dir = DirectionType.LeftDown;
        }
        else if (matchDir == MatchDirectionType.BackSlash)
        {
            dir = DirectionType.LeftUp;
        }
        return dir;
    }

    public static DirectionType LookAtEnd(MatchDirectionType matchDir)
    {
        DirectionType dir = DirectionType.None;
        if (matchDir == MatchDirectionType.Vertical)
        {
            dir = DirectionType.Up;
        }
        else if (matchDir == MatchDirectionType.ForwardSlash)
        {
            dir = DirectionType.RightUp;
        }
        else if (matchDir == MatchDirectionType.BackSlash)
        {
            dir = DirectionType.RightDown;
        }
        return dir;
    }

    public static MatchDirectionType ChangeMatchDirectionType(SpecialBlockType specialBlockType)
    {
        switch (specialBlockType)
        {
            case SpecialBlockType.Straight_Vertical:
                return MatchDirectionType.Vertical;
            case SpecialBlockType.Straight_ForwardSlash: 
                return MatchDirectionType.ForwardSlash;
            case SpecialBlockType.Straight_BackSlash: 
                return MatchDirectionType.BackSlash;
                break;
            default:
                return MatchDirectionType.None;
        }
    }
    /// <summary>
    /// MatchInfo List에서 좌표가 전부 같은 중복을 제거합니다.
    /// </summary>
    public static List<MatchInfo> Distinct(List<MatchInfo> result)
    {
        return result.Distinct(new ListOfMatchInfoComparer()).ToList();
    }

    /// <summary>
    /// MatchInfo List에서 중복을 제거하여 모든 좌표를 반환합니다.
    /// </summary>
    public static List<Vector2Int> GetCoordsAll(List<MatchInfo> results)
    {
        var coordsAll = new List<Vector2Int>();
        foreach (var result in results)
        {
            foreach (var coord in result.coords)
            {
                if (!coordsAll.Exists(x => x.Equals(coord)))
                {
                    coordsAll.Add(coord);
                }
            }
        }
        return coordsAll;
    }
}
//MatchInfo 비교를 위한 Comparer
public class ListOfMatchInfoComparer : IEqualityComparer<MatchInfo>
{
    public bool Equals(MatchInfo a, MatchInfo b)
    {
        return a.coords.SequenceEqual(b.coords);
    }

    public int GetHashCode(MatchInfo l)
    {
        unchecked
        {
            int hash = 0;
            foreach (var it in l.coords)
            {
                hash += (it.x * it.x) + (it.y * it.y) * 10;
            }
            return hash;
        }
    }
}
