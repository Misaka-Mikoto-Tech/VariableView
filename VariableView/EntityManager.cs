using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableView
{
    /// <summary>
    /// 实体管理器
    /// </summary>
    public class EntityManager : Singleton<EntityManager>
    {
        private Dictionary<uint, Entity> _allEntites = new Dictionary<uint, Entity>();

        public Entity GetEntityById(uint id)
        {
            return null;
        }

        public bool AddEntity(Entity entity)
        {
            return false;
        }

        public bool RemoveEntity(uint id)
        {
            return false;
        }
    }
}
