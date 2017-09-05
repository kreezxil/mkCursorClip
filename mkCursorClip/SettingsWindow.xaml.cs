using System.Windows;
using RamGecTools;

namespace mkCursorClip
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        mkCursorClip.Properties.Settings config = mkCursorClip.Properties.Settings.Default;
        KeyboardHook settingsHook = new KeyboardHook();
        public SettingsWindow()
        {
            InitializeComponent();
            settingsHook.KeyDown += new KeyboardHook.KeyboardHookCallback(settingsHook_KeyDown);
            settingsHook.Install();

            this.Closed += new System.EventHandler(SettingsWindow_Closed);
        }

        void SettingsWindow_Closed(object sender, System.EventArgs e)
        {
            settingsHook.Uninstall();
        }

        void settingsHook_KeyDown(KeyboardHook.VKeys key, ref bool handled)
        {
            if (tbKeyOne.IsFocused)
            {
                tbKeyOne.Text = key.ToString();
                handled = true;
            }
            else if (tbKeyTwo.IsFocused)
            {
                tbKeyTwo.Text = key.ToString();
                handled = true;
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            config.top = int.Parse(tbTop.Text);
            config.bottom = int.Parse(tbBottom.Text);
            config.left = int.Parse(tbLeft.Text);
            config.right = int.Parse(tbRight.Text);
            config.keyOne = tbKeyOne.Text;
            config.keyTwo = tbKeyTwo.Text;
            config.Save();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            config.Reset();
            config.Save();
            this.reloadConfig();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.reloadConfig();
        }

        private void reloadConfig()
        {
            config.Reload();
            tbTop.Text = config.top.ToString();
            tbBottom.Text = config.bottom.ToString();
            tbLeft.Text = config.left.ToString();
            tbRight.Text = config.right.ToString();
            tbKeyOne.Text = config.keyOne;
            tbKeyTwo.Text = config.keyTwo;
        }
    }
}
