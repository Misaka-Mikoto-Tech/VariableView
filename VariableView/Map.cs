using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableView
{
    /// <summary>
    /// 地图格子
    /// </summary>
    public class Cell
    {
        public List<Entity> entities = new List<Entity>();
        public List<Entity> watchers = new List<Entity>();
    }

    /// <summary>
    /// 地图
    /// </summary>
    public class Map
    {
        public int CellSize = 10;

        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private Cell[,] _cells;

        /// <summary>
        /// 记录每个实体在哪个[/哪些]格子内
        /// </summary>
        private Dictionary<uint, Vector2> _entity2Cell = new Dictionary<uint, Vector2>();
        //private Dictionary<uint, List<Vector2>> _entity2Cell = new Dictionary<uint, List<Vector2>>();

        public Map(int id, string name, int width, int height)
        {
            Id      = id;
            Name    = name;
            Width   = width / CellSize;
            Height  = height / CellSize;

            _cells  = new Cell[Width, Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _cells[x, y] = new Cell();
        }

        public Vector2 GetCellOfEntity(uint id)
        {
            Vector2 cellIdx;
            if (!_entity2Cell.TryGetValue(id, out cellIdx))
            {
                cellIdx.X = -1;
            }
            return cellIdx;
        }

        /// <summary>
        /// 添加实体到地图
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pos">真实坐标</param>
        /// <returns></returns>
        public bool AddEntity(Entity entity)
        {
            if (_entity2Cell.ContainsKey(entity.Id))
                return false;

            Vector2 cellIdx = PosToCell(entity.Pos);
            if (!CheckPosValid(cellIdx.X, cellIdx.Y))
                return false;

            _entity2Cell.Add(entity.Id, cellIdx);

            var currCell = _cells[cellIdx.X, cellIdx.Y];
            currCell.entities.Add(entity);

            // 向当前格子观测者发送进入消息
            foreach (var __entity in currCell.watchers)
                __entity.NotifyEntityEnter(entity);

            // 设置它的观测范围
            int dis = entity.ViewDistance / CellSize;
            var watchList = GetEntityWatchCells(cellIdx, dis);
            foreach(var cell in watchList)
            {
                cell.watchers.Add(entity);
                entity.WatchCells.Add(cell);
            }

            entity.cell = currCell;
            return true;
        }

        /// <summary>
        /// 移除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveEntity(Entity entity)
        {
            Vector2 cellIdx;
            if (!_entity2Cell.TryGetValue(entity.Id, out cellIdx))
                return false;

            _entity2Cell.Remove(entity.Id);

            Cell cell = _cells[cellIdx.X, cellIdx.Y];
            cell.entities.Remove(entity);
            cell.watchers.Remove(entity); // 自己也关注了自己所在的格子

            // 向当前格子的观测者发送离开消息
            foreach (var __entity in cell.watchers)
                __entity.NotifyEntityLeave(entity);

            return true;
        }

        /// <summary>
        /// 移动实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="to">单位是真实坐标</param>
        /// <returns></returns>
        public bool EntityMove(Entity entity, Vector2 to)
        {
            to = PosToCell(to);
            if (!CheckPosValid(to))
                return false;

            var from = PosToCell(entity.Pos); // 没有换格子
            if (from == to)
                return false;

            Vector2 cellFromIdx;
            if (!_entity2Cell.TryGetValue(entity.Id, out cellFromIdx))
                return false;

            Cell oldCell = _cells[cellFromIdx.X, cellFromIdx.Y];
            oldCell.entities.Remove(entity);
            oldCell.watchers.Remove(entity); // 自己也关注了自己所在的格子

            // 向旧格子的观测者发送离开消息
            foreach (var __entity in oldCell.watchers)
                __entity.NotifyEntityLeave(entity);

            Cell newCell = _cells[to.X, to.Y];
            newCell.entities.Add(entity);

            // 向新格子的观测者发送进入消息
            foreach (var __entity in newCell.watchers)
                __entity.NotifyEntityEnter(entity);

            newCell.watchers.Add(entity);
            _entity2Cell[entity.Id] = to;

            // 更新关注格子列表
            entity.WatchCells.Remove(oldCell);
            entity.WatchCells.Add(newCell);
            entity.cell = newCell;
            return true;
        }

#region Utility
        private bool CheckPosValid(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Width)
                return false;
            else
                return true;
        }

        private bool CheckPosValid(Vector2 pos)
        {
            return CheckPosValid(pos.X, pos.Y);
        }

        public Vector2 PosToCell(Vector2 pos)
        {
            return new Vector2(pos.X / CellSize, pos.Y / CellSize);
        }

        public List<Cell> GetEntityWatchCells(Vector2 cellIdx, int dis)
        {
            List<Cell> list = new List<Cell>();
            for (int x = Math.Max(0, cellIdx.X - dis); x <= Math.Min(Width, cellIdx.X + dis); x++)
            {
                for (int y = Math.Max(0, cellIdx.Y - dis); y <= Math.Min(Height, cellIdx.Y + dis); y++)
                {
                    list.Add(_cells[x, y]);
                }
            }
            return list;
        }


    }
#endregion
}
