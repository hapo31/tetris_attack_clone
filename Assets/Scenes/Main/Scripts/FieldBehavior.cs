using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Core;
using UnityEngine;

namespace Game.Behavior
{
    class FieldBehavior
    {
        public delegate BlockObject OnInstantiateBlock(int id);
        public delegate void OnDeleteBlock(BlockObject blockObject);

        public event OnInstantiateBlock onInstantiateBlock;
        public event OnDeleteBlock onDeleteBlock;

        Block[] blocks;
        Dictionary<int, BlockObject> blockObjects = new Dictionary<int, BlockObject>();

        int Width;
        int Height;

        public int blockDeleteCountLimit = 60 * 5;

        int idCount = 0;

        public FieldBehavior(int width, int height)
        {
            blocks = new Block[width * height];
            for(var i = 0; i < width * height; ++i)
            {
                blocks[i] = new Block();
                blocks[i].deleteCountLimit = blockDeleteCountLimit;
            }
            Width = width;
            Height = height;
        }

        public void Init()
        {
            if (onInstantiateBlock == null)
            {
                throw new InvalidOperationException("'onInstantiateBlock' is null. Must be set eventhandler to this.");
            }

            if (onDeleteBlock == null)
            {
                throw new InvalidOperationException("'onDeleteBlock' is null. Must be set eventhandler to this.");
            }

            var rand = new System.Random();

            for (var y = Height - 4; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var blockType = (BlockType)(rand.Next(4) + 1);
                    CreateBlock(x, y, blockType);
                    
                }
            }
        }


        public void Update()
        {
            // ブロックを落下させる
            for (var x = 0; x < Width; ++x)
            {
                for (var y = Height - 1; y >= 0; --y)
                {
                    var block = blocks[y * Width + x];
                    block.isFloating = false;
                    // 消えているブロックは落下しない
                    if (!block.isDeleting)
                    {
                        // そのマスが空白だったら上に乗っているブロックを一つずつ下にずらす
                        if (block.type == BlockType.NONE)
                        {
                            for (var dy = y; dy > 1; --dy)
                            {
                                // 上に乗っているブロックが NONE だったらスキップ
                                if (blocks[(dy - 1) * Width + x].type == BlockType.NONE)
                                {
                                    continue;
                                }
                                blocks[dy * Width + x].isFloating = true;
                                MoveBlock(x, dy - 1, x, dy);
                            }
                            // 1フレームに1マスずつ落とす
                            break;
                        }
                    }
                }
            }

            // ブロックの消去
            for (var i = 0; i < blocks.Length; ++i)
            {
                var block = blocks[i];
                if (block.isDeleting)
                {
                    block.deleteCount++;

                    if (block.deleteCount > block.deleteCountLimit)
                    {
                        DeleteBlock(i % Width, i / Height);
                    }
                }
            }

            // ブロックの消去チェック
            for (var y = 0; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var block = blocks[y * Width + x];

                    if (!block.isDeleting && !block.isFloating)
                    {
                        if (x + 1 < Width)
                        {
                            // 横
                            if (block.type != BlockType.NONE && block.type == blocks[y * Width + x + 1].type)
                            {
                                LookupDeleteBlockGroup(block, x, y, 0);
                            }
                        }

                        if (y + 1 < Height)
                        {
                            // 縦
                            if (block.type != BlockType.NONE && block.type == blocks[(y + 1) * Width + x].type)
                            {
                                LookupDeleteBlockGroup(block, x, y, 1);
                            }
                        }
                    }
                }
            }
        }


        public void ChangeBlock(int x, int y)
        {
            var a = blocks[y * Width + x];
            var b = blocks[y * Width + x + 1];
            if (!a.isDeleting && !b.isDeleting)
            {
                if (blockObjects.ContainsKey(a.Id))
                {

                    blockObjects[a.Id].MoveTo(PositionToVector3(x + 1, y), 3);
                }

                if (blockObjects.ContainsKey(b.Id))
                {

                    blockObjects[b.Id].MoveTo(PositionToVector3(x, y), 3);
                }

                var t = a.type;
                var id = a.Id;
                a.type = b.type;
                a.Id = b.Id;

                b.type = t;
                b.Id = id;

            }
        }

        /// <summary>
        /// 仮のやつなのでそのうち消すがとりあえずカーソルが実装できるまでやっておく
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void UpdateCursor(int x, int y)
        {

            foreach(var blockObj in blockObjects.Values)
            {
                blockObj.isSelected = false;
            }

            var block1 = blocks[y * Width + x];
            if (blockObjects.ContainsKey(block1.Id))
            {
                blockObjects[block1.Id].isSelected = true;
            }

            var block2 = blocks[y * Width + x + 1];
            if (blockObjects.ContainsKey(block2.Id))
            {
                blockObjects[block2.Id].isSelected = true;
            }
        }

        /// <summary>
        /// 指定した座標値のブロックを設定しBlockObjectをInstantiateする
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        void CreateBlock(int x, int y, BlockType blockType)
        {
            var block = blocks[y * Width + x];
            block.type = blockType;
            block.Id = idCount;
            ++idCount;

            var blockObject = onInstantiateBlock.Invoke(block.Id);
            blockObject.type = blockType;
            blockObject.transform.position = PositionToVector3(x, y);

            blockObjects.Add(block.Id, blockObject);
        }

        /// <summary>
        /// fromX, fromY の座標にあるブロックを toX, toY に移動する
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        void MoveBlock(int fromX, int fromY, int toX, int toY)
        {
            var from = blocks[fromY * Width + fromX];
            var to = blocks[toY * Width + toX];
            Debug.Log("MoveBlock: from " + from.Id + " to " + to.Id);

            to.type = from.type;
            to.Id = from.Id;

            from.type = BlockType.NONE;
            from.Id = -1;

            blockObjects[to.Id].MoveTo(PositionToVector3(toX, toY), 2);
        }

        /// <summary>
        /// 指定した座標のブロックをNONEにし、BlockObjectを消す
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        void DeleteBlock(int x, int y)
        {
            var block = blocks[y * Width + x];
            if (block.Id == -1)
            {
                return;
            }
            onDeleteBlock.Invoke(blockObjects[block.Id]);
            blockObjects.Remove(block.Id);

            Debug.Log("Delete block:" + block.Id);

            block.type = BlockType.NONE;
            block.Id = -1;
            block.deleteCount = 0;
            block.isDeleting = false;
        }

        void SetDeleteFlag(int x, int y)
        {
            blocks[y * Width + x].isDeleting = true;
            blockObjects[blocks[y * Width + x].Id].isDeleting = true;
        }

        void LookupDeleteBlockGroup(Block block, int baseX, int baseY, int checkDir)
        {
            var tempBlocks = new List<(int x, int y)>(5);
            tempBlocks.Add((baseX, baseY));
            if (checkDir == 0)
            {
                for (var dx = baseX + 1; dx < Width; ++dx)
                {
                    var checkBlock = blocks[baseY * Width + dx];
                    if (checkBlock.type != block.type)
                    {
                        break;
                    }

                    tempBlocks.Add((dx, baseY));
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

                    tempBlocks.Add((baseX, dy));
                }
            }

            if (tempBlocks.Count >= 3)
            {
                tempBlocks.ForEach(pos =>
                {
                    SetDeleteFlag(pos.x, pos.y);
                });
            }
        }


        Vector3 PositionToVector3(int x, int y)
        {
            var v = new Vector3(x, Height - 1 - y);
            Debug.Log("x:" + x + " y:" + y +" vec:" + v);
            return v;
        }
    }
}
