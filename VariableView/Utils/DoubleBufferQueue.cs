using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableView.Utils
{
    /// <summary>
    /// 双缓冲队列
    /// </summary>
    public class DoubleBufferList<T>
    {
        private object _locker = new object();
        private List<T> _queueA = new List<T>();
        private List<T> _queueB = new List<T>();
        private List<T> _currentQueue;

        public DoubleBufferList()
        {
            _currentQueue = _queueA;
        }

        /// <summary>
        /// 获取当前队列
        /// </summary>
        /// <returns></returns>
        public List<T> GetCurrent()
        {
            lock (_locker)
                return _currentQueue;
        }

        public List<T> GetBack()
        {
            lock (_locker)
                return _queueA == _currentQueue ? _queueB : _queueA;
        }

        /// <summary>
        /// 交换队列并获取新的当前队列
        /// </summary>
        /// <returns></returns>
        public List<T> SwapQueue()
        {
            lock (_locker)
            {
                _currentQueue = _currentQueue == _queueA ? _queueB : _queueA;
                return _currentQueue;
            }
        }

        /// <summary>
        /// 交换队列并获取旧的队列
        /// </summary>
        /// <returns></returns>
        public List<T> GetCurrentAndSwapQueue()
        {
            lock (_locker)
            {
                var oldQueue = _currentQueue;
                _currentQueue = _currentQueue == _queueA ? _queueB : _queueA;
                return oldQueue;
            }
        }
    }
}
