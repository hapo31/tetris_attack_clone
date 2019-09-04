using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core
{
    public enum BlockType
    {
        NONE,
        RED,
        GREEN,
        BLUE,
        YELLOW
    }
    class Block
    {
        public int id;
        public BlockType type = BlockType.NONE;
        // 消去カウント
        public int deleteCount = 0;
        // 1マス落下するときにそのマスで留まるフレーム数
        public int fallWaitFrame = 0;

        public int nextComboCount = 0;

        public bool IsWillCombo { get => nextComboCount >= 2; }

        // ブロックを消したときの連鎖数
        public int comboCount = 0;

        public bool IsDeleting { get => comboCount >= 1; }

        public bool IsInCombo { get => comboCount >= 2; }

        public Block()
        {
            id = -1;
        }

        public void Move(Block target)
        {
            target.id = id;
            target.type = type;
            target.comboCount = comboCount;
            target.nextComboCount = nextComboCount;
            Init();
        }

        public void Init()
        {
            type = BlockType.NONE;
            id = -1;
            deleteCount = 0;
            fallWaitFrame = 0;
            nextComboCount = 0;
            comboCount = 0;
        }
    }
}
