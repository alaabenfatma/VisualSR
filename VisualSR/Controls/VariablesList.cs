/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VisualSR.BasicNodes;
using VisualSR.Core;
using VisualSR.Properties;
using VisualSR.Tools;

namespace VisualSR.Controls
{
    public class VariablesList : Control, INotifyPropertyChanged
    {
        private readonly ObservableCollection<VariableItem> _backupVariables = new ObservableCollection<VariableItem>();
        private readonly ObservableCollection<VariableItem> _variables = new ObservableCollection<VariableItem>();
        private Button _add, _remove;
        private ListBox _radioList;
        private RadioButton _radioType;
        private TextBox _searchTextBox;
        private TextBox _variableName;
        private ListView _varList;

        public VariablesList( /*VirtualControl host*/)
        {
            //if (host != null) Host = host;
            Style = FindResource("VariablesList") as Style;
            ApplyTemplate();
            Loaded += (s, e) =>
            {
                Types.Add(new TypeItem("Generic", this));
                Types.Add(new TypeItem("Numeric", this));
                Types.Add(new TypeItem("Logical", this));
                Types.Add(new TypeItem("Character", this));
                Types.Add(new TypeItem("Array || List || Matrix || Factor", this));
                Types.Add(new TypeItem("DataFrame", this));

                foreach (var item in _variables)
                    _backupVariables.Add(item);
                VariablesList_Loaded();
            };
        }

        public VirtualControl Host { get; set; }

        public ObservableCollection<TypeItem> Types { get; set; } = new ObservableCollection<TypeItem>();
        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateType(string nameOfType)
        {
            var varListSelectedItem = (VariableItem) _varList.SelectedItem;
            RTypes rtype;
            switch (nameOfType)
            {
                case "Generic":
                    rtype = RTypes.Generic;
                    break;
                case "Character":
                    rtype = RTypes.Character;
                    break;
                case "Logical":
                    rtype = RTypes.Logical;
                    break;
                case "Numeric":
                    rtype = RTypes.Numeric;
                    break;
                case "DataFrame":
                    rtype = RTypes.DataFrame;
                    break;
                default:
                    rtype = RTypes.ArrayOrFactorOrListOrMatrix;
                    break;
            }
            if (varListSelectedItem != null)
            {
                varListSelectedItem.Type = nameOfType;

                foreach (var id in varListSelectedItem.DsOfNodes)
                foreach (var node in Host.Nodes)
                    if (node.Id == id)
                        if (node.InExecPorts.Count == 0)
                        {
                            node.OutputPorts[0].Data.Type = rtype;
                            NodesManager.ChangeColorOfVariableNode(node.OutputPorts[0], nameOfType);
                        }

                        else if (node.InputPorts != null)
                        {
                            NodesManager.ChangeColorOfVariableNode(node.InputPorts[0], nameOfType);
                            node.InputPorts[0].Data.Type = rtype;
                        }
                Application.Current.Dispatcher.BeginInvoke(new Action(UpdateWires));
            }
        }

        private void UpdateWires()
        {
            var varListSelectedItem = (VariableItem) _varList.SelectedItem;
            if (varListSelectedItem != null)
                foreach (var id in varListSelectedItem.DsOfNodes)
                foreach (var node in Host.Nodes)
                    if (node.Id == id)

                        if (node.OutputPorts.Count > 0)
                            foreach (var conn in node.OutputPorts[0].ConnectedConnectors)
                            {
                                var xConn = conn;
                                NodesManager.TryCast(ref xConn);
                            }
                        else if (node.InputPorts.Count > 0)
                            foreach (var conn in node.InputPorts[0].ConnectedConnectors)
                            {
                                var xConn = conn;
                                NodesManager.TryCast(ref xConn);
                            }
        }

