using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using Ookii.Dialogs.Wpf;
using testapp1;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace EvoEditApp
{
    /// <summary>
    /// Interaction logic for _3dPage.xaml
    /// </summary>
    public partial class _3dPage : Page
    {
        private string _globaldestinationpath;
        public _3dPage()
        {
            InitializeComponent();
            _temppath = "";
            _globaldestinationpath = EvoEditWindow.OutputFolderHandler.check_output_path();
        }


        public void model_button_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "OBJ|*.obj|STL|*.stl|VL32|*.vl32";
            var result = openFileDlg.ShowDialog();
            if (result != true) return;
            FileInfo f = new FileInfo(openFileDlg.FileName);
            if (!f.Exists) return;
            lbl_objfile.Content = f.FullName;

        }

        private string ReadStl(string name)
        {
            try
            {
                progressBar.Value = 10;
                progressBarText.Text = "Converting to .obj, this could take a while";
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
            catch (Exception e)
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
                progressBarText.Text = "Converting to .vl32";
                //UpdateProgress(0, $"Converting to .vl32");
                var p_out = Path.Combine(_temppath, "evoOut.vl32");

                var temp = "\"" + Path.Combine(Path.GetDirectoryName(strExeFilePath), "obj2voxel-v1.3.4.exe") + "\"";
                // MessageBox.Show($@"running {temp} {name} {p_out} -r {voxelres}");

                string arguments = $"\"{name}\" \"{p_out}\" -r {voxelres}";
                Console.WriteLine(arguments);
                Process pr = Process.Start(new ProcessStartInfo(temp)
                {
                    Arguments = arguments,
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


        public List<Dictionary<Vector3i, BlockBit>> GetBlockListPairs(Dictionary<Vector3i, BlockBit> _blist, bool mirror, int axis = 0)
        {
            int precount = _blist.Count;
            //axis = 0;
            if (!mirror) return new List<Dictionary<Vector3i, BlockBit>> { _blist, null };
            Console.WriteLine($"premirror = {_blist.Count}");
            Dictionary<Vector3i, BlockBit> blist = new Dictionary<Vector3i, BlockBit>();
            Dictionary<Vector3i, BlockBit> difflist = new Dictionary<Vector3i, BlockBit>();
            //cut in half perfectly


            TryMirror(_blist,axis).Deconstruct(out int bcount,out double d);
            foreach (var b in _blist.Where(b => b.Key[axis] >= d && b.Value.GetSevoId() == 196))
            {
                //blist.Add(b.Key, b.Value);
                var diff = b.Key[axis] - (b.Key[axis] - d) * 2;
                var mirrored = new Vector3i(b.Key.X, b.Key.Y, b.Key.Z);
                mirrored[axis] = (int)diff;
                if (_blist.ContainsKey(mirrored))
                {
                    if (mirrored[axis] == b.Key[axis])
                    {
                        //middle identified
                        difflist.Add(b.Key, b.Value);
                    }
                    else if (_blist[mirrored].GetSevoId() == 196)
                    {
                        blist.Add(b.Key, b.Value);
                        blist.Add(mirrored, b.Value);
                    }
                    else
                    {
                        difflist.Add(b.Key, b.Value);
                    }
                }
                else
                {
                    difflist.Add(b.Key, b.Value);
                }

            }

            int postcount = blist.Count + difflist.Count;
            if (postcount != precount)
            {
                Console.WriteLine("lost blocks");
            }

            return new List<Dictionary<Vector3i, BlockBit>>() { blist, difflist };
        }


        private Tuple<int,double> TryMirror(Dictionary<Vector3i, BlockBit> _blist, int axis)
        {
            int bcount = 0;
            int diffcount = 0;
            int? minAxis = null;
            int? maxAxis = null;

            foreach (var keyPair in _blist)
            {
                if (!minAxis.HasValue)
                {
                    minAxis = keyPair.Key[axis];
                }

                if (!maxAxis.HasValue)
                {
                    maxAxis = keyPair.Key[axis];
                }

                if (keyPair.Key[axis] > maxAxis)
                    maxAxis = keyPair.Key[axis];
                if (keyPair.Key[axis] < minAxis)
                    minAxis = keyPair.Key[axis];
            }
            double d = (double)(maxAxis.Value - minAxis.Value) / 2;
            d = d + minAxis.Value;

            foreach (var b in _blist.Where(b => b.Key[axis] > d && b.Value.GetSevoId() == 196))
            {
                var diff = b.Key[axis] - (b.Key[axis] - d) * 2;
                var mirrored = new Vector3i(b.Key.X, b.Key.Y, b.Key.Z);
                mirrored[axis] = (int)diff;
                if (_blist.ContainsKey(mirrored))
                {
                    if (_blist[mirrored].GetSevoId() == 196)
                    {
                        bcount+=2;
                    }
                }

            }

                progressBar.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(delegate
                    {
                        Snackbar.MessageQueue?.Enqueue($"Mirror match:{(double)(((double)bcount / (double)_blist.Count)*100)}%");
                        return null;
                    }), null);
             
            

            return new Tuple<int, double>(bcount,d);
        }

        private void readvl32(object sender, DoWorkEventArgs e)
        {
            ((Tuple<string, string, int,bool>)e.Argument).Deconstruct(out string p_out, out string n, out int axis, out bool EnableSym);
            try
            {

                UpdateProgress(0, $"Reading .v132");
                using (FileStream fs = File.OpenRead(p_out))
                {

                    using (SmBinary b = new SmBinary(fs))
                    {
                    
                        int count = 0;
                        int m = 0;
                        Console.WriteLine(b.BaseStream.Length);
                        Dictionary<Vector3i, BlockBit> _blist = new Dictionary<Vector3i, BlockBit>((int)b.BaseStream.Length / 16);
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
                            {
                                (sender as BackgroundWorker).ReportProgress(m); m++;
                            }
                            if (_blist.ContainsKey(v))
                            {
                                Console.WriteLine(v.ToString());
                            }
                            else
                            {
                                switch (axis)
                                {
                                    case 0:
                                        _blist.Add(v, new BlockBit(260101, 3));
                                        break;
                                    case 1:
                                        _blist.Add(v, new BlockBit(260101, 4));
                                        break;
                                    case 2:
                                        _blist.Add(v, new BlockBit(260101, 0));
                                        break;
                                    case 3:
                                        _blist.Add(v, new BlockBit(260101, 0));
                                        break;
                                }

                            }
                            count += 16;
                        }
                        Console.WriteLine(_blist.Count);
                        List<Dictionary<Vector3i, BlockBit>> split = GetBlockListPairs(_blist,EnableSym);

                        var ls = new MainWindow.LoadInstance()
                        {
                            blocks = split[0],
                            filepath = p_out,
                            blocksDiff = split[1],
                            newfilename = Path.Combine(_globaldestinationpath, new StringBuilder(n).Append(".sevo").ToString()),
                            min = new Vector3i(0, 0, 0),
                            capturescale = 3
                        };


                        try
                        {
                              UpdateProgress(0, $"Initializing");
                                Voxel_Import Vi = new Voxel_Import(ls.blocks, ls.min, 3, false, ls.capturescale, EnableSym);
                                Voxel_Import Vi2 = new Voxel_Import(ls.blocksDiff, ls.min, 3, false, ls.capturescale, EnableSym);
                                Vi.PropertyChanged += (s, ev) =>
                                {
                                    (sender as BackgroundWorker).ReportProgress(int.Parse(ev.PropertyName.ToString()));
                                };
                                Vi2.PropertyChanged += (s, ev) =>
                                {
                                    (sender as BackgroundWorker).ReportProgress(int.Parse(ev.PropertyName.ToString()));
                                };
                                axis = 0;
                                UpdateProgress(0, $"Optimizing");
                                Vi.OptimizeSym(EnableSym == false ? 3 : axis);
                                UpdateProgress(0, $"Optimizing");
                                Vi2.Optimize();
                                //UpdateProgress(interval, "");
                                WriteSevo(Path.Combine(_globaldestinationpath, ls.newfilename), Vi, Vi2);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Thread Abort Exception {ex}");
                            MessageBox.Show($"Unable to Import: {ex.Message}", "Import error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                        finally
                        {
                            Console.WriteLine("Thread Finished");
                            e.Result = 1;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show($"Unable to Parse: {error.ToString()}", "Parse error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                e.Result = new MainWindow.LoadInstance();
            }

        }

        private bool _enableSym = false;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _enableSym = !_enableSym;
            SymCheckbox.IsChecked = _enableSym;
            if (_enableSym == true)
            {
                Snackbar.MessageQueue?.Enqueue("Warning: This will break asymmetrical objects when center-line differs from center");
            }
        }

        public string CreateUniqueTempDirectory()
        {
            var uniqueTempDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            Directory.CreateDirectory(uniqueTempDir);
            return uniqueTempDir;
        }


        private string _temppath;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_temppath.Length == 0)
                {
                    DirectoryInfo di = new DirectoryInfo(CreateUniqueTempDirectory());
                    _temppath = di.FullName;
                }

                if (lbl_objfile.Content == null)
                {
                    Snackbar.MessageQueue?.Enqueue("No File Loaded");
                    return;
                }

                progressBar.Value = 0;
                //set up a read and convert into vox file.
                //_loadedInstances = new List<LoadInstance>();
                //  LoadInstance[] listInstances = await Task.WhenAll(t);

                var path = lbl_objfile.Content.ToString();
                var outputname = Path.GetFileNameWithoutExtension(Path.GetFileName(path));


                if (path.Length == 0)
                {
                    //We have no file, shouldn't happen
                    return;
                }
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                if (Path.GetExtension(path) == ".stl" || Path.GetExtension(path) == ".STL")
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
                    Mouse.OverrideCursor = null;
                    UpdateProgress(0, "");
                    return;
                }

                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += readvl32;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                worker.RunWorkerAsync(new Tuple<string, string, int,bool>(path, outputname, _axis,_enableSym));
            }
            catch (Exception er)
            {
                MessageBox.Show($"Unable to Read: {er.Message}", "Read error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }



        private static void WriteSevo(string name, Voxel_Import v, Voxel_Import v2)
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

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((int)e.Result == 1)
            {
                Snackbar.MessageQueue?.Enqueue("Import Succeeded");
                progressBarText.Text = "Done";
            }
            else
            {
                Snackbar.MessageQueue?.Enqueue("Import Failed");
            }
              
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
            //var textBox = sender as VisualStyleElement.TextBox;
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
            if (voxelres > 2048 || voxelres < 248)
                bounded = true;
            slider.Value = voxelres;
        }

        private int _axis = 0;
        private void comboBox_Axis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cm = (ComboBox)sender;
            _axis = cm.Items.IndexOf(cm.SelectedItem);
        }

    }
}
