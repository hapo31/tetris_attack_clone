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
        // 消去カウントの限界
        public int deleteCountLimit = 0;
        // 1マス落下するときにそのマスで留まるフレーム数
        public int fallWaitFrame = 0;

        public int comboCount = 0;

        public bool isDeleting = false;

        public Block()
        {
            id = -1;
        }

        public void Move(Block target)
        {
            target.id = id;
            target.type = type;
            target.comboCount = comboCount;

            id = -1;
            type = BlockType.NONE;
            comboCount = 0;
        }

        public void Init()
        {
            type = BlockType.NONE;
            id = -1;
            deleteCount = 0;
            fallWaitFrame = 0;
            comboCount = 0;
            isDeleting = false;
        }
    }
}