        private void VariablesList_Loaded()
        {
            //Variables list init.
            _varList = (ListView) Template.FindName("VarList", this);
            _varList.ItemsSource = _variables;
            _varList.SelectionChanged += VarList_SelectionChanged;
            _varList.PreviewMouseLeftButtonDown += VarList_PreviewMouseDown;
            _varList.MouseMove += _varList_MouseMove;
            //Buttons Init.
            _add = (Button) Template.FindName("VarAdd", this);
            _add.Click += Add_Click;
            _remove = (Button) Template.FindName("VarRemove", this);
            _remove.Click += Remove_Click;
            //Details Init.
            _variableName = (TextBox) Template.FindName("NameTextBox", this);
            _variableName.TextChanged += VariableName_TextChanged;
            _variableName.LostFocus += VariableName_LostFocus;
            _radioList = (ListBox) Template.FindName("RadioList", this);
            _radioList.ItemsSource = Types;
            _searchTextBox = (TextBox) Template.FindName("VarSearchTextBox", this);

            _searchTextBox.KeyUp += SearchTextBox_KeyUp;
        }


        private void _varList_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (_varList.SelectedItem == null) return;
            var item = (VariableItem) _varList.SelectedItem;
            var varHoster = new VariableHoster(Host, ref item);
            varHoster.ShowDialog();
            varHoster.Focus();
        }

