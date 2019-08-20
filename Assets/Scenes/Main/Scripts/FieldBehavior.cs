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

        // 何フレーム間隔でブロックを落とすか
        public int gravityAcceleration = 5;

        int idCount = 0;

        public FieldBehavior(int width, int height)
        {
            blocks = new Block[width * height];
            for (var i = 0; i < width * height; ++i)
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

            for (var y = Height - 10; y < Height; ++y)
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
                    var block = GetBlock(x, y);
                    // 消えているブロックは落下しない
                    if (!block.isDeleting)
                    {
                        // そのマスが空白だったら上に乗っているブロックを一つずつ下にずらす
                        if (block.type == BlockType.NONE)
                        {
                            for (var dy = y; dy > 0; --dy)
                            {
                                var dBlock = GetBlock(x, dy);
                                // 上に乗っているブロックが NONE だったらスキップ
                                if (GetBlock(x, dy - 1)?.type == BlockType.NONE)
                                {
                                    continue;
                                }

                                if (dBlock.fallWaitFrame > 0)
                                {
                                    dBlock.fallWaitFrame--;
                                    continue;
                                }

                                dBlock.fallWaitFrame = gravityAcceleration;
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
                    var block = GetBlock(x, y);

                    if (!block.isDeleting)
                    {
                        LookupDeleteBlockGroup(x, y);
                    }
                }
            }
        }


        public void ChangeBlock(int x, int y)
        {
            var a = GetBlock(x ,y);
            var b = GetBlock(x + 1, y);
            if (!a.isDeleting && !b.isDeleting)
            {
                if (blockObjects.ContainsKey(a.Id))
                {

                    blockObjects[a.Id].MoveTo(PositionToVector3(x + 1, y), 5);
                }

                if (blockObjects.ContainsKey(b.Id))
                {

                    blockObjects[b.Id].MoveTo(PositionToVector3(x, y), 5);
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
        /// 指定した座標値のブロックを設定しBlockObjectをInstantiateする
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        void CreateBlock(int x, int y, BlockType blockType)
        {
            var block = GetBlock(x, y);
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
            var from = GetBlock(fromX, fromY);
            var to = GetBlock(toX, toY);

            to.type = from.type;
            to.Id = from.Id;

            from.type = BlockType.NONE;
            from.Id = -1;

            blockObjects[to.Id].MoveTo(PositionToVector3(toX, toY), gravityAcceleration);
        }

        /// <summary>
        /// 指定した座標のブロックをNONEにし、BlockObjectを消す
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        void DeleteBlock(int x, int y)
        {
            var block = GetBlock(x, y);
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
            var block = GetBlock(x, y);
            block.isDeleting = true;
            blockObjects[block.Id].isDeleting = true;
        }

        void LookupDeleteBlockGroup(int baseX, int baseY)
        {
            var block = GetBlock(baseX, baseY);
            var tempBlocks = new List<(int x, int y)>(5);
            tempBlocks.Add((baseX, baseY));

            // ブロックの1個下が空白だったら判定しない
            if (baseY + 1 < Height && ( block.type == BlockType.NONE || GetBlock(baseX, baseY + 1).type == BlockType.NONE))
            {
                return;
            }

            for (var dx = baseX + 1; dx < Width; ++dx)
            {
                var checkBlock = GetBlock(dx, baseY);

                if (checkBlock.type != block.type)
                {
                    break;
                }

                tempBlocks.Add((dx, baseY));
            }

            for (var dy = baseY + 1; dy < Height; ++dy)
            {
                var checkBlock = GetBlock(baseX, dy);
                if (checkBlock.type != block.type)
                {
                    break;
                }

                tempBlocks.Add((baseX, dy));
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
            return v;
        }


        Block GetBlock(int x, int y)
        {
            if (x >= Width || y >= Height)
            {
                return null;
            }
            return blocks[y * Width + x];
        }
    }
}
