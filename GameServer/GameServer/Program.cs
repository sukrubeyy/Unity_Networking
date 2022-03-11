using System;
using System.Threading;
namespace GameServer
{
    public class Program
    {
        private static bool isRunning = false;
        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            Server.Start(5, 26950);
        }
        private static void MainThread()
        {
            Console.WriteLine($"Main Thread started. Running at {Constants.TICKS_PER_SEC} ticks second.");
            DateTime _NextLoop = DateTime.Now;
            while (isRunning)
            {
                while (_NextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    _NextLoop = _NextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (_NextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_NextLoop-DateTime.Now);
                    }
                }
            }
        }
    }
}