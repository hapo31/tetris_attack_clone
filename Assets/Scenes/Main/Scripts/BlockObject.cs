using Game.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : MonoBehaviour
{
    public BlockType type;

    public Material blueMaterial;
    public Material redMaterial;
    public Material yellowMaterial;
    public Material greenMaterial;

    public bool isDeleting = false;

    private new Renderer renderer;

    private Queue<Vector3> moveQueue = new Queue<Vector3>();
    
    // 最後に呼ばれた MoveTo() が target とする座標
    private Vector3? nextPosition = null;

    public void MoveTo(Vector3 target, int frame)
    {
        var current = nextPosition == null ? gameObject.transform.position : nextPosition;

        nextPosition = target;

        if (frame == 0)
        {
            gameObject.transform.position = target;
            return;
        }

        for (var i = 0; i < frame; ++i)
        {
            moveQueue.Enqueue((target - (Vector3)current) / frame);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        renderer = gameObject.GetComponent<Renderer>();

    }

    // Update is called once per frame
    void Update()
    {
        switch (type)
        {
            case BlockType.NONE:
                renderer.material.color = new Color(0, 0, 0, 0.0f);
                break;
            case BlockType.RED:
                renderer.material = this.redMaterial;
                break;
            case BlockType.BLUE:
                renderer.material = this.blueMaterial;
                break;
            case BlockType.GREEN:
                renderer.material = this.greenMaterial;
                break;
            case BlockType.YELLOW:
                renderer.material = this.yellowMaterial;
                break;
        }

        if (moveQueue.Count > 0)
        {
            var delta = moveQueue.Dequeue();
            gameObject.transform.Translate(delta);
        }
        else
        {
            nextPosition = null;
        }

        if (isDeleting)
        {
            renderer.material.color = new Color(0, 0, 0.5f, 1);
        }
    }
}
