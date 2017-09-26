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
        [Flags]
        public enum VisibleEdge
        {
            Left, Top, Right, Bottom,
        }
        public struct EntityInfo
        {
            public uint         id;
            public bool         isCenter;       // 是否是实体的中心所在格子
            public VisibleEdge  visibleEdge;    // 实体的哪些边是暴露在外的
        }

        // 把锁放在格子内，尽可能减少被锁的可能性
        public object locker = new object();

        /// <summary>
        /// 在此格子内所有的实体，目前只存 id
        /// </summary>
        public List<uint> entities = new List<uint>();
        //public List<EntityInfo> entities = new List<EntityInfo>(); // TODO 大体积怪

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

        private ViewManager _viewManager;
        private object _locker = new object();

        public Map(int id, string name, int width, int height)
        {
            Id      = id;
            Name    = name;
            Width   = width / CellSize;
            Height  = height / CellSize;

            _cells  = new Cell[Width, Height];
        }

        public void SetViewManager(ViewManager viewManager)
        {
            _viewManager = viewManager;
        }

        public Vector2 GetCellOfEntity(uint id)
        {
            Vector2 cellIdx;
            lock (_locker)
            {
                if (!_entity2Cell.TryGetValue(id, out cellIdx))
                {
                    cellIdx.X = -1;
                }
            }
            return cellIdx;
        }

        /// <summary>
        /// 获取特定格子内所有实体(一般只有视野线程需要调用)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool GetCellEntities(int x, int y, ref List<uint> list)
        {
            if (list == null || !CheckPosValid(x, y))
            {
                Console.WriteLine("Invalid cell pos");
                return false;
            }

            Cell cell = _cells[x, y];
            lock (cell.locker)
            {
                // 视野线程获取时每次都复制一份给它，避免出现脏读
                list.AddRange(cell.entities);
                return true;
            }
        }

        /// <summary>
        /// 添加实体到地图
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pos">真实坐标</param>
        /// <returns></returns>
        public bool AddEntity(uint id, Vector2 pos)
        {
            lock (_locker)
            {
                if (_entity2Cell.ContainsKey(id))
                    return false;
            }

            Vector2 cellIdx = PosToCell(pos);

            if (!CheckPosValid(cellIdx.X, cellIdx.Y))
                return false;

            lock(_locker)
                _entity2Cell.Add(id, pos);

            var cell = _cells[cellIdx.X, cellIdx.Y];
            lock(cell.locker)
                cell.entities.Add(id);

            return true;
        }

        /// <summary>
        /// 移除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveEntity(uint id)
        {
            Vector2 cellIdx;
            lock (_locker)
            {
                if (!_entity2Cell.TryGetValue(id, out cellIdx))
                    return false;
            }

            Cell cell = _cells[cellIdx.X, cellIdx.Y];
            lock(cell.locker)
                cell.entities.Remove(id);

            lock(_locker)
                _entity2Cell.Remove(id);

            // 从 ViewManager 中把对应实体立刻移除掉，防止有实体快速的频繁进出 Map 导致出现视野错误
            if (_viewManager != null)
                _viewManager.RemoveEntityFromView(id);
            return true;
        }

        /// <summary>
        /// 移动实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="to">单位是真实坐标</param>
        /// <returns></returns>
        public bool MoveEntity(uint id, Vector2 to)
        {
            if (!CheckPosValid(to))
                return false;

            Vector2 cellIdx;
            lock (_locker)
            {
                if (!_entity2Cell.TryGetValue(id, out cellIdx))
                    return false;
            }

            Cell oldCell = _cells[cellIdx.X, cellIdx.Y];
            lock(oldCell.locker)
                oldCell.entities.Remove(id);

            Vector2 newCellIdx = PosToCell(to);
            Cell newCell = _cells[newCellIdx.X, newCellIdx.Y];
            lock(newCell.locker)
                newCell.entities.Add(id);

            lock(_locker)
                _entity2Cell[id] = newCellIdx;
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
    }
#endregion
}
