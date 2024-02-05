using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using Cokee.ClassService.Helper;
using Cokee.ClassService.Shared;
using Newtonsoft.Json;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;
using Clipboard = Wpf.Ui.Common.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// PostNote.xaml 的交互逻辑
    /// </summary>
    public partial class PostNote : UserControl
    {
        public bool IsEraser;
        public Student stu = null;
        public string stud = "";
        List<Student> students = new List<Student>();

        public PostNote()
        {
            InitializeComponent();
            IsVisibleChanged += (a, b) =>
            {
                List<Student> students = new List<Student>();
                List<string> str = new List<string>();
                students = StudentExtensions.Load();
                foreach (var item in students)
                {
                    str.Add(item.Name);
                }
                atu.ItemsSource = str;
                IsEraser = false;
                ink.Strokes.Clear();
                atu.Text = null;
                pen.Appearance = ControlAppearance.Primary;
                era.Appearance = ControlAppearance.Secondary;
            };
        }

        private void Pen(object sender, RoutedEventArgs e)
        {
            if (IsEraser)
            {
                IsEraser = false;
                ink.EditingMode = InkCanvasEditingMode.Ink;
                Button btn = sender as Button;
                btn.Appearance = ControlAppearance.Primary;
                era.Appearance = ControlAppearance.Secondary;
            }
        }

        private void save(object sender, RoutedEventArgs e)
        {
            //  if (stu == null) return;
            try
            {
                if (!Directory.Exists(Catalog.INK_DIR)) Directory.CreateDirectory(Catalog.INK_DIR);
                if (string.IsNullOrEmpty(stud)) Catalog.ShowInfo("未填写姓名。");
                FileStream fs = new FileStream(@$"{Catalog.INK_DIR}\{stud}.ink", FileMode.OpenOrCreate);
                ink.Strokes.Save(fs);
                fs.Close();
                Catalog.ShowInfo("已保存。");
                ink.Strokes = new StrokeCollection();
                atu.Text = null;
            }
            catch (Exception ex)
            {
                Clipboard.SetText(ex.ToString());
                Catalog.HandleException(ex,"PostNote");
            }
        }

        private void Eraser(object sender, RoutedEventArgs e)
        {
            if (!IsEraser)
            {
                IsEraser = true;
                ink.EditingMode = InkCanvasEditingMode.EraseByStroke;
                Button btn = sender as Button;
                btn.Appearance = ControlAppearance.Primary;
                pen.Appearance = ControlAppearance.Secondary;
            }
        }

        private void Atu_sc(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@$"{Catalog.INK_DIR}\backup"))
                Directory.CreateDirectory(@$"{Catalog.INK_DIR}\backup");
            AutoSuggestBox atu = sender as AutoSuggestBox;
            atu.Text = atu.Text.Trim();
            stud = atu.Text.Trim();
            if (File.Exists(@$"{Catalog.INK_DIR}\{stud}.ink"))
            {
                FileStream fs = new FileStream(@$"{Catalog.INK_DIR}\{stud}.ink", FileMode.Open);
                ink.Strokes = new StrokeCollection(fs);
                fs.Close();
                if (MessageBox.Show("文件已存在。确认覆盖？\n会把你之前写的备份哦。", "FileExist", MessageBoxButton.OKCancel) !=
                    MessageBoxResult.OK)
                {
                    atu.Text = "";
                    stud = "";
                    ink.Strokes.Clear();
                }
                else
                    File.Move(@$"{Catalog.INK_DIR}\{stud}.ink",
                        @$"{Catalog.INK_DIR}\backup\{stud}-bk-{DateTime.Now.ToString("yyyy-MM-dd")}.ink");
            }
        }
    }
}