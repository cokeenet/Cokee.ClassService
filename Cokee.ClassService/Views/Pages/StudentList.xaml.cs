using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
    /// 学生列表页面，负责学生数据展示、编辑、删除和随机抽取
    /// </summary>
    public partial class StudentList : Page
    {
        // 学生数据集合（绑定UI）
        private ObservableCollection<Student> _students = new ObservableCollection<Student>();
        // 右键点击计数（用于触发删除确认）
        private int _rightClickCount;
        // 缓存StudentMgr窗口实例（避免重复查找）
        private StudentMgr? _studentMgrWindow;

        public StudentList()
        {
            InitializeComponent();

            // 设计模式下不执行初始化逻辑
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Loaded += StudentList_Loaded;
                Unloaded += StudentList_Unloaded;
            }
        }

        #region 页面生命周期事件

        /// <summary>
        /// 页面加载时初始化（绑定事件、加载数据）
        /// </summary>
        private async void StudentList_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. 查找StudentMgr窗口并绑定随机事件
                _studentMgrWindow = Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault();
                if (_studentMgrWindow != null)
                {
                    _studentMgrWindow.RandomEvent += StudentList_RandomEvent;
                }
                else
                {
                    Catalog.ShowError("窗口初始化失败", "未找到StudentMgr窗口，随机抽取功能可能异常");
                }

                // 2. 初始化随机控件关联
                if (randomcontrol != null)
                {
                    randomcontrol.RandomResultControl = randomres;
                }

                // 3. 绑定新增学生事件（学生变更后更新UI和保存）
                if (newstu != null)
                {
                    newstu.StudentsChanged += (a, studentList) =>
                    {
                        if (studentList == null) return;

                        // 清空原有数据并添加新数据
                        _students.Clear();
                        foreach (var student in studentList)
                        {
                            _students.Add(student);
                        }
                        Students.ItemsSource = _students;
                        _ = SaveDataAsync(); // 异步保存，避免阻塞UI
                    };
                }

                // 4. 绑定学生卡片点击事件
                if (Students != null)
                {
                    Students.StudentClick += Card_MouseDown;
                    Students.StudentRightClick += Card_MouseRightButtonDown;
                }

                // 5. 绑定学生编辑事件
                if (studentInfo != null)
                {
                    studentInfo.EditStudent += StudentInfo_EditStudent;
                }

                // 6. 加载学生数据
                await LoadStudentDataAsync();
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex, "学生列表页面初始化");
            }
        }

        /// <summary>
        /// 页面卸载时清理（解绑事件、保存数据）
        /// </summary>
        private async void StudentList_Unloaded(object sender, RoutedEventArgs e)
        {
            // 解绑事件，避免内存泄漏
            if (_studentMgrWindow != null)
            {
                _studentMgrWindow.RandomEvent -= StudentList_RandomEvent;
            }

            if (Students != null)
            {
                Students.StudentClick -= Card_MouseDown;
                Students.StudentRightClick -= Card_MouseRightButtonDown;
            }

            if (studentInfo != null)
            {
                studentInfo.EditStudent -= StudentInfo_EditStudent;
            }

            // 保存数据
            await SaveDataAsync();
        }

        #endregion

        #region 数据加载与保存

        /// <summary>
        /// 异步加载学生数据
        /// </summary>
        private async Task LoadStudentDataAsync()
        {
            try
            {
                var classData = await StudentExtensions.LoadAsync();
                if (classData?.Students == null)
                {
                    Catalog.ShowInfo("数据加载提示", "暂无学生数据");
                    _students.Clear();
                    UpdateClassInfoUI(string.Empty, 0);
                    return;
                }

                // 更新学生集合（清空后添加，避免UI绑定异常）
                _students.Clear();
                foreach (var student in classData.Students)
                {
                    _students.Add(student);
                }

                // 绑定UI数据源
                Students.ItemsSource = _students;

                // 更新班级信息显示
                var className = $"{classData.SchoolName} {classData.Name}";
                UpdateClassInfoUI(className, classData.Students.Count);
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex, "加载学生数据");
            }
        }

        /// <summary>
        /// 异步保存学生数据
        /// </summary>
        public async Task SaveDataAsync()
        {
            try
            {
                var studentList = _students.ToList();
                await studentList.SaveAsync();
                Catalog.ShowInfo("数据保存成功", $"共保存 {studentList.Count} 名学生信息");
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex, "保存学生数据");
            }
        }

        /// <summary>
        /// 更新班级信息UI（班级名称、学生数量）
        /// </summary>
        private void UpdateClassInfoUI(string className, int studentCount)
        {
            if (this.className != null)
            {
                this.className.Text = string.IsNullOrEmpty(className) ? "未设置班级信息" : className;
            }

            if (this.stuCount != null)
            {
                this.stuCount.Text = $"共 {studentCount} 名学生";
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 响应随机抽取事件（显示/隐藏随机控件）
        /// </summary>
        private void StudentList_RandomEvent(object? sender, bool isShow)
        {
            if (randomcontrol != null)
            {
                Catalog.ToggleControlVisible(randomcontrol, isShow);
            }
        }

        /// <summary>
        /// 学生卡片左键点击（显示学生详情）
        /// </summary>
        private void Card_MouseDown(object? sender, Student student)
        {
            if (student == null || studentInfo == null) return;

            Catalog.ToggleControlVisible(studentInfo, true);
            studentInfo.DataContext = student;
        }

        /// <summary>
        /// 学生卡片右键点击（累计次数触发删除）
        /// </summary>
        private void Card_MouseRightButtonDown(object? sender, Student student)
        {
            if (student == null) return;

            // 累计右键点击次数
            _rightClickCount++;
            if (_rightClickCount >= 10)
            {
                // 次数达标，触发删除确认
                _rightClickCount = 0;
                var result = MessageBox.Show(
                    "确定删除该学生？\n删除后数据将自动保存",
                    "删除确认",
                    MessageBoxButton.YesNo
                );

                if (result == MessageBoxResult.Yes)
                {
                    Log.Warning($"删除学生记录：姓名={student.Name}，ID={student.ID}");
                    _students.Remove(student);
                    _ = SaveDataAsync(); // 异步保存删除结果
                }
            }
            else
            {
                // 提示剩余点击次数
                Catalog.ShowInfo(
                    "删除提示",
                    $"已点击 {_rightClickCount} 次，还需点击 {10 - _rightClickCount} 次确认删除"
                );
            }
        }

        /// <summary>
        /// 学生编辑完成事件（更新或新增学生）
        /// </summary>
        private void StudentInfo_EditStudent(object? sender, Student editedStudent)
        {
            if (editedStudent == null) return;

            // 查找学生在集合中的索引
            var existingIndex = _students.ToList().FindIndex(s => s.ID == editedStudent.ID);
            if (existingIndex >= 0)
            {
                // 更新现有学生
                _students[existingIndex] = editedStudent;
            }
            else
            {
                // 新增学生（自动分配ID：最大ID+1）
                var maxId = _students.Any() ? _students.Max(s => s.ID) : 0;
                editedStudent.ID = maxId + 1;
                _students.Add(editedStudent);
            }

            // 保存编辑结果
            _ = SaveDataAsync();
        }

        #endregion
    }
}