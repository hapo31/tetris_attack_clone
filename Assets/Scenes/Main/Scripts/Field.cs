using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Game.Core;
using Game.Behavior;

using static BlockObject;

public class Field : MonoBehaviour
{

    public int Width;

    public int Height;

    int cursorX = 0;
    int cursorY = 0;

    public BlockObject blockPrefab;

    FieldBehavior fieldBehavior;

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
        fieldBehavior = new FieldBehavior(Width, Height);

        cursorX = Width / 2;
        cursorY = Height / 2;

        fieldBehavior.onInstantiateBlock += () =>
        {
            Debug.Log(blockPrefab);
            return Instantiate(blockPrefab);
        };

        fieldBehavior.Init();

        fieldBehavior.UpdateCursor(cursorX, cursorY);
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
        if (inputFrames[(int)Button.DOWN] == 1 && cursorY > 0)
        {
            cursorY--;
        }
        if (inputFrames[(int)Button.UP] == 1 && cursorY < Height - 1)
        {
            cursorY++;
        }

        fieldBehavior.UpdateCursor(cursorX, cursorY);
        fieldBehavior.Update();

        if (Input.GetButtonDown("BlockChange"))
        {
            fieldBehavior.ChangeBlock(cursorX, cursorY);
        }
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
