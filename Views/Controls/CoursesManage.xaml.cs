using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Pages;
using Cokee.ClassService.Views.Windows;

using Newtonsoft.Json;

namespace Cokee.ClassService.Views.Controls
{

    public partial class CoursesManage : UserControl
    {
        public CoursesManage()
        {
            InitializeComponent();
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            string[] lines = stutb.Text.Split("\r");
            List<Student> students = new List<Student>();
            foreach (string line in lines)
            {
                string[] values = line.Split(' ');
                string name = values[0];
                int sex = 0;
                if (values[1] == "男") sex = 1;
                DateTime dt = new DateTime();
                values[2] = values[2].Insert(4, "-");
                values[2] = values[2].Insert(7, "-");
                DateTime.TryParse(values[2], out dt);
                students.Add(new Student(name, sex, dt));
                Student.SaveToFile(students);
            }
            this.Visibility = Visibility.Collapsed;
        }

        private void Cancel(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;
    }
}
