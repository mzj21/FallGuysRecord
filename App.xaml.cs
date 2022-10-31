using System.IO;
using System.Reflection;
using System;
using System.Windows;
using System.Linq;
using FallGuysRecord_WPF_Framework.Properties;
using System.Resources;
using System.Drawing;
using System.Windows.Documents;

namespace FallGuysRecord_WPF_Framework
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

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
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
