using Cokee.ClassService.Helper;
using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Application = System.Windows.Forms.Application;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace Cokee.ClassService.Views.Pages
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class About : Page
    {
        public About()
        {
            try
            {
                InitializeComponent();

            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex);
            }
        }

        
    }
}