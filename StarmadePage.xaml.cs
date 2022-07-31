using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
using Color = System.Drawing.Color;
using Path = System.IO.Path;
using ZipFile = Ionic.Zip.ZipFile;
using Ionic.Zip;
using Ookii.Dialogs.Wpf;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace EvoEditApp
{
    /// <summary>
    /// Interaction logic for StarmadePage.xaml
    /// </summary>
    public partial class StarmadePage : Page
    {
        private int _jobs;
    
        private readonly string _globaldestinationpath;
        private FooViewModel root;
        private string _temppath;
        private List<MainWindow.LoadInstance> _loadedInstances;
     
        private int _scale = 3;
        private int _axis = 0;

        public bool? IgnoreSlabs
        {
            get;
            set;
        }
        public bool? EnableSym
        {
            get;
            set;
        }
        public bool? IgnorePaint
        {
            get;
            set;
        }


        public StarmadePage()
        {
            InitializeComponent();
            IgnorePaint = false;
            EnableSym = false;
            IgnoreSlabs = false;
            _globaldestinationpath = EvoEditWindow.OutputFolderHandler.check_output_path();
            _temppath = "";
            root = this.tree.Items[0] as FooViewModel;
           // root.PropertyChanged += UpdateCount;
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

        }

        
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_temppath.Length > 0)
            {
                this.root.Children.Clear();
            }

            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "SMENT|*.sment|ZIP|*.zip";
            var result = openFileDlg.ShowDialog();
            if (result != true) return;

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (!ZipFile.IsZipFile(openFileDlg.FileName) || !openFileDlg.FileName.EndsWith(".sment")) return;
            DirectoryInfo di = new DirectoryInfo(EvoEditWindow.OutputFolderHandler.CreateUniqueTempDirectory());
            FileInfo f = new FileInfo(openFileDlg.FileName);
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
        private async Task<List<MainWindow.LoadInstance>> ReadBlueprints()
        {
            
            bool importFolder = false;
            List<MainWindow.LoadInstance> l = new List<MainWindow.LoadInstance>();
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

            if (_jobs == 0)
            {
                throw new Exception("Error: No blueprints selected for import!");
            }

            //_jobs = this.root.CountChildrenRecur(this.root.Children[0].Children);
            var interval = 100 / _jobs;
            var t = FileList.Select(file => ReadBlueprint(file, basename, interval));

            MainWindow.LoadInstance[] listInstances = await Task.WhenAll(t);
            return listInstances.ToList();
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
        private async Task<MainWindow.LoadInstance> ReadBlueprint(string p, string name, int interval)
        {
            try
            {
                var s = new SmLoad(_scale, IgnoreSlabs.Value);
                s.Read(new DirectoryInfo(p));
                s.Smd.get_min_max_vector().Deconstruct(out Vector3i min, out Vector3i max);
                var b = s.Smd.GetBlockListPairs(EnableSym.Value, 0);
                var l = new MainWindow.LoadInstance()
                {
                    filepath = p,
                    newfilename = Path.Combine(_globaldestinationpath, new StringBuilder(getmultiname(p, name)).Append(".sevo").ToString()),
                    min = min,
                    blocks = b[0],
                    blocksDiff = b[1],
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
            return new MainWindow.LoadInstance();
        }


        private async void ExportBtnClick(object sender, RoutedEventArgs e)
        {
            _loadedInstances = new List<MainWindow.LoadInstance>();
            //Set up a read to load into a list of entities.
            if (_globaldestinationpath.Length == 0)
            {
                Snackbar.MessageQueue?.Enqueue("Destination path not set!");
                return;
            }
            if (_temppath.Length == 0)
            {
                Snackbar.MessageQueue?.Enqueue("Starmade import not chosen!");
                return;
            }
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            progressBar.Value = 0;
            progressBarText.Text = "";
            try
            {
                Task<List<MainWindow.LoadInstance>> t = ReadBlueprints();
                _loadedInstances = await t;
            }
            catch (Exception er)
            {
                Mouse.OverrideCursor = null;
                Snackbar.MessageQueue?.Enqueue(er.Message);
               //MessageBox.Show(er.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            
            Console.WriteLine($"loaded {_loadedInstances.Count}");
            Mouse.OverrideCursor = null;

            if (_globaldestinationpath.Length == 0)
            {
                Snackbar.MessageQueue?.Enqueue("Destination path not set!");
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
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    progressBar.Value = 0;
                    progressBarText.Text = "";
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.WorkerReportsProgress = true;
                    worker.DoWork += worker_write;
                    worker.ProgressChanged += worker_ProgressChanged;
                    worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                    worker.RunWorkerAsync(new Tuple<List<MainWindow.LoadInstance>, bool, int>(_loadedInstances, EnableSym.Value, _axis));
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

            ((Tuple<List<MainWindow.LoadInstance>, bool, int>)e.Argument).Deconstruct(out List<MainWindow.LoadInstance> ls, out bool EnableSym, out int axis);
            Console.WriteLine(ls[0].blocks.Count);
            try
            {
                foreach (var l in ls)
                {
                    UpdateProgress(0, $"Initializing: {i}/{ls.Count}");
                    Voxel_Import Vi = new Voxel_Import(l.blocks, l.min, _scale, IgnorePaint.Value, l.capturescale, EnableSym);
                    Voxel_Import Vi2 = new Voxel_Import(l.blocksDiff, l.min, _scale, IgnorePaint.Value, l.capturescale, EnableSym);
                    Vi.PropertyChanged += (s, ev) =>
                    {
                        (sender as BackgroundWorker).ReportProgress(int.Parse(ev.PropertyName.ToString()));
                    };
                    Vi2.PropertyChanged += (s, ev) =>
                    {
                        (sender as BackgroundWorker).ReportProgress(int.Parse(ev.PropertyName.ToString()));
                    };
                    UpdateProgress(0, $"Optimizing:{i}/{ls.Count}");
                    Vi.OptimizeSym(EnableSym == false ? 3 : axis);
                    UpdateProgress(0, $"Optimizing:{i}/{ls.Count}");
                    Vi2.Optimize();
                    //UpdateProgress(interval, "");
                    UpdateProgress(0, $"Writing:{i}/{ls.Count}");
                    WriteSevo(Path.Combine(_globaldestinationpath, l.newfilename), Vi,Vi2, _scale);
                    i++;
                }
                e.Result = 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Thread Abort Exception {ex}");
                MessageBox.Show($"Unable to Import: {ex.Message}", "Import error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                e.Result = -1;
            }
            finally
            {
                Console.WriteLine("Thread Finished");
            }
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((int)e.Result == 1)
            {
                progressBarText.Text = "Done";
                Snackbar.MessageQueue?.Enqueue("Import Success");
            }
            else
            {
                Snackbar.MessageQueue?.Enqueue("Unknown Error : Import Failed");
            }

            _loadedInstances = new List<MainWindow.LoadInstance>();
            //UpdateProgress(0, $"");
            Mouse.OverrideCursor = null;
        }

        public void UpdateProgress(int newProgress, string newMessage)
        {
            progressBar.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(delegate
                {
                    if (progressBar.Value < 100)
                    {
                        progressBar.Value += newProgress;
                        progressBarText.Text = newMessage;
                    }
                    else
                    {
                        progressBarText.Text = "Done";
                    }
                   
                    return null;
                }), null);
        }

        private static void WriteSevo(string name, Voxel_Import v, Voxel_Import v2, int scale)
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
                var datas = x.BrickDatas.Datas.Concat(v.ExportBricks(x.BrickDatas.Datas.Length)).ToArray();
                ParentEntity parent = new ParentEntity(x);
                BrickDatasSave b = new BrickDatasSave()
                {
                    AdditionalDatas = x.BrickDatas.AdditionalDatas,
                    IdsToRecycle = x.BrickDatas.IdsToRecycle,
                    Datas = datas.Concat(v2.ExportBricks(datas.Length)).ToArray()
                };
                parent.Append(b);
                parent.SaveToDiskAtPath(name, false);
            }
        }

        private void IgnorePaintBtn_Click(object sender, RoutedEventArgs e)
        {
            IgnorePaint = !IgnorePaint;
            IgnorePaintCheckbox.IsChecked = IgnorePaint;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IgnoreSlabs = !IgnoreSlabs;
            IgnoreSlabCheckbox.IsChecked = IgnoreSlabs;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            EnableSym = !EnableSym;
            SymCheckbox.IsChecked = EnableSym;
            if (EnableSym.Value == true)
            {
                Snackbar.MessageQueue?.Enqueue("Warning: This will break asymmetrical ships when center-line differs from center");
            }
        }
    }
}
