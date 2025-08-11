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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Wdkkr.Views
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        private DispatcherTimer closeTimer;

        public PopupWindow(string zikr, int displaySeconds)
        {
            InitializeComponent();
            ZikrText.Text = zikr;

            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var taskbarHeight = screenHeight - SystemParameters.WorkArea.Height;

            double finalTop = screenHeight - this.Height - taskbarHeight - 20;
            double startTop = screenHeight + 10;

            this.Left = screenWidth - this.Width - 20;
            this.Top = finalTop;
            this.Tag = startTop;

            closeTimer = new DispatcherTimer();
            closeTimer.Interval = TimeSpan.FromSeconds(displaySeconds);
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                StartExitAnimation();
            };
            closeTimer.Start();
        }

        private void StartExitAnimation()
        {
            var sb = (Storyboard)this.Resources["ExitStoryboard"];
            sb.Completed += (s, e) => this.Close();
            sb.Begin(this);
        }
    }
}
