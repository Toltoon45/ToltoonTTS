using System.Windows.Controls;
using System.Windows;

namespace ToltoonTTS2.Services
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached(
                "Password",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        // Свойство для отслеживания обновлений, чтобы избежать рекурсии
        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdating",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false));

        public static string GetPassword(DependencyObject obj) =>
            (string)obj.GetValue(PasswordProperty);

        public static void SetPassword(DependencyObject obj, string value) =>
            obj.SetValue(PasswordProperty, value);

        private static bool GetIsUpdating(DependencyObject obj) =>
            (bool)obj.GetValue(IsUpdatingProperty);

        private static void SetIsUpdating(DependencyObject obj, bool value) =>
            obj.SetValue(IsUpdatingProperty, value);

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                // Отписываемся от события, чтобы избежать рекурсии
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

                // Обновляем только если мы не в процессе обновления
                if (!GetIsUpdating(passwordBox))
                {
                    passwordBox.Password = (string)e.NewValue ?? string.Empty;
                }

                // Подписываемся обратно на событие
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                // Устанавливаем флаг, что мы обновляем значение
                SetIsUpdating(passwordBox, true);
                
                // Обновляем привязанное свойство
                SetPassword(passwordBox, passwordBox.Password);
                
                // Снимаем флаг обновления
                SetIsUpdating(passwordBox, false);
            }
        }
    }
}