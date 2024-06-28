using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WinUIAdaptiveMasterDetail {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            GreatThing.IsRightPaneShowing = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            GreatThing.IsRightPaneShowing = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            GreatThing.Footer = null;
        }
    }
}
