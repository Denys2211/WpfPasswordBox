using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf_PasswordBox
{
    public partial class XPasswordBox : TextBox
    {
        public event XPasswordBoxEventHandler OnChanged;

        #region DependencyProperties

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(nameof(Password), typeof(SecureString), typeof(XPasswordBox),
                                                                           new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, propertyChangedCallback: TextPasswordChanged));

        private static void TextPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is XPasswordBox passworBox) == false) return;
            if (e.NewValue is SecureString password == false) return;
            if (password.Length == 0) return;

            passworBox._actualtext = passworBox.SecureStringToString(password);

            if (passworBox.IconPassword.IsChecked.HasValue && !passworBox.IconPassword.IsChecked.Value && passworBox.Text != passworBox._actualtext)
                passworBox.Text = passworBox._actualtext;
            else
                for (int i = 0; passworBox._actualtext.Length >= i; i++)
                    passworBox.Text += "●";
        }

        public SecureString Password
        {
            get { return (SecureString)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register(nameof(IsValid), typeof(bool), typeof(XPasswordBox),
                                                                 new PropertyMetadata(true));
        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(XPasswordBox),
                                                                         new PropertyMetadata(null, new PropertyChangedCallback(OnPlaceholderTextChanged)));

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is XPasswordBox passworBox) == false) return;
            if (e.NewValue == null) return;

            passworBox.SetValue(XPasswordBox.TagProperty, e.NewValue.ToString());
        }

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }
        #endregion

        #region Overrided methods
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (InnerShadow != null && sizeInfo.NewSize.Width >= 0)
            {
                InnerShadow.Rect = new Rect(0, 0, sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (Text != null)
            {
                Dispatcher.Invoke(
                    async () =>
                    {
                        await Task.Delay(1);
                        SelectAll();
                    });
            }
        }
        #endregion

        private string _actualtext = string.Empty;
        private int _caretIndex;

        private bool IsCtrlState => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

        public XPasswordBox()
        {
            InitializeComponent();
            ApplyTemplate();
            Subsribe();
        }

        protected RectangleGeometry InnerShadow => GetTemplateChild("InnerShadow") as RectangleGeometry;
        protected TextBlock Placeholder => GetTemplateChild("Placeholder") as TextBlock;
        protected ToggleButton IconPassword => GetTemplateChild("IconPassword") as ToggleButton;

        private void Subsribe()
        {
            PreviewMouseDoubleClick += (s, b) =>
            {
                if (b.LeftButton == MouseButtonState.Pressed)
                {
                    if (!string.IsNullOrEmpty(SelectedText))
                    {
                        CaretIndex = Text.Length;
                    }
                    else if (string.IsNullOrEmpty(SelectedText))
                    {
                        SelectAll();
                        Focus();
                    }
                }
            };

            TextChanged += XPasswordBox_TextChanged;

            PreviewKeyDown += (s, e) =>
            {
                if(e.Key == Key.C && IsCtrlState && IconPassword.IsChecked.HasValue && !IconPassword.IsChecked.Value)
                {
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Z && IsCtrlState)
                {
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Back && IconPassword.IsChecked.HasValue && !IconPassword.IsChecked.Value)
                {
                    if (!string.IsNullOrEmpty(SelectedText))
                    {
                        _actualtext = _actualtext?.Remove(CaretIndex, SelectedText.Length);
                    }
                    else
                    if (CaretIndex > 0)
                    {
                        _actualtext = _actualtext?.Remove(CaretIndex - 1, 1);
                    }
                }

                if (e.Key == Key.Delete && IconPassword.IsChecked.HasValue && !IconPassword.IsChecked.Value && string.IsNullOrEmpty(SelectedText))
                {
                    if (!string.IsNullOrEmpty(SelectedText))
                    {
                        _actualtext = _actualtext?.Remove(CaretIndex, SelectedText.Length);
                    }
                    else
                    if (CaretIndex < _actualtext.Length)
                    {
                        _actualtext = _actualtext?.Remove(CaretIndex, 1);
                    }
                }
            };
        }

        private void XPasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                Placeholder.Visibility = Visibility.Collapsed;
            }
            else
            {
                _actualtext = string.Empty;
                Placeholder.Visibility = Visibility.Visible;
            }

            if (IconPassword.IsChecked.HasValue && !IconPassword.IsChecked.Value)
            {
                _caretIndex = CaretIndex;

                HideString();

                CaretIndex = _caretIndex;
            }
            else
            {
                _actualtext = Text;
            }

            SetActualPassword();
        }

        private void SetActualPassword()
        {
            var stringPassword = _actualtext;

            if (stringPassword == null)
            {
                return;
            }

            Password = new SecureString();

            foreach (var cr in stringPassword)
            {
                Password.AppendChar(cr);
            }

            OnChanged?.Invoke(this, new XPasswordBoxEventArg(Password));
        }

        private string SecureStringToString(SecureString value)
        {
            if (value == null) return null;

            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToBSTR(value);
                return Marshal.PtrToStringBSTR(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(valuePtr);
            }
        }

        private void HideString()
        {
            var aggreText = Text.Where(c => !c.Equals('●')).Aggregate("", (current, c) => current + c);

            if (!string.IsNullOrEmpty(aggreText) && !Text.Equals(_actualtext))
            {
                if (_actualtext.Length >= CaretIndex - 1)
                    _actualtext = _actualtext.Insert(CaretIndex - 1, aggreText);
                else
                    _actualtext = _actualtext.Insert(CaretIndex - aggreText.Length, aggreText);
            }

            char[] chars = new char[Text.Length];

            for (int i = 0; Text.Length > i; i++)
            {
                chars.SetValue('●', i);
            }

            TextChanged -= XPasswordBox_TextChanged;
            Text = new string(chars);
            TextChanged += XPasswordBox_TextChanged;
        }

        private void IconPassword_Checked(object sender, RoutedEventArgs e)
        {
            Text = _actualtext;
        }

        private void IconPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            HideString();
        }
    }
}
