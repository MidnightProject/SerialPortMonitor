using System.Windows;
using System.Windows.Input;


namespace SerialPortMonitor
{
    public partial class AboutAuthorWindow : Window
    {
        public AboutAuthorWindow()
        {
            InitializeComponent();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CopyEmail_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText("krawczyksebastian84@gmail.com");
        }
    }
}
