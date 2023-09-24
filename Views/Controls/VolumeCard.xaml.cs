using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Office.Interop.PowerPoint;

using NAudio.CoreAudioApi;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// VolumeCard.xaml 的交互逻辑
    /// </summary>
    public partial class VolumeCard : UserControl
    {
        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        MMDevice speakDevice;

        public VolumeCard()
        {
            InitializeComponent();
            CancelTheMute();
            speakDevice = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray().FirstOrDefault();
            spk.Text = speakDevice.DeviceFriendlyName;
            vol.Text= $"{speakDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0f}%";
            slider.Value = speakDevice.AudioEndpointVolume.MasterVolumeLevel;
        }

        private void SliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speakDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)(e.NewValue / 100.0f);
            vol.Text = $"{speakDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0f}%";
        }

        public void CancelTheMute()
        {
            var enumerator = new MMDeviceEnumerator();
            IEnumerable<MMDevice> speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
            foreach (var mMDevice in speakDevices.ToList())
            {
                mMDevice.AudioEndpointVolume.Mute = false;//系统音量静音
            };

        }

        private void Button_Click(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;
    }
}
