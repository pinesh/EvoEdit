using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Path = System.Windows.Shapes.Path;


namespace EvoEditApp
{
    /// <summary>
    /// Interaction logic for WeaponDesigner.xaml
    /// </summary>
    public partial class WeaponDesigner : Window
    {
        public WeaponDesigner()
        {
            InitializeComponent();
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
                            x.BrickDatas.Datas[i].scale = range_to_scale(0,(int)sliderWeapon.Value-1,0);
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

                        x.BrickDatas.Datas[i].gridPosition += new Vector3i(0, 32*((int)sliderWeapon.Value-16), 0);
                        x.BrickDatas.Datas[i].scale = range_to_scale(0, (int)sliderBarrel.Value-1, 0);
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
                MessageBox.Show("Done", "Export Success", MessageBoxButton.OK);
            }
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
                if(cmboxBarrel != null) cmboxBarrel.IsEnabled = true;
                if(sliderBarrel != null) sliderBarrel.IsEnabled = true;
              
            }
        }
    }
}
