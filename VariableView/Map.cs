using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VariableView.Utils.Utils;

namespace VariableView
{
    /// <summary>
    /// 地图格子
    /// </summary>
    public class Cell
    {
        public Vector2 idx;
        public List<Entity> entities = new List<Entity>();
        public List<Entity> watchers = new List<Entity>();
    }

    /// <summary>
    /// 地图
    /// </summary>
    public class Map
    {
        /// <summary>
        /// 格子大小，越小精度越高，但是会占用更多的内存和计算资源
        /// </summary>
        public int CellSize { get; private set; }

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

        public Map(int id, string name, int width, int height, int cellSize)
        {
            Id          = id;
            Name        = name;
            CellSize    = cellSize;
            Width       = width / CellSize;
            Height      = height / CellSize;

            _cells  = new Cell[Width, Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _cells[x, y] = new Cell() { idx = new Vector2(x, y) };
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
            if (entity.radius / CellSize == 0)
                return AddEntity_Point(entity);
            else
                return AddEntity_Non_Point(entity);
        }

        /// <summary>
        /// 移除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveEntity(Entity entity)
        {
            if (entity.radius / CellSize == 0)
                return RemoveEntity_Point(entity);
            else
                return RemoveEntity_Non_Point(entity);
        }

        /// <summary>
        /// 移动实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="to">单位是真实坐标</param>
        /// <returns></returns>
        public bool EntityMove(Entity entity, Vector2 to)
        {
            if (entity.radius / CellSize == 0)
                return EntityMove_Point(entity, to);
            else
                return EntityMove_Non_Point(entity, to);
        }

        /// <summary>
        /// 更改实体的可视范围
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="newViewDistance"></param>
        public void ChangeEntityViewDistance(Entity entity, int newViewDistance)
        {
            if (entity.ViewDistance / CellSize == newViewDistance / CellSize)
                return;

            if (entity.radius / CellSize == 0)
                ChangeEntityViewDistance_Point(entity, newViewDistance);
            else
                ChangeEntityViewDistance_Non_Point(entity, newViewDistance);
        }

        /// <summary>
        /// 更改实体的体积, 比较耗资源，请慎用
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="newRadius"></param>
        public void ChangeEntityRadius(Entity entity, int newRadius)
        {
            newRadius = newRadius / CellSize;
            Vector2 cellIdx = PosToCell(entity.Pos);

            if (entity.radius / CellSize == newRadius)
                return;

            List<Cell> oldPlaceCells = entity.placeCells;
            List<Cell> oldWatchCells = entity.watchCells;

            List<Cell> newPlaceCells = GetEntityPlaceCells(cellIdx, newRadius);
            List<Cell> newWatchCells = GetRangeEntityWatchCells(newPlaceCells, entity.ViewDistance / CellSize);

            // 处理占据格子的改变
            ProcessEntityPlaceCellChange(entity, oldPlaceCells, newPlaceCells);
            // 处理关注列表改变
            ProcessEntityWatchCellChange(entity, oldWatchCells, newWatchCells);
        }


        #region 质点的逻辑
        public bool AddEntity_Point(Entity entity)
        {
            if (_entity2Cell.ContainsKey(entity.Id))
                return false;

            Vector2 cellIdx = PosToCell(entity.Pos);
            if (!CheckPosValid(cellIdx.X, cellIdx.Y))
                return false;

            _entity2Cell.Add(entity.Id, cellIdx);

            // 设置此 Entity 占据的格子
            Cell currCell = _cells[cellIdx.X, cellIdx.Y];
            entity.placeCells.Clear();
            entity.placeCells.Add(currCell);

            currCell.entities.Add(entity);

            // 向此格子观测者发送进入消息
            foreach (var watcher in currCell.watchers)
                watcher.NotifyEntityEnter(entity, currCell.idx);

            // 把自己注册为可观测范围内格子的关注者
            int dis = entity.ViewDistance / CellSize;
            var watchList = GetEntityWatchCells(cellIdx, dis);
            foreach (var cell in watchList)
            {
                cell.watchers.Add(entity);
                entity.watchCells.Add(cell);
            }

            return true;
        }

        public bool RemoveEntity_Point(Entity entity)
        {
            Vector2 cellIdx;
            if (!_entity2Cell.TryGetValue(entity.Id, out cellIdx))
                return false;

            _entity2Cell.Remove(entity.Id);

            Debug.Assert(entity.placeCells != null && entity.placeCells.Count == 1);

            Cell currCell = entity.placeCells[0];

            currCell.entities.Remove(entity);
            currCell.watchers.Remove(entity); // 自己也关注了自己所在的格子

            // 向当前格子的观测者发送离开消息
            foreach (var watcher in currCell.watchers)
                watcher.NotifyEntityLeave(entity, currCell.idx);

            // 把 entity 从它关注的格子中取消自己的注册
            foreach (var cell in entity.watchCells)
                cell.watchers.Remove(entity);

            return true;
        }

        public bool EntityMove_Point(Entity entity, Vector2 to)
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

            _entity2Cell[entity.Id] = to;

            int dis = entity.ViewDistance / CellSize;

            #region 处理占据的格子改变相关逻辑
            {
                Debug.Assert(entity.placeCells != null && entity.placeCells.Count == 1);

                Cell oldPlaceCell = entity.placeCells[0];
                Cell newPlaceCell = _cells[to.X, to.Y];

                // 向旧格子的观测者发送离开消息并把自己从其中移除
                foreach (var watcher in oldPlaceCell.watchers)
                {
                    if (watcher != entity)
                        watcher.NotifyEntityLeave(entity, oldPlaceCell.idx);
                }

                oldPlaceCell.entities.Remove(entity);

                // 把 Entity 添加到新格子的节点列表内并向新格子的观测者发送进入消息
                foreach (var watcher in newPlaceCell.watchers)
                {
                    if (watcher != entity)
                        watcher.NotifyEntityEnter(entity, newPlaceCell.idx);
                }

                newPlaceCell.entities.Add(entity);

                entity.placeCells.Clear();
                entity.placeCells.Add(newPlaceCell);
            }
            #endregion

            #region 处理关注的格子发生改变相关逻辑
            {
                /*
                    因为关注者注册的改变不会触发事件，
                    所以可以暴力处理, 计算差集和直接暴力全移除旧的然后再全部添加新的不清楚哪个性能更高，目前先暴力处理一下
                 */
                List<Cell> oldWatchCells = entity.watchCells;
                List<Cell> newWatchCells = GetEntityWatchCells(to, dis);

                // 把自己从已经离开的关注的格子中反注册自己
                foreach (var cell in oldWatchCells)
                {
                    cell.watchers.Remove(entity);
                }

                // 把自己注册为新关注格子的关注者
                foreach (var cell in newWatchCells)
                {
                    cell.watchers.Add(entity);
                }

                entity.watchCells.Clear();
                entity.watchCells.AddRange(newWatchCells);
            }
            #endregion
            return true;
        }

