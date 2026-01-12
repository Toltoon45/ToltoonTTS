using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToltoonTTS2.Behaviors
{
    public static class TextBoxSelectAllBehavior
    {
        public static readonly DependencyProperty EnableSelectAllProperty =
            DependencyProperty.RegisterAttached(
                "EnableSelectAll",
                typeof(bool),
                typeof(TextBoxSelectAllBehavior),
                new PropertyMetadata(false, OnEnableSelectAllChanged));

        public static bool GetEnableSelectAll(DependencyObject obj)
            => (bool)obj.GetValue(EnableSelectAllProperty);

        public static void SetEnableSelectAll(DependencyObject obj, bool value)
            => obj.SetValue(EnableSelectAllProperty, value);

        private static void OnEnableSelectAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
                return;

            if ((bool)e.NewValue)
            {
                textBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
                textBox.PreviewMouseDown += TextBox_PreviewMouseDown;
            }
            else
            {
                textBox.GotKeyboardFocus -= TextBox_GotKeyboardFocus;
                textBox.PreviewMouseDown -= TextBox_PreviewMouseDown;
            }
        }

        private static void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.SelectAll();
        }

        private static void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox && !textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus();
            }
        }
    }
}
