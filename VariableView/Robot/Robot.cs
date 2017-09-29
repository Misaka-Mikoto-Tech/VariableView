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
            ChangeViewDistance();
            MoveEntity2();
            RemoveEntity();
            ChangeRadius();
        }
        
        public void CreateMap()
        {
            MapManager.Instance.AddMap(1, "亚特兰蒂斯", 400, 400, 10);
        }

        public void CreateEntity()
        {
            Entity[] entities = new Entity[4]
            {
                // Entity 构造函数参数: id, viewDistance, radius, mapId
                new Entity(1, 12, 3, 1) { Pos = new Vector2(20, 20)},
                new Entity(2, 15, 0, 1) { Pos = new Vector2(30, 40)},
                new Entity(3, 34, 0, 1) { Pos = new Vector2(50, 70)},
                new Entity(4, 70, 15, 1) { Pos = new Vector2(100, 125)}
            };

            foreach(var entity in entities)
                EntityManager.Instance.AddEntity(entity);
        }

        public void MoveEntity()
        {
            Entity entity = EntityManager.Instance.GetEntityById(1);
            entity.MoveTo(new Vector2(40, 42));
            entity.MoveTo(new Vector2(160, 170));
        }

        public void RemoveEntity()
        {
            EntityManager.Instance.RemoveEntityById(1);
        }

        public void ChangeViewDistance()
        {
            Entity entity = EntityManager.Instance.GetEntityById(4);
            entity.ChangeViewDistance(12);
        }

        public void MoveEntity2()
        {
            Entity entity = EntityManager.Instance.GetEntityById(1);
            entity.MoveTo(new Vector2(150, 160));
        }

        public void ChangeRadius()
        {
            Entity entity = EntityManager.Instance.GetEntityById(2);
            entity.ChangeRadius(17);
        }
    }
}