        public void ChangeEntityViewDistance_Point(Entity entity, int newViewDistance)
        {
            newViewDistance = newViewDistance / CellSize;

            Vector2 cellIdx = PosToCell(entity.Pos);
            List<Cell> oldWatchCells = entity.watchCells;
            List<Cell> newWatchCells = GetEntityWatchCells(cellIdx, newViewDistance);

            // 把自己从已经离开的关注的格子中反注册自己
            foreach (var cell in oldWatchCells)
            {
                cell.watchers.Remove(entity);
            }

            // 把自己注册为新关注格子的关注者
            foreach (var cell in newWatchCells)
            {
                cell.watchers.Add(entity);
            }

            entity.watchCells.Clear();
            entity.watchCells.AddRange(newWatchCells);
        }
        #endregion

        #region 非质点的逻辑

        public bool AddEntity_Non_Point(Entity entity)
        {
            if (_entity2Cell.ContainsKey(entity.Id))
                return false;

            Vector2 cellIdx = PosToCell(entity.Pos);
            if (!CheckPosValid(cellIdx.X, cellIdx.Y))
                return false;

            _entity2Cell.Add(entity.Id, cellIdx);

            // 设置此 Entity 占据的格子
            var placeList = GetEntityPlaceCells(cellIdx, entity.radius / CellSize);
            entity.placeCells.Clear();
            entity.placeCells.AddRange(placeList);

            foreach(var cell in entity.placeCells)
            {
                cell.entities.Add(entity);

                // 向此格子观测者发送进入消息
                foreach (var watcher in cell.watchers)
                    watcher.NotifyEntityEnter(entity, cell.idx);
            }

            // 把自己注册为可观测范围内格子的关注者
            int dis = entity.ViewDistance / CellSize;
            var watchList = GetRangeEntityWatchCells(entity.placeCells, dis);
            foreach(var cell in watchList)
            {
                cell.watchers.Add(entity);
                entity.watchCells.Add(cell);
            }
            
            return true;
        }

        
        public bool RemoveEntity_Non_Point(Entity entity)
        {
            Vector2 cellIdx;
            if (!_entity2Cell.TryGetValue(entity.Id, out cellIdx))
                return false;

            _entity2Cell.Remove(entity.Id);

            foreach(var cell in entity.placeCells)
            {
                cell.entities.Remove(entity);
                cell.watchers.Remove(entity); // 自己也关注了自己所在的格子

                // 向当前格子的观测者发送离开消息
                foreach (var watcher in cell.watchers)
                    watcher.NotifyEntityLeave(entity, cell.idx);
            }

            // 把 entity 从它关注的格子中取消自己的注册
            foreach (var cell in entity.watchCells)
                cell.watchers.Remove(entity);

            return true;
        }

        
        public bool EntityMove_Non_Point(Entity entity, Vector2 to)
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

