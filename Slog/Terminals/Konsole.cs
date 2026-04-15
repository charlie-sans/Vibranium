using System;
using System.Threading.Tasks;
using Vibranium;

namespace Slog
{
    public class Konsole : ProcessNode
    {
        bool _running = true;
        bool _started = false;

        void Run()
        {
            while (IsRunning)
            {
                var Contents = Sys.IO.ReadLine("$: ");
                if (Contents == null)
                {
                    System.Threading.Thread.Sleep(5);
                    continue;
                }
                Sys.IO.WriteLine(Contents);
            }
        }

        public bool IsRunning
        {
            get { return _running; }
        }

        public void Kill()
        {
            _running = false;
        }

        public string Name { get {return "sh";} private set{} }

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
