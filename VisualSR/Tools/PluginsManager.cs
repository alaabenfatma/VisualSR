using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using VisualSR.Core;

namespace VisualSR.Tools
{
    public class PluginsManager
    {
        private readonly VirtualControl _host;
        private CompositionContainer _container;
        private IList<string> _plugins = new List<string>();

        public PluginsManager(VirtualControl host)
        {
            _host = host;
            try
            {
                _plugins = new List<string>(Directory.GetFiles(@"./Plugins/", "*.dll"));
            }
            catch (Exception){
                Console.WriteLine("Could not find the plugins folder. The application will create one.");
                Directory.CreateDirectory(@"./Plugins/");
                _plugins = new List<string>(Directory.GetFiles(@"./Plugins/", "*.dll"));
            }
        }

        [ImportMany(typeof(Node))]
        public List<Node> LoadedNodes { get; set; }

        public bool LoadPlugins()
        {
            var newFilesList = new List<string>(Directory.GetFiles(@"./Plugins/", "*.dll"));

            var same = true;
            if (newFilesList.Count != _plugins.Count)
            {
                _plugins = new List<string>(newFilesList.GetRange(0, newFilesList.Count));
                same = false;
            }
            if (same && _container != null) return false;

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Node).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(@"./"));
            _container = new CompositionContainer(catalog);
            try
            {
                _container.ComposeExportedValue("host", _host);
                _container.ComposeExportedValue("bool", false);
                _container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
            Hub.LoadedExternalNodes = LoadedNodes;
            return true;
        }
    }
}