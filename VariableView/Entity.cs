using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableView
{
    /// <summary>
    /// 实体
    /// </summary>
    public class Entity
    {
        public uint Id { get; private set; }

        public Vector2 Pos { get; set; }

        /// <summary>
        /// 可视距离
        /// </summary>
        public int ViewDistance { get; private set; }

        /// <summary>
        /// 自己所在格子
        /// </summary>
        public Cell cell;

        /// <summary>
        /// 关注的格子
        /// </summary>
        public List<Cell> WatchCells = new List<Cell>();
        /// <summary>
        /// entity 所在的 map
        /// </summary>
        public Map map { get; private set; }

        public Entity(uint id, int viewDistance, int mapId)
        {
            Id = id;
            ViewDistance = viewDistance;
            map = MapManager.Instance.GetMapById(mapId);
        }

        /// <summary>
        /// 获取关注列表
        /// </summary>
        /// <returns></returns>
        public List<Entity> GetWatchEntityList()
        {
            List<Entity> list = new List<Entity>();
            foreach (var cell in WatchCells)
                list.AddRange(cell.entities);
            return list;
        }

        /// <summary>
        /// 获取关注者列表
        /// </summary>
        /// <returns></returns>
        public List<Entity> GetWatcherList()
        {
            return cell.watchers;
        }

        public void ChangeViewDistance(int viewDistance)
        {
            ViewDistance = viewDistance;
        }

        public void MoveTo(Vector2 newPos)
        {
            map.EntityMove(this, newPos);
            Pos = newPos;
            Console.WriteLine($"Entity {Id} moved to {newPos.ToString()}");
        }

        public void NotifyEntityEnter(Entity otherEntity)
        {
            Console.WriteLine($"Entity {otherEntity.Id} Enter view of entity {Id}");
        }
        public void NotifyEntityLeave(Entity otherEntity)
        {
            Console.WriteLine($"Entity {otherEntity.Id} Leave view of entity {Id}");
        }
    }
}
