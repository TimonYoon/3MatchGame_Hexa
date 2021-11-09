using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TopBlock : Block, IObstacle
{
    public bool isSpin { get; private set; } = false;

    public override void TryDestroy()
    {
        if (!isSpin)
        {
            isSpin = true;
            StartCoroutine(CoSpin());
            return;
        }
        BlockManager.instance.blocks.Remove(this);
        blockImage.sortingOrder = 100;
        checkImage.gameObject.SetActive(false);
        mover.speed = Global.TOP_BLOCK_DESTROY_SPEED;
        mover.ToGoal(UIManager.instance.topCountTextPos.position, delegate
        {
            Destroy(gameObject);
        });
    }

    IEnumerator CoSpin()
    {
        float t = 0;
        bool isPlus = false;
        while (true)
        {
            if (t <= 0)
            {
                isPlus = true;
            }
            else if(t >= 1)
            {
                isPlus = false;
            }
            t = isPlus ? t + Time.deltaTime : t - Time.deltaTime;
            var zEuler = Mathf.Lerp(-10, 10, t);
            blockImage.transform.rotation = quaternion.Euler(0,0,zEuler);
            yield return null;
        }
    }
}
