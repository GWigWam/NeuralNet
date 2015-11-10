using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HandwritingGui {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly Regex UIntRegex = new Regex("^[0-9]+$");
        private readonly Regex UDoubleRegex = new Regex("^[0-9.]+$");
        private string PrevNetworkDimText;

        private BlockCollection Output;

        public MainWindow() {
            InitializeComponent();
            PrevNetworkDimText = Tb_NetworkDimensions.Text;
            Output = Rtb_Out.Document.Blocks;

            Log("UI init complete!");
        }

        public void Log(string msg, Color? foreground = null, Color? background = null) {
            var foreBrush = foreground != null ? new SolidColorBrush(foreground.Value) : Rtb_Out.Foreground;
            var backBrush = background != null ? new SolidColorBrush(background.Value) : Rtb_Out.Background;

            Output.Add(new Paragraph(new Run(msg) { Foreground = foreBrush, Background = backBrush }));

            if(!Keyboard.IsKeyToggled(Key.Scroll)) {
                Rtb_Out.ScrollToEnd();
            }
        }

        private void PreviewUnsignedIntTb(object sender, TextCompositionEventArgs e) {
            e.Handled = !UIntRegex.IsMatch(e.Text);
        }

        private void PreviewPositiveDoubleTb(object sender, TextCompositionEventArgs e) {
            e.Handled = !UDoubleRegex.IsMatch(e.Text);
        }

        private void Bt_ImgPath_Browse_Click(object sender, RoutedEventArgs e) {
            var folderDial = new System.Windows.Forms.FolderBrowserDialog() {
                ShowNewFolderButton = false
            };
            if(folderDial.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                Tb_ImgPath.Text = folderDial.SelectedPath;
            }
        }

        private void Bt_SetupReady_Click(object sender, RoutedEventArgs e) {
            int imgDim;
            if(!int.TryParse(Tb_ImgDimensions.Text, out imgDim) || imgDim <= 1) {
                Log("Invalid image dimentions", Colors.Red);
                return;
            }

            double learningRate;
            if(!double.TryParse(Tb_LearnRate.Text, out learningRate) || learningRate <= 0) {
                Log("Invalid learning rate", Colors.Red);
                return;
            }

            int microBatchsize;
            if(!int.TryParse(Tb_MicroBatchSize.Text, out microBatchsize) || microBatchsize <= 0) {
                Log("Invalid Microbatch size", Colors.Red);
                return;
            }

            int loadingBatchsize;
            if(!int.TryParse(Tb_LoadBatchSize.Text, out loadingBatchsize) || loadingBatchsize <= 0 || loadingBatchsize < microBatchsize) {
                Log("Invalid Loadingbatch size", Colors.Red);
                return;
            }

            if(!Directory.Exists(Tb_ImgPath.Text)) {
                Log("Invalid image folder path", Colors.Red);
                return;
            }

            var match = new Regex(@"^(?<inp>(\d|X)+)\*(?<hid>(\d|X)+\*)+(?<out>(\d|X)+)$").Match(Tb_NetworkDimensions.Text);
            if(!match.Success) {
                Log("Invalid network dimension, invalid format", Colors.Red);
                return;
            }
            if(match.Groups?["inp"].Value != Math.Pow(imgDim, 2).ToString()) {
                Log("Invalid network dimension: Input", Colors.Red);
                return;
            }
            int expectedOutputNr = Rb_Charset_Alphabetic.IsChecked.Value ? 26 : Rb_Charset_Digits.IsChecked.Value ? 10 : 36;
            if(match.Groups?["out"].Value != expectedOutputNr.ToString()) {
                Log("Invalid network dimension: Output", Colors.Red);
                return;
            }

            int?[] hiddenHeights = match.Groups?["hid"].Value.TrimEnd('*').Split('*').Select<string, int?>(h => {
                int outp;
                if(int.TryParse(h, out outp))
                    return outp;
                return null;
            }).ToArray();

            if(hiddenHeights == null || hiddenHeights.Length == 0 || hiddenHeights.Any(i => i == null)) {
                Log("Invalid network dimension: Hidden", Colors.Red);
                return;
            }

            Log("All input seems to be valid", Colors.White, Colors.Green);
        }

        private void Tb_ImgDimensions_TextChanged(object sender, TextChangedEventArgs e) {
            int dim;
            string replacement = "X*";
            if(int.TryParse(Tb_ImgDimensions.Text, out dim) && dim < Math.Sqrt(double.MaxValue) - 1) {
                replacement = Math.Pow(dim, 2) + "*";
            } else {
                Log("Could not update network dimentions", Colors.WhiteSmoke, Colors.OrangeRed);
            }
            Tb_NetworkDimensions.Text = new Regex(@"^[0-9,X]+(E\+\d+)?\*").Replace(Tb_NetworkDimensions.Text, replacement);
        }

        private void Rb_Charset_Checked(object sender, RoutedEventArgs e) {
            if(IsInitialized) {
                Tb_NetworkDimensions.Text = new Regex(@"\*(\d+|X)$").Replace(Tb_NetworkDimensions.Text, "*" + ((string)((RadioButton)sender).Tag));
            }
        }
    }
}