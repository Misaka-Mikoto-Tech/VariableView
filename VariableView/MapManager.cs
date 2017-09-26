using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableView
{
    public class MapManager : Singleton<MapManager>
    {
        Dictionary<int, Map> _maps = new Dictionary<int, Map>();

        public MapManager()
        {
            Map map = new Map(1, "亚特兰蒂斯", 400, 400);
            _maps.Add(map.Id, map);
        }

        public Map GetMapById(int id)
        {
            Map map;
            _maps.TryGetValue(id, out map);
            return map;
        }
    }
}