            _entity2Cell[entity.Id] = to;

            int dis = entity.ViewDistance / CellSize;

            // 处理占据的格子改变相关逻辑
            {
                List<Cell> oldPlaceCells = new List<Cell>(entity.placeCells);
                List<Cell> newPlaceCells = GetEntityPlaceCells(to, entity.radius / CellSize);

                ProcessEntityPlaceCellChange(entity, oldPlaceCells, newPlaceCells);
            }

            // 处理关注的格子发生改变相关逻辑
            {
                List<Cell> oldWatchCells = new List<Cell>(entity.watchCells);
                List<Cell> newWatchCells = GetRangeEntityWatchCells(entity.placeCells, dis);

                ProcessEntityWatchCellChange(entity, oldWatchCells, newWatchCells);
            }

            return true;
        }

        public void ChangeEntityViewDistance_Non_Point(Entity entity, int newViewDistance)
        {
            newViewDistance = newViewDistance / CellSize;

            List<Cell> oldWatchCells = new List<Cell>(entity.watchCells);
            List<Cell> newWatchCells = GetRangeEntityWatchCells(entity.placeCells, newViewDistance);

            // 计算关注的旧的格子和新的格子的交集和差集
            int splitIdx = CalcIntersectionAndSubtraction(oldWatchCells, newWatchCells);

            // 把自己从已经离开的关注的格子中反注册自己
            for (int i = splitIdx; i < oldWatchCells.Count; i++)
            {
                oldWatchCells[i].watchers.Remove(entity);
            }

            // 把自己添加到新增加关注的格子的关注列表
            for (int i = splitIdx; i < newWatchCells.Count; i++)
            {
                newWatchCells[i].watchers.Add(entity);
            }

            entity.watchCells.Clear();
            entity.watchCells.AddRange(newWatchCells);
        }
        #endregion // 非质点

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

        /// <summary>
        /// 真实坐标转换为格子坐标
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2 PosToCell(Vector2 pos)
        {
            return new Vector2(pos.X / CellSize, pos.Y / CellSize);
        }

        /// <summary>
        /// 获取 Entity 关注的所有格子
        /// </summary>
        /// <param name="cellIdx"></param>
        /// <param name="dis"></param>
        /// <returns></returns>
        private List<Cell> GetEntityWatchCells(Vector2 cellIdx, int dis)
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

