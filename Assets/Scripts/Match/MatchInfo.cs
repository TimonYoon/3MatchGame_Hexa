using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatchInfo
{
    public BlockType blockType;
    public MatchType matchType;
    public MatchDirectionType matchDir;
    public List<Vector2Int> coords = new List<Vector2Int>();

    public MatchInfo(BlockType _blockType, MatchType _matchType, MatchDirectionType _matchDir, List<Vector2Int> _coords)
    {
        blockType = _blockType;
        matchType = _matchType;
        matchDir = _matchDir;
        coords = _coords;
    }
}
