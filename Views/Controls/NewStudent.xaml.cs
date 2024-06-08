using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Shared;
using Cokee.ClassService.Views.Windows;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// NewStudent.xaml 的交互逻辑
    /// </summary>
    public partial class NewStudent : UserControl

    {
        public EventHandler<List<Student>>? StudentsChanged;
        public NewStudent()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += (a, b) => Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().AddMuitlStuEvent += (a, b) => Catalog.ToggleControlVisible(this);
        }

        private async void Confirm(object sender, RoutedEventArgs e)
        {
            //if (!DesignerProperties.GetIsInDesignMode(this)) return;
            string[] lines = stutb.Text.Split(Environment.NewLine);
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
            //Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault()?.Close();
            //await StudentExtensions.Save(students);
            StudentsChanged?.Invoke(this,students);
            Catalog.ToggleControlVisible(this);
        }

        private void Cancel(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);
    }
}