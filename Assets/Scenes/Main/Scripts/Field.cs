using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Game.Core;
using Game.Behavior;
using UnityEngine.UI;

public class Field : MonoBehaviour
{

    public int Width;

    public int Height;

    int cursorX = 0;
    int cursorY = 0;

    public BlockObject blockPrefab;

    GameObject cursor;
    GameObject comboText;

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
        cursorY = Height  - 2;

        cursor = GameObject.FindGameObjectWithTag("Cursor");
        comboText = GameObject.FindGameObjectWithTag("ComboText");

        fieldBehavior.onInstantiateBlock += (id) =>
        {
            var instance = Instantiate(blockPrefab);
            instance.name = "Block" + id;
            return instance;
        };

        fieldBehavior.onDeleteBlock += blockObject =>
        {
            Destroy(blockObject.gameObject);
        };

        fieldBehavior.onCombo += (comboCount, chainCount) =>
        {
            Debug.Log("連鎖数:" + comboCount);
            // 連鎖数の表示
            if (comboCount >= 1)
            {
                var text = comboText.GetComponent<Text>();
                text.gameObject.SetActive(true);
                text.text = string.Format("{0} Combo!", comboCount);
            }
        };

        fieldBehavior.onComboReset += () => {
            var text = comboText.GetComponent<Text>();
            text.gameObject.SetActive(false);
            Debug.Log("連鎖リセット");
        };

        fieldBehavior.Init();
        cursor.transform.position = new Vector3(cursorX, Height - 1 - cursorY);
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
        cursor.transform.position = new Vector3(cursorX, Height - 1 - cursorY);
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
