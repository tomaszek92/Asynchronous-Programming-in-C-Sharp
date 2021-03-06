﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsynchronousProgramming.DataAnalyzer.Processors
{
    public class ParallelMapReduceProcessor : IProcessor
    {
        public Dictionary<int, List<int>> Process(string[] lines)
        {
            var resDic = new Dictionary<int, List<int>>();
            var locker = new object();
            Parallel.ForEach(
                lines,
                () => new Dictionary<int, List<int>>(),
                (line, state, localDic) =>
                {
                    var (userId, rate) = ExtractorHelper.GetLineInfo(line);
                    if (localDic.TryGetValue(userId, out var rates))
                    {
                        rates.Add(rate);
                    }
                    else
                    {
                        localDic[userId] = new List<int>(200) {rate};
                    }
                    return localDic;
                },
                localDic =>
                {
                    lock (locker)
                    {
                        foreach (var pair in localDic)
                        {
                            if (resDic.ContainsKey(pair.Key))
                            {
                                resDic[pair.Key].AddRange(pair.Value);
                            }
                            else
                            {
                                resDic[pair.Key] = pair.Value;
                            }
                        }
                    }
                });
            return resDic;
        }
    }
}