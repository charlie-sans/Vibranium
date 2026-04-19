
using System.Threading.Tasks;
using Vibranium;
using Vibranium.Sys;
using Vibranium.Kernel;
namespace Slog
{
    public class Konsole : ProcessNode
    {
        bool _running = true;
        bool _started = false;

        void Run()
        {
            Vibranium.Sys.Environment.RegisterThread(PID);
            while (IsRunning)
            {
                IO.SetConsoleColor(System.ConsoleColor.Green);
                string Contents = IO.ReadLine("@: ");
                IO.SetConsoleColor(System.ConsoleColor.White);
                if (Contents == null)
                {
                    System.Threading.Thread.Sleep(5);

                    continue;
                }
                if (Contents.Trim() == "exit")
                {
                    Environment.ExitCurrentProcess();
                    break;
                }
                IO.WriteLine(Contents);
            }
        }

        public bool IsRunning
        {
            get { return _running; }
        }

        public void Kill()
        {
            _running = false;
           Environment.UnregisterThread();
        }

        public string Name { get { return "Konsole"; } private set { } }

        public int PID { get; set; }

        public int PPID { get; set; }

        public void Tick()
        {
            if (!_started)
            {
                _started = true;
                Run();
            }
        }
    }
}
