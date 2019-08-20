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
        public int Id;
        public BlockType type = BlockType.NONE;
        // 消去カウント
        public int deleteCount = 0;
        // 消去カウントの限界
        public int deleteCountLimit = 0;
        // 1マス落下するときにそのマスで留まるフレーム数
        public int fallWaitFrame = 0;

        public bool isDeleting = false;

        public Block()
        {
            Id = -1;
        }
    }
}
