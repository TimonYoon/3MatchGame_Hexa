using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class BoardUtil
{
    /// <summary>
    /// ���� �������� ���� ��ǥ
    /// </summary>
    /// <param name="coords">���� ������</param>
    public static Vector3 GetPosition(Vector2Int coords)
    {
        int x = coords.x;
        int y = coords.y;
        return new Vector3((x - ((float)Global.MAX_SIZE.x / 2f)) * 0.65f, (y - ((float)Global.MAX_SIZE.y / 2f)) * 0.38f, 0f);
    }

    /// <summary>
    /// ���� ��ǥ���� �̿��� ��ǥ ��ȯ
    /// </summary>
    /// <param name="origin">������ǥ</param>
    /// <param name="dir">����</param>
    /// <returns>�̿� ��ǥ</returns>
    public static Vector2Int GetNeighbor(Vector2Int origin, DirectionType dir)
    {
        switch (dir)
        {
            case DirectionType.LeftUp:
                return new Vector2Int(origin.x - 1, origin.y + 1);
            case DirectionType.Up:
                return new Vector2Int(origin.x, origin.y + 2);
            case DirectionType.RightUp:
                return new Vector2Int(origin.x + 1, origin.y + 1);
            case DirectionType.LeftDown:
                return new Vector2Int(origin.x - 1, origin.y - 1);
            case DirectionType.Down:
                return new Vector2Int(origin.x, origin.y - 2);
            case DirectionType.RightDown:
                return new Vector2Int(origin.x + 1, origin.y - 1);
            case DirectionType.RightUpOffset:
                return new Vector2Int(origin.x + 1, origin.y + 3);
            case DirectionType.RightOffset:
                return new Vector2Int(origin.x + 2, origin.y);
            case DirectionType.RightDownOffset:
                return new Vector2Int(origin.x + 1, origin.y - 3);
            case DirectionType.LeftDownOffset:
                return new Vector2Int(origin.x - 1, origin.y - 3);
            case DirectionType.LeftOffset:
                return new Vector2Int(origin.x - 2, origin.y);
            case DirectionType.LeftUpOffset:
                return new Vector2Int(origin.x - 1, origin.y + 3);
        }
        return Vector2Int.zero;
    }

    /// <summary>
    /// ���� ��ǥ���� �̿��� ��� ��ǥ ��ȯ 
    /// </summary>
    public static IEnumerable<Vector2Int> GetNeighborAll(Vector2Int origin)
    {
        return Global.NEIGHBOR_DIRECTIONS.Select(x => GetNeighbor(origin, x));
    }

    /// <summary>
    /// �� ��ǥ�� ���� �̿��� ��ǥ���� Ȯ��
    /// </summary>
    public static bool IsNeighbor(Vector2Int coords1, Vector2Int coords2)
    {
        int xDiff = Mathf.Abs(coords1.x - coords2.x);
        int yDiff = Mathf.Abs(coords1.y - coords2.y);
        return xDiff <= 1 && yDiff <= 2;
    }

    /// <summary>
    /// ������ġ���� ����ġ������ � �������� ��ȯ
    /// </summary>
    /// <param name="startPos">���� ���� ������</param>
    /// <param name="endPos">�� ���� ������</param>
    /// <returns>����</returns>
    public static DirectionType GetDirection(Vector3 startPos, Vector3 endPos)
    {
        var degree = GetDirectionDegree(startPos, endPos);
        if (degree <= 60f)
        {
            return DirectionType.RightUp;
        }
        else if (degree <= 120f)
        {
            return DirectionType.Up;
        }
        else if (degree <= 180f)
        {
            return DirectionType.LeftUp;
        }
        else if (degree <= 240f)
        {
            return DirectionType.LeftDown;
        }
        else if (degree <= 300f)
        {
            return DirectionType.Down;
        }
        else
        {
            return DirectionType.RightDown;
        }
    }

    public static float GetDirectionDegree(Vector3 origin, Vector3 lookAt)
    {
        Vector3 direction = lookAt - origin;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0f)
        {
            angle += 360f;
        }
        return angle;
    }

    public static DirectionType GetOffsetDirection(DirectionType dir1, DirectionType dir2)
    {
        int intDir1 = (int)dir1;
        int intDir2 = (int)dir2;
        if (intDir1 > 10 || intDir2 > 10)
        {
            throw new Exception("�Ķ������ ��ȿ������ �ƴմϴ�.");
        }
        return (DirectionType)(((intDir1 * intDir1) + (intDir2 * intDir2)) * 10);
    }
}
