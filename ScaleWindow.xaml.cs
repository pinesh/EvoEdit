using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace EvoEditApp
{
    /// <summary>
    ///     Interaction logic for ScaleWindow.xaml
    /// </summary>
    public partial class ScaleWindow : Window
    {
        private List<int> prune = new List<int> {153,154,114,241,7,39,45,40,149,73,46,47,48,52,34,65,86,124,122,170,138,139,142,132,140,33,133,150,130,69,151,62,63,126,128,129,131,158,152,135,137,51,55,42,146,192,145,193,71,72,50,53,54,231,240,237,235,232,239,233,233,238,234,236,16,4,5,11,57, 104, 102, 103, 107, 106 };
        private List<int> blockAtScale = new List<int>();
        public ScaleWindow()
        {
            InitializeComponent();
            initList();
        }

        internal void initList()
        {
            blockAtScale = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                blockAtScale.Add(0);
            }
        }
        private int totalblocks;
        private string currentfile;
        public BrickEntityMp old;

        //Open a Stevo Blueprint
        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            var openFileDlg = new OpenFileDialog();
            openFileDlg.DefaultExt = ".sevo";
            openFileDlg.Filter = "Blueprint (*.sevo)|*.sevo";
            var result = openFileDlg.ShowDialog();
            byte max = 0;
            if (result == true)
            {
                totalblocks = 0;
                initList();
                currentfile = openFileDlg.FileName;
                slider.IsEnabled = true;
                using (var fs = File.OpenRead(openFileDlg.FileName))
                {
                    var x = BrickEntityMp.GetSaveFromFile(fs, true);
                    //take a count of blocks and prepare to prune.  

                    for (var i = 0; i < x.BrickDatasChildrens.Count; i++)
                        if (x.BrickDatasChildrens[i].Datas != null)
                            for (var j = 0; j < x.BrickDatasChildrens[i].Datas.Length; j++)
                                if (!prune.Contains(x.BrickDatasChildrens[i].Datas[j].brickId))
                                {
                                    if(x.BrickDatasChildrens[i].Datas[j].gridSize > max)
                                        max = x.BrickDatasChildrens[i].Datas[j].gridSize;
                                    blockAtScale[x.BrickDatasChildrens[i].Datas[j].gridSize] += 1;
                                }

                    var sb = new StringBuilder();
                    for (var i = 0; i < x.BrickDatas.Datas.Length; i++)
                        if (x.BrickDatas.Datas[i].brickId != 0)
                        {
                            if (!prune.Contains(x.BrickDatas.Datas[i].brickId))
                            {
                               // sb.Append("," + x.BrickDatas.Datas[i].brickId);
                                blockAtScale[x.BrickDatas.Datas[i].gridSize] += 1;
                                if (x.BrickDatas.Datas[i].gridSize > max)
                                {
                                    max = x.BrickDatas.Datas[i].gridSize;
                                }
                            }
                        }

                    //MessageBox.Show(sb.ToString());
                    Console.WriteLine(max);
                    if (max > 8)
                    {
                        MessageBox.Show("This entity contains bricks considered over the maximum scale. Please remove them before attempting to rescale. ",caption:"Error", MessageBoxButton.OK, icon:MessageBoxImage.Error);
                        slider.IsEnabled = false;
                    }
                    else
                    {
                        totalblocks = countAllBlocks(0);
                        LblOgCount.Content = $"{totalblocks}";
                        LblEntityName.Content = Path.GetFileNameWithoutExtension(openFileDlg.FileName);
                        slider.Value = 3;
                        LblRescaleCount.Content = $"{totalblocks}";
                    }

                    
                }
            }
        }

      
        private void button_Click_export(object sender, RoutedEventArgs e)
        {
            if (!new FileInfo(currentfile).Exists) throw new Exception("missing base blueprint template");

            using (var fs = File.OpenRead(currentfile))
            {
                var x = BrickEntityMp.GetSaveFromFile(fs, true);
                
                var touch = new List<int> { 84, 78, 85, 3, 255, 181 };
                List<int> childpurge = new List<int>();
                List<Tuple<int, Vector3i>> shuntmap = new List<Tuple<int, Vector3i>>();
                var emptyBrick = x.BrickDatas.Datas[0];
                int scaleShunt = (int)slider.Value -3;
                //First Step, Prune.

                for (var i = 0; i < x.BrickDatasChildrens.Count; i++)
                    if (x.BrickDatasChildrens[i].Datas != null)
                    {
                        List<int> recycleList = new List<int>();
                        for (var j = 0; j < x.BrickDatasChildrens[i].Datas.Length; j++)
                        {

                            if (x.BrickDatasChildrens[i].Datas[j].brickId != 0)
                            {
                                if (prune.Contains(x.BrickDatasChildrens[i].Datas[j].brickId) || (int)x.BrickDatasChildrens[i].Datas[j].gridSize + scaleShunt < 0)
                                {
                                    var id = x.BrickDatasChildrens[i].Datas[j].brickId;
                                    if (id == 97 || id == 93 || id == 26 || id == 110)
                                    {
                                        childpurge.Add(i);
                                        break;
                                    }
                                    else
                                    {

                                        x.BrickDatasChildrens[i].Datas[j] = emptyBrick;
                                        recycleList.Add(j);
                                    }
                                }
                                else
                                {
                                    double constan = Math.Pow(2, scaleShunt);
                                    x.BrickDatasChildrens[i].Datas[j].gridPosition = new Vector3i((int)(x.BrickDatasChildrens[i].Datas[j].gridPosition.X * constan), (int)(x.BrickDatasChildrens[i].Datas[j].gridPosition.Y * constan), (int)(x.BrickDatasChildrens[i].Datas[j].gridPosition.Z * constan));
                                    x.BrickDatasChildrens[i].Datas[j].gridSize = (byte)(x.BrickDatasChildrens[i].Datas[j].gridSize + scaleShunt);
                                    var id = x.BrickDatasChildrens[i].Datas[j].brickId;
                                    if (id == 97 || id == 93)
                                    {
                                        shuntmap.Add(new Tuple<int, Vector3i>(i, x.BrickDatasChildrens[i].Datas[j].gridPosition));
                                    }

                                    if (id == 110)
                                    {
                                        var tempvec = new Vector3i(x.BrickDatasChildrens[i].Datas[j].gridPosition.X,
                                            x.BrickDatasChildrens[i].Datas[j].gridPosition.Y - 128,
                                            x.BrickDatasChildrens[i].Datas[j].gridPosition.Z);
                                        shuntmap.Add(new Tuple<int, Vector3i>(i, tempvec));
                                    }

                                    if (id == 26)
                                    {
                                        var tempvec = new Vector3i(x.BrickDatasChildrens[i].Datas[j].gridPosition.X,
                                            x.BrickDatasChildrens[i].Datas[j].gridPosition.Y - 108,
                                            x.BrickDatasChildrens[i].Datas[j].gridPosition.Z);
                                        shuntmap.Add(new Tuple<int, Vector3i>(i,tempvec));
                                    }
                                    //if for the mechanism base component fix.

                                }
                            }
                        }

                        var temp = x.BrickDatasChildrens[i].IdsToRecycle.ToList();
                        temp.AddRange(recycleList);
                        var stepchild = x.BrickDatasChildrens[i];
                        stepchild.IdsToRecycle = temp.ToArray();
                        x.BrickDatasChildrens[i] = stepchild;
                    }
                    /*
                foreach (var tup in shuntmap)
                {
                    for (var j = 0; j < x.BrickDatasChildrens[tup.Item1].Datas.Length; j++)
                    {
                        x.BrickDatasChildrens[tup.Item1].Datas[j].gridPosition -= tup.Item2;
                    }

                }
*/
                    foreach (var index in childpurge)
                {
                    x.BrickDatasChildrens.Remove(x.BrickDatasChildrens[index]);
                }
                List<int> mainrecycleList = new List<int>();
                for (var i = 0; i < x.BrickDatas.Datas.Length; i++)
                    if (x.BrickDatas.Datas[i].brickId != 0)
                    {
                        if (prune.Contains(x.BrickDatas.Datas[i].brickId) ||
                            (int)x.BrickDatas.Datas[i].gridSize + scaleShunt < 0)
                        {
                            x.BrickDatas.Datas[i] = emptyBrick; 
                            mainrecycleList.Add(i);
                        }
                        else
                        {
                            var sc = x.BrickDatas.Datas[i].gridSize;
                            double constan = Math.Pow(2, scaleShunt);
                            x.BrickDatas.Datas[i].gridPosition = new Vector3i((int)(x.BrickDatas.Datas[i].gridPosition.X* constan), (int)(x.BrickDatas.Datas[i].gridPosition.Y * constan), (int)(x.BrickDatas.Datas[i].gridPosition.Z * constan));
                            x.BrickDatas.Datas[i].gridPosition += specialOffsetRecur(0,scaleShunt,new Vector3i(0,0,0));
                            x.BrickDatas.Datas[i].gridSize = (byte)(sc + scaleShunt);
                        }
                    }

                var maintemp = x.BrickDatas.IdsToRecycle.ToList();
                maintemp.AddRange(mainrecycleList);
                var parent = new ParentEntity(x);


                var t = x.BrickDatas.Datas;
                var b = new BrickDatasSave
                {
                    AdditionalDatas = x.BrickDatas.AdditionalDatas,
                    IdsToRecycle = maintemp.ToArray(),
                    Datas = x.BrickDatas.Datas
                };
                parent.Append(b);

                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Blueprint (*.sevo)|*.sevo";
                if (saveFileDialog.ShowDialog() == true)
                    parent.SaveToDiskAtPathChild(saveFileDialog.FileName, false, x.BrickDatasChildrens);
                MessageBox.Show("Done!");
            }
        }

        internal Vector3i specialOffsetRecur(int i,int t, Vector3i vec)
        {
            switch (t)
            {
                case -3:
                    return new Vector3i(2, 2, 2);
                case -2:
                    return new Vector3i(4, 4, 4);
                case -1:
                    return new Vector3i(24, 24, 24);
            }

            if (i == t)
                return vec;
            //doubling size was 16,16,16
            //  return new Vector3i(16, 16, 16);//for 2x
            //  return new Vector3i(48, 48, 48);//for 4x
            //  return new Vector3i(0+ 16, 16, 16);//for 8x4(1) 2pow = 2 = 8
            //  return new Vector3i(16+32, 16, 16);//for 8x5(2)//4x 2pow 4 = 8
            //  return new Vector3i(48+64, 16, 16);//for 8x6(3) 2po = 8   / = 8
         
            return specialOffsetRecur(i+1,t,new Vector3i(vec.X + 8*(int)Math.Pow(2,i+1), vec.X + 8 * (int)Math.Pow(2, i+1), vec.X + 8 * (int)Math.Pow(2, i+1)));
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ScalarLabel != null)
            {
                ScalarLabel.Content = slider.Value < 3 ? $"1/{Math.Pow(2, Math.Abs(slider.Value - 3))}" : $"{Math.Pow(2, slider.Value - 3)}";
                LblRescaleCount.Content = $"{CountTotalBlocks((int)slider.Value)}";
            }

            if (slider != null && BtnExport != null)
            {
                BtnExport.IsEnabled = slider.Value != 3;
            }
            
        }

        internal int countAllBlocks(int x)
        {
            int total = 0;
            for (int i = x; i < 9; i++)
            {
                total += blockAtScale[i];
            }

            return total;
        }

        internal int CountTotalBlocks(int ceiling)
        {
            if (ceiling >= 3)
            {
                return totalblocks;
            }

            return countAllBlocks(3 - ceiling);
        }
    }
}
