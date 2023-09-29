using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;

using Serilog;

using Application = Microsoft.Office.Interop.PowerPoint.Application;

namespace Cokee.ClassService.Helper
{
    /*public static class SyntacticSugar
    {
        public static void TryCatchAction(Action tryAction, Action finallyAction = null)
        {
            try
            {
                tryAction();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            finally
            {
                if (finallyAction != null)
                {
                    finallyAction();
                }
            }
        }

        public static void CurrentDispatcherInvoke(Action action)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(action.Invoke));
        }

        public static void CurrentDispatcherBeginInvoke(Action action)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(action.Invoke), new object[0]);
        }

        public static void CurrentDispatcherBeginInvoke(DispatcherPriority dispatcherPriority, Action action)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(dispatcherPriority, new Action(action.Invoke));
        }
    }

    public class PptOperator
    {
        public bool IsShowing { get; set; }

        public Color PointerColor { get; set; }

        public int SlidesCount { get; private set; }

        public string PptFilePath { get; private set; }

        public List<Slide> Slides { get; private set; }

        public static PptOperator Instance { get; set; }

        public Presentation Presentation { get; private set; }

        public IntPtr SlideShowWindowIntPtr { get; private set; }

        public PpSlideShowRangeType ShowRangeType { get; private set; }

        public PpSlideShowPointerType PointerType
        {
            get
            {
                PpSlideShowPointerType type = PpSlideShowPointerType.ppSlideShowPointerNone;
                SyntacticSugar.TryCatchAction(delegate
                {
                    SlideShowWindow slideShowWindow = this._slideShowWindow;
                    type = ((slideShowWindow != null) ? slideShowWindow.View.PointerType : PpSlideShowPointerType.ppSlideShowPointerNone);
                }, null);
                return type;
            }
            set
            {
                SyntacticSugar.TryCatchAction(delegate
                {
                    if (this._slideShowWindow != null)
                    {
                        this._slideShowWindow.View.PointerType = value;
                    }
                }, null);
            }
        }

        public int CurrentShowPosition
        {
            get
            {
                int result;
                try
                {
                    result = this._slideShowWindow.View.CurrentShowPosition;
                }
                catch
                {
                    result = 1;
                }
                return result;
            }
        }

        public int EndingSlide
        {
            get
            {
                return this.Presentation.SlideShowSettings.EndingSlide;
            }
        }

        public int StartingSlide
        {
            get
            {
                return this.Presentation.SlideShowSettings.StartingSlide;
            }
        }

        public PpSlideShowType? PptShowType
        {
            get
            {
                return new PpSlideShowType?(this.Presentation.SlideShowSettings.ShowType);
            }
        }

        private void SetSlideAdvanceOnClickProperty(MsoTriState value)
        {
            SyntacticSugar.TryCatchAction(delegate
            {
                if (this.Slides == null || !this.Slides.Any<Slide>())
                {
                    return;
                }
                foreach (Slide slide in this.Slides)
                {
                    slide.SlideShowTransition.AdvanceOnClick = value;
                }
            }, null);
        }

        public void PreviousPage()
        {
            if (this._stopwatch.ElapsedMilliseconds < (long)PptOperator.MinPageTurningTimerSpan)
            {
                return;
            }
            SyntacticSugar.TryCatchAction(delegate
            {
                Task.Run(delegate ()
                {
                    this._slideShowWindow.View.Previous();
                });
            }, null);
        }

        public void NextPage()
        {
            if (this._stopwatch.ElapsedMilliseconds < (long)PptOperator.MinPageTurningTimerSpan)
            {
                return;
            }
            SyntacticSugar.TryCatchAction(delegate
            {
                Task.Run(delegate ()
                {
                    this._slideShowWindow.View.Next();
                });
            }, null);
        }

        public void Exit()
        {
            PptOperator.SetActive((IntPtr)this._app.HWND);
            SyntacticSugar.TryCatchAction(delegate
            {
                Task.Run(delegate ()
                {
                    PptAnalyzer.StopAnalyzeAllAsyn();
                    this._slideShowWindow.View.Exit();
                });
            }, null);
        }

        public void GotoSlide(int index)
        {
            new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowNextSlide").RemoveEventHandler(this._app, new EApplication_SlideShowNextSlideEventHandler(this, (UIntPtr)ldftn(OnSlideShowNextSlide)));
            this._slideShowWindow.View.GotoSlide(index, MsoTriState.msoFalse);
            new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowNextSlide").AddEventHandler(this._app, new EApplication_SlideShowNextSlideEventHandler(this, (UIntPtr)ldftn(OnSlideShowNextSlide)));
            this._slideShowWindow.Activate();
        }

        public void UpdatePointerColor()
        {
            SyntacticSugar.TryCatchAction(delegate
            {
                Color red = Colors.Red;
                SlideShowWindow slideShowWindow = this._slideShowWindow;
                string text = ((slideShowWindow != null) ? slideShowWindow.View.PointerColor.RGB : 0).ToString("X2").PadLeft(6, '0');
                red.R = Convert.ToByte(text.Substring(4, 2), 16);
                red.G = Convert.ToByte(text.Substring(2, 2), 16);
                red.B = Convert.ToByte(text.Substring(0, 2), 16);
                this.PointerColor = red;
            }, null);
        }

        public void SetPointerColor(int color)
        {
            SyntacticSugar.TryCatchAction(delegate
            {
                this.PointerType = PpSlideShowPointerType.ppSlideShowPointerPen;
                this._slideShowWindow.View.PointerColor.RGB = color;
            }, null);
        }

        private bool CheckMainObject()
        {
            if (this._slideShowWindow == null)
            {
                Log.Error("SlideShowWindowIsNull");
                return false;
            }
            SlideShowWindow slideShowWindow = this._slideShowWindow;
            this.SlideShowWindowIntPtr = (IntPtr)((slideShowWindow != null) ? new int?(slideShowWindow.HWND) : null).Value;
            SlideShowWindow slideShowWindow2 = this._slideShowWindow;
            this.Presentation = ((slideShowWindow2 != null) ? slideShowWindow2.Presentation : null);
            if (this.Presentation == null)
            {
                Log.Error("PresentationIsNull");
                return false;
            }
            return true;
        }

        public event EventHandler ShowBegin;

        public event EventHandler ShowEnd;

        public event EventHandler NextSlide;

        private void OnShowBegin()
        {
            EventHandler showBegin = this.ShowBegin;
            if (showBegin == null)
            {
                return;
            }
            showBegin(null, EventArgs.Empty);
        }

        private void OnShowEnd()
        {
            EventHandler showEnd = this.ShowEnd;
            if (showEnd == null)
            {
                return;
            }
            showEnd(null, EventArgs.Empty);
        }

        private void OnNextSlide()
        {
            EventHandler nextSlide = this.NextSlide;
            if (nextSlide == null)
            {
                return;
            }
            nextSlide(this, EventArgs.Empty);
        }

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern bool SetActive(IntPtr hWnd);

        public void Initialize()
        {
            this._stopwatch = new Stopwatch();
            this._checkPowerPointTimer.Interval = TimeSpan.FromMilliseconds(2500.0);
            this._checkPowerPointTimer.Tick += this.CheckPowerPointProcess;
            this._checkPowerPointTimer.Start();
            this._clearEmptyPowerPointTimer.Interval = TimeSpan.FromMilliseconds(2500.0);
            this._clearEmptyPowerPointTimer.Tick += this.ClearEmptyPowerPointProcess;
            this.CheckPowerPointProcess();
        }

        private void CheckPowerPointProcess(object sender, EventArgs e)
        {
            this.CheckPowerPointProcess();
            int times = this._times;
            this._times = times + 1;
            if (times > 10)
            {
                this._times = 0;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void ClearEmptyPowerPointProcess(object sender, EventArgs e)
        {
            this.ClearEmptyPowerPointProcess();
        }

        private void CheckPowerPointProcess()
        {
            if (ProcessHelper.HasPowerPointProcess())
            {
                try
                {
                    this.CreatePowerPointApplication();
                    this.InitializeControl();
                    this.SetTimerEnable(true);
                }
                catch (Exception)
                {
                    this._app = null;
                    ConfigHelper.Instance.IsInitApplicationSuccessful = false;
                }
            }
            this._isCheckPptStateFirstly = false;
        }

        private void ClearEmptyPowerPointProcess()
        {
            try
            {
                if (!ProcessHelper.HasPowerPointProcess())
                {
                    this.CleanAndListen();
                }
                else if (this._app != null && this._app.Presentations.Count == 0)
                {
                    this.CleanAndListen();
                    if (this._app.Caption.ToLower().Contains("wps") || this._app.Path.ToLower().Contains("wps") || this._app.Path.ToLower().Contains("wpp"))
                    {
                        ProcessHelper.TryKillWppProcess();
                    }
                }
            }
            catch (COMException ex)
            {
                if (ex.HResult == -2147023174)
                {
                    this.CleanAndListen();
                }
            }
            catch (Exception ex2)
            {
                Log.Error(ex2.Message);
            }
        }

        private void SetTimerEnable(bool isPowerPointProcessStart)
        {
            if (isPowerPointProcessStart)
            {
                this._checkPowerPointTimer.Stop();
                this._clearEmptyPowerPointTimer.Start();
                return;
            }
            this._clearEmptyPowerPointTimer.Stop();
            this._checkPowerPointTimer.Start();
        }

        private void InitializeControl()
        {
            if (this._app == null || this.IsShowing)
            {
                return;
            }
            SlideShowWindows slideShowWindows = this._app.SlideShowWindows;
            if (slideShowWindows.Count > 0)
            {
                if (this._isCheckPptStateFirstly)
                {
                    try
                    {
                        if (this._app.ActivePresentation.SlideShowSettings.ShowType == PpSlideShowType.ppShowTypeSpeaker)
                        {
                            return;
                        }
                        this.AppSlideShowBegin(slideShowWindows[1]);
                        return;
                    }
                    catch (Exception arg)
                    {
                        Log.Error(string.Format("InitializeControl：{0}", arg));
                        return;
                    }
                }
                this.AppSlideShowBegin(slideShowWindows[1]);
            }
        }

        private void CreatePowerPointApplication()
        {
            if (this._app != null)
            {
                new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowBegin").RemoveEventHandler(this._app, new EApplication_SlideShowBeginEventHandler(this, (UIntPtr)ldftn(AppSlideShowBegin)));
                new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowEnd").RemoveEventHandler(this._app, new EApplication_SlideShowEndEventHandler(this, (UIntPtr)ldftn(OnAppSlideShowEnd)));
                new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowNextSlide").RemoveEventHandler(this._app, new EApplication_SlideShowNextSlideEventHandler(this, (UIntPtr)ldftn(OnSlideShowNextSlide)));
            }
            this._app = new Application();
            //this._app = (Microsoft.Office.Interop.PowerPoint.Application)Marshal.GetA("PowerPoint.Application");
            PptVersionManager.UpdatePptApplicationInfo(this._app.Path, this._app.Version);
            new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowBegin").AddEventHandler(this._app, new EApplication_SlideShowBeginEventHandler(this, (UIntPtr)ldftn(AppSlideShowBegin)));
            new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowEnd").AddEventHandler(this._app, new EApplication_SlideShowEndEventHandler(this, (UIntPtr)ldftn(OnAppSlideShowEnd)));
            new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowNextSlide").AddEventHandler(this._app, new EApplication_SlideShowNextSlideEventHandler(this, (UIntPtr)ldftn(OnSlideShowNextSlide)));
            new ComAwareEventInfo(typeof(EApplication_Event), "PresentationCloseFinal").AddEventHandler(this._app, new EApplication_PresentationCloseFinalEventHandler(this, (UIntPtr)ldftn(AppOnPresentationCloseFinal)));
            ConfigHelper.Instance.IsInitApplicationSuccessful = true;
            Log.Debug("PPTApplicationInitialized");
        }

        private void AppOnPresentationCloseFinal(Presentation pres)
        {
            if (this._app.Presentations.Count == 0 || this._app.Presentations.Count == 1)
            {
                try
                {
                    if (this._app.ActivePresentation == pres)
                    {
                        System.Windows.Application application = System.Windows.Application.Current;
                        if (application != null)
                        {
                            application.Dispatcher.InvokeAsync(new Action(this.CleanAndListen));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex.Message);
                }
            }
        }

        private void CleanAndListen()
        {
            this.TryCleanComObject();
            this.SetTimerEnable(false);
        }

        private void TryCleanComObject()
        {
            try
            {
                if (this._app != null)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowBegin").RemoveEventHandler(this._app, new EApplication_SlideShowBeginEventHandler(this, (UIntPtr)ldftn(AppSlideShowBegin)));
                    new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowEnd").RemoveEventHandler(this._app, new EApplication_SlideShowEndEventHandler(this, (UIntPtr)ldftn(OnAppSlideShowEnd)));
                    new ComAwareEventInfo(typeof(EApplication_Event), "SlideShowNextSlide").RemoveEventHandler(this._app, new EApplication_SlideShowNextSlideEventHandler(this, (UIntPtr)ldftn(OnSlideShowNextSlide)));
                    Marshal.ReleaseComObject(this._app);
                    Marshal.ReleaseComObject(this._app);
                    this._app = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private void AppSlideShowBegin(SlideShowWindow wn)
        {
            try
            {
                if (ConfigHelper.Instance.IsNeedStart())
                {
                    this._slideShowWindow = wn;
                    if (this.CheckMainObject())
                    {
                        PpSlideShowType? pptShowType = this.PptShowType;
                        PpSlideShowType ppSlideShowType = PpSlideShowType.ppShowTypeSpeaker;
                        this._isShowTypeSpeaker = (pptShowType.GetValueOrDefault() == ppSlideShowType & pptShowType != null);
                        ConfigHelper.Instance.IsPptShowTypeSpeaker = this._isShowTypeSpeaker;
                        if (this._isShowTypeSpeaker)
                        {
                            this.ShowRangeType = this.Presentation.SlideShowSettings.RangeType;
                            this.SlidesCount = this.Presentation.Slides.Count;
                            this.PptFilePath = this.Presentation.FullName;
                            this.Slides = this.Presentation.Slides.Cast<Slide>().ToList<Slide>();
                            PptAnalyzer.InitPptAnalyzer(this.PptFilePath);
                            this.IsShowing = true;
                            this.OnShowBegin();
                            this.SetSlideAdvanceOnClickProperty(MsoTriState.msoFalse);
                            ConfigHelper.Instance.IsShowing = this.IsShowing;
                            SyntacticSugar.CurrentDispatcherBeginInvoke(delegate ()
                            {
                                this._stopwatch.Restart();
                            });
                            ConfigHelper.Instance.IsComOk = true;
                            Log.Debug("PPTShowBegin");
                        }
                    }
                }
            }
            catch (Exception arg)
            {
                ConfigHelper.Instance.IsComOk = false;
                Log.Error(string.Format("ShowBeginError：{0}", arg));
            }
        }

        private void OnAppSlideShowEnd(Presentation pres)
        {
            this.SetSlideAdvanceOnClickProperty(MsoTriState.msoTrue);
            SyntacticSugar.TryCatchAction(delegate
            {
                if (!this._isShowTypeSpeaker)
                {
                    return;
                }
                this.IsShowing = false;
                PptAnalyzer.StopAnalyzeAllAsyn();
                SyntacticSugar.CurrentDispatcherBeginInvoke(delegate ()
                {
                    this._stopwatch.Stop();
                });
                ConfigHelper.Instance.IsShowing = this.IsShowing;
                this._isShowTypeSpeaker = false;
                this.OnShowEnd();
                Log.Debug("PPTShowEnd");
            }, null);
        }

        private void OnSlideShowNextSlide(SlideShowWindow wn)
        {
            SyntacticSugar.TryCatchAction(delegate
            {
                if (!this._isShowTypeSpeaker)
                {
                    return;
                }
                SyntacticSugar.CurrentDispatcherBeginInvoke(delegate ()
                {
                    this._stopwatch.Restart();
                });
                this.OnNextSlide();
            }, null);
        }

        public PptOperator()
        {
        }

        // Note: this type is marked as 'beforefieldinit'.
        static PptOperator()
        {
        }

        private int _times;

        private Microsoft.Office.Interop.PowerPoint.Application _app;

        private Stopwatch _stopwatch;

        private bool _isShowTypeSpeaker;

        private SlideShowWindow _slideShowWindow;

        private const int PptProcessCheckInterval = 2500;

        private static readonly int MinPageTurningTimerSpan = 30;

        private bool _isCheckPptStateFirstly = true;

        private readonly DispatcherTimer _checkPowerPointTimer = new DispatcherTimer();

        private readonly DispatcherTimer _clearEmptyPowerPointTimer = new DispatcherTimer();
    }*/
}
