using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Xceed.Wpf.Toolkit;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace EvoEditApp
{
    /// <summary>
    /// Interaction logic for StevoPage.xaml
    /// </summary>
    public partial class StevoPage : Page
    {
        public StevoPage()
        {
            InitializeComponent();
            initList();
            ColorBox.ItemsSource = MyListBoxData;
        

        }

        private string savename = "";
        private bool compareEqual;
        private static string currentfile;
        private Vector3i _relocation = new Vector3i(0, 0, 0);
        private bool _relocateChecked = false;


        #region ScaleBoxControl
        private void txtX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtY_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtZ_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9-]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        private void txtY_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtY.Text == "")
            {
                txtY.Text = "0";
            }

            if (txtY.Text.Contains(" "))
            {
                txtY.Text = txtY.Text.Replace(" ", string.Empty);
            }
        }

        private void txtZ_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtZ.Text == "")
            {
                txtZ.Text = "0";
            }

            if (txtZ.Text.Contains(" "))
            {
                txtZ.Text = txtZ.Text.Replace(" ", string.Empty);
            }
        }
        private void txtX_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtX.Text == "")
            {
                txtX.Text = "0";
            }

            if (txtX.Text.Contains(" "))
            {
                txtX.Text = txtX.Text.Replace(" ", string.Empty);
            }
        }


        #endregion

        
        private void updateBlocks(SevoCol c, bool comp)
        {
            if (ColorCount != null)
            {
                try
                {
                    lblblocks.Content =
                        comp
                            ? $"Move ({ColorCount[new Tuple<SevoCol, int>(c, 0)]}) blocks where color is"
                            : $"Move ({ColorCount.Where(keypair => !keypair.Key.Equals(c)).Sum(keypair => keypair.Value)}) blocks where color is";
                }
                catch(Exception e)
                {
                    lblblocks.Content = $"Move (NA) blocks where color is";
                }
             
            }
           
        }

        internal void Flip()
        {
            txtX.IsEnabled = !txtX.IsEnabled;
            txtY.IsEnabled = !txtY.IsEnabled;
            txtZ.IsEnabled = !txtZ.IsEnabled;
            // checkBox_cursed.IsEnabled = !checkBox_cursed.IsEnabled;
            resetOriginCheckbox.IsEnabled = !resetOriginCheckbox.IsEnabled;
            ColorChoice.IsEnabled = !ColorChoice.IsEnabled;
            EqualityBox.IsEnabled = !EqualityBox.IsEnabled;
            checkBox.IsEnabled = !checkBox.IsEnabled;
            ExportButton.IsEnabled = !ExportButton.IsEnabled;
        }

     

        private void EqualityBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            compareEqual = (sender as ComboBox).SelectedIndex == 0;
           // if (ColorChoice.SelectedColor != null)
            //    updateBlocks(new SevoCol(ColorChoice.SelectedColor.Value), compareEqual);
        }

        private void ColorChoice_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            updateBlocks(new SevoCol(e.NewValue.Value), compareEqual);
        }

        

        private void resetOrigin_Checked(object sender, RoutedEventArgs e)
        {
            _relocateChecked = !_relocateChecked;

            if (_relocateChecked == false)
            {
                _relocation = new Vector3i(0, 0, 0);
                return;
            }

            using (FileStream fs = File.OpenRead(currentfile))
            {
                var x = BrickEntityMp.GetSaveFromFile(fs, true);

                for (var i = 0; i < x.BrickDatas.Datas.Length; i++)
                {
                    if (x.BrickDatas.Datas[i].brickId != 14) continue;
                    _relocation = x.BrickDatas.Datas[i].gridPosition;
                    _relocation.X += (int)Math.Pow(2, x.BrickDatas.Datas[i].gridSize + 1);
                    _relocation.Z += (int)Math.Pow(2, x.BrickDatas.Datas[i].gridSize + 1);
                    MessageBox.Show($"Relocating global origin to {_relocation.X},{_relocation.Y},{_relocation.Z}.",
                        "Relocation", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                MessageBox.Show("No Starter Block located. Check the base entity.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }



        private static ObservableCollection<ColorSwap> MyListBoxData =
            new ObservableCollection<ColorSwap>();

        #region SkywanderersPaints
        //Default StevoPaints
        private ObservableCollection<ColorItem> Skywanderers = new ObservableCollection<ColorItem>()
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
                ), ""),
            new ColorItem(
                Color.FromArgb(0, 0,
                    0,
                    0
                ), ""
            )

        };

        #endregion 

        private List<int> prune = new List<int>
        {
            153, 154, 114, 241, 7, 39, 45, 40, 149, 73, 46, 47, 48, 52, 34, 65, 86, 124, 122, 170, 138, 139, 142, 132,
            140, 33, 133, 150, 130, 69, 151, 62, 63, 126, 128, 129, 131, 158, 152, 135, 137, 51, 55, 42, 146, 192, 145,
            193, 71, 72, 50, 53, 54, 231, 240, 237, 235, 232, 239, 233, 233, 238, 234, 236, 16, 4, 5, 11, 57, 104, 102,
            103, 107, 106
        };

        private static List<int> glowBlocks = new List<int>
        {
            14, 10, 8, 17, 9, 12, 13, 6, 123, 4, 165, 229, 163, 229, 164, 229, 99, 156, 155, 159, 160, 161, 5, 114, 57,
            121, 157, 19, 15, 3, 78, 181, 18, 47, 48, 122, 86, 11, 153, 154, 170, 52, 34, 65, 46, 129, 126, 128, 135,
            152, 158, 137, 51, 55, 124
        };

        private List<int> blockAtScale = new List<int>();

        internal Dictionary<int, Dictionary<Tuple<SevoCol, int>, int>> blockDataDictionary =
            new Dictionary<int, Dictionary<Tuple<SevoCol, int>, int>>();


        internal void initList()
        {
            blockAtScale = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                blockAtScale.Add(0);
            }
        }

        private Dictionary<Tuple<SevoCol, int>, int> ColorCount;
        private int totalblocks;
        Dictionary<SevoCol, int> CC;
        //Open a Stevo Blueprint

        /// <summary>
        /// Contains the relevant content loading for all blueprint aspects.
        /// </summary>
        /// <param name="FileName"></param>
        private void RefreshButtonLoad(string FileName)
        {
            byte max = 0;
            lbl_current.Content = Path.GetFileName(FileName);
            totalblocks = 0;
          
            currentfile = FileName;
            slider.IsEnabled = true;
            using (FileStream fs = File.OpenRead(FileName))
            {
                var x = BrickEntityMp.GetSaveFromFile(fs, true);

                ColorCount = new Dictionary<Tuple<SevoCol, int>, int>();
                CC = new Dictionary<SevoCol, int>();
                foreach (var c in x.BrickDatas.Datas.Where(childData => childData.brickId != 0))
                {
                    var d = new Tuple<SevoCol, int>(new SevoCol(c.color), (int)c.material);
                    if (ColorCount.ContainsKey(d))
                    {
                        ColorCount[d] += 1;
                    }
                    else
                    {
                        ColorCount.Add(d, 1);
                    }

                    if (blockDataDictionary.ContainsKey(c.gridSize))
                    {
                        if (blockDataDictionary[c.gridSize].ContainsKey(d))
                        {
                            blockDataDictionary[c.gridSize][d] += 1;
                        }
                        else
                        {
                            blockDataDictionary[c.gridSize].Add(d,1);
                        }
                    }
                    else
                    {
                        blockDataDictionary.Add(c.gridSize,new Dictionary<Tuple<SevoCol, int>, int>{{d,1}});
                    }
                    if (c.gridSize > max)
                    {
                        max = c.gridSize;
                    }
                }


                foreach (var c in from child in x.BrickDatasChildrens
                                  where child.Datas?.Length > 0
                                  from d in child.Datas
                                  select new Tuple<SevoCol, int, int,byte>(new SevoCol(d.color),
                                      (int)(d.material), d.brickId,d.gridSize))
                {
                    if (c.Item3 == 0) continue;
                    var tempc = new Tuple<SevoCol, int>(c.Item1, c.Item2);
                 

                    if (ColorCount.ContainsKey(tempc))
                    {
                        ColorCount[tempc] += 1;
                    }
                    else
                    {
                        ColorCount.Add(tempc, 1);
                    }
                    if (blockDataDictionary.ContainsKey(c.Item4))
                    {
                        if (blockDataDictionary[c.Item4].ContainsKey(tempc))
                        {
                            blockDataDictionary[c.Item4][tempc] += 1;
                        }
                        else
                        {
                            blockDataDictionary[c.Item4].Add(tempc, 1);
                        }
                    }
                    else
                    {
                        blockDataDictionary.Add(c.Item4, new Dictionary<Tuple<SevoCol, int>, int> { { tempc, 1 } });
                    }
                    if (c.Item4 > max)
                    {
                        max = c.Item4;
                    }

                }
               
                ObservableCollection<ColorItem> shipColors = new ObservableCollection<ColorItem>();
                foreach (var pair in ColorCount)
                {
                    shipColors.Add(new ColorItem(pair.Key.Item1.C(), $"{pair.Key.Item1.C().ToString()}"));
                }

                shipColors = new ObservableCollection<ColorItem>(shipColors.Distinct());

                var val = from ele in ColorCount
                          orderby ele.Value descending
                          select ele;
                foreach (var ele in val)
                {
                    var CS = new ColorSwap(
                       ele.Key.Item1.C(),
                        shipColors, Skywanderers,
                        ele.Key.Item2);
                    CS.PropertyChanged += CSOnPropertyChanged;

                    MyListBoxData.Add(CS);
                }

              
                ColorChoice.SelectedColor = val.ElementAt(0).Key.Item1.C();
                ColorChoice.AvailableColors = shipColors;
                Flip();


            
                Console.WriteLine(max);
                if (max > 8)
                {
                    MessageBox.Show(
                        "This entity contains bricks considered over the maximum scale. Please remove them before attempting to rescale. ",
                        caption: "Error", MessageBoxButton.OK, icon: MessageBoxImage.Error);
                    slider.IsEnabled = false;
                }
                else
                {
                    totalblocks = countAllBlocks(0);
                    LblOgCount.Content = $"{totalblocks}";
                    slider.Value = 3;
                    LblOgCount.Content = $"{totalblocks}";
                }
            }
        }

        private void CSOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           // var CSWap = (ColorSwap)sender;

           int t = CountTotalBlocks((int)slider.Value);
           LblOgCount.Foreground = t < totalblocks ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.GreenYellow);
           LblOgCount.Content = $"{t}";
            // throw new NotImplementedException();
        }

        private void ImportBlueprintButton(object sender, RoutedEventArgs e)
        {
            MyListBoxData.Clear();
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.DefaultExt = ".sevo";
            openFileDlg.Filter = "Blueprint (*.sevo)|*.sevo";
            bool? result = openFileDlg.ShowDialog();
       
            if (result == true)
            {
               RefreshButtonLoad(openFileDlg.FileName);
            }
        }


        /// <summary>
        /// Paints import function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportButton_Click(object sender, RoutedEventArgs e)
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
                        Skywanderers.Add(new ColorItem(Color.FromArgb(255, r, g, b), $"{ Color.FromArgb(255, r, g, b).ToString() }"));
                    }
                }
            }
            catch (Exception er)
            {
                MessageBox.Show($"Unable to Import: {er.Message}", "Paint Import error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }



        internal Vector3i SpecialOffsetRecur(int i, int t, Vector3i vec)
        {
            while (true)
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

                if (i == t) return vec;

                var i1 = i;
                i = i + 1;
                vec = new Vector3i(vec.X + 8 * (int)Math.Pow(2, i1 + 1), vec.X + 8 * (int)Math.Pow(2, i1 + 1), vec.X + 8 * (int)Math.Pow(2, i1 + 1));
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ScalarLabel == null) return;
            ScalarLabel.Content = slider.Value < 3
                ? $"1/{Math.Pow(2, Math.Abs(slider.Value - 3))}"
                : $"{Math.Pow(2, slider.Value - 3)}";

            int t = CountTotalBlocks((int)slider.Value);
            LblOgCount.Foreground = t < totalblocks ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.GreenYellow);
            LblOgCount.Content = $"{t}";

        }

            /*
            if (slider != null && BtnExport != null)
            {
                BtnExport.IsEnabled = slider.Value != 3;
            }(*/

        

        internal int countAllBlocks(int x)
        {
            int total = 0;
            foreach (var keypair in blockDataDictionary.Where(keys => keys.Key - x >= 0))
            {
                foreach (var v in MyListBoxData.Where(cs => cs.RemoveBool == true))
                {
                    if(keypair.Value.ContainsKey(new Tuple<SevoCol, int>(new SevoCol(v.ColorOld).GetGlow(), v.MaterialIndex)))
                        total = total + keypair.Value[new Tuple<SevoCol, int>(new SevoCol(v.ColorOld).GetGlow(), v.MaterialIndex)];
                }

            }
            return total;
        }

        internal int CountTotalBlocks(int ceiling)
        {
            return countAllBlocks(3 - ceiling);
        }


        public class ColorSwap : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            //Set observable collection of colorItems
            public ObservableCollection<ColorItem> AllColors
            {
                get;
                set;
            }

            private bool? removeBool;

            public bool? RemoveBool
            {
                get { return removeBool;}
                set
                {
                    removeBool = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(removeBool)));
                }
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

        #region  SevoColors

        

    
        public struct SevoCol : IEquatable<SevoCol>
        {
            private byte r;
            private byte g;
            private byte b;

            internal struct colorfloat
            {
                public float r;
                public float g;
                public float b;

                public colorfloat(Color c)
                {
                    r = (float)c.R / 255;
                    g = (float)c.G / 255;
                    b = (float)c.B / 255;
                }

                public Color fromfloat()
                {
                    return Color.FromRgb((byte)Math.Round(r * 255), (byte)Math.Round(g * 255), (byte)Math.Round(b * 255));
                }
            }

            internal static void RGBToHSV(colorfloat rgbColor, out float H, out float S, out float V)
            {
                bool flag = rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r;
                if (flag)
                {
                    RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
                }
                else
                {
                    bool flag2 = rgbColor.g > rgbColor.r;
                    if (flag2)
                    {
                        RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
                    }
                    else
                    {
                        RGBToHSVHelper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
                    }
                }
            }

            // Token: 0x06000F8B RID: 3979 RVA: 0x000159E0 File Offset: 0x00013BE0
            internal static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
            {
                V = dominantcolor;
                bool flag = V != 0f;
                if (flag)
                {
                    bool flag2 = colorone > colortwo;
                    float num;
                    if (flag2)
                    {
                        num = colortwo;
                    }
                    else
                    {
                        num = colorone;
                    }
                    float num2 = V - num;
                    bool flag3 = num2 != 0f;
                    if (flag3)
                    {
                        S = num2 / V;
                        H = offset + (colorone - colortwo) / num2;
                    }
                    else
                    {
                        S = 0f;
                        H = offset + (colorone - colortwo);
                    }
                    H /= 6f;
                    bool flag4 = H < 0f;
                    if (flag4)
                    {
                        H += 1f;
                    }
                }
                else
                {
                    S = 0f;
                    H = 0f;
                }
            }

            internal SevoCol GetGlowColor()
            {
                float h;
                float num;
                float v;
                RGBToHSV(new colorfloat(Color.FromRgb(r,g,b)), out h, out num, out v);
                return new SevoCol(HSVToRGB(h, (num < 0.05f) ? num : 0.7f, v).fromfloat());
            }

            // Token: 0x06000F8C RID: 3980 RVA: 0x00015A94 File Offset: 0x00013C94
            internal static colorfloat HSVToRGB(float H, float S, float V)
            {
                return HSVToRGB(H, S, V, true);
            }

            // Token: 0x06000F8D RID: 3981 RVA: 0x00015AB0 File Offset: 0x00013CB0
            internal static colorfloat HSVToRGB(float H, float S, float V, bool hdr)
            {
                colorfloat white = new colorfloat(Color.FromRgb(255, 255, 255));
                bool flag = S == 0f;
                if (flag)
                {
                    white.r = V;
                    white.g = V;
                    white.b = V;
                }
                else
                {
                    bool flag2 = V == 0f;
                    if (flag2)
                    {
                        white.r = 0f;
                        white.g = 0f;
                        white.b = 0f;
                    }
                    else
                    {
                        white.r = 0f;
                        white.g = 0f;
                        white.b = 0f;
                        float num = H * 6f;
                        int num2 = (int)Math.Floor(num);
                        float num3 = num - (float)num2;
                        float num4 = V * (1f - S);
                        float num5 = V * (1f - S * num3);
                        float num6 = V * (1f - S * (1f - num3));
                        switch (num2)
                        {
                            case -1:
                                white.r = V;
                                white.g = num4;
                                white.b = num5;
                                break;
                            case 0:
                                white.r = V;
                                white.g = num6;
                                white.b = num4;
                                break;
                            case 1:
                                white.r = num5;
                                white.g = V;
                                white.b = num4;
                                break;
                            case 2:
                                white.r = num4;
                                white.g = V;
                                white.b = num6;
                                break;
                            case 3:
                                white.r = num4;
                                white.g = num5;
                                white.b = V;
                                break;
                            case 4:
                                white.r = num6;
                                white.g = num4;
                                white.b = V;
                                break;
                            case 5:
                                white.r = V;
                                white.g = num4;
                                white.b = num5;
                                break;
                            case 6:
                                white.r = V;
                                white.g = num6;
                                white.b = num4;
                                break;
                        }
                        bool flag3 = !hdr;
                        if (flag3)
                        {
                            white.r = Clamp(white.r, 0f, 1f);
                            white.g = Clamp(white.g, 0f, 1f);
                            white.b = Clamp(white.b, 0f, 1f);
                        }
                    }
                }
                return white;
            }
            public static float Clamp(float value, float min, float max)
            {
                bool flag = value < min;
                if (flag)
                {
                    value = min;
                }
                else
                {
                    bool flag2 = value > max;
                    if (flag2)
                    {
                        value = max;
                    }
                }
                return value;
            }

        

            public SevoCol(Color color)
            {
                r = (byte)color.R;
                g = (byte)color.G;
                b = (byte)color.B;
            }

            public SevoCol(object color)
            {
                var c = (object[])color;
                r = (byte)c[0];
                g = (byte)c[1];
                b = (byte)c[2];
                var a = (byte)c[3];
                //Console.WriteLine(a);
            }

            public SevoCol GetGlow()
            {
                return this.GetGlowColor();
            }

            public object sevoC()
            {
                if (r + g + b == 0)
                {
                    return new object[] { r, g, b,0 };
                }

                return new object[] { r, g, b, 255 };
            }

            public Color C()
            {
                return r + g + b == 0 ? Color.FromArgb(0, r, g, b) : Color.FromRgb(r, g, b);
            }

            public bool Equals(SevoCol other)
            {
                return r == other.r && g == other.g && b == other.b;
            }

            public override bool Equals(object obj)
            {
                return obj is SevoCol other && Equals(other);
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


            internal static ushort range_to_scale(int x, int y, int z)
            {
                var u = (ushort)z;
                u = (ushort)(u << (ushort)4 | (ushort)y);
                u = (ushort)(u << (ushort)4 | (ushort)x);
                return u;
            }


        }
        #endregion

        private static List<int> ingore = new List<int>() { 92, 96, 25, 109, 97, 93, 26, 110 };

        internal void WriteOutput(BrickEntityMp x, List<int> maintemp = null)
        {

            if(maintemp == null)
                maintemp = x.BrickDatas.IdsToRecycle.ToList();

            ParentEntity parent = new ParentEntity(x);
            BrickDatasSave b = new BrickDatasSave()
            {
                AdditionalDatas = x.BrickDatas.AdditionalDatas,
                IdsToRecycle = maintemp.ToArray(),
                Datas = x.BrickDatas.Datas
            };
            parent.Append(b);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Blueprint (*.sevo)|*.sevo";
            try
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    parent.SaveToDiskAtPathChild(saveFileDialog.FileName, false, x.BrickDatasChildrens);
                    Snackbar.MessageQueue?.Enqueue($"Exported {saveFileDialog.FileName}");

                }
                else
                {
                    Snackbar.MessageQueue?.Enqueue("Export Cancelled");
                }

            }
            catch (Exception e)
            {
                Snackbar.MessageQueue?.Enqueue(e.Message);
            }
            

        }

   
   

      
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            string currentd = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            string currentfile = "";
            switch (cmboxType.SelectedIndex)
            {
                case 0://Laser
                    currentfile = System.IO.Path.Combine(currentd, "lasertemplate.sevo");
                    break;
                case 1://Beam
                    currentfile = System.IO.Path.Combine(currentd, "beamtemplate.sevo");
                    break;
                case 2://Repair
                    currentfile = System.IO.Path.Combine(currentd, "repairtemplate.sevo");
                    break;
            }

            if (!new FileInfo(currentfile).Exists)
            {
                throw new Exception("missing base blueprint template");
            }


            using (FileStream fs = File.OpenRead(currentfile))
            {
                var x = BrickEntityMp.GetSaveFromFile(fs, true);

                bool procWeapon = false;
                bool procAddon = false;
                int weaponlen = 0;
                //Affix Barrel code 
                for (var i = 0; i < x.BrickDatas.Datas.Length; i++)
                {
                    if (x.BrickDatas.Datas[i].brickId == 0) continue;
                    if (x.BrickDatas.Datas[i].brickId == 3 || x.BrickDatas.Datas[i].brickId == 78 || x.BrickDatas.Datas[i].brickId == 36)
                    {
                        if (!procWeapon)
                        {
                            procWeapon = true;
                            x.BrickDatas.Datas[i].scale = range_to_scale(0, (int)sliderWeapon.Value - 1, 0);
                        }
                        else
                        {
                            x.BrickDatas.Datas[i] = x.BrickDatas.Datas[0];
                        }
                    }
                    if (x.BrickDatas.Datas[i].brickId == 181)//addon
                    {
                        weaponlen++;
                        if (weaponlen > sliderWeapon.Value)
                        {
                            x.BrickDatas.Datas[i] = x.BrickDatas.Datas[0];
                        }
                    }


                    if (x.BrickDatas.Datas[i].brickId == 82 && cmboxBarrel.IsEnabled)
                    {
                        switch (cmboxBarrel.SelectedIndex)
                        {
                            case 0://none
                                x.BrickDatas.Datas[i] = x.BrickDatas.Datas[0];
                                break;
                            case 2://recoil
                                x.BrickDatas.Datas[i].brickId = 84;
                                break;
                            case 3://heat
                                x.BrickDatas.Datas[i].brickId = 83;
                                break;
                            case 4://gat
                                x.BrickDatas.Datas[i].brickId = 85;
                                break;
                        }

                        x.BrickDatas.Datas[i].gridPosition += new Vector3i(0, 32 * ((int)sliderWeapon.Value - 16), 0);
                        x.BrickDatas.Datas[i].scale = range_to_scale(0, (int)sliderBarrel.Value - 1, 0);
                    }
                }
                WriteOutput(x);
            }
        }


        private void Export()
        {
            if (currentfile.Length == 0) return;

            if (!new FileInfo(currentfile).Exists)
            {
                Snackbar.MessageQueue?.Enqueue("Error: Missing base blueprint template");
                return;
            }

            bool check = true;
            check &= int.TryParse(txtX.Text, out int xOffset);
            check &= int.TryParse(txtY.Text, out int yOffset);
            check &= int.TryParse(txtZ.Text, out int zOffset);

            if (!check)
            {
                Snackbar.MessageQueue?.Enqueue("Error: Bad unit field, ensure valid number");
                return;
            }

            BrickEntityMp x = null;
            List<int> mainrecycleList = new List<int>();
            using (var fs = File.OpenRead(currentfile))
            {
                 x = BrickEntityMp.GetSaveFromFile(fs, true);

                Dictionary<Tuple<SevoCol, int>, Tuple<SevoCol, int, bool>> remap =
                    MyListBoxData
                        .Where(cswap => cswap.diff()).ToDictionary(cswap => new Tuple<SevoCol, int>(new SevoCol(cswap.ColorOld), cswap.MaterialIndex),
                            cswap => new Tuple<SevoCol, int, bool>(new SevoCol(cswap.ColorNew), cswap.MaterialIndexNew, cswap.RemoveBool.Value));
                int sc = 1;
                List<int> childpurge = new List<int>();
                int scaleShunt = (int)slider.Value - 3;
                //Child Entities
                for (int i = 0; i < x.BrickDatasChildrens.Count; i++)
                {
                    if (x.BrickDatasChildrens[i].Datas == null) continue;
                    List<int> recycleList = new List<int>();
                    for (int j = 0; j < x.BrickDatasChildrens[i].Datas.Length; j++)
                    {
                        List<int> avoid = new List<int>() { 97, 93, 26, 110 };
                        //The clipping component
                        if (checkBox.IsChecked.Value)
                        {
                           
                            if (new SevoCol(x.BrickDatasChildrens[i].Datas[j].color).Equals(new SevoCol(ColorChoice.SelectedColor.Value)) ==
                                compareEqual && !avoid.Contains(x.BrickDatasChildrens[i].Datas[j].brickId))
                            {
                                x.BrickDatasChildrens[i].Datas[j].gridPosition +=
                                    new Vector3i(xOffset, yOffset, zOffset);
                            }
                        }
                        //The repaint component
                        var c = new Tuple<SevoCol, int>(
                            new SevoCol(x.BrickDatasChildrens[i].Datas[j].color),
                            (int)(x.BrickDatasChildrens[i].Datas[j].material));
                        var t = new Tuple<SevoCol, int>(c.Item1, c.Item2);

                        if (remap.ContainsKey(c) || remap.ContainsKey(t))
                        {
                            if (remap.ContainsKey(c))
                            {
                                if (remap[c].Item3 || avoid.Contains(x.BrickDatasChildrens[i].Datas[j].brickId))
                                {
                                    x.BrickDatasChildrens[i].Datas[j].color = glowBlocks.Contains(x.BrickDatasChildrens[i].Datas[j].brickId) ? remap[c].Item1.GetGlow().sevoC() : remap[c].Item1.sevoC();
                                    x.BrickDatasChildrens[i].Datas[j].material = (uint)remap[c].Item2;
                                }
                                else
                                {
                                    if (j > 3)
                                        if (!ingore.Contains(x.BrickDatasChildrens[i].Datas[j].brickId))
                                            x.BrickDatasChildrens[i].Datas[j].brickId = 0;
                                }
                            }
                            else
                            {
                                if (remap[t].Item3 || avoid.Contains(x.BrickDatasChildrens[i].Datas[j].brickId))
                                {
                                    x.BrickDatasChildrens[i].Datas[j].color = glowBlocks.Contains(x.BrickDatasChildrens[i].Datas[j].brickId) ? remap[t].Item1.GetGlow().sevoC() : remap[t].Item1.sevoC();
                                    x.BrickDatasChildrens[i].Datas[j].material = (uint)remap[t].Item2;
                                }
                                else
                                {
                                    if (j > 3)
                                        if (!ingore.Contains(x.BrickDatasChildrens[i].Datas[j].brickId))
                                            x.BrickDatasChildrens[i].Datas[j].brickId = 0;
                                }
                            }

                        }
                        //The rescale component
                        //if we already flagged delete, we don't resize.
                        if (x.BrickDatasChildrens[i].Datas[j].brickId == 0 && j > 3)
                        {
                            recycleList.Add(j);
                        }
                        else
                        {
                            //If we're resizing down and we have a non-resize object, we purge it
                            if ((int)x.BrickDatasChildrens[i].Datas[j].gridSize + scaleShunt < 0)
                            {
                                if (avoid.Contains(x.BrickDatasChildrens[i].Datas[j].brickId))
                                {
                                    childpurge.Add(i);
                                    break;
                                }
                                else
                                {
                                    x.BrickDatasChildrens[i].Datas[j].brickId = 0;
                                    recycleList.Add(j);
                                }
                            }
                            else
                            {
                                double constan = Math.Pow(2, scaleShunt);
                                x.BrickDatasChildrens[i].Datas[j].gridPosition = new Vector3i(
                                    (int)(x.BrickDatasChildrens[i].Datas[j].gridPosition.X * constan),
                                    (int)(x.BrickDatasChildrens[i].Datas[j].gridPosition.Y * constan),
                                    (int)(x.BrickDatasChildrens[i].Datas[j].gridPosition.Z * constan));
                                x.BrickDatasChildrens[i].Datas[j].gridSize =
                                    (byte)(x.BrickDatasChildrens[i].Datas[j].gridSize + scaleShunt);
                                var id = x.BrickDatasChildrens[i].Datas[j].brickId;
                            }
                        }
                      
                        var temp = x.BrickDatasChildrens[i].IdsToRecycle.ToList();
                        temp.AddRange(recycleList);
                        var stepchild = x.BrickDatasChildrens[i];
                        stepchild.IdsToRecycle = temp.ToArray();
                        x.BrickDatasChildrens[i] = stepchild;
                    }
                }
                foreach (var index in childpurge)
                {
                    x.BrickDatasChildrens.Remove(x.BrickDatasChildrens[index]);
                }
                //Main Entity
                int count = 0;
                //int check = 0;
                mainrecycleList = new List<int>();
                for (var i = 0; i < x.BrickDatas.Datas.Length; i++)
                {
                    if (x.BrickDatas.Datas[i].brickId == 0) continue;

                    //clipping check
                    if (new SevoCol(x.BrickDatas.Datas[i].color).Equals(
                            new SevoCol(ColorChoice.SelectedColor.Value)) == compareEqual)
                    {
                        x.BrickDatas.Datas[i].gridPosition += new Vector3i(xOffset, yOffset, zOffset);
                    }

                    //Paint check
                    var c = new Tuple<SevoCol, int>(
                        new SevoCol(x.BrickDatas.Datas[i].color),
                        (int)(x.BrickDatas.Datas[i].material));
                    if (remap.ContainsKey(c))
                    {
                        if (remap[c].Item3 || x.BrickDatas.Datas[i].brickId == 97 ||
                            x.BrickDatas.Datas[i].brickId == 93 ||
                            x.BrickDatas.Datas[i].brickId == 26 ||
                            x.BrickDatas.Datas[i].brickId == 110)
                        {
                            x.BrickDatas.Datas[i].color = glowBlocks.Contains(x.BrickDatas.Datas[i].brickId) ? remap[c].Item1.GetGlow().sevoC() : remap[c].Item1.sevoC();
                            //x.BrickDatas.Datas[i].color = remap[c].Item1.sevoC();
                            x.BrickDatas.Datas[i].material = (uint)remap[c].Item2;
                        }
                        else
                        {
                            if (!ingore.Contains(x.BrickDatas.Datas[i].brickId))
                                x.BrickDatas.Datas[i].brickId = 0;
                        }
                    }
                  

                    if (x.BrickDatas.Datas[i].brickId == 0)
                    {
                        mainrecycleList.Add(i);
                        continue;
                    }
                        

                    //Rescale check
                    if ((int)x.BrickDatas.Datas[i].gridSize + scaleShunt < 0)
                    {
                        x.BrickDatas.Datas[i].brickId = 0;
                        mainrecycleList.Add(i);
                    }
                    else
                    {
                        double constan = Math.Pow(2, scaleShunt);
                        x.BrickDatas.Datas[i].gridPosition = new Vector3i(
                            (int)(x.BrickDatas.Datas[i].gridPosition.X * constan),
                            (int)(x.BrickDatas.Datas[i].gridPosition.Y * constan),
                            (int)(x.BrickDatas.Datas[i].gridPosition.Z * constan));
                        x.BrickDatas.Datas[i].gridPosition +=
                            SpecialOffsetRecur(0, scaleShunt, new Vector3i(0, 0, 0));
                        x.BrickDatas.Datas[i].gridSize = (byte)(x.BrickDatas.Datas[i].gridSize + scaleShunt);
                    }
                }
            }
            var maintemp = x.BrickDatas.IdsToRecycle.ToList();
            maintemp.AddRange(mainrecycleList);
            WriteOutput(x, maintemp);

        }

            internal static ushort range_to_scale(int x, int y, int z)
        {
            var u = (ushort)z;
            u = (ushort)(u << (ushort)4 | (ushort)y);
            u = (ushort)(u << (ushort)4 | (ushort)x);
            return u;
        }
        private void cmboxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != 0)
            {
                cmboxBarrel.IsEnabled = false;
                sliderBarrel.IsEnabled = false;
            }
            else
            {
                if (cmboxBarrel != null) cmboxBarrel.IsEnabled = true;
                if (sliderBarrel != null) sliderBarrel.IsEnabled = true;

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                " The Rescaler will remove any blocks it cannot resize to avoid clipping. This will include all logic, controls and some decorative bricks.",
                "The Rescaler", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StevoExport_click(object sender, RoutedEventArgs e)
        {
            Flip();
            this.Export();
            Flip();
        }
    }
}

