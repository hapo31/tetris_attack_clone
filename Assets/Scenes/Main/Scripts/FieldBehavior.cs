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
        public delegate BlockObject OnInstantiateBlock();

        public event OnInstantiateBlock onInstantiateBlock;

        Block[] blocks;
        Dictionary<int, BlockObject> blockObjects = new Dictionary<int, BlockObject>();

        int Width;
        int Height;

        public int blockDeleteCountLimit = 60 * 5;

        public FieldBehavior(int width, int height)
        {
            blocks = new Block[width * height];
            for(var i = 0; i < width * height; ++i)
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
            var rand = new System.Random();

            for (var y = Height - 4; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var blockType = (BlockType)rand.Next(5);
                    CreateBlock(x, y, blockType);
                }
            }
        }


        public void Update()
        {
            // TODO: ブロックを動かす処理で見た目(BlockObject)が移動していない

            // ブロックを落下させる
            for (var x = 0; x < Width; ++x)
            {
                for (var y = Height - 1; y >= 0; --y)
                {
                    var block = blocks[y * Width + x];
                    // そのマスが空白だったら上に乗っているブロックを一つずつ下にずらす
                    if (block.type == BlockType.NONE)
                    {
                        for (var dy = y; dy > 1; --dy)
                        {
                            blocks[dy * Width + x].type = blocks[(dy - 1) * Width + x].type;
                        }

                        blocks[0 * Width + x].type = BlockType.NONE;
                        // 1フレームに1マスずつ落とす
                        break;
                    }
                }
            }

            // TODO: このままだとブロック落下中に揃っても消えてしまうので isDropping 的なフラグが要る

            // ブロックの消去チェック
            for (var y = 0; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var block = blocks[y * Width + x];

                    if (!block.isDeleting)
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
            if (!blocks[y * Width + x].isDeleting && !blocks[y * Width + x + 1].isDeleting)
            {
                var t = blocks[y * Width + x].type;
                blocks[y * Width + x].type = blocks[y * Width + x + 1].type;
                blocks[y * Width + x + 1].type = t;
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
            if (blockObjects.ContainsKey(y * Width + x))
            {
                blockObjects[y * Width + x].isSelected = true;
            }
            if (blockObjects.ContainsKey(y * Width + x + 1))
            {
                blockObjects[y * Width + x + 1].isSelected = true;
            }
        }

        /// <summary>
        /// 指定した座標値のブロックを設定しBlockObjectをInstantiateする
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        void CreateBlock(int x, int y, BlockType blockType)
        {
            blocks[y * Width + x].type = blockType;
            var blockObject = onInstantiateBlock.Invoke();
            blockObject.type = blockType;
            blockObject.transform.position = new Vector3(x, y);

            blockObjects.Add(y * Width + x, blockObject);
        }

        /// <summary>
        /// 指定した座標のブロックをNONEにし、BlockObjectを消す
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        void DeleteBlock(int x, int y)
        {
            blocks[y * Width + x].type = BlockType.NONE;
            blockObjects.Remove(y * Width + x);
        }

        void SetDeleteFlag(int x, int y)
        {
            blocks[y * Width + x].isDeleting = true;
            blockObjects[y * Width + x].isDeleting = true;
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
    }
}
