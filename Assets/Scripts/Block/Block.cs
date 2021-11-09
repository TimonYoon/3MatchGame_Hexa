using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockType type => _type;
    public Vector2Int coords { get; private set; }
    public SpecialBlockType specialBlockType { get; private set; }
    

    [SerializeField] private BlockType _type;

    [SerializeField] protected SpriteRenderer blockImage;
    [SerializeField] protected SpriteRenderer checkImage;

    protected Mover mover { get; private set; }
    private void Awake()
    {
        checkImage.gameObject.SetActive(false);
        mover = gameObject.AddComponent<Mover>();
        mover.stopDistance = 0.01f;
        mover.speed = Global.BLOCK_SWAP_SPEED;
    }

    public void ChangeCoordsAndMove(Vector2Int targetCoords, float speed)
    {
        if (coords.x == targetCoords.x)
        {
            var pos = BoardUtil.GetPosition(targetCoords);
            ToGoal(pos, speed);
        }
        else
        {
            var via = GetVia(coords, targetCoords);
            var des = new List<Vector3>();
            if (mover.IsMoving())
            {
                des.Add(mover.destination);
            }
            des.Add(via);
            des.Add(BoardUtil.GetPosition(targetCoords));
            mover.speed = speed;
            mover.ToGoal(des);
        }
        SetCoords(targetCoords);
    }

    public void SetDestroyCheck()
    {
        checkImage.gameObject.SetActive(true);
    }
    
    public virtual void TryDestroy()
    {
        //Debug.Log($"블록 제거 {type} {coords}");
        BlockManager.instance.blocks.Remove(this);
        Destroy(gameObject);
    }

    public void ToGoal(Vector3 position, float speed)
    {
        mover.speed = speed;
        mover.ToGoal(position);
    }
    public bool IsMoving()
    {
        return mover.IsMoving();
    }

    public void SetCoords(Vector2Int value)
	{        
        coords = value;
        gameObject.name = $"Block_{type}_{coords}";
    }

    public void SetChangeSpecialBlock(SpecialBlockType value)
    {
        string spriteName = type.ToString();
        specialBlockType = value;
		switch (specialBlockType)
		{
			case SpecialBlockType.None:
				break;
			case SpecialBlockType.Straight_Vertical:
                spriteName += "_block_arrow";
                blockImage.sprite = AtlasManager.instance.GetBlockSprite(spriteName);
				break;
			case SpecialBlockType.Straight_ForwardSlash:
                spriteName += "_block_arrow";
                blockImage.sprite = AtlasManager.instance.GetBlockSprite(spriteName);
                blockImage.transform.rotation = quaternion.Euler(0, 0, -45);
				break;
			case SpecialBlockType.Straight_BackSlash:
                spriteName += "_block_arrow";
                blockImage.sprite = AtlasManager.instance.GetBlockSprite(spriteName);
                blockImage.transform.rotation = quaternion.Euler(0, 0, 45);
				break;
			case SpecialBlockType.Boomerang:
                spriteName += "_block_boomerang";
                blockImage.sprite = AtlasManager.instance.GetBlockSprite(spriteName);
				break;
        }
	}


    private Vector3 GetVia(Vector2Int startCoords, Vector2Int targetCoords)
    {
        var coords = startCoords;
        var count = Mathf.Abs(targetCoords.x - startCoords.x);
        var dir = DirectionType.LeftDown;
        if (startCoords.x < targetCoords.x)
        {
            dir = DirectionType.RightDown;
        }
        for (int i = 0; i < count; i++)
        {
            coords = BoardUtil.GetNeighbor(coords, dir);
        }
        return BoardUtil.GetPosition(coords);
    }
}
