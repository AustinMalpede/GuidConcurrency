using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GuidConcurrency
{
    public class Program
    {
        
        static void Main(string[] args)
        {
            GuidMaker maker = new GuidMaker();
            List<Task> guidTasks = new List<Task>();

            HashSet<string> res = maker.PerformTask(111).Result;

            Console.WriteLine(String.Join("\n",  res.Count));
        }


    }

    class GuidMaker
    {
        private HashSet<string> guids;

        private AsyncLocal<ImmutableHashSet<string>> _operationIds = new AsyncLocal<ImmutableHashSet<string>>();

        SemaphoreSlim myLock;

        public GuidMaker()
        {
            guids = new HashSet<string>();
            myLock = new SemaphoreSlim(1);
        }

        public async Task<HashSet<string>> PerformTask(int n)
        {
            List<Task> guidTasks = new List<Task>();
            for (int i = 0; i < n; ++i)
            {
                guidTasks.Add(DoLongOperationAsync());
            }

            await Task.WhenAll(guidTasks);
            return guids;
        }


        public async Task DoLongOperationAsync()
        {
            await myLock.WaitAsync();
            guids.Add(Guid.NewGuid().ToString());
            await PerformLongStep();
            myLock.Release();
        }
           

        public async Task DoLongOperationAsyncStack()
        {
            //using (_operationIds.Value.Add(Guid.NewGuid().ToString()));
            await PerformLongStep();

        }

        async Task PerformLongStep()
        {
            await Task.Delay(1);
        }
    }
}

