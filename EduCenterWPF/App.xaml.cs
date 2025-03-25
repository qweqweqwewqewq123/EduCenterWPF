using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace EduCenterWPF
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Настройка стиля для MessageBox
            MessageBoxHelper.ApplyDarkThemeToMessageBoxes();
        }
    }
    
    // Класс для стилизации MessageBox
    public static class MessageBoxHelper
    {
        public static void ApplyDarkThemeToMessageBoxes()
        {
            // Переопределение стиля для всех MessageBox
            // Используем глобальный обработчик событий для MessageBox
            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
        }
        
        private static void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Проверяем, является ли окно MessageBox
            Window window = sender as Window;
            if (window != null && window.GetType().Name.Contains("MessageBox"))
            {
                // Применяем темный стиль к MessageBox
                window.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                window.Foreground = new SolidColorBrush(Colors.White);
                
                // Находим все текстовые элементы и кнопки
                ApplyDarkThemeToVisualTree(window);
            }
        }
        
        private static void ApplyDarkThemeToVisualTree(DependencyObject root)
        {
            // Применяем стиль рекурсивно ко всем элементам
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                
                if (child is TextBlock textBlock)
                {
                    textBlock.Foreground = new SolidColorBrush(Colors.White);
                }
                else if (child is Button button)
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204));
                    button.Foreground = new SolidColorBrush(Colors.White);
                    button.BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70));
                }
                
                ApplyDarkThemeToVisualTree(child);
            }
        }
    }
}
