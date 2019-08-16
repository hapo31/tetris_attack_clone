﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Block;

public class Field : MonoBehaviour
{

    public int Width;

    public int Height;

    int cursorX = 0;
    int cursorY = 0;

    public Block blockPrefab;

    List<Block> blocks = new List<Block>();

    int[] inputFrames = new int[6];
   
    enum Button
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        CHANGE,
        RAISE_FIELD
    }

    // Start is called before the first frame update
    void Start()
    {
        var r = new System.Random();

        cursorX = Width / 2;
        cursorY = Height / 2;

        for(var i = 0; i < Width * Height; ++i)
        {
            var block = Instantiate(blockPrefab);
            block.type = (Type)r.Next(5);
            block.transform.position = new Vector3(i % Width - Width / 2, i / Height);
            blocks.Add(block);
        }
        UpdateCursor();
    }

    // Update is called once per frame
    void Update()
    {
        for (var y = Height - 1; y >= 0; --y)
        {
            for (var x = 0; x < Width; ++x)
            {
                var block = blocks[y * Width + x];
                block.isSelected = false;
                // そのマスが空白だったら上に乗っているブロックを一つずつ下にずらす
                if (block.type == Type.NONE)
                {
                    var by = y;
                    for (var cy = y + 1; cy < Height; ++cy )
                    {
                        blocks[by * Width + x].type = blocks[cy * Width + x].type;
                        by = cy;
                    }

                    blocks[by * Width + x].type = Type.NONE;
                }
            }
        }

        UpdateAxis();

        var XAxis = Input.GetAxis("Horizontal");
        var YAxis = Input.GetAxis("Vertical");

        if (inputFrames[(int)Button.RIGHT] == 1 && cursorX < Width - 2)
        {
            cursorX++;
        }
        if (inputFrames[(int)Button.LEFT] == 1 && cursorX > 0)
        {
            cursorX--;
        }
        if (inputFrames[(int)Button.DOWN] == 1 && cursorY < Height - 1)
        {
            cursorY++;
        }
        if (inputFrames[(int)Button.UP] == 1 && cursorY > 0)
        {
            cursorY--;
        }

        if (Input.GetButtonDown("BlockChange"))
        {
            var t = blocks[cursorY * Width + cursorX].type;
            blocks[cursorY * Width + cursorX].type = blocks[cursorY * Width + cursorX + 1].type;
            blocks[cursorY * Width + cursorX + 1].type = t;
        }

        UpdateCursor();

        Debug.Log("cursorX:" + cursorX);
        Debug.Log("cursorY:" + cursorY);
        Debug.Log("XAxis:" + XAxis);
        Debug.Log("YAxis:" + YAxis);
    }

    void UpdateCursor()
    {
        blocks[cursorY * Width + cursorX].isSelected = true;
        blocks[cursorY * Width + cursorX + 1].isSelected = true;
    }


    void UpdateAxis()
    {
        var XAxis = Input.GetAxis("Horizontal");
        var YAxis = Input.GetAxis("Vertical");

        if (XAxis > 0)
        {
            inputFrames[(int)Button.RIGHT]++;
        }
        else
        {
            inputFrames[(int)Button.RIGHT] = 0;
        }
        if (XAxis < 0)
        {
            inputFrames[(int)Button.LEFT]++;
        }
        else
        {
            inputFrames[(int)Button.LEFT] = 0;
        }
        if (YAxis > 0)
        {
            inputFrames[(int)Button.DOWN]++;
        }
        else
        {
            inputFrames[(int)Button.DOWN] = 0;
        }
        if (YAxis < 0)
        {
            inputFrames[(int)Button.UP]++;
        }
        else
        {
            inputFrames[(int)Button.UP] = 0;
        }
    }
}
