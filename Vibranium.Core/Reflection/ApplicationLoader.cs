using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vibranium.Core.interfaces;
using System.Reflection;
namespace Vibranium.Core.Reflection
{
    public class ApplicationLoader
    {
        private Dictionary<string, Application> apps =
         new Dictionary<string, Application>();

        public void LoadApplications()
        {
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.IsSubclassOf(typeof(Application)));

            foreach (var type in types)
            {
                Application app =
                    (Application)Activator.CreateInstance(type);

                app.Init();

                apps.Add(app.Name.ToLower(), app);
            }
        }
    }
}
