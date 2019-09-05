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
        public delegate void OnCombo(int comboCount, int chainCount);
        public delegate void OnComboReset();

        public event OnInstantiateBlock onInstantiateBlock;
        public event OnDeleteBlock onDeleteBlock;
        public event OnCombo onCombo;
        public event OnComboReset onComboReset;


        Block[] blocks;
        Dictionary<int, BlockObject> blockObjects = new Dictionary<int, BlockObject>();

        int Width;
        int Height;

        // ブロックが何フレームで消えるか
        public int blockDeleteCountLimit = 60 * 10;

        // 何フレーム間隔でブロックを落とすか
        public int gravityAcceleration = 5;


        int globalComboCount = 0;

        int idCount = 0;

        public FieldBehavior(int width, int height)
        {
            blocks = new Block[width * height];
            for (var i = 0; i < width * height; ++i)
            {
                blocks[i] = new Block();
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
            DropBlock();

            // ブロックの消去
            for (var i = 0; i < blocks.Length; ++i)
            {
                var block = blocks[i];
                var x = i % Width;
                var y = i / Height;
                if (block.IsDeleting)
                {
                    block.deleteCount++;

                    if (block.deleteCount > blockDeleteCountLimit)
                    {
                        // 上に乗っているブロックに連鎖フラグを付ける
                        for (var dy = y - 1; dy > 0; --dy)
                        {
                            var targetBlock = GetBlock(x, dy);
                            if (targetBlock == null || targetBlock.type == BlockType.NONE)
                            {
                                break;
                            }

                            if (targetBlock.IsDeleting)
                            {
                                continue;
                            }
                            // 消去によって落下するブロックに次の連鎖数を与える
                            targetBlock.nextComboCount = block.comboCount + 1;
                        }
                        DeleteBlock(x, y);
                    }
                }
            }


            var deleteCount = 0;
            var isIncreasesCombo = false;
            // ブロックの消去チェック
            for (var y = 0; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var block = GetBlock(x, y);
                    // もう消しているブロックは処理しない
                    if (block.IsDeleting)
                    {
                        continue;
                    }

                    // そのブロックが浮いていたら処理しない
                    if (isFloatingChunk(x, y))
                    {
                        continue;
                    }

                    // 消去対象のブロックを列挙する
                    (var h, var v) = LookupDeleteBlockGroup(x, y);

                    if (h.Count >= 3)
                    {
                        h.ForEach(pos =>
                        {
                            var deleteTargetBlock = GetBlock(pos.x, pos.y);
                            if (deleteTargetBlock.IsWillCombo)
                            {
                                isIncreasesCombo = true;
                                SetDeleteFlag(pos.x, pos.y, globalComboCount + 1);
                            }
                            else
                            {
                                SetDeleteFlag(pos.x, pos.y, 1);
                            }
                        });
                    }

                    if (v.Count >= 3)
                    {
                        v.ForEach(pos =>
                        {
                            var deleteTargetBlock = GetBlock(pos.x, pos.y);
                            if (deleteTargetBlock.IsWillCombo)
                            {
                                isIncreasesCombo = true;
                                SetDeleteFlag(pos.x, pos.y, globalComboCount + 1);
                            }
                            else
                            {
                                SetDeleteFlag(pos.x, pos.y, 1);
                            }
                        });
                    }

                    deleteCount += h.Count + v.Count - (v.Count >= 3 && h.Count >= 3 ? 1 : 0);
                }
            }

            if (deleteCount >= 3)
            {
                Debug.Log("isIncreasesCombo:" + isIncreasesCombo);
                // 連鎖数をカウントアップするかどうか
                if (isIncreasesCombo)
                {
                    // 連鎖数を上げる
                    globalComboCount++;
                    onCombo?.Invoke(globalComboCount, deleteCount);
                }
                else
                {
                    // 1連鎖目のとき
                    if (globalComboCount == 0)
                    {
                        globalComboCount = 1;
                    }
                    onCombo?.Invoke(1, deleteCount);
                }


            }

            BlockComboCleanup();
            // DebugProc();
        }

        public void DebugProc()
        {
            var s = "";
            for (var y = 0; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var b = GetBlock(x, y);

                    s += b.comboCount.ToString();

                    //switch (b.type)
                    //{
                    //    case BlockType.NONE:
                    //        s += "-";
                    //        break;
                    //    case BlockType.RED:
                    //        s += "a";
                    //        break;
                    //    case BlockType.GREEN:
                    //        s += "b";
                    //        break;
                    //    case BlockType.BLUE:
                    //        s += "c";
                    //        break;
                    //    case BlockType.YELLOW:
                    //        s += "d";
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
                s += "\n";
            }

            Debug.Log(s);
        }


        public void ChangeBlock(int x, int y)
        {
            var a = GetBlock(x, y);
            var b = GetBlock(x + 1, y);
            if (!a.IsDeleting && !b.IsDeleting)
            {
                if (blockObjects.ContainsKey(a.id))
                {
                    blockObjects[a.id].MoveTo(PositionToVector3(x + 1, y), 5);
                }

                if (blockObjects.ContainsKey(b.id))
                {
                    blockObjects[b.id].MoveTo(PositionToVector3(x, y), 5);
                }

                var type = a.type;
                var id = a.id;
                a.type = b.type;
                a.id = b.id;

                b.type = type;
                b.id = id;

                // nextComboCount は入れ替えたら無効化する
                a.nextComboCount = 0;
                b.nextComboCount = 0;
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

            from.Move(to);

            blockObjects[to.id].MoveTo(PositionToVector3(toX, toY), gravityAcceleration);
        }

        void DropBlock()
        {
            for (var x = 0; x < Width; ++x)
            {
                for (var y = Height - 2; y >= 0; --y)
                {
                    // ブロックが浮いているなら落下処理をする
                    if (isFloatingBlock(x, y))
                    {
                        for (var dy = y + 1; dy >= 0; --dy)
                        {
                            var block = GetBlock(x, dy);
                            var underBlock = GetBlock(x, dy + 1);

                            if (block.type == BlockType.NONE)
                            {
                                continue;
                            }

                            if (block.IsDeleting || underBlock.IsDeleting || underBlock.type != BlockType.NONE)
                            {
                                continue;
                            }

                            if (block.fallWaitFrame > 0)
                            {
                                block.fallWaitFrame--;
                                continue;
                            }
                            else
                            {
                                MoveBlock(x, dy + 1, x, dy);
                            }

                            block.fallWaitFrame = gravityAcceleration;
                        }
                        break;
                    }

                }
            }
        }

        void BlockComboCleanup()
        {
            var blockAllLanded = true;
            var nextComboMax = 0;
            for (var y = Height - 1; y >= 0; --y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var block = GetBlock(x, y);

                    if (block.type == BlockType.NONE)
                    {
                        continue;
                    }

                    var isFloating = isFloatingChunk(x, y);

                    if (!isFloating)
                    {
                        block.nextComboCount = 0;
                    }

                    if (nextComboMax <= block.nextComboCount)
                    {
                        nextComboMax = block.nextComboCount;
                    }

                    if (block.comboCount == globalComboCount)
                    {
                        blockAllLanded = false;
                    }
                }
            }



            if (blockAllLanded && globalComboCount >= 2 && nextComboMax < globalComboCount)
            {
                globalComboCount = 0;
                onComboReset?.Invoke();
            }
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
            block.Init();
        }

        void SetDeleteFlag(int x, int y, int comboCount)
        {
            var block = GetBlock(x, y);
            block.comboCount = comboCount;
            block.nextComboCount = 0;
            blockObjects[block.id].isDeleting = true;
        }

        /// <summary>
        /// そのブロックのひとつ下が空白かどうかを調べる
        /// </summary>
        /// <param name="x">調べるブロックのX座標</param>
        /// <param name="y">調べるブロックのY座標</param>
        /// <returns>ブロックが浮いているかどうか</returns>
        bool isFloatingBlock(int x, int y)
        {
            var block = GetBlock(x, y);
            var underBlock = GetBlock(x, y + 1);

            // 空白ブロックは浮いてないということにする
            if (block.type != BlockType.NONE &&
                // 下のブロックがない == 一番下のブロックなら浮いてない
                underBlock != null &&
                underBlock.type == BlockType.NONE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 指定した位置のブロックの下方向に空白ブロックがないかをチェックする
        /// </summary>
        /// <param name="x">調べるブロックのX座標</param>
        /// <param name="y">調べるブロックのY座標</param>
        /// <returns></returns>
        bool isFloatingChunk(int x, int y)
        {
            for (var dy = y + 1; dy < Height; ++dy)
            {
                var block = GetBlock(x, dy);
                if (block.type == BlockType.NONE)
                {
                    return true;
                }
                // 消去中のブロックは床の役割をする
                if (block.IsDeleting)
                {
                    return false;
                }
            }

            return false;
        }

        (List<(int x, int y)> h, List<(int x, int y)> v) LookupDeleteBlockGroup(int baseX, int baseY)
        {
            var block = GetBlock(baseX, baseY);
            var horizontalDeletingBlocks = new List<(int x, int y)>(5);

            horizontalDeletingBlocks.Add((baseX, baseY));

            // 横方向
            for (var dx = baseX + 1; dx < Width; ++dx)
            {
                // 1マス下が空白なら判定を中断
                if (isFloatingBlock(dx, baseY))
                {
                    break;
                }

                var checkBlock = GetBlock(dx, baseY);
                // 消去中か空白ブロックか違うブロックだったら判定終わり
                if (checkBlock.deleteCount > 0 || checkBlock.type == BlockType.NONE || checkBlock.type != block.type)
                {
                    break;
                }

                horizontalDeletingBlocks.Add((dx, baseY));
            }

            if (horizontalDeletingBlocks.Count < 3)
            {
                horizontalDeletingBlocks.Clear();
            }

            var verticalDeletingBlocks = new List<(int x, int y)>(5);
            verticalDeletingBlocks.Add((baseX, baseY));

            // 縦方向
            for (var dy = baseY + 1; dy < Height; ++dy)
            {
                // 1マス下が空白なら判定を中断
                if (isFloatingBlock(baseX, dy))
                {
                    break;
                }

                var checkBlock = GetBlock(baseX, dy);
                // 空白ブロックか違うブロックだったら判定終わり
                if (checkBlock.deleteCount > 0 || checkBlock.type == BlockType.NONE || checkBlock.type != block.type)
                {
                    break;
                }
                verticalDeletingBlocks.Add((baseX, dy));
            }

            if (verticalDeletingBlocks.Count < 3)
            {
                verticalDeletingBlocks.Clear();
            }

            // 水平方向と垂直方向の消去結果を返す
            return (horizontalDeletingBlocks, verticalDeletingBlocks);
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
