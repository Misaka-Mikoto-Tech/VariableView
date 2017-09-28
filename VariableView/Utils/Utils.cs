using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableView.Utils
{
    public static class Utils
    {
        /// <summary>
        /// 计算两个集合的交集和差集, 此函数会修改两个参数列表内的节点顺序
        /// </summary>
        /// <param name="firstList"></param>
        /// <param name="secondList"></param>
        /// <returns>交集和差集的分界索引, 指向第一个不相同元素的索引</returns>
        public static int CalcIntersectionAndSubtraction(List<Cell> firstList, List<Cell> secondList)
        {
            /*
                原理是以第一个 List 为基准, 把两个 List 相同的部分都摆放在前面，
                如果一个循环第一个 List 里的元素在第二个 List 里没找到，那么就把这个元素放在最后面, 直到全部元素都被检测过时停止
             */

            int splitIdx = 0;
            int lastUnmatchIdx  = firstList.Count - 1;
            int secListCount    = secondList.Count;
            
            while (splitIdx <= lastUnmatchIdx && splitIdx < secListCount)
            {
                bool bFind = false;

                // 根据陈文超的建议优化一下，两个索引相等时就不交换了
                if (firstList[splitIdx] == secondList[splitIdx])
                {
                    bFind = true;
                    splitIdx++;
                }
                else
                {
                    for (int j = splitIdx + 1; j < secListCount; j++)
                    {
                        if (firstList[splitIdx] == secondList[j])
                        {
                            Cell tmp = secondList[splitIdx];
                            secondList[splitIdx] = secondList[j];
                            secondList[j] = tmp;

                            bFind = true;
                            splitIdx++;
                            break;
                        }
                    }
                }
                    

                if (!bFind)
                {
                    Cell tmp = firstList[splitIdx];
                    firstList[splitIdx] = firstList[lastUnmatchIdx];
                    firstList[lastUnmatchIdx] = tmp;

                    lastUnmatchIdx--;  
                }
            }
            return splitIdx;
        }

    }
}
