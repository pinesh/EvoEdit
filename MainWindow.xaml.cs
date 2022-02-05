using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using Ionic.Zip;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using testapp1;
using Path = System.IO.Path;
using ZipFile = Ionic.Zip.ZipFile;

namespace EvoEditApp
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _currentdirectories;
        private string _temppath;
        private int _scale;
        private int _jobs;
        private string _globaldestinationpath;
        private FooViewModel root;

        public bool IgnorePaint;
        public bool IgnoreSlabs = false;

        public MainWindow()
        {
            InitializeComponent();

            _temppath = "";
            _globaldestinationpath = @"";
            SetProgress(0, "");
            check_output_path();
            root = this.tree.Items[0] as FooViewModel;
            root.PropertyChanged += UpdateCount;
            base.CommandBindings.Add(
                new CommandBinding(
                    ApplicationCommands.Undo,
                    (sender, e) => // Execute
                    {
                        e.Handled = true;
                        root.IsChecked = false;
                        this.tree.Focus();
                    },
                    (sender, e) => // CanExecute
                    {
                        e.Handled = true;
                        e.CanExecute = (root.IsChecked != false);
                    }));

           // this.tree.Focus();
        }

        private void prompt_path()
        {
            if (MessageBox.Show("You have not set an output folder, would you like to do so now?",
                    "Set output folder",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Setpath(); // Do something here
            }
        }

        private void Setpath()
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                _globaldestinationpath = ookiiDialog.SelectedPath;
                CurrentFolder.Text = new StringBuilder("Output: ").Append("Set").ToString();
                write_output_path();
            }
        }
        private void check_output_path()
        {
            try
            {
                string p = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "data.xml");
                FileInfo f = new FileInfo(p);
                if (f.Exists)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(p);
                    _globaldestinationpath = doc.InnerText;
                    CurrentFolder.Text = new StringBuilder("Output: ").Append("Set").ToString();
                    if (!new DirectoryInfo(_globaldestinationpath).Exists)
                    {
                        prompt_path();
                    }
                }
                else
                {
                    prompt_path();   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception();
            }
        }

        private void write_output_path()
        {
            var p = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "data.xml");
            var writer = new System.Xml.Serialization.XmlSerializer(typeof(String));
            var file = System.IO.File.Create(p);
            writer.Serialize(file, _globaldestinationpath);
            file.Close();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.DefaultExt = ".sevo";

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                
                using (FileStream fs = File.OpenRead(openFileDlg.FileName))
                {
                   var x=  BrickEntityMp.GetSaveFromFile(fs, true);
                   var s = new StringBuilder();

                   foreach (var d in x.BrickDatas.Datas)
                   {
                       s.AppendLine("["+ d.gridPosition.X+","+ d.gridPosition.Y + ","+ d.gridPosition.Z+"]");
                   }

                }
            }
        }

        public string CreateUniqueTempDirectory()
        {
            var uniqueTempDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            Directory.CreateDirectory(uniqueTempDir);
            return uniqueTempDir;
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cleanup();
        }

        private void Cleanup()
        {
           
        }
        
        //Internal scale setting changed
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cm = (ComboBox)sender;
            _scale = cm.Items.IndexOf(cm.SelectedItem);
        }

        //this is the thread entry
        private void RealStart(string p,string name, bool optimize = true, bool paint = false)
        {
            try
            {
                var interval = Math.Abs(100 / _jobs) / 3;
                if (!new DirectoryInfo(_globaldestinationpath).Exists)
                {
                    throw new Exception("Path not valid");
                }
                var s = new SmLoad(_scale,IgnoreSlabs);
                UpdateProgress(0, $"Reading:{name} this could take a while");
                s.Read(new DirectoryInfo(p));
                UpdateProgress(interval, $"");
                s.Smd.get_min_max_vector().Deconstruct(out Vector3i min, out Vector3i max);
                if (optimize)
                {
                    var m = new Mergenew(s.Smd.GetBlockList().Blist, min, max, _scale,IgnorePaint);
                    m.PropertyChanged += UpdateStateCount;
                    UpdateProgress(0, $"Optimizing:{name}");
                    m.Merge();
                    UpdateProgress(interval, "");
                    var filename = new StringBuilder(getmultiname(p, name)).Append(".sevo").ToString();
                    UpdateProgress(0, $"Writing:{name}");
                    WriteSevo(Path.Combine(_globaldestinationpath, filename), m,_scale);
                    UpdateProgress(interval, $"");
                }
                else
                {
                    UpdateProgress(interval, "");
                    var filename = new StringBuilder(getmultiname(p,name)).Append(".sevo").ToString();
                    UpdateProgress(0, $"Writing:{name}");
                    Testport(Path.Combine(_globaldestinationpath, filename), s.DumpData(),_scale);
                    UpdateProgress(interval, $"");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Thread Abort Exception {e}");
                MessageBox.Show($"Unable to import: {e.Message}", "Import error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateProgress(100, $"");
            }
            finally
            {
                Console.WriteLine("Thread Finished");
                UpdateJob();
            }
        }

        private static string getmultiname(string path,string basename)
        {
            var d = new DirectoryInfo(path);
            List<string> head = new List<string>();
            try
            {
                while (d.Name != basename)
                {
                    head.Add(d.Name);
                    d = d.Parent;
                }
                head.Add(d.Name);
                head.Reverse();
                var joinedNames = head.Aggregate((a, b) => a + "-" + b);
                return joinedNames;
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to find directory this shouldn't have happened");
                return "failed";
            }

          
        }

        //port the unoptimized blueprint. 
        private static void Testport(string name,Dictionary<Vector3i,BlockBit> blocks,int scale)
        { 
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string p = Path.Combine(Path.GetDirectoryName(strExeFilePath), "testentity.sevo");

            if (!new FileInfo(p).Exists)
            {
                throw new Exception("missing base blueprint template");
            }
            using (FileStream fs = File.OpenRead(p))
            {
                var x = BrickEntityMp.GetSaveFromFile(fs, true);
                List<BrickInstanceData> d = new List<BrickInstanceData>();
                // Launch OpenFileDialog by calling ShowDialog method
                ParentEntity parent = new ParentEntity(x);
                    int i = x.BrickDatas.Datas.Length;
                    int multiplier = (int)Math.Pow(2, scale);
                    foreach (KeyValuePair<Vector3i, BlockBit> newblock in blocks)
                    {
                        Vector3i paint = newblock.Value.GetSevoPaint();
                        byte rot = (byte)(sbyte)newblock.Value.get_Sevo_rot();
                        d.Add(new BrickInstanceData
                        {
                            scale = 0,
                            rotation = rot,
                            gridPosition = newblock.Key +  multiplier* BlockOffsets.GetOffsets(rot) + BlockOffsets.GetScaleOffsets((byte)scale),
                            brickId = (ushort)newblock.Value.GetSevoId(),
                            color = (object)new object[] { paint.X, paint.Y, paint.Z, 255 },
                            material = 0,
                            healthScore = 255,
                            gridSize = (byte)(scale),
                            instanceId = i
                        });
                        i += 1;
                    }
                    BrickDatasSave b = new BrickDatasSave()
                    {
                        AdditionalDatas = x.BrickDatas.AdditionalDatas,
                        IdsToRecycle = x.BrickDatas.IdsToRecycle,
                        Datas = x.BrickDatas.Datas.Concat(d).ToArray()
                    };

                    parent.Append(b);
                    parent.SaveToDiskAtPath(name, false);
                    Console.WriteLine("Done");
            }
        }
        private static void WriteSevo(string name,Mergenew m,int scale)
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string p = Path.Combine(Path.GetDirectoryName(strExeFilePath), "testentity.sevo");

            if (!new FileInfo(p).Exists)
            {
                throw new Exception("missing base blueprint template");
            }
            using (FileStream fs = File.OpenRead(p))
            {
                var x = BrickEntityMp.GetSaveFromFile(fs, true);
                int multiplier = (int)Math.Pow(2, scale);
                List<BrickInstanceData> d = new List<BrickInstanceData>();
                ParentEntity parent = new ParentEntity(x);
                int i = x.BrickDatas.Datas.Length;
                foreach (var newBlock in m.Mblist)
                {
                    Vector3i additional = new Vector3i(0, 0, 0);
                    Vector3i paint = BlockTypes.Sevo_Paint((short)newBlock.Type);
                    byte rot = (byte)(sbyte)newBlock.Rot;
                    var s = (ushort)BlockTypes.Sevo_ID((short)newBlock.Type);
                    //if (s == 244) rot = 0;
                    var sc = m.get_scale(newBlock.Startpos, newBlock.Endpos);
                    switch (s)
                    {
                        case 196:
                            rot = 0;
                            break;
                        case 197:
                            m.get_scale_wedge(newBlock.Startpos, newBlock.Endpos, rot)
                                .Deconstruct(out sc, out additional);
                            break;
                        case 244:
                             m.get_scale_slab(newBlock.Startpos, newBlock.Endpos, rot)
                                .Deconstruct(out sc, out additional);
                            break;
                        case 243:
                            m.get_scale_slab(newBlock.Startpos, newBlock.Endpos, rot)
                                .Deconstruct(out sc, out additional);
                            break;
                        case 245:
                            m.get_scale_slab(newBlock.Startpos, newBlock.Endpos, rot)
                                .Deconstruct(out sc, out additional);
                            m.get_scale_thiccslab(newBlock.Startpos, newBlock.Endpos, rot).Deconstruct(out sc, out Vector3i extra);
                            s = 243;
                            d.Add(new BrickInstanceData
                            {
                                scale = sc,
                                rotation = rot,
                                gridPosition = newBlock.Startpos + multiplier * BlockOffsets.GetOffsets(rot) + multiplier * extra + BlockOffsets.GetScaleOffsets((byte)scale) ,
                                brickId = (ushort)244,
                                color = (object)new object[] { paint.X, paint.Y, paint.Z, 255 },
                                material = 0,
                                healthScore = 255,
                                gridSize = (byte)(scale),
                                instanceId = i++
                            });
                            //special slab, add an additional.
                            break;
                    }
                    d.Add(new BrickInstanceData
                    {
                        scale = sc,
                        rotation = rot,
                        gridPosition = newBlock.Startpos + multiplier*BlockOffsets.GetOffsets(rot) + multiplier* additional + BlockOffsets.GetScaleOffsets((byte)scale),
                        brickId = s,
                        color = (object)new object[] { paint.X, paint.Y, paint.Z, 255 },
                        material = 0,
                        healthScore = 255,
                        gridSize = (byte)(scale),
                        instanceId = i
                    });
                    i += 1;
                }
                BrickDatasSave b = new BrickDatasSave()
                {
                    AdditionalDatas = x.BrickDatas.AdditionalDatas,
                    IdsToRecycle = x.BrickDatas.IdsToRecycle,
                    Datas = x.BrickDatas.Datas.Take(3).Concat(d).ToArray()
                };

                parent.Append(b);
                parent.SaveToDiskAtPath(name, false);
            }
        }


        public void UpdateCount(object sender, PropertyChangedEventArgs e)
        {
          //  Console.WriteLine("triggered");
            if (root.Children.Count > 0)
            {
                int i = this.root.CountChildrenRecur(this.root.Children[0].Children);
                if (i>0)
                {
                    btn_import.IsEnabled = true;
                    ((Label)((StackPanel)btn_import.Content).Children[1]).Content = $"Import[{i}] Blueprints";
                }
                else
                {
                    btn_import.IsEnabled = false;
                    ((Label)((StackPanel)btn_import.Content).Children[1]).Content = $"Import[] Blueprints";
                }
            }
        }

        public void UpdateJob()
        {
            progressBar.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new DispatcherOperationCallback(delegate
                {
                    _jobs--;
                    if (_jobs == 0)
                    {
                        progressBar.Value = 0;
                        Mouse.OverrideCursor = null;
                        Unblocker();
                    }
                    return null;
                }), null);
        }

        public void UpdateProgress(int newProgress, string newMessage)
        {
            progressBar.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new DispatcherOperationCallback(delegate
                {
                   
                    progressBar.Value += newProgress;
                    State.Text = newMessage;
                    if (progressBar.Value >= 90)
                    {
                        Mouse.OverrideCursor = null;
                        Unblocker();
                    }
                    return null;
                }), null);
        }

        public void UpdateStateCount(object sender, EventArgs e)
        {
            var ev = (PropertyChangedEventArgs)e;
            progressBar.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new DispatcherOperationCallback(delegate
                {
                    State.Text = ev.PropertyName;
                    return null;
                }), null);
        }

        public void SetProgress(int newProgress, string newMessage)
        {
            progressBar.Value = newProgress;
            State.Text = newMessage;
        }

        //Import currently selected blueprints
        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            
                if (_globaldestinationpath.Length == 0)
                {
                    MessageBox.Show("Destination path not set!");
                    return;
                }

                if (_temppath.Length == 0)
                {
                    MessageBox.Show("Folder path not set!");
                    return;
                }
                Blocker();
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            progressBar.Value = 0;
                State.Text = "Starting";
                var basename = Path.GetFileNameWithoutExtension(root.Children[0].Children[0].path);
                _jobs = this.root.CountChildrenRecur(this.root.Children[0].Children);
                var tasks = new Task[_jobs];
                int i = 0;

                bool optimize = checkBox_Optimize.IsChecked.Value;
                bool paint = checkBox_Paint.IsChecked.Value;

                var FileList = this.root.GetPathsRecur(this.root.Children[0].Children, this.root.Children[0].path, this.root.Children[0].IsChecked);
                foreach (var file in FileList)
                {
                    tasks[i] = new Task(() =>
                    {
                    RealStart(file, basename,optimize ,paint );
                    });
                    tasks[i].Start();
                    i++;
                }
        }

        public FooViewModel PopulateTree(DirectoryInfo d)
        {
            FooViewModel c = new FooViewModel(d.Name, d.FullName);
            var l = d.GetDirectories();
            if (l.Length == 0)
            {
                return c;
            }
           
            foreach (var dir in l)
            {
                c.Children.Add(PopulateTree(dir));
            }

            return c;
        }

        //import click
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (_temppath.Length >0)
            {
                this.root.Children.Clear();
                Cleanup();
            }
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "SMENT|*.sment|ZIP|*.zip";
            var result = openFileDlg.ShowDialog();
            if (result != true) return;
            if (!ZipFile.IsZipFile(openFileDlg.FileName) || !openFileDlg.FileName.EndsWith(".sment")) return;
            DirectoryInfo di = new DirectoryInfo(CreateUniqueTempDirectory());
            FileInfo f = new FileInfo(openFileDlg.FileName);
            Console.WriteLine("extracting .sment");
            using (ZipFile zip = ZipFile.Read(f.FullName))
            {
                foreach (ZipEntry entry in zip)
                {

                    entry.Extract(di.FullName);
                }
            }
            _temppath = di.FullName;

            DirectoryInfo d = new DirectoryInfo(_temppath);
            root.Children.Add(PopulateTree(d));
            root.Initialize();
            root.Children[0].Children[0].Children.Last().SetIsChecked(true,true,true);
            lbl_current.Content = Path.GetFileNameWithoutExtension(f.Name);
            Console.WriteLine(di);
        }

        private void Import_Folder(object sender, RoutedEventArgs e)
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                // do something with the folder path
                if (_temppath.Length > 0)
                {
                    this.root.Children.Clear();
                    Cleanup();
                }
                DirectoryInfo di = new DirectoryInfo(ookiiDialog.SelectedPath);

                this.root.Children.Add(PopulateTree(di));
                root.Initialize();
                root.Children[0].Children[0].Children.Last().SetIsChecked(true, true, true);
                _temppath = di.FullName;
                _currentdirectories = 1;
                lbl_current.Content = Path.GetFileNameWithoutExtension(di.Name);
                Console.WriteLine(di);
            }
        }

        private void Path_Set_Click(object sender, RoutedEventArgs e)
        {
            Setpath();
        }

        private void Coming_Soon(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming in a later update");
        }

        private void Open_Github_Guide(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Out of the box: Set up an output path, import your '.sment' blueprint, hit import!");
        }

        private void Open_Github(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/pinesh/EvoEdit");
        }

        private void Blocker()
        {
            FileHeader.IsEnabled = false;
            ConfigureHeader.IsEnabled = false;
            btn_import.IsEnabled = false;
            checkBox_Optimize.IsEnabled = false;
            checkBox_Paint.IsEnabled = false;
            cmbox_ratio.IsEnabled = false;
            groupBox.IsEnabled = false;
        }

        private void Unblocker()
        {
            FileHeader.IsEnabled = true;
            ConfigureHeader.IsEnabled = true;
            btn_import.IsEnabled = true;
            checkBox_Optimize.IsEnabled = true;
            checkBox_Paint.IsEnabled = true;
            cmbox_ratio.IsEnabled = true;
            groupBox.IsEnabled = true;
        }

        private void checkBox_Paint_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void checkBox_Slab_Checked(object sender, RoutedEventArgs e)
        {
            this.IgnoreSlabs = !IgnoreSlabs;
        }
    }
    public static class BlockOffsets
    {
        private static Dictionary<byte, Vector3i> _scaleoffsets = new Dictionary<byte, Vector3i>
        {
            { 0, new Vector3i(2, 0, 2) },
            { 1, new Vector3i(4, 0, 4) },
            { 2, new Vector3i(8, 0, 8) },
            { 3, new Vector3i(0, 16, 0) },//1m
            { 4, new Vector3i(16, -16, 16) },//2m //8 = 0.375m
            { 5, new Vector3i(48, -16, 48) },//4m
            { 6, new Vector3i(112,-16, 112) },//8m
            { 7, new Vector3i(240, -16, 240) },//16m
            { 8, new Vector3i(496, -16, 496) } //32m
        };


        private static Dictionary<byte, Vector3i> _offsets = new Dictionary<byte, Vector3i>
        {
            {0,new Vector3i(0,0,0)},
            {1,new Vector3i(0,0,0)},
            {2,new Vector3i(0,0,0)},
            {3,new Vector3i(0,0,0)},

            {4,new Vector3i(0,4,0)},
            {5,new Vector3i(0,4,0)},
            {6,new Vector3i(0,4,0)},
            {7,new Vector3i(0,4,0)},

            {8,new Vector3i(0,2,-2)},
            {9,new Vector3i(0,2,-2)},
            {10,new Vector3i(0,2,-2)},// {10,new Vector3i(0,8,-8)},
            {11,new Vector3i(0,2,-2)},// {11,new Vector3i(0,8,-8)},

            {12,new Vector3i(0,2,2)},
            {13,new Vector3i(0,2,2)},
            {14,new Vector3i(0,2,2)},
            {15,new Vector3i(0,2,2)},

            {16,new Vector3i(-2,2,0)},//{2,new Vector3i(-8,8,0)},
            {17,new Vector3i(-2,2,0)},// {17,new Vector3i(-8,8,0)},
            {18,new Vector3i(-2,2,0)},
            {19,new Vector3i(-2,2,0)},//  {19,new Vector3i(-8,8,0)},

            {20,new Vector3i(2,2,0)},
            {21,new Vector3i(2,2,0)},//  {21,new Vector3i(8,8,0)},
            {22,new Vector3i(2,2,0)},// {22,new Vector3i(8,8,0)},
            {23,new Vector3i(2,2,0)},
        };

        public static Vector3i GetScaleOffsets(byte key)
        {

            return _scaleoffsets[key];
        }

        public static Vector3i GetOffsets(byte key)
        {
            
            return _offsets[key];
        }
    }
}
