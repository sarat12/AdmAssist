using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AdmAssist.Views.Controls
{
    /// <summary>
    /// Interaction logic for IpAddressControl.xaml
    /// </summary>
    public partial class IpAddressControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(IpAddressControl), new UIPropertyMetadata(default(string), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var _this = dependencyObject as IpAddressControl;
            var newValue = (string)dependencyPropertyChangedEventArgs.NewValue;
            IPAddress ip;

            if (_this != null && IPAddress.TryParse(newValue, out ip))
            {
                var bytesStrArray = ip.ToString().Split('.');

                _this.Box1.Text = bytesStrArray[0];
                _this.Box2.Text = bytesStrArray[1];
                _this.Box3.Text = bytesStrArray[2];
                _this.Box4.Text = bytesStrArray[3];
            }
        }

        public string Text
        {
            //get { return (string) GetValue(TextProperty); }
            get { return Box1.Text + "." + Box2.Text + "." + Box3.Text + "." + Box4.Text; }
            set { SetValue(TextProperty, value); }
        }

        public IpAddressControl()
        {
            InitializeComponent();
        }

        private void Box_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text[0]);
        }

        private void Box_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;

            if (box.Text.Length == 3)
            {
                var elementWithFocus = Keyboard.FocusedElement as UIElement;

                if (elementWithFocus != null)
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        private void Box_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }

            var box = (TextBox)sender;

            if ((e.Key == Key.Left || e.Key == Key.Back) && box.SelectionStart == 0 && !Equals(box, Box1))
            {
                var elementWithFocus = Keyboard.FocusedElement as UIElement;

                if (elementWithFocus != null)
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));

                var newFocusedElement = Keyboard.FocusedElement as TextBox;

                if (newFocusedElement != null)
                    newFocusedElement.CaretIndex = newFocusedElement.Text.Length;
                if (e.Key != Key.Back)
                    e.Handled = true;
            }

            if ((e.Key == Key.Right || (e.Key == Key.Decimal && box.Text.Length > 0)) && box.SelectionStart == box.Text.Length && !Equals(box, Box4))
            {
                var elementWithFocus = Keyboard.FocusedElement as UIElement;

                if (elementWithFocus != null)
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                var newFocusedElement = Keyboard.FocusedElement as TextBox;

                if (newFocusedElement != null)
                    newFocusedElement.CaretIndex = 0;
                e.Handled = true;
            }
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //var exp = GetBindingExpression(TextProperty);
            //exp?.UpdateSource();
        }

        private void IpAddressControl_OnLostFocus(object sender, RoutedEventArgs e)
        {
            Text = Box1.Text + '.' + Box2.Text + '.' + Box3.Text + '.' + Box4.Text;
        }

        void TextBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focused, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }
    }
}
