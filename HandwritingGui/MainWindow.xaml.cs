using BitmapHelper;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
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

        private NetworkGuiLink Network;

        public MainWindow() {
            InitializeComponent();
            PrevNetworkDimText = Tb_NetworkDimensions.Text;
            Output = Rtb_Out.Document.Blocks;
            Network = new NetworkGuiLink();
            DataContext = Network;
            Network.PropertyChanged += NetworkPropertyChanged;

            //Defaults
            Tb_ImgDimensions.Text = "12";
            //Tb_ImgPath.Text = @"E:\Handwriting Data\HSF_0";
            Tb_ImgPath.Text = @"..\..\..\HandwritingData\HSF_0";
            Tb_LearnRate.Text = "0.001";
            Tb_LoadBatchSize.Text = "300";
            Tb_MicroBatchSize.Text = "1"; //Using micro batches improves parallelism but severely degrades effectivity of backpropagation
            Tb_NetworkDimensions.Text = "144*30*10";
            Rb_Charset_Digits.IsChecked = true;
            Rb_TFunc_HyperTan.IsChecked = true;

            Closing += (s, a) => {
                Network?.Dispose();
            };

            Log("UI init complete!");
        }

        public void Log(string msg, Color? foreground = null, Color? background = null) {
            Dispatcher.InvokeAsync(() => {
                var foreBrush = foreground != null ? new SolidColorBrush(foreground.Value) : Rtb_Out.Foreground;
                var backBrush = background != null ? new SolidColorBrush(background.Value) : Rtb_Out.Background;

                Output.Add(new Paragraph(new Run($"HG> {msg}") { Foreground = foreBrush, Background = backBrush }));

                if(!Keyboard.IsKeyToggled(Key.Scroll)) {
                    Rtb_Out.ScrollToEnd();
                }
            });
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

            string imgPath = Tb_ImgPath.Text;
            if(!Directory.Exists(imgPath)) {
                Log("Invalid image folder path", Colors.Red);
                return;
            }

            var loadNumbers = (Rb_Charset_All.IsChecked ?? false) || (Rb_Charset_Digits.IsChecked ?? false);
            var loadChars = (Rb_Charset_All.IsChecked ?? false) || (Rb_Charset_Alphabetic.IsChecked ?? false);

            var match = new Regex(@"^(?<inp>(\d|X)+)\*(?<hid>(\d|X)+\*)+(?<out>(\d|X)+)$").Match(Tb_NetworkDimensions.Text);
            if(!match.Success) {
                Log("Invalid network dimension, invalid format", Colors.Red);
                return;
            }
            int inputHeight;
            if(!int.TryParse(match.Groups?["inp"].Value, out inputHeight) || inputHeight != Math.Pow(imgDim, 2)) {
                Log("Invalid network dimension: Input", Colors.Red);
                return;
            }
            int expectedOutputNr = Rb_Charset_Alphabetic.IsChecked.Value ? (26 * 2) : Rb_Charset_Digits.IsChecked.Value ? 10 : ((26 * 2) + 10);
            if(match.Groups?["out"].Value != expectedOutputNr.ToString()) {
                Log("Invalid network dimension: Output", Colors.Red);
                return;
            }

            List<string> hidCaptures = new List<string>();
            foreach(Capture hidCapture in match.Groups?["hid"].Captures) {
                hidCaptures.Add(hidCapture.Value.TrimEnd('*'));
            }

            int?[] hiddenHeights = hidCaptures.Select<string, int?>(h => {
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

            var transFunc = Rb_TFunc_HyperTan.IsChecked ?? false ? TransferFunctionType.HyperbolicTangent : TransferFunctionType.Sigmoid;

            Log("Creating NeuralNet...");
            Task.Run(() => {
                Network.Init(imgDim, learningRate, microBatchsize, loadingBatchsize, imgPath, transFunc, inputHeight, expectedOutputNr, hiddenHeights.Select(nu => nu.Value).ToArray(), loadNumbers, loadChars);
            }).ContinueWith((t) => {
                Dispatcher.Invoke(() => {
                    if(t.IsCompleted) {
                        ((TabItem)Tc_Tabs.Items[1]).IsSelected = true;
                        Network.StartTraining();
                        Log("Network create done, start training");
                    } else {
                        Log($"Network creation failed: {t?.Exception?.InnerExceptions?[0]}", Colors.Red);
                    }
                });
            });
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
                int nr =
                    Rb_Charset_Digits.IsChecked ?? false ? 10 :
                    Rb_Charset_Alphabetic.IsChecked ?? false ? (26 * 2) :
                    Rb_Charset_All.IsChecked ?? false ? ((26 * 2) + 10) :
                        -1;

                Tb_NetworkDimensions.Text = new Regex(@"\*(\d+|X)$").Replace(Tb_NetworkDimensions.Text, "*" + nr);
            }
        }

        private void Bt_PauseLearning_Click(object sender, RoutedEventArgs e) {
            if(Network.IsTraining) {
                Log("Pausing training...");
                Network.PauseTraining();
            } else {
                Log("Starting training...");
                Network.StartTraining();
            }
        }

        private void RecogImgGrid_Drop(object sender, DragEventArgs e) {
            RecogImgGrid.Background = Brushes.Transparent;
            if(IsValidBMPImgDrop(e)) {
                string file = (e.Data.GetData(DataFormats.FileDrop) as string[])?[0];
                if(file != null) {
                    System.Drawing.Bitmap img = ImageReader.ReadImg(file, true, true, Network.ImageDimensions);
                    float[] greyVals = img.GreyValues(Network.TransferFunc.ExtremeMin, Network.TransferFunc.ExtremeMax);

                    BitmapImage displayImg;
                    using(var ms = new MemoryStream()) {
                        img.Save(ms, ImageFormat.Bmp);
                        ms.Position = 0;
                        displayImg = new BitmapImage();
                        displayImg.BeginInit();
                        displayImg.StreamSource = ms;
                        displayImg.CacheOption = BitmapCacheOption.OnLoad;
                        displayImg.EndInit();
                    }

                    Img_CurSelection.Source = displayImg;
                    Tb_ClickImgHint.Visibility = Visibility.Hidden;

                    var output = Network.Network.GetOutputForInput(greyVals);
                    var maxIndex = output.Select((val, indx) => new { val, indx }).Aggregate((i0, i1) => i0.val > i1.val ? i0 : i1).indx;
                    Log($"Image dropped, max index: {maxIndex}");

                    OxyPlot_NetworkOut.Model = new PlotModels.NetworkOutputModel(output, Network.TransferFunc.ExtremeMin, Network.TransferFunc.ExtremeMax).Model;
                    OxyPlot_NetworkOut.InvalidatePlot(true);
                }
            } else {
                Log("Invalid file", Colors.Red);
            }
        }

        private void RecogImgGrid_DragEnter(object sender, DragEventArgs e) {
            if(IsValidBMPImgDrop(e)) {
                RecogImgGrid.Background = Brushes.Green;
            } else {
                RecogImgGrid.Background = Brushes.Red;
            }
        }

        private void RecogImgGrid_DragLeave(object sender, DragEventArgs e) {
            RecogImgGrid.Background = Brushes.Transparent;
        }

        private bool IsValidBMPImgDrop(DragEventArgs e) {
            if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if(file.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }

        private void NetworkPropertyChanged(object sender, PropertyChangedEventArgs e) {
            Dispatcher.BeginInvoke((Action)(() => {
                if(e.PropertyName == "IsTraining") {
                    if(Network.IsTraining) {
                        Log("Training started", null, Colors.WhiteSmoke);
                        Bt_PauseLearning.Content = "Pause";
                        RecogImgGrid.IsEnabled = false;
                        Tb_ClickImgHint.Text = "Pause For User Input";
                    } else {
                        Log("Training paused", null, Colors.WhiteSmoke);
                        Bt_PauseLearning.Content = "Start";
                        RecogImgGrid.IsEnabled = true;
                        Tb_ClickImgHint.Text = "Drag & Drop Image";
                    }
                }
            }));
        }
    }
}