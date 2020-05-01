using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Components
{
    public partial class CodeUI : Window
    {
        int displayTime;
        public CodeUI(int displayTime)
        {
            InitializeComponent();
            this.displayTime = displayTime;

            

            var area = SystemParameters.WorkArea;
            Left = 20;
            Top = area.Height - 70;

            Loaded += CodeUI_Loaded;
            Closing += CodeUI_Closing;
        }

        private void CodeUI_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("Code UI Exited");
        }

        private void CodeUI_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Display TIme: " + displayTime);
           

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(displayTime * 1000);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Close();
                }));
                // Do things here.
                // NOTE: You may need to invoke this to your main thread depending on what you're doing
            });
           
        }
        public CodeUI SetText(string Text)
        {
            _tbCode.Text = Text;
            return this;
        }
        public CodeUI SetLocation(int X, int Y)
        {
            Left = X;
            Top = Y;
            return this;
        }

        public void PushUp()
        {
            Top = Top - this.Height;
        }
    }
}
