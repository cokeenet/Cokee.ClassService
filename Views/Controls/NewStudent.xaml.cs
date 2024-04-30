using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Shared;
using Cokee.ClassService.Views.Windows;
using iNKORE.UI.WPF.Modern.Themes.DesignTime;


namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// NewStudent.xaml 的交互逻辑
    /// </summary>
    public partial class NewStudent : UserControl
    {
        public NewStudent()
        {
            InitializeComponent();
            if (DesignerAttribute.) return;
            this.Loaded += (a, b) => Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().AddMuitlStuEvent += (a, b) => Catalog.ToggleControlVisible(this);
            //this.Unloaded += (a, b) => Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().AddMuitlStuEvent -= (a, b) => Catalog.ToggleControlVisible(this);
        }

        private async void Confirm(object sender, RoutedEventArgs e)
        {
            if (DesignerHelper.IsInDesignMode) return;
            string[] lines = stutb.Text.Split("\r");
            List<Student> students = new List<Student>();
            foreach (string line in lines)
            {
                string[] values = line.Split(' ');
                string name = values[0];
                Sex sex = Sex.Girl;
                if (values[1] == "男") sex = Sex.Boy;
                DateTime dt = new DateTime();
                values[2] = values[2].Insert(4, "-");
                values[2] = values[2].Insert(7, "-");
                DateTime.TryParse(values[2], out dt);
                students.Add(new Student { Name = name, Sex = sex, BirthDay = dt });
            }
            await StudentExtensions.Save(students);
            Catalog.ToggleControlVisible(this);
        }

        private void Cancel(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);
    }
}