using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace FallGuysRecord
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        readonly string[] dlls = new string[] { "Newtonsoft.Json" };
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string resources = null;
            foreach (var item in dlls)
            {
                if (args.Name.StartsWith(item))
                {
                    resources = item + ".dll";
                    break;
                }
            }
            if (string.IsNullOrEmpty(resources)) return null;

            var assembly = Assembly.GetExecutingAssembly();
            resources = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(resources));

            if (string.IsNullOrEmpty(resources)) return null;

            using (Stream stream = assembly.GetManifestResourceStream(resources))
            {
                if (stream == null) return null;
                var block = new byte[stream.Length];
                stream.Read(block, 0, block.Length);
                return Assembly.Load(block);
            }
        }
    }
}
