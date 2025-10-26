using System.Windows;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Pages;

using iNKORE.UI.WPF.Modern.Controls;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// CourseMgr.xaml 的交互逻辑
    /// </summary>

    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            Closing += (a, b) =>
            {
                Catalog.settings.Save();
            };
        }

        private void OnNavigationViewSelectionChanged(iNKORE.UI.WPF.Modern.Controls.NavigationView sender, iNKORE.UI.WPF.Modern.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            
            if (args.SelectedItem is NavigationViewItem)
            {
                var a = args.SelectedItem as NavigationViewItem;
                switch (a?.Tag)
                {
                    case "0":
                        if (rootFrame.CurrentSourcePageType != typeof(MainSetting))
                        {
                            rootFrame.Navigate(typeof(MainSetting));
                        }
                        break;
                    case "1":
                        if (rootFrame.CurrentSourcePageType != typeof(About))
                        {
                            rootFrame.Navigate(typeof(About));
                        }
                        break;
                    default:
                        if (rootFrame.CurrentSourcePageType != typeof(MainSetting))
                        {
                            rootFrame.Navigate(typeof(MainSetting));
                        }
                        break;
                }
                
            }
            else
            {/*
                var selectedItem = args.SelectedItemContainer;

                if (selectedItem == _allControlsMenuItem)
                {
                    if (rootFrame.CurrentSourcePageType != typeof(SettingPage))
                    {
                        rootFrame.Navigate(typeof(AllControlsPage));
                    }
                }
                else if (selectedItem == _newControlsMenuItem)
                {
                    if (rootFrame.CurrentSourcePageType != typeof(NewControlsPage))
                    {
                        rootFrame.Navigate(typeof(NewControlsPage));
                    }
                }
                else
                {
                    if (selectedItem.DataContext is ControlInfoDataGroup)
                    {
                        var itemId = ((ControlInfoDataGroup)selectedItem.DataContext).UniqueId;
                        rootFrame.Navigate(typeof(SectionPage), itemId);
                    }
                    else if (selectedItem.DataContext is ControlInfoDataItem)
                    {
                        var item = (ControlInfoDataItem)selectedItem.DataContext;
                        rootFrame.Navigate(typeof(ItemPage), item.UniqueId);
                    }
                }*/
            }
        }

        private void OnRootFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void OnRootFrameNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {

        }
    }
}