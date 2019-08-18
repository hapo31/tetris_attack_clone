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
        public BlockType type = BlockType.NONE;
        public int deleteCount = 0;
        public int deleteCountLimit = 0;
        public bool isDeleting = false;

        void Update()
        {
            if (isDeleting)
            {
                ++deleteCount;
            }

            if (deleteCount > deleteCountLimit)
            {
                type = BlockType.NONE;
                deleteCount = 0;
                isDeleting = false;
            }
        }
    }
}
