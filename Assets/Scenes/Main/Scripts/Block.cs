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
        public int deleteCount = 0;
        public int deleteCountLimit = 0;
        public int fallWaitFrame = 0;

        public bool isDeleting = false;

        public Block()
        {
            Id = -1;
        }
    }
}
