/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VisualSR.BasicNodes;
using VisualSR.Properties;
using VisualSR.Tools;
using Microsoft.Win32;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace VisualSR.Controls
{
    public class ExplorerItem : Control, INotifyPropertyChanged
    {
        private readonly string[] _audioFiles =
        {
            ".mp3", ".ogg", ".wav", ".bgm"
        };

        private readonly string[] _codeFiles =
        {
            ".r", ".cpp", ".c", ".cs", ".vb", ".py", ".html", ".js", ".css", ".asp", ".aspx", ".pl", ".lua", ".bat",
            ".php", ".rb", ".coffee", ".pas"
        };

        private readonly string[] _dataTypes =
            {".db", ".xls", ".xlsx", ".accdb", ".nsf", "fp7"};

        private readonly string[] _documentTypes =
            {".pdf", ".doc", ".docx", ".rtf", ".ppt", ".odt"};

        private readonly string[] _executableTypes =
            {".exe", ".run", ".jar", ".bar"};

        private readonly string[] _imageTypes =
            {".png", ".jpg", ".dds", ".jpeg", ".gif", ".psd", ".ai", ".tif", ".tiff", ".bmp", ".ico", ".svg", ".ps"};

        private readonly string[] _textTypes =
            {".txt", ".lst", ".bib", ".textclipping", ".fodt", ".text", ".tex", ".wks", ".wps", ".wpd"};

        private readonly string[] _videoTypes =
            {".avi", ".mp4", ".mpeg", ".flv"};

        private Brush _backcolor;
        private Brush _color;
        private Border _container;
        private object _core;
        private string _extension;
        private FileTypes _fileTypes = FileTypes.Unknown;
        private string _icon;
        private bool _isSelected;
        private string _name;
        private string _path;
        private TextBox _renameBox;
        private string _size;
        private Label _tag;

        private string ItemCategory = "File";
        public ObservableCollection<ExplorerItem> Items = new ObservableCollection<ExplorerItem>();
        public string NextPath;

        /// <summary>
        ///     This module represents each directory/file presented in the contents browser.
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="name">Name of the file/dir.</param>
        /// <param name="ext">Extension of the file/dir.</param>
        /// <param name="path">Path of the file/dir.</param>
        /// <param name="dir">Is it a directory?</param>
        public ExplorerItem(string icon, string name, string ext, string path, bool dir)
        {
            Icon = path;
            ItemName = name;
            Extension = ext;
            Path = path;
            try
            {
                Size = UnitsCalculator.SizeCalc(new FileInfo(path).Length);
            }
            catch
            {
            }
            Folder = dir;
            BuildColors();
            Foreground = Brushes.WhiteSmoke;

            Style = FindResource("ExplorerItem") as Style;
            Loaded += (s, e) =>
            {
                _tag = (Label) Template.FindName("Tag", this);
                _renameBox = (TextBox) Template.FindName("RenameBox", this);
                _renameBox.KeyUp += _renameBox_PreviewKeyDown;
                _container = (Border) Template.FindName("ExplorerItemContainer", this);
                _container.ContextMenu = BuildContextMenu();
                _renameBox.LostFocus += (s_rename, e_rename) => { ApplyNewName(); };
                KeyUp += ExplorerItem_KeyDown;
                if (Folder)
                {
                    _core = Template.FindName("Folder", this) as Grid;
                    ((Grid) _core).Visibility = Visibility.Visible;
                }
                else
                {
                    if (_fileTypes == FileTypes.Image)
                    {
                        _core = Template.FindName("Image", this) as Canvas;
                        ((Canvas) _core).Visibility = Visibility.Visible;
                        MakeThumbnail(Path);
                    }
                    else
                    {
                        _core = Template.FindName("Image", this) as Canvas;
                        ((Canvas) _core).Visibility = Visibility.Visible;

                        MakeThumbnail();
                    }
                }
            };

            PreviewMouseDoubleClick += (s, e) =>
            {
                Load(Path);
                e.Handled = true;
            };
        }

        public ContentsBrowser Container { get; set; }

        public Control This => this;
        public ExplorerItem Parent { get; set; }

        private void _renameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:

                    ApplyNewName();
                    break;
            }
        }

        private void ExplorerItem_KeyDown(object sender, KeyEventArgs e)
        {
        }

        public void Rename(string fakeName = "", bool forcedToFocus = true)
        {
            try
            {
                if (_tag == null || _renameBox == null)
                    Thread.Sleep(100);
                if (fakeName != "")
                    if (fakeName != null) _renameBox.Text = fakeName;
                    else
                        _renameBox.Text = "";
                _tag.Visibility = Visibility.Collapsed;
                _renameBox.Visibility = Visibility.Visible;
                if (forcedToFocus)
                    _renameBox.Focus();
                _renameBox.CaretIndex = 0;
                _renameBox.SelectAll();
            }
            catch (Exception)
            {
                //Ignored, this kind of errors occurs because of threading mis-behaviours.
            }
        }

        private void ApplyNewName()
        {
            _tag.Visibility = Visibility.Visible;
            _renameBox.Visibility = Visibility.Collapsed;
            var oldPAth = Path;
            var newPath = Parent.Path + @"\" + _renameBox.Text;
            MagicLaboratory.CleanPath(ref newPath);
            try
            {
                if (!Folder)
                    if (File.Exists(Path))
                    {
                        File.Move(Path
                            , newPath);
                        ItemName = _renameBox.Text;
                        var itemName = ItemName;
                        MagicLaboratory.CleanPath(ref itemName);
                        ItemName = itemName;
                        Path = newPath;
                    }

                if (Directory.Exists(Path) && Path != newPath)
                {
                    Directory.Move(Path
                        , Uri.UnescapeDataString(newPath));
                    ItemName = _renameBox.Text;
                    var itemName = ItemName;
                    MagicLaboratory.CleanPath(ref itemName);
                    ItemName = itemName;
                    Path = newPath;
                    var node = Parent.Container.GetNode(oldPAth, Parent.Container.FoldersTree.Items);
                    var parentNode = Parent.Container.GetNode(Parent.Path, Parent.Container.FoldersTree.Items);
                    if (node == null)
                    {
                        parentNode.Items.Add(new TreeViewItem {Header = new FolderItem(Path, ItemName)});
                    }
                    else
                    {
                        var folderItem = node.Header as FolderItem;
                        if (folderItem != null)
                        {
                            folderItem.FolderName = itemName;
                            folderItem.Path = Path;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var result = MessageBox.Show(ex.Message + "\n" + "Try again?", "Couldn't modify the name.",
                    MessageBoxButton.YesNo, MessageBoxImage.Error);
                Parent.Container.Disable();
                if (result == MessageBoxResult.Yes)
                {
                    Parent.Container.Enable();
                    Rename();
                }
            }
        }

        private void Refresh()
        {
        }

        private ContextMenu BuildContextMenu()
        {
            var sp = new Separator {Foreground = Brushes.Gray};
            var sp1 = new Separator {Foreground = Brushes.Gray};
            var cMenu = new ContextMenu();
            var open = new MenuItem
            {
                Header = new TextBlock {Foreground = Brushes.WhiteSmoke, Text = "Open"}
            };
            open.Click += (s, e) => { Process.Start(Path); };
            cMenu.Items.Add(open);

            cMenu.Items.Add(sp);
            var addToVirtualControl = new MenuItem();
            var addNodeStack = new StackPanel {Orientation = Orientation.Horizontal};
            addNodeStack.Children.Add(new TextBlock {Text = "◰", FontSize = 18, Foreground = Brushes.DarkCyan});
            addNodeStack.Children.Add(new TextBlock
            {
                Text = "Create node.",
                Foreground = Brushes.WhiteSmoke,
                VerticalAlignment = VerticalAlignment.Center
            });
            addToVirtualControl.Header = addNodeStack;
            addToVirtualControl.Click += (s, e) =>
            {
                var node = new ReadFile(Hub.CurrentHost, false);
                node.OutputPorts[0].Data.Value = Path;
                Hub.CurrentHost.AddNode(node, Hub.CurrentHost.ActualWidth / 2 - node.ActualWidth / 2,
                    Hub.CurrentHost.ActualHeight / 2);
            };
            cMenu.Items.Add(addToVirtualControl);
            cMenu.Items.Add(sp1);
            var cut = new MenuItem
            {
                Header = new TextBlock
                {
                    Foreground = Brushes.WhiteSmoke,
                    Text = "Cut"
                }
            };
            var copy = new MenuItem
            {
                Header = new TextBlock
                {
                    Foreground = Brushes.WhiteSmoke,
                    Text = "Copy"
                }
            };


            var rename = new MenuItem
            {
                Header = new TextBlock
                {
                    Foreground = Brushes.WhiteSmoke,
                    Text = "Rename"
                }
            };
            var deleteTextBlock = new TextBlock();
            deleteTextBlock.Inlines.Add(new TextBlock {Text = "X", Foreground = Brushes.Red});
            deleteTextBlock.Inlines.Add(new TextBlock
            {
                Text = $" Delete {ItemCategory}",
                Foreground = Brushes.WhiteSmoke
            });
            var delete = new MenuItem
            {
                Header = deleteTextBlock
            };
            delete.Click += (s, e) =>
            {
                var res = MessageBox.Show($"Do you really want to delete this {ItemCategory}?", "Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (res == MessageBoxResult.No) return;
                try
                {
                    if (Folder)
                    {
                        Directory.Delete(Path);
                        Task.Factory.StartNew(() =>
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                Parent.Container.GetNode(Parent.Path, Parent.Container.FoldersTree.Items).Items
                                    .Remove(Parent.Container.GetNode(Path, Parent.Container.FoldersTree.Items));
                            }));
                        });
                    }
                    else
                    {
                        File.Delete(Path);
                    }
                    Parent.Items.Remove(this);
                }
                catch (Exception exception)
                {
                    if (!exception.Message.Contains("The directory is not empty."))
                    {
                        MessageBox.Show(exception.Message, $"Could not delete the {ItemCategory}",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        var answer = MessageBox.Show(exception.Message + "\n" + "Delete everything in this directory?",
                            $"Could not delete the {ItemCategory}",
                            MessageBoxButton.YesNo, MessageBoxImage.Error);
                        if (answer == MessageBoxResult.Yes)
                        {
                            Directory.Delete(Path, true);
                            Parent.Container.GetNode(Parent.Path, Parent.Container.FoldersTree.Items).Items
                                .Remove(Parent.Container.GetNode(Path, Parent.Container.FoldersTree.Items));
                            Parent.Items.Remove(this);
                        }
                    }
                }
            };
            cut.Click += (s, e) => { Parent.Container.PathToCut = Path; };
            copy.Click += (s, e) =>
            {
                Parent.Container.PathToCut = "";
                Parent.Container.PathToCopy = Path;
            };

            rename.Click += (ss, e) => { Rename(); };
            cMenu.Items.Add(cut);
            cMenu.Items.Add(copy);
            cMenu.Items.Add(rename);
            var sp2 = new Separator();
            cMenu.Items.Add(sp2);
            cMenu.Items.Add(delete);
            return cMenu;
        }

        private void MakeThumbnail(string path)
        {
            try
            {
                var thumbnail = Template.FindName("Thumbnail", this) as Rectangle;
                var bmp = new Bitmap(Path);
                thumbnail.Fill = new ImageBrush(MagicLaboratory.ImageSourceForBitmap(bmp));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void MakeThumbnail()
        {
            try
            {
                var thumbnail = Template.FindName("Thumbnail", this) as Rectangle;
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(Path);
                var bmp = icon.ToBitmap();
                thumbnail.Fill = new ImageBrush(MagicLaboratory.ImageSourceForBitmap(bmp));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Load(string path, bool forced = false)
        {
            if (!Folder && !forced) return;
            if (Parent.Container != null)
            {
                Parent.Path = path;
                int x;
                Parent.Container.Disable();

                var d = new DirectoryInfo(path); //Assuming Test is your Folder
                var files = d.GetFiles(); //Getting Text files
                var dirs = d.GetDirectories();
                x = files.Length + dirs.Length;


                Parent.Items.Clear();


                if (x == 0)
                {
                    Parent.Container.Enable();
                    Parent.Container.UpdateFoldersTree();
                    return;
                }


                foreach (var dirr in dirs)
                    Task.Factory.StartNew(() =>
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Input,
                            new Action(() =>
                            {
                                if (Parent.Path == Directory.GetParent(dirr.FullName).FullName)
                                    Parent.AddFolder(dirr.Name, dirr.Extension, dirr.FullName);

                                if (x == Parent.Items.Count)
                                {
                                    Parent.Container.Enable();
                                    Parent.Container.UpdateFoldersTree();
                                }
                            }));
                    });
                foreach (var file in files)
                    Task.Factory.StartNew(() =>
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Input,
                            new Action(() =>
                            {
                                if (Parent.Path == Directory.GetParent(file.FullName).FullName)
                                    Parent.AddFile(file.Name, file.Extension, file.FullName);
                                if (x == Parent.Items.Count)
                                {
                                    Parent.Container.Enable();
                                    Parent.Container.UpdateFoldersTree();
                                }
                            }));
                    });
            }
        }

        public void AddFile(string name, string ext, string path)
        {
            var item = new ExplorerItem(Ico(ext), name, ext, path, false)
            {
                Parent = this
            };
            Items.Add(item);
        }

        public void AddFolder(string name, string ext, string path)
        {
            var item = new ExplorerItem("", name, ext, path, true)
            {
                Parent = this
            };
            Items.Add(item);
        }

        private string Ico(string ext)
        {
            return null;
        }

        public void BuildColors()
        {
            BackColor = Brushes.Transparent;
            if (_textTypes.Any(t => t == Extension))
            {
                ItemColor = Brushes.Aqua;
                _fileTypes = FileTypes.Text;
                return;
            }
            if (_imageTypes.Any(t => t == Extension))
            {
                ItemColor = Brushes.Lime;
                _fileTypes = FileTypes.Image;
                return;
            }
            if (_documentTypes.Any(t => t == Extension))
            {
                ItemColor = Brushes.DarkCyan;
                _fileTypes = FileTypes.Document;
                return;
            }
            if (_executableTypes.Any(t => t == Extension))
            {
                ItemColor = Brushes.Black;
                _fileTypes = FileTypes.Executable;
                return;
            }
            if (_dataTypes.Any(t => t == Extension))
            {
                ItemColor = Brushes.Orange;
                _fileTypes = FileTypes.Data;
                return;
            }
            if (_videoTypes.Any(t => t == Extension))
            {
                ItemColor = Brushes.Violet;
                _fileTypes = FileTypes.Video;
                return;
            }
            if (_codeFiles.Any(t => t == Extension))
            {
                ItemColor = Brushes.DarkViolet;
                _fileTypes = FileTypes.Code;
                return;
            }
            if (_audioFiles.Any(t => t == Extension))
            {
                ItemColor = Brushes.Yellow;
                _fileTypes = FileTypes.Code;
                return;
            }

            ItemColor = Brushes.Gray;
        }

        public void UnSelectAll()
        {
            foreach (var item in Items)
                item.IsSelected = false;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Properties

        public string ItemName
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged();
            }
        }

        public string Size
        {
            get { return _size; }
            set
            {
                _size = value;
                OnPropertyChanged();
            }
        }

        public string Extension
        {
            get { return _extension; }
            set
            {
                _extension = value;
                OnPropertyChanged();
            }
        }

        public string Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        public Brush ItemColor
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

        public Brush BackColor
        {
            get { return _backcolor; }
            set
            {
                _backcolor = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                Foreground = _isSelected ? Brushes.Black : Brushes.WhiteSmoke;
                OnPropertyChanged();
            }
        }

        private bool folder;

        public bool Folder
        {
            get { return folder; }
            set
            {
                if (value)
                    ItemCategory = "Folder";
                else
                    ItemCategory = "File";
                folder = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class FolderItem : Control, INotifyPropertyChanged
    {
        private Brush _color;
        private string _fullname;
        private string _name;

        public FolderItem(string path, string name)
        {
            FolderName = name;
            Path = path;
            Foreground = Brushes.AliceBlue;
            Color = Brushes.Gray;
            Style = FindResource("FolderItem") as Style;
        }

        public string FolderName
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get { return _fullname; }
            set
            {
                _fullname = value;
                OnPropertyChanged();
            }
        }

        public Brush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ContentsBrowser : Control, INotifyPropertyChanged
    {
        private readonly ObservableCollection<ExplorerItem> _selectedItems = new ObservableCollection<ExplorerItem>();
        private Border _import, _save, _prev, _next;
        private ExplorerItem _item = new ExplorerItem("", @"", "", @"", true);
        private ProgressBar _pBar;

        public TreeView FoldersTree;
        public string PathToCopy;
        public string PathToCut;

        public ContentsBrowser()
        {
            Style = FindResource("ContentsBrowser") as Style;
            _item.MouseDoubleClick += _item_MouseDoubleClick;
            Item.Container = this;
            Loaded += (s, e) =>
            {
                ApplyTemplate();
                Item.Items.CollectionChanged += Items_CollectionChanged;
                FoldersTree = Template.FindName("FoldersTree", this) as TreeView;
                Contents = Template.FindName("Contents", this) as ListView;
                Contents.ContextMenu = contentsMenu();
                FoldersTree.SelectedItemChanged += FoldersTree_SelectedItemChanged;
                _import = (Border) Template.FindName("Import", this);
                _prev = (Border) Template.FindName("Prev", this);
                _next = (Border) Template.FindName("Next", this);
                _pBar = (ProgressBar) Template.FindName("pBar", this);
                Item.Parent = Item;
                if (Hub.WorkSpace[Hub.WorkSpace.Length - 1] == '\\')
                    Hub.WorkSpace = Hub.WorkSpace.Remove(Hub.WorkSpace.Length - 1);
                Item.Load(Hub.WorkSpace);
                ListDirectory(FoldersTree, Hub.WorkSpace);
                _next.IsEnabled = false;
                MagicLaboratory.Blur(_next);
                //Import item
                _import.PreviewMouseLeftButtonUp += _import_PreviewMouseLeftButtonUp;
                //Previous folder
                _prev.PreviewMouseLeftButtonUp += (ps, pe) =>
                {
                    if (Item.Path != "")
                    {
                        var di = new DirectoryInfo(Item.Path);
                        if (di.Parent != null && Item.Path != Hub.WorkSpace)
                        {
                            Item.NextPath = Item.Path;
                            Item.Load(di.Parent.FullName);
                            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                            {
                                _next.IsEnabled = true;
                                MagicLaboratory.UnBlur(_next);
                            }));
                        }
                    }
                };
                KeyDown += ContentsBrowser_KeyDown;
                //Next folder
                _next.PreviewMouseLeftButtonUp += (ns, ne) =>
                {
                    if (Item.Path != "")
                        if (Item.NextPath != "")
                            Item.Load(Item.NextPath);
                    _next.IsEnabled = false;
                    MagicLaboratory.Blur(_next);
                };
                _selectedItems.CollectionChanged += (c, ch) =>
                {
                    foreach (var item in _selectedItems)
                    {
                        var explorerItem = item;
                        if (explorerItem != null)
                        {
                            explorerItem.IsSelected = true;
                            explorerItem.MouseDoubleClick += ExplorerItem_MouseDoubleClick;
                        }
                    }
                };
                if (Contents != null)
                {
                    Contents.ItemsSource = Item.Items;
                    Contents.SelectionChanged += Contents_SelectionChanged;
                    Contents.PreviewKeyDown += Contents_PreviewKeyDown;
                }
                var DTimer = new DispatcherTimer();
                DTimer.Interval = new TimeSpan(0, 0, 3);
                DTimer.IsEnabled = true;
                DTimer.Tick += (ss, ee) =>
                {
                    var numOfElement = Directory.GetDirectories(Item.Path).Length +
                                       Directory.GetFiles(Item.Path).Length;
                    if (numOfElement != Item.Items.Count)
                        Refresh();
                };
            };
        }

        private ListView Contents { get; set; }

        public ExplorerItem Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }

        public string WorkSpace { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private ContextMenu contentsMenu()
        {
            var cm = new ContextMenu();
            var import_tb = new TextBlock();
            import_tb.Inlines.Add(new Run("⇓")
            {
                Foreground = Brushes.White,
                FontSize = 14,
                FontFamily = new FontFamily("Yu Gothic UI Semibold")
            });
            import_tb.Inlines.Add(new Run(" Import") {Foreground = Brushes.White});
            var new_tb = new TextBlock();
            new_tb.Inlines.Add(new Run("🗋")
            {
                Foreground = Brushes.White,
                FontSize = 14,
                FontFamily = new FontFamily("Verdana")
            });
            new_tb.Inlines.Add(new Run(" New") {Foreground = Brushes.White});
            var openexplorer_tb = new TextBlock();
            openexplorer_tb.Inlines.Add(new Run("🗀")
            {
                Foreground = Brushes.Yellow,
                FontSize = 16,
                FontFamily = new FontFamily("Yu Gothic UI Semibold")
            });
            openexplorer_tb.Inlines.Add(new Run(" Open in Folder in File Explorer") {Foreground = Brushes.White});
            var refresh_tb = new TextBlock();
            refresh_tb.Inlines.Add(new Run("↻")
            {
                Foreground = Brushes.Cyan,
                FontSize = 14,
                FontFamily = new FontFamily("Yu Gothic UI Semibold")
            });
            refresh_tb.Inlines.Add(new Run(" Refresh") {Foreground = Brushes.White});
            var import = new MenuItem
            {
                Header = import_tb
            };
            import.Click += (s, e) => { ImportFile(); };
            var newitem = new MenuItem
            {
                Header = new_tb
            };
            NewItemMenu(newitem);
            var openexplorer = new MenuItem
            {
                Header = openexplorer_tb
            };
            openexplorer.Click += (s, e) => { Process.Start(Item.Path); };
            var refresh = new MenuItem
            {
                Header = refresh_tb
            };
            refresh.Click += (s, e) => { Item.Load(Item.Path); };
            cm.Items.Add(openexplorer);
            cm.Items.Add(new Separator {Foreground = Brushes.White});
            cm.Items.Add(import);
            cm.Items.Add(newitem);
            var paste = new MenuItem
            {
                Header = new TextBlock
                {
                    Foreground = Brushes.WhiteSmoke,
                    Text = "Paste"
                }
            };
            paste.Click += (s, e) =>
            {
                try
                {
                    var p = Item.Path;
                    if (p.Substring(0, Math.Max(0, p.Length - 1)) == @"\")
                        File.Copy(PathToCopy, Item.Path + Path.GetFileName(PathToCopy));
                    else
                        File.Copy(PathToCopy, Item.Path + @"\" + Path.GetFileName(PathToCopy));
                    Item.Load(Item.Path);
                }
                catch
                {
                    //Ignored
                }
            };
            cm.Items.Add(paste);
            cm.Items.Add(new Separator {Foreground = Brushes.White});
            cm.Items.Add(refresh);
            return cm;
        }

        private void NewItemMenu(MenuItem item)
        {
            var folder_tb = new TextBlock();
            folder_tb.Inlines.Add(new Run("📁")
            {
                Foreground = Brushes.LightGoldenrodYellow,
                FontSize = 16,
                FontFamily = new FontFamily("Vendana")
            });
            folder_tb.Inlines.Add(new Run(" New Folder.") {Foreground = Brushes.White});
            var file_tb = new TextBlock();
            file_tb.Inlines.Add(new Run("🗋")
            {
                Foreground = Brushes.LightGoldenrodYellow,
                FontSize = 16,
                FontFamily = new FontFamily("Vendana")
            });
            file_tb.Inlines.Add(new Run(" New Folder.") {Foreground = Brushes.White});
            var folder = new MenuItem
            {
                Header = folder_tb
            };
            folder.Click += (s, e) =>
            {
                var name = MagicLaboratory.RandomString(6);
                Directory.CreateDirectory(Item.Path + @"\" + name);
                Item.AddFolder(name, "", Item.Path + @"\" + name);
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                    new Action(() => { Item.Items[Item.Items.Count - 1].Rename("New folder..."); }));
            };
            var file = new MenuItem
            {
                Header = file_tb
            };
            item.Items.Add(folder);
            item.Items.Add(file);
        }

        private void ImportFile()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var p = Item.Path;
                if (p.Substring(0, Math.Max(0, p.Length - 1)) == @"\")
                    File.Copy(openFileDialog.FileName, Item.Path + openFileDialog.SafeFileName);

                else
                    File.Copy(openFileDialog.FileName, Item.Path + @"\" + openFileDialog.SafeFileName);
                Refresh();
            }
        }

        public void Refresh()
        {
            Item.Load(Item.Path);
        }

        private void _import_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ImportFile();
        }

        private void ContentsBrowser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                Item.Load(Item.Path);
        }

        public void UpdateFoldersTree()
        {
            var path = Item.Path;
            var item = GetNode(path);
            if (item != null)
            {
                item.IsSelected = true;
                item.IsExpanded = true;
            }
        }

        private void FoldersTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var item = (TreeViewItem) FoldersTree.SelectedItem;
                var path = (item.Header as FolderItem).Path;
                if (path != Item.Path) Item.Load(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private TreeViewItem GetNode(string key)
        {
            TreeViewItem n = null;
            n = GetNode(key, FoldersTree.Items);
            return n;
        }

        public TreeViewItem GetNode(string key, ItemCollection nodes)
        {
            TreeViewItem n = null;


            foreach (TreeViewItem tn in nodes)
            {
                var path = (tn.Header as FolderItem).Path;
                if (path == key) return tn;
                n = GetNode(key, tn.Items);
                if (n != null) break;
            }


            return n;
        }

        public void Disable()
        {
            MagicLaboratory.Blur(Contents);

            _pBar.Visibility = Visibility.Visible;

            _pBar.IsIndeterminate = true;
        }

        public void Enable()
        {
            MagicLaboratory.UnBlur(Contents);

            _pBar.Visibility = Visibility.Collapsed;
            _pBar.IsIndeterminate = false;
        }

        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Items.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Items.Add(CreateDirectoryNode(rootDirectoryInfo));
        }

        private TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeViewItem {Header = new FolderItem(directoryInfo.FullName, directoryInfo.Name)};
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    try
                    {
                        foreach (var directory in directoryInfo.GetDirectories())
                            Task.Factory.StartNew(() =>
                            {
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                                    () => { directoryNode.Items.Add(CreateDirectoryNode(directory)); }));
                            });
                    }
                    catch
                    {
                    }
                }));
            });
            return directoryNode;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _next.IsEnabled = false;
            MagicLaboratory.Blur(_next);
        }

        private void ExplorerItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void _item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void Contents_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Contents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var i in _selectedItems)
                i.IsSelected = false;
            _selectedItems.Clear();
            foreach (ExplorerItem i in Contents.SelectedItems)
                _selectedItems.Add(i);
            e.Handled = true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}