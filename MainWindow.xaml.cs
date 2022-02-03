using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Ionic.Zip;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Path = System.IO.Path;
using ZipFile = Ionic.Zip.ZipFile;

namespace EvoEditApp
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int currentdirectories;
        private string temppath;
        private int scale;
        private int jobs;
        private string globaldestinationpath;
        public MainWindow()
        {
            InitializeComponent();
            CreateCheckBoxList(); this.DataContext = this;
            temppath = "";
            globaldestinationpath = @"";
            SetProgress(0, "");
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
                   var x=  BrickEntity_mp.GetSaveFromFile(fs, true);
                   var s = new StringBuilder();

                   foreach (var d in x.brickDatas.datas)
                   {
                       s.AppendLine("["+ d.gridPosition.x+","+ d.gridPosition.y + ","+ d.gridPosition.z+"]");
                   }

                   //textBox.Text = s.ToString();
                }
            }
        }

        public string CreateUniqueTempDirectory()
        {
            var uniqueTempDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            Directory.CreateDirectory(uniqueTempDir);
            return uniqueTempDir;
        }
        
        public class BoolStringClass
        {
            public string name { get; set; }
            public string path { get; set; }
            public bool IsSelected { get; set; }
        }
        public ObservableCollection<BoolStringClass> FileList { get; set; }
        public void CreateCheckBoxList()
        {
            FileList = new ObservableCollection<BoolStringClass>();
            MyListView.ItemsSource = FileList;
            this.DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cleanup();
        }

        private void cleanup()
        {
           
        }
        
        //Internal scale setting changed
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cm = (ComboBox)sender;
            var i = cm.Items.IndexOf(cm.SelectedItem);
            scale = (int)Math.Pow(2, i);
        }

        //this is the thread entry
        private void RealStart(string p,string name, bool optimize = true, bool paint = false)
        {
            try
            {
                var interval = Math.Abs(100 / jobs) / 3;
                if (!new DirectoryInfo(globaldestinationpath).Exists)
                {
                    throw new Exception("Path not valid");
                }
                var s = new SM_Load(scale);
                UpdateProgress(0, $"Reading:{name}");
                s.Read(new DirectoryInfo(p));
                UpdateProgress(interval, $"");
                s.smd.get_min_max_vector().Deconstruct(out Vector3i min, out Vector3i max);
                if (optimize)
                {
                    var m = new mergenew(s.smd.getBlockList().blist, min, max, scale);
                    UpdateProgress(0, $"Optimizing:{name}");
                    m.merge();
                    UpdateProgress(interval, "");
                    var filename = new StringBuilder(name).Append("-" + Path.GetFileName(p)).Append(".sevo").ToString();
                    UpdateProgress(0, $"Writing:{name}");
                    WriteSevo(Path.Combine(globaldestinationpath, filename), m,scale);
                    UpdateProgress(interval, $"");
                }
                else
                {
                    UpdateProgress(interval, "");
                    var filename = new StringBuilder(name).Append("-" + Path.GetFileName(p)).Append(".sevo").ToString();
                    UpdateProgress(0, $"Writing:{name}");
                    testport(Path.Combine(globaldestinationpath, filename), s.DumpData(),scale);
                    UpdateProgress(interval, $"");
                }
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine("Thread Abort Exception");
            }
            finally
            {
                Console.WriteLine("Thread Finished");
                UpdateProgress(0, $"");
            }
        }
        //port the unoptimized blueprint. 
        private static void testport(string name,Dictionary<Vector3i,BlockBit> blocks,int scale)
        {
            var scalenum = (int)Math.Log(scale, 2);
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string p = Path.Combine(Path.GetDirectoryName(strExeFilePath), "testentity.sevo");

            if (!new FileInfo(p).Exists)
            {
                throw new Exception("missing base blueprint template");
            }
            using (FileStream fs = File.OpenRead(p))
            {
                var x = BrickEntity_mp.GetSaveFromFile(fs, true);
                List<BrickInstanceData> d = new List<BrickInstanceData>();
                // Launch OpenFileDialog by calling ShowDialog method
                ParentEntity P = new ParentEntity(x);
                    int i = x.brickDatas.datas.Length;
                    foreach (KeyValuePair<Vector3i, BlockBit> newblock in blocks)
                    {
                        Vector3i paint = newblock.Value.GetSevoPaint();
                        byte rot = (byte)(sbyte)newblock.Value.get_Sevo_rot();
                        d.Add(new BrickInstanceData
                        {
                            scale = 0,
                            rotation = rot,
                            gridPosition = newblock.Key + scale * BlockOffsets.getOffsets(rot),
                            brickId = (ushort)newblock.Value.GetSevoID(),
                            color = (object)new object[] { paint.x, paint.y, paint.z, 255 },
                            material = 0,
                            healthScore = 255,
                            gridSize = (byte)(3+ scalenum),
                            instanceId = i
                        });
                        i += 1;
                    }
                    BrickDatasSave b = new BrickDatasSave()
                    {
                        additionalDatas = x.brickDatas.additionalDatas,
                        idsToRecycle = x.brickDatas.idsToRecycle,
                        datas = x.brickDatas.datas.Concat(d).ToArray()
                    };

                    P.Append(b);
                    P.SaveToDiskAtPath(name, false);
                    Console.WriteLine("Done");
            }
        }
        private static void WriteSevo(string name,mergenew m,int scale)
        {
            var scalenum = (int)Math.Log(scale,2);
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string p = Path.Combine(Path.GetDirectoryName(strExeFilePath), "testentity.sevo");

            if (!new FileInfo(p).Exists)
            {
                throw new Exception("missing base blueprint template");
            }
            using (FileStream fs = File.OpenRead(p))
            {
                var x = BrickEntity_mp.GetSaveFromFile(fs, true);

                List<BrickInstanceData> d = new List<BrickInstanceData>();
                ParentEntity P = new ParentEntity(x);
                int i = x.brickDatas.datas.Length;
                foreach (var newBlock in m.mblist)
                {
                    Vector3i additional = new Vector3i(0, 0, 0);
                    Vector3i paint = BlockTypes.Sevo_Paint((short)newBlock.type);
                    byte rot = (byte)(sbyte)newBlock.rot;
                    var s = (ushort)BlockTypes.Sevo_ID((short)newBlock.type);
                    var sc = m.get_scale(newBlock.startpos, newBlock.endpos);
                    switch (s)
                    {
                        case 196:
                            rot = 0;
                            break;
                        case 197:
                            m.get_scale_wedge(newBlock.startpos, newBlock.endpos, rot)
                                .Deconstruct(out sc, out additional);
                            break;
                    }



                    d.Add(new BrickInstanceData
                    {
                        scale = sc,
                        rotation = rot,
                        gridPosition = newBlock.startpos + scale*BlockOffsets.getOffsets(rot) + scale*additional,
                        brickId = (ushort)BlockTypes.Sevo_ID((short)newBlock.type),
                        color = (object)new object[] { paint.x, paint.y, paint.z, 255 },
                        material = 0,
                        healthScore = 255,
                        gridSize = (byte)(3 + scalenum),
                        instanceId = i
                    });
                    i += 1;
                }
                BrickDatasSave b = new BrickDatasSave()
                {
                    additionalDatas = x.brickDatas.additionalDatas,
                    idsToRecycle = x.brickDatas.idsToRecycle,
                    datas = x.brickDatas.datas.Take(3).Concat(d).ToArray()
                };

                P.Append(b);
                P.SaveToDiskAtPath(name, false);
            }
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
                        unblocker();
                    }
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
             
                if (globaldestinationpath.Length == 0)
                {
                    MessageBox.Show("Destination path not set!");
                    return;
                }

                if (temppath.Length == 0)
                {
                    MessageBox.Show("Folder path not set!");
                    return;
                }
                blocker();
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            progressBar.Value = 0;
                State.Text = "Starting";
                var basename = Path.GetFileNameWithoutExtension(lbl_filename.Content.ToString());
                jobs = FileList.Count(x => x.IsSelected == true);
                var tasks = new Task[jobs];
                int i = 0;

                bool optimize = checkBox_Optimize.IsChecked.Value;
                bool paint = checkBox_Paint.IsChecked.Value;
                foreach (var file in FileList.Where(x => x.IsSelected == true))
                {
                    tasks[i] = new Task(() =>
                    {
                        RealStart(file.path, basename,optimize ,paint );
                    });
                    tasks[i].Start();
                    i++;
                }
        }

        //import click
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (temppath.Length >0)
            {
                FileList.Clear();
                cleanup();
            }
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "SMENT|*.sment|ZIP|*.zip";
            var result = openFileDlg.ShowDialog();
            if (result != true) return;
            if (!ZipFile.IsZipFile(openFileDlg.FileName) || !openFileDlg.FileName.EndsWith(".sment")) return;
            DirectoryInfo di = new DirectoryInfo(CreateUniqueTempDirectory());
            FileInfo f = new FileInfo(openFileDlg.FileName);
            lbl_filename.Content = Path.GetFileName(openFileDlg.FileName);

            Console.WriteLine("extracting .sment");
            using (ZipFile zip = ZipFile.Read(f.FullName))
            {
                foreach (ZipEntry entry in zip)
                {

                    entry.Extract(di.FullName);
                }
            }
            temppath = di.FullName;

            DirectoryInfo d = new DirectoryInfo(temppath);
            foreach (var cur_d in (d.GetDirectories())[0].GetDirectories())
            {
                var b = new BoolStringClass { name = cur_d.Name, path = cur_d.FullName, IsSelected = false };
                if (cur_d.Name == "DATA")
                {
                    b.IsSelected = true;
                }
                FileList.Add(b);
            }
            currentdirectories=1;
            
            Console.WriteLine(di);
            ((Label)((StackPanel)btn_import.Content).Children[1]).Content = "Import[1] Blueprints";
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            currentdirectories++;
            ((Label)((StackPanel)btn_import.Content).Children[1]).Content = $"Import[{currentdirectories}] Blueprints";
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            currentdirectories--;
            ((Label)((StackPanel)btn_import.Content).Children[1]).Content = $"Import[{currentdirectories}] Blueprints";
        }

        private void Import_Folder(object sender, RoutedEventArgs e)
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                // do something with the folder path
                if (temppath.Length > 0)
                {
                    FileList.Clear();
                    cleanup();
                }
                DirectoryInfo di = new DirectoryInfo(ookiiDialog.SelectedPath);
                

                lbl_filename.Content = Path.GetFileName(di.Name);
                temppath = di.FullName;

                foreach (var cur_d in di.GetDirectories())
                {
                    var b = new BoolStringClass { name = cur_d.Name, path = cur_d.FullName, IsSelected = false };
                    if (cur_d.Name == "DATA")
                    {
                        b.IsSelected = true;
                    }
                    FileList.Add(b);
                }
                currentdirectories = 1;

                Console.WriteLine(di); 
                ((Label)((StackPanel)btn_import.Content).Children[1]).Content = "Import[1] Blueprints";
            }
        }

        private void Path_Set_Click(object sender, RoutedEventArgs e)
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                globaldestinationpath = ookiiDialog.SelectedPath;
                CurrentFolder.Text = new StringBuilder("Output: ").Append(globaldestinationpath).ToString();
            }
        }

        private void Coming_Soon(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming in the next update");
        }

        private void Open_Github_Guide(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Out of the box: Set up a path, import your '.sment' blueprint, hit import!");
        }

        private void Open_Github(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/pinesh/EvoEdit");
        }

        private void blocker()
        {
            FileHeader.IsEnabled = false;
            ConfigureHeader.IsEnabled = false;
            btn_import.IsEnabled = false;
            checkBox_Optimize.IsEnabled = false;
            checkBox_Paint.IsEnabled = false;
            cmbox_ratio.IsEnabled = false;
            groupBox.IsEnabled = false;
        }

        private void unblocker()
        {
            FileHeader.IsEnabled = true;
            ConfigureHeader.IsEnabled = true;
            btn_import.IsEnabled = true;
            checkBox_Optimize.IsEnabled = true;
            checkBox_Paint.IsEnabled = true;
            cmbox_ratio.IsEnabled = true;
            groupBox.IsEnabled = true;
        }

    }


    public static class BlockOffsets
    {
        private static Dictionary<byte, Vector3i> offsets = new Dictionary<byte, Vector3i>
        {
            {0,new Vector3i(0,0,0)},
            {1,new Vector3i(0,0,0)},
            {2,new Vector3i(0,0,0)},
            {3,new Vector3i(0,0,0)},

            {4,new Vector3i(0,32,0)},
            {5,new Vector3i(0,32,0)},
            {6,new Vector3i(0,32,0)},
            {7,new Vector3i(0,32,0)},

            {8,new Vector3i(0,16,-16)},
            {9,new Vector3i(0,16,-16)},
            {10,new Vector3i(0,16,-16)},// {10,new Vector3i(0,8,-8)},
            {11,new Vector3i(0,16,-16)},// {11,new Vector3i(0,8,-8)},

            {12,new Vector3i(0,16,16)},
            {13,new Vector3i(0,16,16)},
            {14,new Vector3i(0,16,16)},
            {15,new Vector3i(0,16,16)},

            {16,new Vector3i(-16,16,0)},//{16,new Vector3i(-8,8,0)},
            {17,new Vector3i(-16,16,0)},// {17,new Vector3i(-8,8,0)},
            {18,new Vector3i(-16,16,0)},
            {19,new Vector3i(-16,16,0)},//  {19,new Vector3i(-8,8,0)},

            {20,new Vector3i(16,16,0)},
            {21,new Vector3i(16,16,0)},//  {21,new Vector3i(8,8,0)},
            {22,new Vector3i(16,16,0)},// {22,new Vector3i(8,8,0)},
            {23,new Vector3i(16,16,0)},
        };
        


        public static Vector3i getOffsets(byte key)
        {
            
            return offsets[key];
        }
    }
}