        /// <summary>
        /// 批量获取关注格子(非质点 entity)
        /// </summary>
        /// <param name="cellIdxes"></param>
        /// <param name="dis"></param>
        /// <returns></returns>
        private List<Cell> GetRangeEntityWatchCells(List<Cell> cellIdxes, int dis)
        {
            HashSet<Cell> hashset = new HashSet<Cell>();
            foreach(var cell in cellIdxes)
            {
                var watchList = GetEntityWatchCells(cell.idx, dis);
                foreach (var wcell in watchList)
                    hashset.Add(wcell);
            }
            
            return new List<Cell>(hashset);
        }

        /// <summary>
        /// 获取 Entity 占据的所有格子(radius 大于 0 的)
        /// </summary>
        /// <param name="cellIdx"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private List<Cell> GetEntityPlaceCells(Vector2 cellIdx, int radius)
        {
            List<Cell> list = new List<Cell>();

            // 找4个顶点的坐标
            int minX = _cells[Math.Max(0, cellIdx.X - radius), cellIdx.Y].idx.X;
            int maxX = _cells[Math.Min(Width, cellIdx.X + radius), cellIdx.Y].idx.X;
            int minY = _cells[cellIdx.X, Math.Max(0, cellIdx.Y - radius)].idx.Y;
            int maxY = _cells[cellIdx.X, Math.Min(cellIdx.Y + radius, Height)].idx.Y;

            for(int x = minX; x <= maxX; x++)
            {
                for(int y = minY; y <= maxY; y++)
                {
                    list.Add(_cells[x, y]);
                }
            }

            return list;
        }

        /// <summary>
        /// 处理 Entity 占据格子列表改变
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="oldPlaceCells"></param>
        /// <param name="newPlaceCells"></param>
        private void ProcessEntityPlaceCellChange(Entity entity, List<Cell> oldPlaceCells, List<Cell> newPlaceCells)
        {
            // 计算占据的旧的格子和新的格子的交集和差集
            int splitIdx = CalcIntersectionAndSubtraction(oldPlaceCells, newPlaceCells);

            // 向差集中旧格子的观测者发送离开消息并把自己从其中移除
            for (int i = splitIdx; i < oldPlaceCells.Count; i++)
            {
                Cell cell = oldPlaceCells[i];
                foreach (var watcher in cell.watchers)
                {
                    if (watcher != entity)
                        watcher.NotifyEntityLeave(entity, cell.idx);
                }

                cell.entities.Remove(entity);
            }

            // 把 Entity 添加到差集中新格子的节点列表内
            for (int i = splitIdx; i < newPlaceCells.Count; i++)
            {
                Cell cell = newPlaceCells[i];

                // 向新格子的观测者发送进入消息
                foreach (var watcher in cell.watchers)
                {
                    if (watcher != entity)
                        watcher.NotifyEntityEnter(entity, cell.idx);
                }

                cell.entities.Add(entity);
            }

            entity.placeCells.Clear();
            entity.placeCells.AddRange(newPlaceCells);
        }

        /// <summary>
        /// 处理 Entity 关注列表改变
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="oldWatchCells"></param>
        /// <param name="newWatchCells"></param>
        private void ProcessEntityWatchCellChange(Entity entity, List<Cell> oldWatchCells, List<Cell> newWatchCells)
        {
            // 计算关注的旧的格子和新的格子的交集和差集
            int splitIdx = CalcIntersectionAndSubtraction(oldWatchCells, newWatchCells);

            // 把自己从已经离开的关注的格子中反注册自己
            for (int i = splitIdx; i < oldWatchCells.Count; i++)
            {
                oldWatchCells[i].watchers.Remove(entity);
            }

            // 把自己添加到新增加关注的格子的关注列表
            for (int i = splitIdx; i < newWatchCells.Count; i++)
            {
                newWatchCells[i].watchers.Add(entity);
            }

            entity.watchCells.Clear();
            entity.watchCells.AddRange(newWatchCells);
        }
        #endregion
    }

}
