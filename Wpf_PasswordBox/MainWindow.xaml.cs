using System.Security;
using System.Windows;

namespace Wpf_PasswordBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SecureString _password;
        public SecureString Password
        {
            get => _password;
            set { if (_password != value) { _password = value; } }
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
