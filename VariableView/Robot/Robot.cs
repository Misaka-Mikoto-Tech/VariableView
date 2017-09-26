using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableView.Robot
{
    public class Robot
    {
        public void Run()
        {
            CreateMap();
            CreateEntity();
            MoveEntity();
            RemoveEntity();
        }
        
        public void CreateMap()
        {
            MapManager.Instance.AddMap(1, "亚特兰蒂斯", 400, 400);
        }

        public void CreateEntity()
        {
            Entity[] entities = new Entity[4]
            {
                new Entity(1, 1, 1) { Pos = new Vector2(20, 20)},
                new Entity(2, 1, 1) { Pos = new Vector2(30, 40)},
                new Entity(3, 3, 1) { Pos = new Vector2(50, 45)},
                new Entity(4, 10, 1) { Pos = new Vector2(40, 80)}
            };

            foreach(var entity in entities)
                EntityManager.Instance.AddEntity(entity);
        }

        public void MoveEntity()
        {
            Entity entity = EntityManager.Instance.GetEntityById(2);
            entity.MoveTo(new Vector2(40, 30));

            entity = EntityManager.Instance.GetEntityById(1);
            entity.MoveTo(new Vector2(46, 85));
        }

        public void RemoveEntity()
        {
            EntityManager.Instance.RemoveEntityById(3);
        }
    }
}
