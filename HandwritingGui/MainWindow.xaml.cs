using System;
using System.Collections.Generic;
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
        private readonly Regex uintRegex = new Regex("^[0-9]+$");
        private readonly Regex udoubleRegex = new Regex("^[0-9.]+$");

        public MainWindow() {
            InitializeComponent();
        }

        private void PreviewUnsignedIntTb(object sender, TextCompositionEventArgs e) {
            e.Handled = !uintRegex.IsMatch(e.Text);
        }

        private void PreviewPositiveDoubleTb(object sender, TextCompositionEventArgs e) {
            e.Handled = !udoubleRegex.IsMatch(e.Text);
        }

        private void Bt_SetupReady_Click(object sender, RoutedEventArgs e) {
        }

        private void Bt_ImgPath_Browse_Click(object sender, RoutedEventArgs e) {
        }
    }
}