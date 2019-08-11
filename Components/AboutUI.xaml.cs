using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Components
{
    /// <summary>
    /// Interaction logic for AboutUI.xaml
    /// </summary>
    public partial class AboutUI : Window
    {
        public AboutUI()
        {
            InitializeComponent();

            _github.Click += (o, e) =>
              {
                  Process.Start("https://github.com/KaustubhPatange/Kling");
              };
        }
    }
}
