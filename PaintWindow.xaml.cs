using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Security.RightsManagement;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace EvoEditApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PaintWindow : Window
    {
        private ObservableCollection<ColorSwap> MyListBoxData = new ObservableCollection<ColorSwap>();

        //Default StevoPaints
        private  ObservableCollection<ColorItem> Skywanderers = new ObservableCollection<ColorItem>()
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
            public ObservableCollection<ColorItem> AllColors
            {
                get;
                set;
            }

            public bool? RemoveBool
            {
                get;
                set;
            }

            public string MaterialName
            {
                get;
                set;
            }

            public int MaterialIndex
            {
                get;
                set;
            }

        public int MaterialIndexNew
            {
                get;
                set;
            }

        private static List<string> materialnames = new List<string>() { "Steel", "Gold", "Brass", "Copper", "Silver", "Dark", "Matte" };

        public ColorSwap(Color old, ObservableCollection<ColorItem> norm, ObservableCollection<ColorItem> Skywanderers, int index)
        {
            ColorOld = old;
            ColorNew = old;
            AllColors = Skywanderers;
            NonstandardColors = new ObservableCollection<ColorItem>(norm.ToList().Distinct());
            MaterialIndex = index;
            MaterialName = materialnames[index];
            MaterialIndexNew = index;
            RemoveBool = true;

        }

        public bool diff()
        {
            return ColorOld != ColorNew || MaterialIndex != MaterialIndexNew || RemoveBool == false;
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
                     var s = new StringBuilder();
                    // color = (object)new object[] { paint.X, paint.Y, paint.Z, 255 }
                    Dictionary<Tuple<sevocol,int>, int> ColorCount = new Dictionary<Tuple<sevocol, int>, int>();
                    foreach (var d in x.BrickDatas.Datas)
                    {

                        if(d.brickId == 0) continue;
                        //Console.WriteLine(d.brickId);
                        var c = new Tuple<sevocol, int>(new sevocol(d.color), (int)d.material);
                        //var c = new sevocol(d.color);
                        if (ColorCount.ContainsKey(c))
                        {
                            ColorCount[c] += 1;
                        }
                        else
                        {
                            ColorCount.Add(c, 1);
                        }
                    }

                    
                    foreach (var c in from child in x.BrickDatasChildrens where child.Datas?.Length > 0 from d in child.Datas select new Tuple<sevocol,int,int>(new sevocol(d.color), (int)(d.material),d.brickId))
                    {
                        if(c.Item3 == 0) continue;
                        var tempc = new Tuple<sevocol, int>(c.Item1, c.Item2);
                        if (ColorCount.ContainsKey(tempc))
                        {
                            ColorCount[tempc] += 1;
                        }
                        else
                        {
                            ColorCount.Add(tempc, 1);
                        }
                    }

                    ObservableCollection<ColorItem> shipColors = new ObservableCollection<ColorItem>();
                    foreach (var pair in ColorCount)
                    {
                        shipColors.Add(new ColorItem(pair.Key.Item1.C(), ""));
                    }

                    var val = from ele in ColorCount
                              orderby ele.Value descending
                              select ele;
                    foreach (var ele in val)
                    {
                        MyListBoxData.Add(new ColorSwap(ele.Key.Item1.C(), shipColors,Skywanderers,ele.Key.Item2));
                    }
                }
            }
        }

        public struct sevocol : IEquatable<sevocol>
        {
           
            private byte r;
            private byte g;
            private byte b;

            // Token: 0x06000F8A RID: 3978 RVA: 0x00015938 File Offset: 0x00013B38
         
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
                return r+g+b == 0 ? Color.FromArgb(0,r,g,b) : Color.FromRgb(r, g, b);
            }

            public bool Equals(sevocol other)
            {
                return r == other.r && g == other.g && b == other.b;
            }

            public override bool Equals(object obj)
            {
                return obj is sevocol other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = r.GetHashCode();
                    hashCode = (hashCode * 397) ^ g.GetHashCode();
                    hashCode = (hashCode * 397) ^ b.GetHashCode();
                    return hashCode;
                }
            }
        }

        internal static ushort range_to_scale(int x, int y, int z)
        {
            var u = (ushort)z;
            u = (ushort)(u << (ushort)4 | (ushort)y);
            u = (ushort)(u << (ushort)4 | (ushort)x);
            return u;
        }

        private List<int> ingore = new List<int>() { 92, 96, 25, 109,97,93,26,110 };
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

                Dictionary<Tuple<sevocol,int>, Tuple<sevocol, int,bool>> remap = MyListBoxData.Where(cswap => cswap.diff()).ToDictionary(cswap =>new Tuple<sevocol, int>(new sevocol(cswap.ColorOld),cswap.MaterialIndex)  , cswap => new Tuple<sevocol, int,bool>(new sevocol(cswap.ColorNew), cswap.MaterialIndexNew,cswap.RemoveBool.Value));
                int sc = 1;

                for (int i = 0; i < x.BrickDatasChildrens.Count; i++)
                {
                    if (x.BrickDatasChildrens[i].Datas != null)
                    {
                        for (int j = 0; j < x.BrickDatasChildrens[i].Datas.Length; j++)
                        {
                            if (x.BrickDatasChildrens[i].Datas[j].brickId != 0)
                            {
                                var c = new Tuple<sevocol, int>(new sevocol(x.BrickDatasChildrens[i].Datas[j].color), (int)(x.BrickDatasChildrens[i].Datas[j].material)) ;
                                if (!remap.ContainsKey(c)) continue;
                                if (remap[c].Item3 || x.BrickDatasChildrens[i].Datas[j].brickId == 97 ||
                                    x.BrickDatasChildrens[i].Datas[j].brickId == 93 ||
                                    x.BrickDatasChildrens[i].Datas[j].brickId == 26 ||
                                    x.BrickDatasChildrens[i].Datas[j].brickId == 110)
                                {
                                    x.BrickDatasChildrens[i].Datas[j].color = remap[c].Item1.sevoC();
                                    x.BrickDatasChildrens[i].Datas[j].material = (uint)remap[c].Item2;
                                }
                                else
                                {
                                    if (j > 3)
                                        if (!ingore.Contains(x.BrickDatasChildrens[i].Datas[j].brickId)) 
                                            x.BrickDatasChildrens[i].Datas[j].brickId = 0;
                                }
                            }
                        }
                    }
                }
                
                int count = 0;
                int check = 0;
                for (int i = 0; i < x.BrickDatas.Datas.Length; i++)
                {
                    if (x.BrickDatas.Datas[i].brickId != 0)
                    {
                        var c = new Tuple<sevocol, int>(new sevocol(x.BrickDatas.Datas[i].color), (int)(x.BrickDatas.Datas[i].material));
                        if (!remap.ContainsKey(c)) continue;
                        if (remap[c].Item3 || x.BrickDatas.Datas[i].brickId == 97 ||
                                    x.BrickDatas.Datas[i].brickId == 93 ||
                                    x.BrickDatas.Datas[i].brickId == 26 ||
                                    x.BrickDatas.Datas[i].brickId == 110)
                        {
                            x.BrickDatas.Datas[i].color = remap[c].Item1.sevoC();
                            x.BrickDatas.Datas[i].material = (uint)remap[c].Item2;
                        }
                        else
                        {
                            if (!ingore.Contains(x.BrickDatas.Datas[i].brickId))
                                x.BrickDatas.Datas[i].brickId = 0;
                        }
                    }
                }

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
                    parent.SaveToDiskAtPathChild(saveFileDialog.FileName, false, x.BrickDatasChildrens);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog o = new OpenFileDialog()
                {
                    Filter = "xml files (*.xml)|*.xml",
                };
                bool? result = o.ShowDialog();
                if (result == true)
                {
                    //Get the path of specified file
                    var filePath = o.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = o.OpenFile();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(o.FileName);
                    XmlNodeList elemList = doc.GetElementsByTagName("Color32");

                    Skywanderers.Clear();

                    foreach (XmlNode el in elemList)
                    {
                        if (!el.HasChildNodes) continue;
                        int.TryParse(el.ChildNodes.Item(0).InnerText, out int x);
                        byte r = Convert.ToByte(x);
                        int.TryParse(el.ChildNodes.Item(1).InnerText, out x);
                        byte g = Convert.ToByte(x);
                        int.TryParse(el.ChildNodes.Item(2).InnerText, out x);
                        byte b = Convert.ToByte(x);
                        Skywanderers.Add(new ColorItem(Color.FromArgb(255,r,g,b), ""));
                    }
                }
            }
            catch (Exception er)
            {
                MessageBox.Show($"Unable to Import: {er.Message}", "Paint Import error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}