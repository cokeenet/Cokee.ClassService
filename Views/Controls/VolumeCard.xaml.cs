using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Cokee.ClassService.Helper;
using NAudio.CoreAudioApi;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// VolumeCard.xaml 的交互逻辑
    /// </summary>
    public partial class VolumeCard : UserControl
    {
        
        MMDevice speakDevice;
        List<MMDevice> devices;
        public VolumeCard()
        {
            InitializeComponent();
            
            UpdateSpkList();
            IsVisibleChanged += (a,b) => { UpdateSpkList(); CancelTheMute(); };
            
            

        }
        public void UpdateSpkList()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
            spk.ItemsSource = devices;
            speakDevice = devices.FirstOrDefault();
            spk.SelectedItem = speakDevice;
            vol.Text = $"{(speakDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0f).ToString("0")}%";
            slider.Value = speakDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0f;
        }
        private void spk_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            speakDevice = spk.SelectedItem as MMDevice;
            if (speakDevice != null)
            {
                slider.Value = speakDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0f;
                vol.Text = $"{(speakDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0f).ToString("0")}%";
            }
        }
        private void SliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speakDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)(e.NewValue / 100.0f);
            vol.Text = $"{(speakDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0f).ToString("0")}%";
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

        private void Button_Click(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);

        
    }
}
