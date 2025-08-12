using Microsoft.Win32;
using System.IO;
using System.Media;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Wdkkr.Models;


namespace Wdkkr.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private List<Zikr> azkar;
        private int currentIndex = 0;
        private Settings settings;
        private string azkarFile = "azkar.json";
        private string settingsFile = "settings.json";
        private NotifyIcon notifyIcon;
        private bool isExit;

        public MainWindow()
        {
            InitializeComponent();

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(SystemIcons.Information, 40, 40);
            notifyIcon.Visible = false;
            notifyIcon.Text = "تطبيق الأذكار";

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("فتح التطبيق", null, (s, e) => ShowMainWindow());
            contextMenu.Items.Add("إيقاف العرض", null, (s, e) => StopDisplay());
            contextMenu.Items.Add("خروج", null, (s, e) => ExitApplication());

            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

            LoadSettings();
            LoadAzkar();

            AzkarList.ItemsSource = azkar?.ConvertAll(z => z.Text);
            IntervalText.Text = settings?.IntervalMinutes.ToString() ?? "1";
            DisplayTimeText.Text = settings?.DisplaySeconds.ToString() ?? "5";
            SoundCheck.IsChecked = settings?.PlaySound ?? false;
            StartupCheck.IsChecked = settings?.StartWithWindows ?? false;

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
        }

        private void LoadAzkar()
        {
            if (File.Exists(azkarFile))
            {
                string json = File.ReadAllText(azkarFile);
                azkar = JsonSerializer.Deserialize<List<Zikr>>(json);
            }
            else
            {
                azkar = new List<Zikr>
                {
                    new Zikr { Text = "سبحان الله" },
                    new Zikr { Text = "الحمد لله" },
                    new Zikr { Text = "لا إله إلا الله" },
                    new Zikr { Text = "الله أكبر" }
                };
                SaveAzkar();
            }
        }

        private void SaveAzkar()
        {
            string json = JsonSerializer.Serialize(azkar, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(azkarFile, json);
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsFile))
            {
                string json = File.ReadAllText(settingsFile);
                settings = JsonSerializer.Deserialize<Settings>(json);
            }
            else
            {
                settings = new Settings();
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFile, json);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (azkar.Count == 0) return;

            if (settings.PlaySound)
            {
                SystemSounds.Asterisk.Play();
            }

            var popup = new PopupWindow(azkar[currentIndex].Text, settings.DisplaySeconds);
            popup.Show();

            currentIndex = (currentIndex + 1) % azkar.Count;
        }

        private void AddZikr_Click(object sender, RoutedEventArgs e)
        {
            string newZikr = NewZikrText.Text.Trim();
            if (!string.IsNullOrEmpty(newZikr))
            {
                azkar.Add(new Zikr { Text = newZikr });
                AzkarList.ItemsSource = azkar.ConvertAll(z => z.Text);
                AzkarList.Items.Refresh();
                SaveAzkar();
                NewZikrText.Clear();
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(IntervalText.Text, out int minutes) && minutes > 0 &&
                int.TryParse(DisplayTimeText.Text, out int seconds) && seconds > 0)
            {
                settings.IntervalMinutes = minutes;
                settings.DisplaySeconds = seconds;
                settings.PlaySound = SoundCheck.IsChecked ?? false;
                SaveSettings();

                timer.Interval = TimeSpan.FromMinutes(minutes);
                timer.Start();

                notifyIcon.Visible = true;
                notifyIcon.Icon = new System.Drawing.Icon("Assets/icon.ico");
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.BalloonTipTitle = "تذكير";
                notifyIcon.BalloonTipText = "حان وقت الذكر";
                notifyIcon.ShowBalloonTip(3000);

                this.Hide();

            }
            else
            {
                System.Windows.MessageBox.Show("أدخل قيم صحيحة");
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            System.Windows.MessageBox.Show("تم إيقاف العرض");
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            notifyIcon.Visible = false;
        }

        private void StopDisplay()
        {
            timer.Stop();
            System.Windows.MessageBox.Show("تم إيقاف عرض الأذكار");
        }

        private void ExitApplication()
        {
            isExit = true;
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!isExit)
            {
                e.Cancel = true;
                this.Hide();
                notifyIcon.Visible = true;
            }
            else
            {
                base.OnClosing(e);
            }
        }

        private void SetStartup(bool enable)
        {
            string appName = "Wdkkr"; // اسم البرنامج في الـ Registry
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (enable)
                {
                    registryKey.SetValue(appName, exePath);
                }
                else
                {
                    registryKey.DeleteValue(appName, false);
                }
            }
        }

        private void StartupCheck_Checked(object sender, RoutedEventArgs e)
        {
            settings.StartWithWindows = true;
            SetStartup(true);
            SaveSettings();
        }

        private void StartupCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.StartWithWindows = false;
            SetStartup(false);
            SaveSettings();
        }

        private void AzkarListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AzkarList.SelectedItem != null)
            {
                string selectedZikr = AzkarList.SelectedItem.ToString();

                var result = System.Windows.MessageBox.Show(
                    $"هل تريد حذف الذكر:\n\n{selectedZikr} ؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // حذف الذكر من القائمة
                    var itemToRemove = azkar.FirstOrDefault(z => z.Text == selectedZikr);
                    if (itemToRemove != null)
                    {
                        azkar.Remove(itemToRemove);
                        AzkarList.ItemsSource = azkar.ConvertAll(z => z.Text);
                        AzkarList.Items.Refresh();
                        SaveAzkar(); // حفظ القائمة بعد التعديل
                    }
                }
            }
        }

    }
}
