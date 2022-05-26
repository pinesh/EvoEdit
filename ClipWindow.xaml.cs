using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace EvoEditApp
{
    /// <summary>
    /// Interaction logic for ClipWindow.xaml
    /// </summary>
    public partial class ClipWindow : Window
    {
        public ClipWindow()
        {
            InitializeComponent();
            ColorChoice.ShowStandardColors = false;
            ColorChoice.ShowTabHeaders = false;
        }

        private bool compareEqual;
        private string currentfile;
        private Dictionary<PaintWindow.sevocol, int> ColorCount;
        private void button_Click_1(object sender, RoutedEventArgs e)
        {
      
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
                   
                    
                    ColorCount = new Dictionary<PaintWindow.sevocol, int>();
                    foreach (var d in x.BrickDatas.Datas)
                    {
                        var c = new PaintWindow.sevocol(d.color);
                        if (ColorCount.ContainsKey(c))
                        {
                            ColorCount[c] += 1;
                        }
                        else
                        {
                            ColorCount.Add(c, 1);
                        }
                    }

                    foreach (var c in from child in x.BrickDatasChildrens where child.Datas?.Length > 0 from d in child.Datas select new PaintWindow.sevocol(d.color))
                    {
                        if (ColorCount.ContainsKey(c))
                        {
                            ColorCount[c] += 1;
                        }
                        else
                        {
                            ColorCount.Add(c, 1);
                        }
                    }

                    ObservableCollection<ColorItem> shipColors = new ObservableCollection<ColorItem>();
                    foreach (var pair in ColorCount)
                    {
                        shipColors.Add(new ColorItem(pair.Key.C(), ""));
                    }
                    
                    var val = from ele in ColorCount
                              orderby ele.Value descending
                              select ele;
                    ColorChoice.SelectedColor = val.ElementAt(0).Key.C();
                    ColorChoice.AvailableColors = shipColors;
                  
                    lblBP.Content = Path.GetFileName(currentfile);
                    Flip();
                }
            }
        }

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

        private void updateBlocks(PaintWindow.sevocol c,bool comp)
        {
            lblblocks.Content = comp ? $"Move all ({ColorCount[c]}) blocks where color is" : $"Move all ({ColorCount.Where(keypair => !keypair.Key.Equals(c)).Sum(keypair => keypair.Value)}) blocks where color is";
        }
        private void export_button_Click(object sender, RoutedEventArgs e)
        {
            if (!new FileInfo(currentfile).Exists)
            {
                throw new Exception("missing base blueprint template");
            }

            bool check = true;
            check = check & int.TryParse(txtX.Text, out int xOffset);
            check = check & int.TryParse(txtY.Text, out int yOffset);
            check = check & int.TryParse(txtZ.Text, out int zOffset);

            if (!check)
            {
                MessageBox.Show("Bad unit field, ensure valid number", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            using (FileStream fs = File.OpenRead(currentfile))
            {
                var x = BrickEntityMp.GetSaveFromFile(fs, true);

                if (checkBox.IsChecked.Value)
                {
                    for (int i = 0; i < x.BrickDatasChildrens.Count; i++)
                    {
                        if (x.BrickDatasChildrens[i].Datas == null) continue;
                        for (int j = 0; j < x.BrickDatasChildrens[i].Datas.Length; j++)
                        {
                            if (x.BrickDatasChildrens[i].Datas[j].brickId == 0) continue;
                            if (new PaintWindow.sevocol(x.BrickDatasChildrens[i].Datas[j].color).Equals(
                                    new PaintWindow.sevocol(ColorChoice.SelectedColor.Value)) == compareEqual)
                            {
                                if (x.BrickDatasChildrens[i].Datas[j].brickId == 97 ||
                                    x.BrickDatasChildrens[i].Datas[j].brickId == 93 ||
                                    x.BrickDatasChildrens[i].Datas[j].brickId == 26 ||
                                    x.BrickDatasChildrens[i].Datas[j].brickId == 110)
                                {

                                }
                                else
                                {
                                    x.BrickDatasChildrens[i].Datas[j].gridPosition += new Vector3i(xOffset, yOffset, zOffset);
                                }
                            }
                        }
                    }
                }

                for (var i = 0; i < x.BrickDatas.Datas.Length; i++)
                {
                    if (x.BrickDatas.Datas[i].brickId == 0) continue;
                    if (new PaintWindow.sevocol(x.BrickDatas.Datas[i].color).Equals(
                            new PaintWindow.sevocol(ColorChoice.SelectedColor.Value)) == compareEqual)
                    {
                        x.BrickDatas.Datas[i].gridPosition += new Vector3i(xOffset, yOffset, zOffset);
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
                MessageBox.Show("Done", "Export Success",MessageBoxButton.OK);
            }

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

        internal void Flip()
        {
            txtX.IsEnabled = !txtX.IsEnabled;
            txtY.IsEnabled = !txtY.IsEnabled;
            txtZ.IsEnabled = !txtZ.IsEnabled;

            ColorChoice.IsEnabled = !ColorChoice.IsEnabled;
            EqualityBox.IsEnabled = !EqualityBox.IsEnabled;
            checkBox.IsEnabled = !checkBox.IsEnabled;
            BtnExport.IsEnabled = !BtnExport.IsEnabled;
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

        private void EqualityBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            compareEqual = (sender as ComboBox).SelectedIndex == 0;
            if(ColorChoice.SelectedColor != null)
                updateBlocks(new PaintWindow.sevocol(ColorChoice.SelectedColor.Value), compareEqual);
        }

        private void ColorChoice_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
          
            updateBlocks(new PaintWindow.sevocol(e.NewValue.Value),compareEqual);
           
        }
    }
}
