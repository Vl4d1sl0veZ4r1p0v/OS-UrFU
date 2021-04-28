using System;
using System.IO.Compression;
using System.Net.Cache;
using System.Threading;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ExceptionServices;
using System.Runtime.CompilerServices;

namespace philosophers_os
{
    class Fork //
    {
        private Mutex _m = new Mutex();
        public bool Used { get; set; }

        public Fork()
        {
            Used = false;
        }
        public void Take()
        {
            
            _m.WaitOne(); 
            Used = true;
        }

        public void Put()
        {
            Used = false;
            _m.ReleaseMutex(); 
        }
    };
    //+

    class Philosopher
    {
        //+
        public uint Id { set; get; }
        public Fork ForkLeft { set; get; }
        public Fork ForkRight { set; get; }
        public uint EatCount { set; get; }
        public double WaitTime { set; get; }
        public DateTime WaitStart { set; get; }
        public bool StopFlag { set; get; }
        public bool DebugFlag { set; get; }
        Random _random;
        private Waiter _waiter;
        
        public enum States
        {
            Thinking, 
            Hungry, 
            Eating
        }

        public States State { set; get; }
        //+
        void Think()
        {
            State = States.Thinking;

            if (this.DebugFlag)
            {
                Console.WriteLine(this.Id + " thinking");
            }
            
            Thread.Sleep(this._random.Next(0, 100));

            State = States.Hungry;
            
            if (this.DebugFlag)
            {
                Console.WriteLine(this.Id + " hungry");
            }

            this.WaitStart = DateTime.Now;
        }
        //+

        async Task Eat()
        {
            State = States.Eating;
            
            this.WaitTime += DateTime.Now.Subtract(this.WaitStart).TotalMilliseconds;
            if (this.DebugFlag)
            {
                Console.WriteLine(this.Id + " eating");
            }

            Thread.Sleep(this._random.Next(0, 100));

            EatCount++;
            State = States.Thinking;
        }
        //+

        public Philosopher(uint number, Fork left, Fork right, bool dbg, Waiter waiter)
        {
            this.Id = number;
            this.ForkLeft = left;
            this.ForkRight = right;
            this.EatCount = 0;
            this.WaitTime = 0;
            this.DebugFlag = dbg;
            this.StopFlag = false;
            this._random = new Random();
            this.State = States.Thinking;
            _waiter = waiter;
        }
        //+
        
        public async void Run()//run philosoph, run...
        {
            while (!StopFlag)
            {
                Think();

                await _waiter.RequestTakeForks(Id);
                
                await Eat();

                await _waiter.RequestPutForks(Id);
            }
        }
        
        //+
        public void Stop()
        {
            StopFlag = true;
        }

        public void PrintStats()
        {
            Console.WriteLine(this.Id + " " + this.EatCount + " " + Convert.ToInt32(this.WaitTime));
        }
        //+
    }

    class Waiter
    {
        //private static SemaphoreSlim _tableSemaphore = new SemaphoreSlim(1);
        private static SemaphoreSlim[] _philosopherSemaphores;
        public static Mutex _mp = new Mutex();
        public static Mutex _mt = new Mutex();
        

        private Philosopher[] _philosophers;
        
        
            ~Waiter()
            {
                //_tableSemaphore.Release();
                _mt.ReleaseMutex();
                for (var i = 0; i < _philosopherSemaphores.Length; ++i)
                {
                    _philosopherSemaphores[i].Release();
                }
            }

            public Waiter(Philosopher[] philosophers)
        {
            _philosophers = philosophers;
            _philosopherSemaphores = new SemaphoreSlim[Program.n];
            for (int i = 0; i < Program.n; i++)
                _philosopherSemaphores[i] = new SemaphoreSlim(1);
        }

            public async Task RequestPutForks(uint philosopherId)
        {
            //await _tableSemaphore.WaitAsync();
            _mp.WaitOne();
            if (_philosophers[philosopherId].ForkLeft.Used &&
                _philosophers[philosopherId].ForkRight.Used)
            {
                _philosophers[philosopherId].ForkLeft.Put();
                if (_philosophers[philosopherId].DebugFlag)
                {
                    Console.WriteLine(philosopherId + " put right fork");
                }

                _philosopherSemaphores[(philosopherId - 1 + Program.n) % Program.n].Release();

                _philosophers[philosopherId].ForkRight.Put();
                if (_philosophers[philosopherId].DebugFlag)
                {
                    Console.WriteLine(philosopherId + " put left fork");
                }

                _philosopherSemaphores[(philosopherId + 1) % Program.n].Release();
            }
            _mp.ReleaseMutex();
            //_tableSemaphore.Release();
        }
        
        public async Task RequestTakeForks(uint philosopherId)
        {
            bool hasForks = false;
            while (!hasForks)
            {
                //await _tableSemaphore.WaitAsync();
                _mt.WaitOne();
                if (_philosophers[philosopherId].State == Philosopher.States.Hungry &&
                    _philosophers[(philosopherId + Program.n - 1) % Program.n].State != Philosopher.States.Eating &&
                    _philosophers[(philosopherId + 1) % Program.n].State != Philosopher.States.Eating && 
                    !_philosophers[philosopherId].ForkLeft.Used && 
                    !_philosophers[philosopherId].ForkRight.Used)
                {
                    _philosophers[philosopherId].ForkLeft.Take();
                    if (_philosophers[philosopherId].DebugFlag)
                    {
                        Console.WriteLine(philosopherId + " took left fork");
                    }
                    _philosophers[philosopherId].ForkRight.Take();
                    if (_philosophers[philosopherId].DebugFlag)
                    {
                        Console.WriteLine(philosopherId + " took right fork");
                    }
                    hasForks = true;
                }
                _mt.ReleaseMutex();
                //_tableSemaphore.Release();
                if (!hasForks)
                    await _philosopherSemaphores[philosopherId].WaitAsync();
            }
        }
    }

    class Program
    {
        
        public static int n = 10;
        static void Main(string[] args)
        {
            
            
            bool dbg = false;
            int duration = 3 *60000;

            Fork[] forks = new Fork[n];
            for (int i = 0; i < n; i++)
            {
                forks[i] = new Fork();
            }

            Philosopher[] phils = new Philosopher[n];
            
            Waiter waiter = new Waiter(phils);
            for (uint i = 0; i < n; i++)
            {
                phils[i] = new Philosopher(i, forks[(i + 1) % n], forks[i], dbg, waiter);
            }

            Thread[] runners = new Thread[n];
            for (int i = 0; i < n; i++)
            {
                runners[i] = new Thread(phils[i].Run);
            }
            
            for (int i = 0; i < n; i++)
            {
                runners[i].Start();
            }
            
            Thread.Sleep(duration);

            for (int i = 0; i < n; i++)
            {
                phils[i].Stop();
            }

            for (int i = 0; i < n; i++)
            {
                runners[i].Join();
            }
    
            for (int i = 0; i < n; i++)
            {
                phils[i].PrintStats();
            }
        }
    }
}