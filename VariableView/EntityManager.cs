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
            Entity entity;
            if (_allEntites.TryGetValue(id, out entity))
            {
                return entity;
            }
            return null;
        }

        public bool AddEntity(Entity entity)
        {
            entity.map.AddEntity(entity);
            _allEntites.Add(entity.Id, entity);
            return true;
        }

        public bool RemoveEntity(Entity entity)
        {
            entity.map.RemoveEntity(entity);
            _allEntites.Remove(entity.Id);
            return true;
        }

        public bool RemoveEntityById(uint id)
        {
            Entity entity;
            if(_allEntites.TryGetValue(id, out entity))
            {
                RemoveEntity(entity);
                return true;
            }
            return false;
        }
    }
}
