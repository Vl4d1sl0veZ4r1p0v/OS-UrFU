using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ExceptionServices;
using System.Runtime.CompilerServices;

namespace DDProblem
{
    
    class Program
    {
        
        private static SemaphoreSlim _tableSemaphore = new SemaphoreSlim(1);
        private static SemaphoreSlim[] _philosopherSemaphores;

        private const int PhilosophersAmount = 5;
        private static Stopwatch watch;
        private static long[] _waitTime = Enumerable.Repeat(0L, PhilosophersAmount).ToArray();
        private static int[] eatenFood = new int[PhilosophersAmount];

        private static int[] lastEatenFood = new int[PhilosophersAmount];

        private static int[] thoughts = new int[PhilosophersAmount];
        private static int[] forks = Enumerable.Repeat(0, PhilosophersAmount).ToArray();
        private static int Left(int i) => i;
        
        private static int LeftPhilosopher(int i) => (PhilosophersAmount + i - 1) % PhilosophersAmount;
        
        private static int Right(int i) => (i + 1) % PhilosophersAmount;
        
        private static int RightPhilosopher(int i) => (i + 1) % PhilosophersAmount;
        

        private static void Think(int philosopherInx)
        {
            Thread.Sleep(5000);
            thoughts[philosopherInx]++;
        }

        public async Task Run(int i)
        {
            while (true)
            {
                watch.Restart();
                await TakeForks(i);
                _waitTime[i] += watch.ElapsedMilliseconds;

                eatenFood[i] = (eatenFood[i] + 1) % (int.MaxValue - 1);

                watch.Restart();
                await PutForks(i);
                _waitTime[i] += watch.ElapsedMilliseconds;

                Think(i);
            }
        }

        async Task TakeForks(int i)
        {
            bool hasForks = false;
            while (!hasForks)
            {
                await _tableSemaphore.WaitAsync();
                if (forks[Left(i)] == 0 && forks[Right(i)] == 0)
                {
                    forks[Left(i)] = i+1;
                    forks[Right(i)] = i+1;
                    hasForks = true;
                }
                _tableSemaphore.Release();
                if (!hasForks)
                    await _philosopherSemaphores[i].WaitAsync();
            }
        }

        async Task PutForks(int i)
        {
            await _tableSemaphore.WaitAsync();
            forks[Left(i)] = 0;
            _philosopherSemaphores[LeftPhilosopher(i)].Release();
            forks[Right(i)] = 0;
            _philosopherSemaphores[RightPhilosopher(i)].Release();
            _tableSemaphore.Release();
        }
        private const int philosophersAmount = 4;
        
        private static DateTime startTime;

        public static void Main(string[] args)
        {
            _philosopherSemaphores = new SemaphoreSlim[PhilosophersAmount];
            for (int i = 0; i < PhilosophersAmount; i++)
                _philosopherSemaphores[i] = new SemaphoreSlim(1);
            // Observer:
            Console.WriteLine("Starting...");
            startTime = DateTime.Now;
            var philosophers = new Task[philosophersAmount];
            for (int i = 0; i < philosophersAmount; i++)
            {
                int icopy = i;
                philosophers[i] = new Task(() => Run(icopy));
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();

            for (int i = 0; i < philosophersAmount; i++)
            {
                Console.WriteLine($"P{i+1} {eatenFood[i]} eaten, {thoughts[i]} thoughts.");
            }

            Console.WriteLine("Exit");
        }
    }
}