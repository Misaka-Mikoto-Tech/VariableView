//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using VariableView.Utils;

//namespace VariableView
//{
//    public enum ViewType
//    {
//        Near,
//        Midium,
//        Far,
//        All = Near | Midium | Far,
//    }

//    /// <summary>
//    /// 视野管理器
//    /// TODO 可以每个地图一个实例,也可以开多个实例，但是每个实例只处理部分地图
//    /// </summary>
//    public class ViewManager
//    {
//        public const int NearDis    = 2; // 要保证最近视野也比玩家可攻击到的范围要远，防止刷新不及时导致看不到攻击者
//        public const int MidiumDis  = 5;
//        public const int FarDis     = 7;

//        private class EntityView
//        {
//            public uint         id;
//            public int          viewDistance;
//            public uint         counter;
//            // 近视野多少次后扫描一次远视野, 设置不同值以防止玩家同时进入导致同时扫描远视野造成近视野不能及时刷新
//            public byte         midiumInterval;
//            public byte         farInterval;

//            public object       locker = new object();
            
//            public DoubleBufferList<uint> listenList    = new DoubleBufferList<uint>();
//            public DoubleBufferList<uint> listenerList  = new DoubleBufferList<uint>();
//        }

//        private Map     _map; // TODO List<Map>
//        private Thread  _workThread;
//        private bool    _isTerminated  = false;

//        // 实体id对应的视野数据
//        private Dictionary<uint, EntityView> _entityViews = new Dictionary<uint, EntityView>();
//        private object _locker;
//        private Random _rnd = new Random();

//        private void Init()
//        {
//            _map = MapManager.Instance.GetMapById(1);
//            Debug.Assert(_map != null);
//        }

//        public void Start()
//        {
//            if(_workThread != null)
//            {
//                Console.WriteLine("thread already started");
//                return;
//            }

//            _isTerminated = false;
//            _workThread = new Thread(ThreadFunc);
//            _workThread.Start(this);
//        }

//        public void Stop()
//        {
//            _isTerminated = true;
//            _workThread.Join();
//            _workThread = null;
//        }

//        /// <summary>
//        /// 从视野中移除实体，一般由 map 调用
//        /// </summary>
//        /// <param name="id"></param>
//        /// <remarks>调用线程：主线程</remarks>
//        public void RemoveEntityFromView(uint id)
//        {
//            lock(_locker)
//            _entityViews.Remove(id);
//        }

//        /// <summary>
//        /// 添加实体到视野，并立刻生成近视野，一般由 map  调用
//        /// </summary>
//        /// <param name="entity"></param>
//        /// /// <remarks>调用线程：主线程</remarks>
//        public void AddEntity(Entity entity)
//        {
//            EntityView entityView       = new EntityView();
//            entityView.id               = entity.Id;
//            entityView.midiumInterval   = (byte)(5 + _rnd.Next(2));
//            entityView.midiumInterval   = (byte)(10 + _rnd.Next(3));
//            entityView.viewDistance     = entity.ViewDistance / _map.CellSize;

//            lock (_locker)
//                _entityViews.Add(entity.Id, entityView);

//            // 立刻生成近视野
//            Vector2 cellIdx = _map.PosToCell(entity.Pos);
//            ProcessEntityView(entity.Id, cellIdx.X, cellIdx.Y, true);
//        }

//        /// <summary>
//        /// 获取关注列表(主线程调用,角色刚调到地图时第一次仅能获取近视野[即使他是千里眼])
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="viewType"></param>
//        /// <returns></returns>
//        public List<uint> GetListenList(uint id)
//        {
//            EntityView entityView;
//            lock (_locker)
//                _entityViews.TryGetValue(id, out entityView);

//            if (entityView == null)
//                return null;

//            return entityView.listenList.GetCurrent();
//        }

//        /// <summary>
//        /// 获取关注者列表(主线程调用)
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="viewType"></param>
//        /// <returns></returns>
//        public List<uint> GetListenerList(uint id, ViewType viewType)
//        {
//            EntityView entityView;
//            lock (_locker)
//                _entityViews.TryGetValue(id, out entityView);

//            if (entityView == null)
//                return null;

//            return entityView.listenerList.GetCurrent();
//        }

//#region Thread
//        private void ThreadFunc(object viewManager)
//        {
//            while (!_isTerminated)
//                ScanMap();
//        }

//        private void ScanMap()
//        {
//        }

//        /// <summary>
//        /// 获取一个实体的视野，可能由线程调用，也可能由主线程调用(人物跳到当前地图需要立即获取近视野时[TODO 或许可以等一会儿再下发可见角色？])
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="x"></param>
//        /// <param name="y"></param>
//        /// <param name="onlyNear">是否只获取近视野</param>
//        private void ProcessEntityView(uint id, bool onlyNear = false)
//        {
//            EntityView entityView;
//            int maxDis = 0;

//            lock (_locker)
//            {
//                if (!_entityViews.TryGetValue(id, out entityView)) { return; }
//            }

//            if (onlyNear)
//                maxDis = NearDis;
//            else
//            {
//                lock (entityView.locker)
//                {
//                    if (entityView.counter % entityView.farInterval == 0)
//                        maxDis = FarDis;
//                    else if (entityView.counter % entityView.midiumInterval == 0)
//                        maxDis = MidiumDis;
//                    else
//                        maxDis = NearDis;
//                }
//            }
//            maxDis = Math.Min(entityView.viewDistance, maxDis);

//            Vector2 cellIdx = _map.GetCellOfEntity(id);
//            if (cellIdx.X == -1)
//                return;

//            List<uint> list = new List<uint>();
//            for(int x = Math.Min(0, cellIdx.X - maxDis); x <= Math.Max(_map.Width - 1, cellIdx.X + maxDis); x++)
//            {
//                for(int y = Math.Min(0, cellIdx.Y - maxDis); y <= Math.Max(_map.Height - 1, cellIdx.Y + maxDis); y++)
//                {
//                    _map.GetCellEntities(x, y, ref list);
//                }
//            }

//            // 更新当前 Entity 的订阅列表
//            lock(entityView.locker)
//            {
//                var backList = entityView.listenList.GetBack();
//                backList.AddRange(list); // 这是增加，删除怎么办
//                entityView.listenList.SwapQueue();
//            }
//        }
//#endregion
//    }
//}
