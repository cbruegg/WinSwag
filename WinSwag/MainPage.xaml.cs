﻿using GalaSoft.MvvmLight.Messaging;
using System;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WinSwag.Controls;
using WinSwag.Services;
using WinSwag.ViewModels;
using WinSwag.Views;

namespace WinSwag
{
    public sealed partial class MainPage : Page
    {
        [Inject]
        private readonly IMessenger _messenger = null;

        [Inject]
        public ISessionManagerVM SessionManagerVM { get; private set; }

        [Inject]
        public IOperationManagerVM OperationManagerVM { get; private set; }

        [Inject]
        public IViewStateManagerVM ViewStateManagerVM { get; private set; }

        public MainPage()
        {
            ApplicationInstance.Current.Services.Populate(this);
            InitializeComponent();

            Window.Current.SetTitleBar(TitleBar);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = Colors.Gray;
            titleBar.ButtonPressedBackgroundColor = Colors.DimGray;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonHoverForegroundColor = Colors.White;
            titleBar.ButtonPressedForegroundColor = Colors.White;
            titleBar.ButtonInactiveForegroundColor = Colors.Gray;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _messenger.Register<CloseDashboard>(this, _ => DashboardPopup.Hide());

            _messenger.Register<NavigatedToOperation>(this, msg =>
            {
                switch (msg.NavigationMode)
                {
                    case NavigationMode.New:
                        if (msg.Operation == null)
                            ContentFrame.Navigate(typeof(ApiInfoPage), SessionManagerVM.CurrentDocument);
                        else
                            ContentFrame.Navigate(typeof(SwaggerOperationPage), msg.Operation);
                        break;

                    case NavigationMode.Back: ContentFrame.GoBack(); break;
                    case NavigationMode.Forward: ContentFrame.GoForward(); break;
                }

                OperationsListView.SelectedItem = msg.Operation;
            });

            KeyDown += OnKeyDown;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _messenger.Unregister(this);
            KeyDown -= OnKeyDown;
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            OperationManagerVM.GoBack();
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.F5 when OperationManagerVM.IsOperationSelected:
                    OperationManagerVM.SelectedOperation.BeginSendRequest();
                    e.Handled = true;
                    break;

                case VirtualKey.Escape when !e.Handled:
                    DashboardPopup.IsOpen = !DashboardPopup.IsOpen;
                    e.Handled = true;
                    break;

                case VirtualKey.GoBack:
                case VirtualKey.Left when e.KeyStatus.IsMenuKeyDown:
                    OperationManagerVM.GoBack();
                    break;

                case VirtualKey.GoForward:
                case VirtualKey.Right when e.KeyStatus.IsMenuKeyDown:
                    OperationManagerVM.GoForward();
                    break;
            }
        }

        private void OperationClicked(object sender, ItemClickEventArgs e)
        {
            var operation = (SwaggerOperationViewModel)e.ClickedItem;
            OperationManagerVM.NavigateToOperation(operation);
        }

        private void DashboardPopupOpened(PopupWindow sender, object args)
        {
            Window.Current.SetTitleBar(DashboardPopup.TitleBar);
        }

        private void DashboardPopupClosed(PopupWindow sender, object args)
        {
            Window.Current.SetTitleBar(TitleBar);
        }
    }
}
