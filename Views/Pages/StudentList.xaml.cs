using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Shared;
using Cokee.ClassService.Views.Windows;

using Serilog;

using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace Cokee.ClassService.Views.Pages
{
    /// <summary>
    /// StudentList.xaml 的交互逻辑
    /// </summary>
    public partial class StudentList : Page
    {
        private ObservableCollection<Student> students = new ObservableCollection<Student>();
        private int RightClickCount;

        public StudentList()
        {
            try
            {
                InitializeComponent();
                if (!DesignerProperties.GetIsInDesignMode(this))
                {
                    this.Loaded += async (c, b) =>
                    {
                        Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().RandomEvent +=
                            StudentList_RandomEvent;
                        randomcontrol.RandomResultControl = randomres;
                        Students.StudentClick += Card_MouseDown;
                        Students.StudentRightClick += Card_MouseRightButtonDown;
                        studentInfo.EditStudent += StudentInfo_EditStudent;
                        var a = await StudentExtensions.Load();
                        students = new ObservableCollection<Student>(a.Students);
                        if (students != null)
                        {
                            Students.ItemsSource = students;
                            className.Text = $"{a.SchoolName} {a.Name} ID {a.ID}";
                            stuCount.Text = $"共 {students.Count} 名学生";
                        }
                    };
                    this.Unloaded += (a, b) => SaveData();
                }
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex);
            }
        }

        private void StudentList_RandomEvent(object? sender, bool e) => Catalog.ToggleControlVisible(randomcontrol);

        public async void SaveData()
        {
            await StudentExtensions.Save(students.ToList());
            //students = new ObservableCollection<Student>();
            //Students.ItemsSource = students;
            Catalog.ShowInfo("数据已保存.");
        }

        private void Card_MouseDown(object? sender, Student e)
        {
            Catalog.ToggleControlVisible(studentInfo);
            studentInfo.DataContext = e;
        }

        private void StudentInfo_EditStudent(object? sender, Student e)
        {
            int index = students.ToList().FindIndex(f => f.ID == e.ID);
            if (index != -1) students[index] = e;
            else students.Add(e);
            SaveData();
        }

        private void Card_MouseRightButtonDown(object? sender, Student e)
        {
            RightClickCount++;
            if (RightClickCount >= 10)
            {
                RightClickCount = 0;
                if (MessageBox.Show("确定删除？", "确定删除？\n删除记录将保存.", MessageBoxButton.YesNoCancel) == System.Windows.MessageBoxResult.Yes)
                {
                    Log.Warning($"Tried to DELETE Student {e.Name} ID {e.ID}");
                    students.Remove(e);
                    SaveData();
                }
            }
            else Catalog.ShowInfo($"{RightClickCount}", $"Click More {10 - RightClickCount} Times to Delete It.");
        }
    }
}