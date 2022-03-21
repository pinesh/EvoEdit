﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;

namespace EvoEditApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PaintWindow : Window
    {
        private ObservableCollection<ColorSwap> MyListBoxData = new ObservableCollection<ColorSwap>();
        //Default StevoPaints
        private static ObservableCollection<ColorItem> Skywanderers = new ObservableCollection<ColorItem>()
            {
                new ColorItem(Color.FromArgb(255, 204, 71, 71), ""),
                new ColorItem(
                    Color.FromArgb(255, 204,
                        113,
                        71
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 204,
                        187,
                        71
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 154,
                        204,
                        71
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 107,
                        184,
                        88
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 71,
                        204,
                        164
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 71,
                        172,
                        204
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 71,
                        124,
                        204
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 170,
                        71,
                        204
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 204,
                        71,
                        171
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 196,
                        10,
                        10
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 196,
                        68,
                        10
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 196,
                        172,
                        10
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 125,
                        196,
                        10
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 69,
                        177,
                        42
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 10,
                        196,
                        140
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 10,
                        152,
                        196
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 10,
                        84,
                        196
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 148,
                        10,
                        196
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 196,
                        10,
                        150
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 153,
                        20,
                        20
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 153,
                        61,
                        20
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 153,
                        136,
                        20
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 102,
                        153,
                        20
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 61,
                        138,
                        42
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 20,
                        153,
                        113
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 20,
                        121,
                        153
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 20,
                        73,
                        153
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 118,
                        20,
                        153
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 153,
                        20,
                        120
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 220,
                        220,
                        220
                    ), ""
                ),

                new ColorItem(
                    Color.FromArgb(255, 160,
                        160,
                        160
                    ), ""
                ),

                new ColorItem(
                    Color.FromArgb(255, 100,
                        103,
                        105
                    ), ""
                ),

                new ColorItem(
                    Color.FromArgb(255, 80,
                        82,
                        84
                    ), ""
                ),

                new ColorItem(
                    Color.FromArgb(255, 10,
                        10,
                        12
                    ), ""
                ),
                new ColorItem(
                    Color.FromArgb(255, 156,
                        105,
                        82
                    ), ""
                ),

                new ColorItem(
                    Color.FromArgb(255, 142,
                        75,
                        49
                    ), ""
                ),

                new ColorItem(
                    Color.FromArgb(255, 112,
                        62,
                        34
                    ), ""
                ),

                new ColorItem(
                    Color.FromArgb(255, 237,
                        232,
                        203
                    ), ""
                ),

                new ColorItem(
                    Color.FromArgb(255, 225,
                        206,
                        150
                    ), ""
                ),

                new ColorItem(
                    Color.FromArgb(255, 188,
                        186,
                        163
                    ), "")
            };



        public PaintWindow()
        {
            InitializeComponent();
         
            ColorBox.ItemsSource = MyListBoxData;
        }

        public class ColorSwap
        {
            //Set observable collection of colorItems
            public  ObservableCollection<ColorItem> AllColors
            {
                get;
                set;
            }

            public ColorSwap(Color old,ObservableCollection<ColorItem> norm)
            {
                ColorOld = old;
                ColorNew = old;
                AllColors = Skywanderers;
                NonstandardColors = norm;
            }

            public bool diff()
            {
                return ColorOld != ColorNew;
            }

            public Color ColorOld
            {
                get;
                set;
            }

            public Color ColorNew
            {
                get;
                set;
            }

            
            public ObservableCollection<ColorItem> NonstandardColors
            {
                get;
                set;
            }

        }


        private string currentfile;

        public BrickEntityMp old;
        //Open a Stevo Blueprint
        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            MyListBoxData.Clear();
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.DefaultExt = ".sevo";
            openFileDlg.Filter = "Blueprint (*.sevo)|*.sevo";
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                currentfile = openFileDlg.FileName;
                using (FileStream fs = File.OpenRead(openFileDlg.FileName))
                {
                    var x = BrickEntityMp.GetSaveFromFile(fs, true);
                    if (old == null)
                    {
                        old = x;
                    }
                    else
                    {
                        finddiff(old,x);
                    }

                    var s = new StringBuilder();
                    // color = (object)new object[] { paint.X, paint.Y, paint.Z, 255 }
                    Dictionary<sevocol, int> ColorCount = new Dictionary<sevocol, int>();
                    foreach (var d in x.BrickDatas.Datas)
                    {
                        //Console.WriteLine(d.brickId);
                        var c = new sevocol(d.color);
                        if (ColorCount.ContainsKey(c))
                        {
                            ColorCount[c] += 1;
                        }
                        else
                        {
                            ColorCount.Add(c,1);
                        }
                    }

                    ObservableCollection<ColorItem> shipColors = new ObservableCollection<ColorItem>();
                    foreach (var pair in ColorCount)
                    {
                        shipColors.Add(new ColorItem(pair.Key.C(),""));
                    }

                    var val = from ele in ColorCount
                        orderby ele.Value descending 
                        select ele;
                    foreach (var ele in val)
                    {
                        MyListBoxData.Add(new ColorSwap(ele.Key.C(), shipColors));
                    }
                }
            }
        }

        private struct sevocol
        {
            private byte r;
            private byte g;
            private byte b;

            public sevocol(Color color)
            {
               
                r = (byte)color.R;
                g = (byte)color.G;
                b = (byte)color.B;
            }
            public sevocol(object color)
            {
                var c = (object[])color;
                r = (byte)c[0];
                g = (byte)c[1];
                b = (byte)c[2];
                var a = (byte)c[3];
                //Console.WriteLine(a);
            }
          

            public object sevoC()
            {
                return new object[] { r, g, b, 255 };
            }

            public Color C()
            {
                return Color.FromRgb(r, g, b);
            }

        }
        internal static ushort range_to_scale(int x, int y, int z)
        {
            var u = (ushort)z;
            u = (ushort)(u << (ushort)4 | (ushort)y);
            u = (ushort)(u << (ushort)4 | (ushort)x);
            return u;
        }

        private void finddiff(BrickEntityMp x, BrickEntityMp y)
        {
            
            int child = 0;
            int brick = 0;
            List<List<int>> instance = new List<List<int>>();
            foreach (BrickDatasSave b in x.BrickDatasChildrens)
            {
                var yb = y.BrickDatasChildrens[child];
                int id = 0;
                while (id < b.Length)
                {
                    if (yb.Datas[id] != b.Datas[id])
                    {
                        Console.WriteLine($"{child},{id}");
                    }

                    id++;
                }
                child++;
            }
        }

        private void button_Click_export(object sender, RoutedEventArgs e)
        {

            if (!new FileInfo(currentfile).Exists)
            {
                throw new Exception("missing base blueprint template");
            }

            using (FileStream fs = File.OpenRead(currentfile))
            {
                var x = BrickEntityMp.GetSaveFromFile(fs, true);
                var s = new StringBuilder();

                foreach (var d in x.BrickDatas.Datas)
                {
                    s.AppendLine("[" + d.gridPosition.X + "," + d.gridPosition.Y + "," + d.gridPosition.Z + "]");
                }

                Dictionary<sevocol, sevocol> remap = MyListBoxData.Where(cswap => cswap.diff()).ToDictionary(cswap => new sevocol(cswap.ColorOld), cswap => new sevocol(cswap.ColorNew));
                int sc = 1;

                

                List<int> touch = new List<int>() { 84,78, 85, 3, 255, 181 };
              
                for (int i = 0; i < x.BrickDatasChildrens.Count; i++)
                {
                    if (x.BrickDatasChildrens[i].Datas != null)
                    {
                        for (int j = 0; j < x.BrickDatasChildrens[i].Datas.Length; j++)
                        {
                            if (x.BrickDatasChildrens[i].Datas[j].brickId != 0)
                            {
                                var c = new sevocol(x.BrickDatasChildrens[i].Datas[j].color);
                                if (remap.ContainsKey(c))
                                {
                                    x.BrickDatasChildrens[i].Datas[j].color = remap[c].sevoC();
                                }
                            }
                        }
                    }
                }
                
                //x.BrickDatasChildrens[12].Datas[63].scale = range_to_scale(2, 0, 5);
                //x.BrickDatasChildrens[1].Datas[1].gridPosition = new Vector3i(0, 2, 0);
                //x.BrickDatasChildrens[2].Datas[1].gridPosition = new Vector3i(0, 2, 0);
             
                int count = 0;
                int check = 0;
                for (int i = 0; i < x.BrickDatas.Datas.Length; i++)
                {
                    if (x.BrickDatas.Datas[i].brickId != 0)
                    {
                        //if (touch.Contains(x.BrickDatas.Datas[i].brickId)) {
                        //x.BrickDatas.Datas[i].gridPosition.X += 32;
                        var c = new sevocol(x.BrickDatas.Datas[i].color);
                        if (remap.ContainsKey(c))
                        {
                            x.BrickDatas.Datas[i].color = remap[c].sevoC();
                        }
                        
                    }
                }
                           // if (x.BrickDatas.Datas[i].brickId == 3)
                         //   {
                           //     x.BrickDatas.Datas[i].brickId = 78;
                         //   }
                        //if (x.BrickDatas.Datas[i].brickId == 84)
                        // {
                        // x.BrickDatas.Datas[i].gridPosition.X += 12;
                        // x.BrickDatas.Datas[i].scale = range_to_scale(0, 7, 0);
                        //x.BrickDatas.Datas[i].gridPosition.X -= 18;
                        // }
                        //if (x.BrickDatas.Datas[i].brickId == 80)
                        //{
                        // x.BrickDatas.Datas[i].scale = range_to_scale(0, 7, 0);
                        //  //x.BrickDatas.Datas[i].gridPosition.X -= 18;
                        //  }

                        //x.BrickDatas.Datas[i].gridPosition = new Vector3i(x.BrickDatas.Datas[i].gridPosition.X,
                        // (x.BrickDatas.Datas[i].gridPosition.Y / (4 * (int)Math.Pow(2, x.BrickDatas.Datas[i].gridSize))) * 4 * (int)Math.Pow(2, sc), x.BrickDatas.Datas[i].gridPosition.Z);
                        //x.BrickDatas.Datas[i].gridPosition += new Vector3i(4, 0, 4);
                        /*
                          if (x.BrickDatas.Datas[i].brickId == 3)
                          {
                            if(check == 1)
                            {
                                x.BrickDatas.Datas[i] = x.BrickDatas.Datas[0];
                            }
                            else
                            {
                                
                                x.BrickDatas.Datas[i].scale = range_to_scale(0, 7, 0);
                            }
                            check = 1;
                            //x.BrickDatas.Datas[i].scale = 240;
                            //x.BrickDatas.Datas[i].gridPosition -= 3*x.BrickDatas.Datas[i].gridPosition;

                          }
                        */
                        //x.BrickDatas.Datas[i].brickId = 85;
                        //x.BrickDatas.Datas[i].gridSize = (byte)sc;
                        //x.BrickDatas.Datas[i].scale = range_to_scale(1,0, 1);
                      
                        //count++;
                        

                    /* { 0, new Vector3i(2, 0, 2) },
                    { 1, new Vector3i(4, 0, 4) },
                    { 2, new Vector3i(8, 0, 8) },
                    { 3, new Vector3i(0, 16, 0) },//1m
                    { 4, new Vector3i(16, -16, 16) },//2m //8 = 0.375m
                    { 5, new Vector3i(48, -16, 48) },//4m
                    { 6, new Vector3i(112,-16, 112) },//8m
                    { 7, new Vector3i(240, -16, 240) },//16m
                    { 8, new Vector3i(496, -16, 496) } //32m*/


                

                ParentEntity parent = new ParentEntity(x);
                
                BrickDatasSave b = new BrickDatasSave()
                {
                    AdditionalDatas = x.BrickDatas.AdditionalDatas,
                    IdsToRecycle = x.BrickDatas.IdsToRecycle,
                    Datas = x.BrickDatas.Datas
                };
                parent.Append(b);

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Blueprint (*.sevo)|*.sevo";
                if (saveFileDialog.ShowDialog() == true)
                    parent.SaveToDiskAtPathChild(saveFileDialog.FileName, false,x.BrickDatasChildrens);
                   // parent.SaveToDiskAtPath(saveFileDialog.FileName, false);
            }
        }
        }}