        private void VariableName_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Filter(_searchTextBox.Text);
        }

        private void Filter(string token)
        {
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    if (token != "")
                    {
                        var l = _backupVariables.Where(i => i.Name.ToUpper().Contains(token.ToUpper())).ToList();
                        _variables.Clear();
                        foreach (var item in l)
                            _variables.Add(item);
                    }
                    else
                    {
                        _variables.Clear();
                        foreach (var item in _backupVariables)
                            _variables.Add(item);
                    }
                }));
            });
        }

        private void VarList_Drop(object sender, DragEventArgs e)
        {
        }

        private void VarList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void VariableName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_varList.SelectedItem != null)
            {
                var item = (VariableItem) _varList.SelectedItem;
                item.Name = _variableName.Text;
                var index = _backupVariables.IndexOf(item);
                _backupVariables[index].Name = item.Name;
                RenameNodes(ref item);
            }

            var c = _variables.Count(item => item.Name == _variableName.Text);
            var varListSelectedItem = (VariableItem) _varList.SelectedItem;
            if (varListSelectedItem != null && (c > 1 || varListSelectedItem.Name == ""))
                _variableName.Foreground = Brushes.Red;
            else
                _variableName.Foreground = Brushes.AliceBlue;

            if (Equals(_variableName.Foreground, Brushes.AliceBlue)) return;

            _variableName.Focus();
            _variableName.CaretIndex = _variableName.Text.Length;
            e.Handled = true;
        }

        private void RenameNodes(ref VariableItem item)
        {
            foreach (var node in Host.Nodes)
                if (item.DsOfNodes.Contains(node.Id))
                    if (node.Types == NodeTypes.VariableGet)
                        node.OutputPorts[0].Text = "Return " + item.Name;

                    else if (node.Types == NodeTypes.VariableSet)
                    {
                        node.InputPorts[0].Text = item.Name;
                        var get = node as Get;
                        get?.Update(item.Name);
                    }
            foreach (var nodeItem in item.NodesTreeItems)
                if (nodeItem.NodeName.Contains("Set "))
                    nodeItem.NodeName = "Set " + item.Name;
                else
                    nodeItem.NodeName = "Get " + item.Name;
        }

        private void VarList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_varList.SelectedItem != null)
            {
                var element = (VariableItem) _varList.SelectedItem;
                _variableName.Text = ((VariableItem) _varList.SelectedItem).Name;
                _variableName.Focus();
                _variableName.CaretIndex = _variableName.Text.Length;
                foreach (var item in Types)
                    if (item.Name.Contains(element.Type))
                        item.Chosen = true;
                foreach (var item in _variables)
                    item.IsSelected = false;
                element.IsSelected = true;
            }
        }

        public void Type_Changed()
        {
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var item = (VariableItem) _varList.SelectedItem;
            if (item == null)
                return;
            _variables.Remove(item);
            _backupVariables.Remove(item);
            foreach (var i in item.DsOfNodes)
                Console.WriteLine(i);
            Console.WriteLine("Nodes:");
            foreach (var id in item.DsOfNodes)
            {
                Node node = null;
                foreach (var n in Host.Nodes)
                    if (n.Id == id)
                    {
                        node = n;
                        break;
                    }
                node?.Delete();
            }
            var rootI = Host.NodesTree.VariablesTreeRootIndex();
            foreach (var nodeItem in item.NodesTreeItems)
                if (rootI != -1) Host.NodesTree.Roots[rootI].Nodes.Remove(nodeItem);
        }


        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var item = new VariableItem("");
            _variables.Add(item);
            _backupVariables.Add(item);
            var i = Host.NodesTree.VariablesTreeRootIndex();
            var getItem = new NodeItem("", "Get " + item.Name, new Get(Host, item, false));
            item.Gets--;
            var setItem = new NodeItem("", "Set " + item.Name, new Set(Host, item, false));
            item.Sets--;
            item.NodesTreeItems.Add(getItem);
            item.NodesTreeItems.Add(setItem);
            if (i > -1)
            {
                Host.NodesTree.Roots[i].Nodes.Add(getItem);
                Host.NodesTree.Roots[i].Nodes.Add(setItem);
            }

            _variableName.Focus();
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TypeItem : INotifyPropertyChanged
    {
        private readonly VariablesList _vl;
        private bool _chosen;

        public TypeItem(string name, VariablesList vl)
        {
            Name = name;
            switch (Name)
            {
                case "Generic":
                    Icon = "../MediaResources/VariablesListIcons/Types/Generic.png";
                    break;
                case "Numeric":
                    Icon = "../MediaResources/VariablesListIcons/Types/Numeric.png";
                    break;
                case "Character":
                    Icon = "../MediaResources/VariablesListIcons/Types/Character.png";
                    break;
                case "Logical":
                    Icon = "../MediaResources/VariablesListIcons/Types/Logical.png";
                    break;
                case "DataFrame":
                    Icon = "../MediaResources/VariablesListIcons/Types/DataFrame.png";
                    break;
                default:
                    Icon = "../MediaResources/VariablesListIcons/Types/List_Matrix_etc.png";
                    break;
            }
            _vl = vl;
        }

        public string Icon { get; set; }
        public string Name { get; set; }

        public bool Chosen
        {
            get { return _chosen; }
            set
            {
                _chosen = value;
                if (_chosen) _vl.UpdateType(Name);
                OnPropertyChanged("Chosen");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VariableItem : INotifyPropertyChanged
    {
        private int _gets;
        private string _icon;
        private bool _isSelected;
        private string _name;
        private int _ref;
        private int _sets;
        private string _type = "Generic";
        public IList<string> DsOfNodes = new List<string>();
        public List<NodeItem> NodesTreeItems = new List<NodeItem>();
        public bool Public = false;

        public VariableItem(string name = "")
        {
            Name = name;
            BuildIcons(_type);
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Returns how many times this variale has been modified.
        /// </summary>
        public int Sets
        {
            get { return _sets; }
            set
            {
                _sets = value;
                References = value + _gets;
            }
        }

        /// <summary>
        ///     Returns how many times the value of this variable has been used.
        /// </summary>
        public int Gets
        {
            get { return _gets; }
            set
            {
                _gets = value;
                References = value + Sets;
            }
        }

        public string GetsTip => Gets + " gets.";
        public string SetsTip => Sets + " sets.";

        /// <summary>
        ///     Returns how many times we did set/get the value of this variable.
        /// </summary>
        public int References
        {
            get { return _ref; }
            set
            {
                _ref = Gets + Sets;
                OnPropertyChanged("Ref");
            }
        }

        public string Ref => @"This variable has been used  " + References + " times.";

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                BuildIcons(_type);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BuildIcons(string type = "Generic")
        {
            switch (type)
            {
                case "Generic":
                    Icon = "../MediaResources/VariablesListIcons/Types/Generic.png";
                    break;
                case "Numeric":
                    Icon = "../MediaResources/VariablesListIcons/Types/Numeric.png";
                    break;
                case "Character":
                    Icon = "../MediaResources/VariablesListIcons/Types/Character.png";
                    break;
                case "Logical":
                    Icon = "../MediaResources/VariablesListIcons/Types/Logical.png";
                    break;
                case "DataFrame":
                    Icon = "../MediaResources/VariablesListIcons/Types/DataFrame.png";
                    break;
                default:
                    Icon = "../MediaResources/VariablesListIcons/Types/List_Matrix_etc.png";
                    break;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VariableHoster : Window, INotifyPropertyChanged
    {
        private readonly ImageBrush _ib = new ImageBrush();
        private readonly DispatcherTimer _positioner = new DispatcherTimer();
        private string _icon = @"../MediaResources/VariablesListIcons/Types/Generic.png";
        private VariableItem _item;
        private string _name;
        private string _type;

        public VariableHoster(VirtualControl host, ref VariableItem variable)
        {
            if (host != null) Host = host;
            Style = (Style) FindResource("VariableHoster");
            _item = variable;
            Loaded += (t, y) =>
            {
                var tb = (TextBlock) Template.FindName("VarName", this);
                var icon = (Border) Template.FindName("Icon", this);
                BuildIcons(_item.Type);
                CaptureMouse();
                Move();
                tb.Text = _item.Name;
                _ib.ImageSource = new BitmapImage(
                    new Uri(_icon));
                icon.Background = _ib;
                _positioner.Tick += (c, e) => Move();
                _positioner.Interval = new TimeSpan(0);
                _positioner.Start();
            };

            MouseLeftButtonUp += VariableHoster_PreviewMouseUp;
            PreviewMouseMove += VariableHoster_PreviewMouseMove;
        }

        private VirtualControl Host { get; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }


        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                BuildIcons(_type);
                OnPropertyChanged("Type");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void VariableHoster_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Move();
            e.Handled = true;
        }

        private void PlantTheVariable()
        {
            var hostPoisition = Host.PointToScreen(new Point(0, 0));
            var hostwidth = Host.ActualWidth;
            var hostheight = Host.ActualHeight;
            if (!(GetCursorPosition().X > hostPoisition.X) || !(GetCursorPosition().X < hostPoisition.X + hostwidth) ||
                !(GetCursorPosition().Y > hostPoisition.Y) ||
                !(GetCursorPosition().Y < hostPoisition.Y + hostheight)) return;

            // Host.AddVariable(ref _item, 100,50);

            Host.AddVariable(ref _item, Mouse.GetPosition(Host).X - 10, Mouse.GetPosition(Host).Y - 8);
            //Host.msg();
        }

        private void VariableHoster_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _positioner.Stop();
            PlantTheVariable();
            Close();
            e.Handled = true;
        }

        public void Init()
        {
        }

        /// <summary>
        ///     Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

        private void Move()
        {
            Left = GetCursorPosition().X - Width / 2;
            Top = GetCursorPosition().Y - Height / 2;
        }


        private void BuildIcons(string type = "Generic")
        {
            switch (type)
            {
                case "Generic":
                    _icon =
                        "pack://application:,,,/VisualSR;component/MediaResources/VariablesListIcons/Types/Generic.png";
                    break;
                case "Numeric":
                    _icon =
                        "pack://application:,,,/VisualSR;component/MediaResources/VariablesListIcons/Types/Numeric.png";
                    break;
                case "Character":
                    _icon =
                        "pack://application:,,,/VisualSR;component/MediaResources/VariablesListIcons/Types/Character.png";
                    break;
                case "Logical":
                    _icon =
                        "pack://application:,,,/VisualSR;component/MediaResources/VariablesListIcons/Types/Logical.png";
                    break;
                case "DataFrame":
                    _icon =
                        "pack://application:,,,/VisualSR;component/MediaResources/VariablesListIcons/Types/DataFrame.png";
                    break;
                default:
                    _icon =
                        "pack://application:,,,/VisualSR;component/MediaResources/VariablesListIcons/Types/List_Matrix_etc.png";
                    break;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }
    }

    public class RTypesGallery : Control
    {
        public RTypesGallery()
        {
            Style = FindResource("RTypesGallery") as Style;
        }

        public RTypes SelectedType { get; set; }
    }
}