using Adb_gui_Apkbox_plugin;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Components
{
    /// <summary>
    /// Interaction logic for KeyUI.xaml
    /// </summary>
    public partial class SettingsUI : Window
    {
        int RWidth, RHeight;
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        public SettingsUI(int height, int width)
        {
            InitializeComponent();
            RWidth = width;
            RHeight = height;

            // Setting Options for Location combo box
            _locationComBo.ItemsSource = new string[] { "Top Left", "Top Right", "Bottom Left", "Bottom Right" };
            _cancel.Click += (o, e) => { Close(); };
            _save.Click += (o, e) =>
            {
              File.WriteAllText(@"config.ini",
              "[Settings]" + Environment.NewLine +
              $"displayindex={_locationComBo.SelectedIndex.ToString()}" + Environment.NewLine +
              $"xaxis={_xaxis.Text}" + Environment.NewLine +
              $"yaxis={_yaxis.Text}" + Environment.NewLine +
              $"displaytime={(int)_timeSlider.Value}" + Environment.NewLine +
              $"notify={_messageCheckBox.IsChecked.ToString()}" + Environment.NewLine +
              $"stdkeys={_standardCheckBox.IsChecked.ToString()}" + Environment.NewLine);
                System.Windows.MessageBox.Show("Restart the application in order to apply the settings", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            };
            loadconfigs();
            _locationComBo.SelectionChanged += (o, e) => {
                var ht = SystemInformation.VirtualScreen.Height;
                var wt = SystemInformation.VirtualScreen.Width;
                switch (_locationComBo.SelectedIndex)
                {
                    case 0:
                        updateValues("20", "20");
                        break;
                    case 1:
                        updateValues((wt - RWidth).ToString(), "20");
                        break; 
                    case 2:
                        updateValues("20", (ht - Convert.ToInt16(RHeight) - 60).ToString());
                        break;
                    case 3:
                        updateValues((wt - RWidth).ToString(), (ht - Convert.ToInt16(RHeight) - 60).ToString());
                        break;
                }
            };
        }
        private void updateValues(string x, string y)
        {
            _xaxis.Text = x;
            _yaxis.Text = y;
        }
        private void loadconfigs()
        {
            if (File.Exists(@"config.ini"))
            {
                var myIni = new IniFile(@"config.ini");
                _locationComBo.SelectedIndex = Convert.ToInt16(myIni.Read("displayindex", "Settings"));
                _xaxis.Text = myIni.Read("xaxis", "Settings");
                _yaxis.Text = myIni.Read("yaxis", "Settings");
                _timeSlider.Value = Convert.ToDouble(myIni.Read("displaytime", "Settings"));
                _messageCheckBox.IsChecked = Convert.ToBoolean(myIni.Read("notify", "Settings"));
                _standardCheckBox.IsChecked = Convert.ToBoolean(myIni.Read("stdkeys", "Settings"));
            }
        }

        private void _timeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           try
            {
                _timeText.Text = $"{(int)e.NewValue} seconds, till the text will fade out.";
            }
            catch { }
        }
    }
}
