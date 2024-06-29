using System;
using System.Security.Cryptography;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace WinUIAdaptiveMasterDetail {
    public sealed partial class WinUIMasterDetail : UserControl {
        public WinUIMasterDetail() {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty RightContentProperty = DependencyProperty.Register(
            nameof(RightContent), typeof(UIElement), typeof(WinUIMasterDetail), new PropertyMetadata(default));

        public UIElement RightContent {
            get { return (UIElement)GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }

        public static readonly DependencyProperty LeftContentProperty = DependencyProperty.Register(
            nameof(LeftContent), typeof(UIElement), typeof(WinUIMasterDetail), new PropertyMetadata(default));

        public UIElement LeftContent {
            get { return (UIElement)GetValue(LeftContentProperty); }
            set { SetValue(LeftContentProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            nameof(Footer), typeof(UIElement), typeof(WinUIMasterDetail), new PropertyMetadata(default));

        public UIElement Footer {
            get { return (UIElement)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); CheckFooter(); }
        }

        public static readonly DependencyProperty IsRightPaneShowingProperty = DependencyProperty.Register(
            nameof(IsRightPaneShowing), typeof(bool), typeof(WinUIMasterDetail), new PropertyMetadata(default));

        public bool IsRightPaneShowing {
            get { return (bool)GetValue(IsRightPaneShowingProperty); }
            set { SetValue(IsRightPaneShowingProperty, value); SwitchPane(value); }
        }

        public static readonly DependencyProperty LeftPaneIsCompactProperty = DependencyProperty.Register(
            nameof(LeftPaneIsCompact), typeof(bool), typeof(WinUIMasterDetail), new PropertyMetadata(default));

        public bool LeftPaneIsCompact {
            get { return (bool)GetValue(LeftPaneIsCompactProperty); }
            set { SetValue(LeftPaneIsCompactProperty, value); SetUpView(); LeftPaneIsCompactChanged?.Invoke(this, EventArgs.Empty); }
        }

        public double RightContentWidth { get { return ActualWidth >= 720 ? RightContentContainer.ActualWidth : ActualWidth; } }

        public event EventHandler RightPaneShowingChanged;
        public event EventHandler LeftPaneIsCompactChanged;


        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetUpStatusBar();
            SetUpView();
            CoreApplication.GetCurrentView().CoreWindow.KeyUp += CoreWindow_KeyUpDown;
            CoreApplication.GetCurrentView().CoreWindow.KeyDown += CoreWindow_KeyUpDown;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            SizeChanged -= UserControl_SizeChanged;
            Loaded -= UserControl_Loaded;
            Unloaded -= UserControl_Unloaded;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            SetUpView();
        }

        private void CheckFooter() {
            FooterContainer.Visibility = Footer == null ? Visibility.Collapsed : Visibility.Visible;
        }


        private void SetUpStatusBar() {
            UpdateLayoutDueNavBar(ApplicationView.GetForCurrentView().VisibleBounds);
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += (c, d) => {
                UpdateLayoutDueNavBar(c.VisibleBounds);
            };
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") {
                DisplayInformation di = DisplayInformation.GetForCurrentView();
                SetUpStatusBar(di.CurrentOrientation);
                di.OrientationChanged += (a, b) => {
                    SetUpStatusBar(a.CurrentOrientation);
                };
            } else if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop") {
                var tb = CoreApplication.GetCurrentView().TitleBar;
                TopPlaceholder.Height = tb.Height;
                tb.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            }
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) {
            TopPlaceholder.Height = sender.Height;
        }

        private void SetUpStatusBar(DisplayOrientations currentOrientation) {
            StatusBar sb = StatusBar.GetForCurrentView();
            if (currentOrientation == DisplayOrientations.Portrait || currentOrientation == DisplayOrientations.PortraitFlipped) {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                TopPlaceholder.Height = sb.OccludedRect.Height;
            } else {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                TopPlaceholder.Height = 0;
            }
        }

        private void UpdateLayoutDueNavBar(Rect vb) {
            DisplayInformation di = DisplayInformation.GetForCurrentView();
            Rect ws = Window.Current.Bounds;
            if (di.CurrentOrientation == DisplayOrientations.Portrait || di.CurrentOrientation == DisplayOrientations.PortraitFlipped) {
                LayoutRoot.Margin = new Thickness(0, 0, 0, ws.Height - vb.Bottom);
            } else {
                LayoutRoot.Margin = new Thickness(0);
            }
        }

        private void SetUpView() {
            if (ActualWidth >= 720) {
                double pw = ActualWidth / 3.25;
                if (pw < 320) pw = 320;

                Grid.SetColumnSpan(LeftContentContainer, 1);
                Grid.SetColumn(RightContentContainer, 1);
                Grid.SetRowSpan(RightContentContainer, 2);
                Grid.SetColumnSpan(FooterContainer, 1);
                LeftCD.MaxWidth = ActualWidth / 2;
                if (LeftPaneIsCompact) {
                    LeftCD.MinWidth = 72;
                    LeftCD.Width = new GridLength(72);
                } else {
                    LeftCD.MinWidth = 320;
                    LeftCD.Width = new GridLength(320, GridUnitType.Pixel);
                }

                LayerBackground.Opacity = 0;
                LayerBackgroundRight.Opacity = 1;
                LeftContentContainer.Visibility = Visibility.Visible;
                RightContentContainer.Visibility = Visibility.Visible;
                Splitter.Visibility = LeftPaneIsCompact ? Visibility.Collapsed : Visibility.Visible;
                LeftDivider.Opacity = 1;
            } else {
                Grid.SetColumnSpan(LeftContentContainer, 2);
                Grid.SetColumn(RightContentContainer, 0);
                Grid.SetRowSpan(RightContentContainer, 1);
                Grid.SetColumnSpan(FooterContainer, 2);

                LayerBackground.Opacity = 1;
                LayerBackgroundRight.Opacity = 0;
                SwitchPane(IsRightPaneShowing);
                Splitter.Visibility = Visibility.Collapsed;
            }
        }


        private void SwitchPane(bool isOpen) {
            if (ActualWidth < 720) {
                RightContentContainer.LayoutUpdated += RightContentContainer_LayoutUpdated;
                if (isOpen) {
                    LeftContentContainer.Visibility = Visibility.Collapsed;
                    RightContentContainer.Visibility = Visibility.Visible;
                    LayerBackground.Visibility = Visibility.Visible;
                    LeftDivider.Opacity = 0;
                } else {
                    LeftContentContainer.Visibility = Visibility.Visible;
                    RightContentContainer.Visibility = Visibility.Collapsed;
                    LayerBackground.Visibility = Visibility.Collapsed;
                    LeftDivider.Opacity = 1;
                }
            }
        }

        private void RightContentContainer_LayoutUpdated(object sender, object e) {
            RightContentContainer.LayoutUpdated -= RightContentContainer_LayoutUpdated;
            RightPaneShowingChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CoreWindow_KeyUpDown(CoreWindow sender, KeyEventArgs args) {
            bool ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            bool shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            bool f6 = Window.Current.CoreWindow.GetKeyState(VirtualKey.F6).HasFlag(CoreVirtualKeyStates.Down);
            LeftContentContainer.Opacity = ctrl && shift && f6 ? 0 : 1;
            RightContentContainer.Opacity = ctrl && shift && f6 ? 0 : 1;
            FCP.Opacity = ctrl && shift && f6 ? 0 : 1;
        }
    }
}
