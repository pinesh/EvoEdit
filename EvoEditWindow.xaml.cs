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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Ookii.Dialogs.Wpf;
using Path = System.IO.Path;

namespace EvoEditApp
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    /// 
    public partial class EvoEditWindow : Window
    {
        public String GlobalDestinationPath;

        public EvoEditWindow()
        {
            InitializeComponent();
            GlobalDestinationPath = OutputFolderHandler.check_output_path();
            outputPath.Content = GlobalDestinationPath;
            OutputFolderHandler.SomeEvent += RefreshPath;
            _starshipFrame.Navigate(new StevoPage());
            _starmadeFrame.Navigate(new StarmadePage());
            _3dFrame.Navigate(new _3dPage());
        }

        internal void RefreshPath(object sender, EventArgs args)
        {
            GlobalDestinationPath = OutputFolderHandler.check_output_path();
            outputPath.Content = GlobalDestinationPath;
        }


        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnHelpButtonClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/pinesh/EvoEdit");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OutputFolderHandler.Setpath();
        }


        public static class OutputFolderHandler
        {


            public static event EventHandler SomeEvent = delegate { };



            private static string prompt_path()
            {
                if (MessageBox.Show(
                        "You have not set an output folder, would you like to do so now? This is where your Starship Evo Blueprints will appear.",
                        "Set output folder",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    return Setpath(); // Do something here
                }

                return "";
            }

            public static string CreateUniqueTempDirectory()
            {
                var uniqueTempDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
                Directory.CreateDirectory(uniqueTempDir);
                return uniqueTempDir;
            }

            public static string Setpath()
            {
                var ookiiDialog = new VistaFolderBrowserDialog();
                if (ookiiDialog.ShowDialog() == true)
                {
                    write_output_path(ookiiDialog.SelectedPath);
                    return ookiiDialog.SelectedPath;
                }

                return "";
            }

            public static string check_output_path()
            {
                string destination;
                try
                {
                    string p = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                        "data.xml");
                    FileInfo f = new FileInfo(p);
                    try
                    {
                        if (f.Exists)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(p);
                            destination = doc.InnerText;
                            //CurrentFolder.Text = new StringBuilder("Output: ").Append("Set").ToString();
                            if (!new DirectoryInfo(destination).Exists)
                            {
                                return prompt_path();
                            }
                        }
                        else
                        {
                            return prompt_path();
                        }
                    }
                    catch (Exception e)
                    {
                        return prompt_path();
                    }
                  

                    return destination;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw new Exception();
                }

            }

            private static void write_output_path(string destination)
            {
                var p = System.IO.Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "data.xml");
                var writer = new System.Xml.Serialization.XmlSerializer(typeof(String));
                var file = System.IO.File.Create(p);
                writer.Serialize(file, destination);
                file.Close();
                SomeEvent(null, EventArgs.Empty);
            }
        }

    }
}
