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

        public Vector2 Pos { get; private set; }

        /// <summary>
        /// 可视距离
        /// </summary>
        public int ViewDistance { get; private set; }

        public Entity(uint id, int viewDistance)
        {
            Id = id;
            ViewDistance = viewDistance;
        }

        /// <summary>
        /// 获取关注列表
        /// </summary>
        /// <returns></returns>
        public List<uint> GetListenList()
        {
            return null;
        }

        /// <summary>
        /// 获取关注者列表
        /// </summary>
        /// <returns></returns>
        public List<uint> GetListenerList()
        {
            return null;
        }

        public void ChangeViewDistance(int viewDistance)
        {
            ViewDistance = viewDistance;
        }

        public void MoveTo(Vector2 newPos)
        {

        }
    }
}
