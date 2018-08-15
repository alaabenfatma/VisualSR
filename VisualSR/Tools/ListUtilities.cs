using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using VisualSR.Core;

namespace VisualSR.Tools
{
    /// <summary>
    ///     A list that contains <c>Connectors</c>
    /// </summary>
    public sealed class ConnectorsList : ObservableCollection<Connector>
    {
        public ConnectorsList()
        {
            CollectionChanged += ConnectorsList_CollectionChanged;
        }

        public void RemoveConnector(int index)
        {
            this[index].Delete();
        }

        public void ClearConnectors()
        {
            for (var i = Count - 1; i >= 0; i--)
                if (Count > i)
                    switch (this[i].Type)
                    {
                        case ConnectorTypes.Execution:
                            this[i].Delete();
                            break;
                        case ConnectorTypes.Object:
                            var objectsConnector = this[i] as ObjectsConnector;
                            objectsConnector?.Delete();
                            break;
                    }

            Clear();
        }

        private void ConnectorsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Validations
            if (Count <= 0) return;
            if (Count == 1)
                for (var index = 0; index < Count; index++)
                    try
                    {
                        var c = this[index];
                        if (c.EndPort.ParentNode
                            .IsCollapsed)
                        {
                            c.StartPort.Linked = false;
                            c.Wire.Visibility = Visibility.Collapsed;
                            c.EndPort.Linked = false;
                            ClearConnectors();
                            break;
                        }
                        if (!c.StartPort.ParentNode
                            .IsCollapsed) continue;
                        c.StartPort.Linked = false;
                        c.EndPort.Linked = false;
                        c.Wire.Visibility = Visibility.Collapsed;
                        ClearConnectors();
                        break;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
            foreach (var c in this)
                if (Equals(c.EndPort.ParentNode, c.StartPort.ParentNode))
                    c.Wire.Visibility = Visibility.Collapsed;
        }
    }

    public static class NodesTreeUtilities
    {
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable
        {
            var sorted = collection.OrderBy(x => x).ToList();
            for (var i = 0; i < sorted.Count(); i++)
                if (sorted.Count > i) collection.Move(collection.IndexOf(sorted[i]), i);
        }
    }
}