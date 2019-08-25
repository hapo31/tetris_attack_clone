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
        public delegate void OnCombo(int count);

        public event OnInstantiateBlock onInstantiateBlock;
        public event OnDeleteBlock onDeleteBlock;
        public event OnCombo onCombo;


        Block[] blocks;
        Dictionary<int, BlockObject> blockObjects = new Dictionary<int, BlockObject>();

        int Width;
        int Height;

        // ブロックが何フレームで消えるか
        public int blockDeleteCountLimit = 60 * 5;

        // 何フレーム間隔でブロックを落とすか
        public int gravityAcceleration = 5;


        int comboCount = 0;

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
                    var underBlock = GetBlock(x, y + 1);

                    // 消えているブロックは落下しない 
                    if (!block.isDeleting)
                    {
                        // 下のブロックが空白なら落下する
                        if (underBlock != null && underBlock.type == BlockType.NONE)
                        {
                            for (var dy = y; dy >= 0; --dy)
                            {
                                var dBlock = GetBlock(x, dy);
                                var underBlock2 = GetBlock(x, dy + 1);

                                if (dBlock.type == BlockType.NONE)
                                {
                                    continue;
                                }

                                if (dBlock.isDeleting || underBlock2.isDeleting || underBlock2.type != BlockType.NONE)
                                {
                                    continue;
                                }

                                if (dBlock.fallWaitFrame > 0)
                                {
                                    dBlock.fallWaitFrame--;
                                    continue;
                                }
                                else
                                {
                                    MoveBlock(x, dy + 1, x, dy);
                                }

                                dBlock.fallWaitFrame = gravityAcceleration;
                            }
                            break;
                        }
                        else
                        {
                            block.isWillCombo = false;
                        }
                    }
                }
            }

            // ブロックの消去
            for (var i = 0; i < blocks.Length; ++i)
            {
                var block = blocks[i];
                var x = i % Width;
                var y = i / Height;
                if (block.isDeleting)
                {
                    block.deleteCount++;

                    if (block.deleteCount > block.deleteCountLimit)
                    {
                        // 上に乗っているブロックに連鎖フラグを付ける
                        for (var dy = y - 1; dy > 0; --dy)
                        {
                            var targetBlock = GetBlock(x, dy);
                            if (targetBlock == null || targetBlock.type == BlockType.NONE)
                            {
                                break;
                            }
                            targetBlock.isWillCombo = true;
                        }
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
                    var underBlock = GetBlock(x, y + 1);
                    if (underBlock != null && underBlock.type == BlockType.NONE)
                    {
                        continue;
                    }

                    if (block.deleteCount == 0 && block.type != BlockType.NONE)
                    {
                        LookupDeleteBlockGroup(x, y);
                    }
                }
            }
            DebugProc();
        }

        public void DebugProc()
        {
            var s = "";
            for (var y = 0; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var b = GetBlock(x, y);

                    switch (b.type)
                    {
                        case BlockType.NONE:
                            s += "-";
                            break;
                        case BlockType.RED:
                            s += "a";
                            break;
                        case BlockType.GREEN:
                            s += "b";
                            break;
                        case BlockType.BLUE:
                            s += "c";
                            break;
                        case BlockType.YELLOW:
                            s += "d";
                            break;
                        default:
                            break;
                    }
                }
                s += "\n";
            }

            Debug.Log(s);
        }


        public void ChangeBlock(int x, int y)
        {
            var a = GetBlock(x ,y);
            var b = GetBlock(x + 1, y);
            if (!a.isDeleting && !b.isDeleting)
            {
                if (blockObjects.ContainsKey(a.id))
                {
                    blockObjects[a.id].MoveTo(PositionToVector3(x + 1, y), 5);
                }

                if (blockObjects.ContainsKey(b.id))
                {
                    blockObjects[b.id].MoveTo(PositionToVector3(x, y), 5);
                }

                var t = a.type;
                var id = a.id;
                a.type = b.type;
                a.id = b.id;

                b.type = t;
                b.id = id;
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
            block.id = idCount;
            ++idCount;

            var blockObject = onInstantiateBlock.Invoke(block.id);
            blockObject.type = blockType;
            blockObject.transform.position = PositionToVector3(x, y);

            blockObjects.Add(block.id, blockObject);
        }

        /// <summary>
        /// fromX, fromY の座標にあるブロックを toX, toY に移動する
        /// </summary>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        void MoveBlock(int toX, int toY, int fromX, int fromY)
        {
            var from = GetBlock(fromX, fromY);
            var to = GetBlock(toX, toY);

            if (to.type != BlockType.NONE)
            {
                throw new InvalidOperationException(
                    string.Format("to BlockType is not NONE.(toX:{0} toY:{1} type:{2} fromX:{3} fromY:{4} type:{5}", toX, toY, to.type.ToString(), fromX, fromY, from.type.ToString()));
            }


            if (from.type == BlockType.NONE)
            {
                throw new InvalidOperationException(string.Format("from BlockType is not NONE.(toX:{0} toY:{1} type:{2} fromX:{3} fromY:{4} type:{5}", toX, toY, to.type.ToString(), fromX, fromY, from.type.ToString()));
            }

            from.MoveTo(to);

            blockObjects[to.id].MoveTo(PositionToVector3(toX, toY), gravityAcceleration);
        }

        /// <summary>
        /// 指定した座標のブロックをNONEにし、BlockObjectを消す
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        void DeleteBlock(int x, int y)
        {
            var block = GetBlock(x, y);
            if (block.id == -1)
            {
                return;
            }
            onDeleteBlock.Invoke(blockObjects[block.id]);
            blockObjects.Remove(block.id);

            Debug.Log("Delete block:" + block.id);

            block.Init();
        }

        void SetDeleteFlag(int x, int y)
        {
            var block = GetBlock(x, y);
            block.isDeleting = true;
            blockObjects[block.id].isDeleting = true;
        }

        void LookupDeleteBlockGroup(int baseX, int baseY)
        {
            var block = GetBlock(baseX, baseY);
            var horizontalDeletingBlocks = new List<(int x, int y)>(5);
            horizontalDeletingBlocks.Add((baseX, baseY));

            bool willCombo = false;

            // 横方向
            for (var dx = baseX + 1; dx < Width; ++dx)
            {
                var checkBlock = GetBlock(dx, baseY);
                var underBlock = GetBlock(dx, baseY + 1);

                // 1マス下が空白なら判定を中断
                if (underBlock != null && underBlock.type == BlockType.NONE)
                {
                    break;
                }

                // 消去中か空白ブロックか違うブロックだったら判定終わり
                if (checkBlock.deleteCount > 0 || checkBlock.type == BlockType.NONE || checkBlock.type != block.type)
                {
                    break;
                }

                if (checkBlock.isWillCombo)
                {
                    willCombo = true;
                }

                horizontalDeletingBlocks.Add((dx, baseY));
            }

            var verticalDeletingBlocks = new List<(int x, int y)>(5);
            verticalDeletingBlocks.Add((baseX, baseY));

            // 縦方向
            for (var dy = baseY + 1; dy < Height; ++dy)
            {
                var checkBlock = GetBlock(baseX, dy);
                var underBlock = GetBlock(baseX, dy + 1);

                // 1マス下が空白なら判定を中断
                if (underBlock != null && underBlock.type == BlockType.NONE)
                {
                    break;
                }

                // 空白ブロックか違うブロックだったら判定終わり
                if (checkBlock.deleteCount > 0 || checkBlock.type == BlockType.NONE || checkBlock.type != block.type)
                {
                    break;
                }

                if (checkBlock.isWillCombo)
                {
                    willCombo = true;
                }

                verticalDeletingBlocks.Add((baseX, dy));
            }

            var isDeleted = false;
            if (horizontalDeletingBlocks.Count >= 3)
            {
                horizontalDeletingBlocks.ForEach(pos =>
                {
                    SetDeleteFlag(pos.x, pos.y);
                });
                isDeleted = true;
            }

            if (verticalDeletingBlocks.Count >= 3)
            {
                verticalDeletingBlocks.ForEach(pos =>
                {
                    SetDeleteFlag(pos.x, pos.y);
                });
                isDeleted = true;
            }


            if (isDeleted && willCombo)
            {
                onCombo?.Invoke(comboCount);
                ++comboCount;
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
