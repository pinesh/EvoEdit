using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using Ionic.Zip;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using testapp1;
using Color = System.Drawing.Color;
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
        private bool importFolder = false;
        public bool IgnorePaint;
        private List<LoadInstance> _loadedInstances;
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
            if (MessageBox.Show("You have not set an output folder, would you like to do so now? This is where your Starship Evo Blueprints will appear.",
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
                // CurrentFolder.Text = new StringBuilder("Output: ").Append("Set").ToString();
                write_output_path();
            }
        }

        private void check_output_path()
        {
            try
            {
                string p = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "data.xml");
                FileInfo f = new FileInfo(p);
                if (f.Exists)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(p);
                    _globaldestinationpath = doc.InnerText;
                    //CurrentFolder.Text = new StringBuilder("Output: ").Append("Set").ToString();
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
            var p = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "data.xml");
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
                    var x = BrickEntityMp.GetSaveFromFile(fs, true);
                    var s = new StringBuilder();

                    foreach (var d in x.BrickDatas.Datas)
                    {
                        s.AppendLine("[" + d.gridPosition.X + "," + d.gridPosition.Y + "," + d.gridPosition.Z + "]");
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

        }

        //Internal scale setting changed
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cm = (ComboBox)sender;
            _scale = cm.Items.IndexOf(cm.SelectedItem);

        }

        private static string getmultiname(string path, string basename)
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

        private static void WriteSevo(string name, Voxel_Import v, int scale)
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string p = Path.Combine(Path.GetDirectoryName(strExeFilePath), "import_template_blank.sevo");

            if (!new FileInfo(p).Exists)
            {
                throw new Exception("missing base blueprint template");
            }

            using (FileStream fs = File.OpenRead(p))
            {
                var x = BrickEntityMp.GetSaveFromFile(fs, true);
                ParentEntity parent = new ParentEntity(x);
                BrickDatasSave b = new BrickDatasSave()
                {
                    AdditionalDatas = x.BrickDatas.AdditionalDatas,
                    IdsToRecycle = x.BrickDatas.IdsToRecycle,
                    Datas = x.BrickDatas.Datas.Concat(v.ExportBricks(x.BrickDatas.Datas.Length)).ToArray()
                };
                parent.Append(b);
                parent.SaveToDiskAtPath(name, false);
            }
        }

        public void UpdateCount(object sender, PropertyChangedEventArgs e)
        {
            //  Console.WriteLine("triggered");
            if (root.Children.Count <= 0) return;
            int i = this.root.CountChildrenRecur(this.root.Children[0].Children);
            if (i > 0)
            {
                btn_readSM.IsEnabled = true;
                ((Label)((StackPanel)btn_readSM.Content).Children[1]).Content = $"Read In [{i}] Blueprints";
                ((Label)((StackPanel)btn_import.Content).Children[1]).Content = $"Export [{i}] Blueprints";
            }
            else
            {
                btn_readSM.IsEnabled = false;
                ((Label)((StackPanel)btn_readSM.Content).Children[1]).Content = $"Read In [] Blueprints";
                ((Label)((StackPanel)btn_import.Content).Children[1]).Content = $"Export [] Blueprints";
            }
        }

        public void UpdateJob()
        {
            progressBar.Dispatcher.BeginInvoke(DispatcherPriority.Background,
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
            progressBar.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(delegate
                {
                    progressBar.Value += newProgress;
                    State.Text = newMessage;
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

            if (_loadedInstances == null)
            {
                MessageBox.Show("Nothing to Import!");
                return;
            }
            switch (_loadedInstances.Count)
            {
                case 0:
                    return;
                default:
                    Blocker();
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    progressBar.Value = 0;
                    State.Text = "Starting";
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.WorkerReportsProgress = true;
                    worker.DoWork += worker_write;
                    worker.ProgressChanged += worker_ProgressChanged;
                    worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                    worker.RunWorkerAsync(new Tuple<List<LoadInstance>, bool,int>(_loadedInstances, true,_axis));
                    break;
            }
        }
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        void worker_write(object sender, DoWorkEventArgs e)
        {
            int i = 1;
           ((Tuple<List<LoadInstance>, bool,int>)e.Argument).Deconstruct(out List<LoadInstance> ls, out bool fast,out int axis);
            try
            {
                foreach (var l in ls)
                {
                    UpdateProgress(0, $"Initializing: {i}/{ls.Count}");
                    Voxel_Import Vi = new Voxel_Import(l.blocks, l.min, _scale, IgnorePaint, l.capturescale,fast);
                    Vi.PropertyChanged += (s, ev) =>
                    {
                        (sender as BackgroundWorker).ReportProgress(int.Parse(ev.PropertyName.ToString()));
                    };
                    UpdateProgress(0, $"Optimizing:{i}/{ls.Count}");
                    Vi.Optimize(axis);
                    //UpdateProgress(interval, "");
                    UpdateProgress(0, $"Writing:{i}/{ls.Count}");
                    WriteSevo(Path.Combine(_globaldestinationpath, l.newfilename), Vi, _scale);
                    i++;
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Thread Abort Exception {ex}");
                MessageBox.Show($"Unable to Import: {ex.Message}", "Import error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Console.WriteLine("Thread Finished");
                e.Result = 1;
                UpdateJob();
            }
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if((int)e.Result == 1)
                MessageBox.Show("Done Writing, Check your output!: ");
            Unblocker();
          
            btn_import.IsEnabled = false;
            LoadedFile.Text = "No Files Loaded";
            LoadedFile.Foreground = Brushes.Red;
            _loadedInstances = new List<LoadInstance>();
           
         
           
            UpdateProgress(0, $"");
            Mouse.OverrideCursor = null;
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
            importFolder = false;
            if (_temppath.Length > 0)
            {
                this.root.Children.Clear();
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
            root.Children[0].Children[0].Children.Last().SetIsChecked(true, true, true);
            lbl_current.Content = Path.GetFileNameWithoutExtension(f.Name);
            btn_readSM.IsEnabled = true;
            Console.WriteLine(di);
        }

        private void Import_Folder(object sender, RoutedEventArgs e)
        {
            importFolder = true;
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                // do something with the folder path
                if (_temppath.Length > 0)
                {
                    this.root.Children.Clear();
                }

                DirectoryInfo di = new DirectoryInfo(ookiiDialog.SelectedPath);

                this.root.Children.Add(PopulateTree(di));
                root.Initialize();
                root.Children[0].Children.Last().SetIsChecked(true, true, true);
                _temppath = di.FullName;
                _currentdirectories = 1;
                lbl_current.Content = Path.GetFileNameWithoutExtension(di.Name);
                Console.WriteLine(di);
                btn_readSM.IsEnabled = true;
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
            MessageBox.Show("To Import a StarMade blueprint: [IMPORT->Starmade->Folder/Blueprint(.sment)] [READ BLUEPRINT] [EXPORT BLUEPRINT]\n To Import a 3D Object: [IMPORT->object)][READ OBJECT][EXPORT BLUEPRINT]\n ");
        }

        private void Open_Github(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/pinesh/EvoEdit");
        }

        private void Blocker()
        {
            FileHeader.IsEnabled = false;
            ImportHeader.IsEnabled = false;
            ConfigureHeader.IsEnabled = false;
            btn_readSM.IsEnabled = false;
            btn_readObj.IsEnabled = false;
            btn_import.IsEnabled = false;
            checkBox_Paint.IsEnabled = false;
            cmbox_ratio.IsEnabled = false;
            groupBox.IsEnabled = false;
            checkBox_Paint_Copy.IsEnabled = false;
            slider.IsEnabled = false;
        }

        private void Unblocker()
        {
            ImportHeader.IsEnabled = true;
            FileHeader.IsEnabled = true;
            ConfigureHeader.IsEnabled = true;
            btn_import.IsEnabled = true;
            checkBox_Paint.IsEnabled = true;
            cmbox_ratio.IsEnabled = true;
            groupBox.IsEnabled = true;
            if(lbl_current.Content.ToString().Length != 0)
                btn_readSM.IsEnabled = true;
            if(lbl_objfile.Content.ToString().Length != 0)
                btn_readObj.IsEnabled = true;
            checkBox_Paint_Copy.IsEnabled = true;
            slider.IsEnabled = true;
        }

        private void checkBox_Paint_Checked(object sender, RoutedEventArgs e)
        {
            IgnorePaint = !IgnorePaint;
        }

        private void checkBox_Slab_Checked(object sender, RoutedEventArgs e)
        {
            this.IgnoreSlabs = !IgnoreSlabs;
        }

        public void model_button_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "OBJ|*.obj|STL|*.stl|VL32|*.vl32";
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result != true) return;
            FileInfo f = new FileInfo(openFileDlg.FileName);
            if (!f.Exists) return;
            lbl_objfile.Content = f.FullName;
            btn_readObj.IsEnabled = true;

        }

        private void Help3D_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                ">Resolution: The Voxel resolution of a model. This equates to the final max len/w/h of an object",
                "3D Importer", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HelpSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                ">Optimize (All): Should EvoEdit attempt to optimize the blocks on the currently loaded model (recommended)\n\n >IgnorePaint (StarMade): Convert all blocks to a base color (Will affect merging)\n\n >IgnoreSlabs (StarMade): Should the Importer ignore slab blocks? \n\n >Translation Ratio (All): Will port the blueprint into the set scale (1/8thm to 32m)",
                "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HelpStarmade_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "In StarMade, Docked entities (Turrets/Logic) and treated as separate entities and are named ATTACHED-X.\n\n Attached Entities can have their own Attached Entities. Block Data is found in the DATA folder.\n\n Selecting an ATTACHED folder will attempt to import the selected entity with it's own name.",
                "StarMade", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public struct LoadInstance
        {
            public string filepath;
            public string newfilename;
            public Vector3i min;
            public Dictionary<Vector3i, BlockBit> blocks;
            public int capturescale;

        }

        void worker_RunParseWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                MessageBox.Show("Parsing done, you can now import!: ");
                _loadedInstances = new List<LoadInstance>() { (LoadInstance)e.Result };
                Console.WriteLine("Obj Port Success");
                btn_import.IsEnabled = true;
                LoadedFile.Foreground = Brushes.Green;
                LoadedFile.Text = $"({_loadedInstances.Count}) File(s) Ready to Import";
        
            }
            UpdateProgress(0, $"");
            Unblocker();
            Mouse.OverrideCursor = null;
            // btn_import.IsEnabled = true;
        }
        private string ReadStl(string name)
        {
            try
            {
                progressBar.Value = 10;
                State.Text = "Converting to .obj, this could take a while";
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string p = "";
                UpdateProgress(0, $"Converting to .obj");
                var p_out = Path.Combine(_temppath, "evoOut.obj");
                var t = "\"" + Path.Combine(Path.GetDirectoryName(strExeFilePath), "stl2obj.exe") + "\"";
                Process stlProc = Process.Start(
                    new ProcessStartInfo(t)
                    {
                        Arguments = $"\"{name}\" \"{p_out}\"",
                        WindowStyle = ProcessWindowStyle.Normal,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true
                    });
                stlProc?.WaitForExit();
                if (!new FileInfo(p_out).Exists)
                {
                    throw new Exception("File wasn't written");
                }

                return p_out;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        private string Readobj(string name)
        {
            try
            {
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                progressBar.Value += 20;
                State.Text = "Converting to .vl32";
                //UpdateProgress(0, $"Converting to .vl32");
                var p_out = Path.Combine(_temppath, "evoOut.vl32");

                var temp = "\""+ Path.Combine(Path.GetDirectoryName(strExeFilePath), "obj2voxel-v1.3.4.exe")+"\"";
               // MessageBox.Show($@"running {temp} {name} {p_out} -r {voxelres}");
                Process pr = Process.Start(new ProcessStartInfo(temp)
                {
                    Arguments = $"\"{ name}\" \"{p_out}\" -r {voxelres}",
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                });
                pr.WaitForExit();
                Thread.Sleep(1000);
                if (!new FileInfo(p_out).Exists)
                {
                    throw new Exception($"Obj convert failed, no file at {p_out}");
                }
                //MessageBox.Show("convert worked");
                return p_out;
            }
            catch (Exception e)
            {
               
                Console.WriteLine(e.Message);
                MessageBox.Show(e.Message);
                return "";
            }
        }

        private void readvl32(object sender, DoWorkEventArgs e)
        {
            ((Tuple<string,string,int>)e.Argument).Deconstruct(out string p_out,out string n,out int axis);
            try
            {
              
                UpdateProgress(0, $"Reading .v132");
                using (FileStream fs = File.OpenRead(p_out))
                {
                    
                    using (SmBinary b = new SmBinary(fs))
                    {
                        int count = 0;
                        int m = 0;
                        Dictionary<Vector3i, BlockBit> test = new Dictionary<Vector3i, BlockBit>((int)b.BaseStream.Length/16);
                        while (b.BaseStream.Position <= b.BaseStream.Length - 16)
                        {
                            int x = b.ReadInt32();
                            int y = b.ReadInt32();
                            int z = b.ReadInt32();
                            _ = b.ReadByte();
                            _ = b.ReadByte();
                            _ = b.ReadByte();
                            _ = b.ReadByte();

                            var v = new Vector3i(x, y, z);
                            v.Rotate(axis);
                            double d = ((float)count / (float)b.BaseStream.Length) * 100;
                            if (d > m)
                            { (sender as BackgroundWorker).ReportProgress(m); m++;
                            }
                            if(test.ContainsKey(v))
                            {
                                Console.WriteLine(v.ToString());
                            }
                            else
                            {
                                switch (axis)
                                {
                                    case 0:
                                        test.Add(v, new BlockBit(260101, 3));
                                        break;
                                    case 1:
                                        test.Add(v, new BlockBit(260101, 4));
                                        break;
                                    case 2:
                                        test.Add(v, new BlockBit(260101, 0));
                                        break;
                                    case 3:
                                        test.Add(v, new BlockBit(260101, 0));
                                        break;
                                }
                             
                            }
                            //BlobCache.InMemory.InsertObject(v.ToString(), new BlockBit(260101, 3));
                            count += 16;
                        }
                        e.Result = new LoadInstance()
                        {
                            blocks = test,
                            filepath = p_out,
                            newfilename = Path.Combine(_globaldestinationpath, new StringBuilder(n).Append(".sevo").ToString()),
                            min = new Vector3i(0, 0, 0),
                            capturescale = _scale
                        };
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show($"Unable to Parse: {error.ToString()}", "Parse error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                e.Result = new LoadInstance();
            }
        }

        private async Task<List<LoadInstance>> ReadBlueprints()
        {
            List<LoadInstance> l = new List<LoadInstance>();
            string basename = "";
            List<string> FileList = new List<string>();
            if (importFolder)
            {
                basename = Path.GetFileNameWithoutExtension(root.Children[0].path);
                _jobs = this.root.CountChildrenRecur(this.root.Children[0].Children);
                FileList = this.root.GetPathsRecur(this.root.Children[0].Children, this.root.Children[0].path, this.root.Children[0].IsChecked);
            }
            else
            {
                basename = Path.GetFileNameWithoutExtension(importFolder ? root.Children[0].path : root.Children[0].Children[0].path);
                _jobs = this.root.CountChildrenRecur(this.root.Children[0].Children);
                FileList = this.root.GetPathsRecur(this.root.Children[0].Children, this.root.Children[0].path, this.root.Children[0].IsChecked);
            }
            //_jobs = this.root.CountChildrenRecur(this.root.Children[0].Children);
            var interval = 100 / _jobs;
            var t = FileList.Select(file => ReadBlueprint(file, basename, interval));
   
            LoadInstance[] listInstances = await Task.WhenAll(t);
            return listInstances.ToList();
        }
        private async Task<LoadInstance> ReadBlueprint(string p,string name,int interval)
        {
            try
            {
                var s = new SmLoad(_scale, IgnoreSlabs);
                s.Read(new DirectoryInfo(p));
                s.Smd.get_min_max_vector().Deconstruct(out Vector3i min, out Vector3i max);

                var l = new LoadInstance()
                {
                    filepath = p,
                    newfilename = Path.Combine(_globaldestinationpath, new StringBuilder(getmultiname(p, name)).Append(".sevo").ToString()),
                    min = min,
                    blocks = s.Smd.GetBlockList().Blist,
                    capturescale = _scale
                    
                };
                UpdateProgress(interval, $"");
                return l;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Thread Abort Exception {e}");
                MessageBox.Show($"Unable to Read: {e.Message}", "Read error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Console.WriteLine("Thread Finished");
                //UpdateJob();
               
            }
            return new LoadInstance();
        }
        
        private async void btn_readSM_Click(object sender, RoutedEventArgs e)
        {
            _loadedInstances = new List<LoadInstance>();
            btn_import.IsEnabled = false;
            //Set up a read to load into a list of entities.
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
            Task<List<LoadInstance>> t = ReadBlueprints();
            _loadedInstances = await t;
            LoadedFile.Foreground = Brushes.Green;
            LoadedFile.Text = $"({_loadedInstances.Count}) File(s) Ready to Import";
            Console.WriteLine($"loaded {_loadedInstances.Count}");
            Unblocker();
            Mouse.OverrideCursor = null;
            btn_import.IsEnabled = true;
            MessageBox.Show("Done reading, you can now Import!");

        }

        private void btn_readObj_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_temppath.Length == 0)
                {
                    DirectoryInfo di = new DirectoryInfo(CreateUniqueTempDirectory());
                    _temppath = di.FullName;
                }

  
                progressBar.Value = 0;
                //set up a read and convert into vox file.
                //_loadedInstances = new List<LoadInstance>();
                btn_import.IsEnabled = false;
                //  LoadInstance[] listInstances = await Task.WhenAll(t);

                var path = lbl_objfile.Content.ToString();
                var outputname = Path.GetFileNameWithoutExtension(Path.GetFileName(path));


                if (path.Length == 0)
                {
                    //We have no file, shouldn't happen
                    return;
                }
                Blocker();
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                if(Path.GetExtension(path) == ".stl")
                {
                    path = ReadStl(path);
                }

                if (Path.GetExtension(path) == ".obj")
                {
                    path = Readobj(path);
                }

                Mouse.OverrideCursor = null;
                if (Path.GetExtension(path) != ".vl32" || path == "")
                {
                    Unblocker();
                    Mouse.OverrideCursor = null;
                    UpdateProgress(0,"");
                    return;
                }

                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += readvl32;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerCompleted += worker_RunParseWorkerCompleted;
                worker.RunWorkerAsync(new Tuple<string,string,int>(path,outputname, _axis));
            }
            catch(Exception er)
            {
                MessageBox.Show($"Unable to Read: {er.Message}", "Read error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txt_slider == null) return;
            if (bounded)
            {
                bounded = false;
            }
            else
            {
                txt_slider.Text = e.NewValue.ToString();
            }


        }

        private void txt_slider_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private int voxelres;
        private bool bounded = false;

        private void txt_slider_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txt_slider.Text.Length == 0)
            {
                txt_slider.Text = "0";
            }

            voxelres = int.Parse(txt_slider.Text);
            if (voxelres > 1024 || voxelres < 248)
                bounded = true;
            slider.Value = voxelres;
        }

        private void OpenColorConfig_click(object sender, RoutedEventArgs e)
        {
            var window = new PaintWindow();
            window.Owner = this;
            window.Show();
        }

        private int _axis = 0;
        private void comboBox_Axis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cm = (ComboBox)sender;
            _axis = cm.Items.IndexOf(cm.SelectedItem);
        }
    }
    
}
