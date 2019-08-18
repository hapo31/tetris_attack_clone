using System.Collections;
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

    public int blockDeleteCountLimit = 60 * 5;

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

        for (var i = 0; i < Width * Height; ++i)
        {
            var block = Instantiate(blockPrefab);
            block.deleteCountLimit = blockDeleteCountLimit;
            block.type = (Type)r.Next(5);
            block.transform.position = new Vector3(i % Width - Width / 2, i / Height);
            blocks.Add(block);
        }
        UpdateCursor();
    }

    // Update is called once per frame
    void Update()
    {

        UpdateAxis();

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

        UpdateBlockGravity();
        UpdateCursor();


        if (Input.GetButtonDown("BlockChange"))
        {
            var t = blocks[cursorY * Width + cursorX].type;
            blocks[cursorY * Width + cursorX].type = blocks[cursorY * Width + cursorX + 1].type;
            blocks[cursorY * Width + cursorX + 1].type = t;
        }

        UpdateDeleteBlock();

    }

    void UpdateCursor()
    {

        blocks.ForEach(block =>
        {
            block.isSelected = false;
        });

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

    void UpdateDeleteBlock()
    {
        for (var y = 0; y < Height; ++y)
        {
            for (var x = 0; x < Width; ++x)
            {
                var block = blocks[y * Width + x];

                if (!block.isDeleting)
                {
                    if (x + 1 < Width)
                    {
                        // 横
                        if (block.type != Type.NONE && block.type == blocks[y * Width + x + 1].type)
                        {
                            LookupDeleteBlockGroup(block, x, y, 0);
                        }
                    }

                    if (y + 1 < Height)
                    {
                        // 縦
                        if (block.type != Type.NONE && block.type == blocks[(y + 1) * Width + x].type)
                        {
                            LookupDeleteBlockGroup(block, x, y, 1);
                        }
                    }
                }
            }
        }
    }

    void UpdateBlockGravity()
    {
        for (var x = 0; x < Width; ++x)
        {
            for (var y = Height - 1; y >= 0; --y)
            {

                var block = blocks[y * Width + x];
                // そのマスが空白だったら上に乗っているブロックを一つずつ下にずらす
                if (block.type == Type.NONE)
                {
                    var by = y;
                    for (var cy = y + 1; cy < Height; ++cy)
                    {
                        blocks[by * Width + x].type = blocks[cy * Width + x].type;
                        by = cy;
                    }

                    blocks[by * Width + x].type = Type.NONE;
                }
            }
        }
    }

    void LookupDeleteBlockGroup(Block block, int baseX, int baseY, int checkDir)
    {
        var tempBlocks = new List<Block>(5);
        tempBlocks.Add(block);
        var nextBlock = blocks[(baseY + checkDir == 1 ? 1 : 0) * Width + baseX + (checkDir == 0 ? 1 : 0)];
        if (checkDir == 0)
        {
            for (var dx = baseX + 1; dx < Width; ++dx)
            {
                var checkBlock = blocks[baseY * Width + dx];
                if (checkBlock.type != block.type)
                {
                    break;
                }

                tempBlocks.Add(checkBlock);
            }
        }
        else
        {
            for (var dy = baseY + 1; dy < Height; ++dy)
            {
                var checkBlock = blocks[dy * Width + baseX];
                if (checkBlock.type != block.type)
                {
                    break;
                }

                tempBlocks.Add(checkBlock);
            }
        }

        if (tempBlocks.Count >= 3)
        {
            tempBlocks.ForEach(tempBlock =>
            {
                tempBlock.isDeleting = true;
            });

        }

    }
}
