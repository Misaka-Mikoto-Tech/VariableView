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
        /// entity 的半径(质点类型的 entity 半径为 0)
        /// </summary>
        public int radius { get; private set; }

        /// <summary>
        /// 可视距离
        /// </summary>
        public int ViewDistance { get; private set; }

        /// <summary>
        /// 自己所在格子们
        /// </summary>
        public List<Cell> placeCells = new List<Cell>();

        /// <summary>
        /// 关注的格子
        /// </summary>
        public List<Cell> watchCells = new List<Cell>();
        /// <summary>
        /// entity 所在的 map
        /// </summary>
        public Map map { get; private set; }

        public Entity(uint id, int viewDistance, int radius, int mapId)
        {
            Id = id;
            ViewDistance = viewDistance;
            this.radius = radius;
            map = MapManager.Instance.GetMapById(mapId);
        }

        /// <summary>
        /// 获取关注列表
        /// </summary>
        /// <returns></returns>
        public List<Entity> GetWatchEntityList()
        {
            List<Entity> list = new List<Entity>();
            foreach (var cell in watchCells)
                list.AddRange(cell.entities);
            return list;
        }

        /// <summary>
        /// 获取关注者列表
        /// </summary>
        /// <returns></returns>
        public HashSet<Entity> GetWatcherList()
        {
            HashSet<Entity> hashset = new HashSet<Entity>();
            foreach(var cell in placeCells)
            {
                foreach(var entity in cell.watchers)
                {
                    hashset.Add(entity);
                }
            }
                
            return hashset;
        }

        public void ChangeViewDistance(int viewDistance)
        {
            map.ChangeEntityViewDistance(this, ViewDistance);
            ViewDistance = viewDistance;
        }

        /// <summary>
        /// 更改实体的体积, 比如变身等行为
        /// </summary>
        /// <param name="newRadius"></param>
        public void ChangeRadius(int newRadius)
        {
            Console.WriteLine($">>> Entity {Id} change radius from {radius} to {newRadius}");

            map.ChangeEntityRadius(this, newRadius);
            radius = newRadius;
        }

        public void MoveTo(Vector2 newPos)
        {
            Console.WriteLine($">>> Entity {Id} moved to {newPos.ToString()}");

            map.EntityMove(this, newPos);
            Pos = newPos;
        }

        public void NotifyEntityEnter(Entity otherEntity, Vector2 cellIdx)
        {
            Console.WriteLine($"Entity {otherEntity.Id} Enter view of entity {Id} at {cellIdx.ToString()}");
        }
        public void NotifyEntityLeave(Entity otherEntity, Vector2 cellIdx)
        {
            Console.WriteLine($"Entity {otherEntity.Id} Leave view of entity {Id} at {cellIdx.ToString()}");
        }
    }
}
