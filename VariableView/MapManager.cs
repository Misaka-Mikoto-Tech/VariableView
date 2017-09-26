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

        public void AddMap(int id, string name, int width, int height)
        {
            Map map = new Map(id, name, width, height);
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
