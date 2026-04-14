using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vibranium.Core.interfaces
{
    public interface Application
    {
        string Name { get; set; }
        string Description { get; set; }
        string Version { get; set; }

        /// <summary>
        /// Runs once when the application is loaded before execution. 
        /// use this as a replacement for the constructor().
        /// </summary>
        void Init();

        /// <summary>
        /// Runs once to start the aplication
        /// </summary>
        void Run();
    }
}
