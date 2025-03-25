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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EduCenterWPF
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // Обработка нажатия кнопки "Войти"
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim(); // для PasswordBox используем свойство Password

            // Аутентификация пользователя
            var (isAuthenticated, isAdmin, userId) = DatabaseHelper.AuthenticateUser(username, password);

            if (isAuthenticated)
            {
                MessageBox.Show("Вход выполнен успешно!");
                
                if (isAdmin)
                {
                    // Открываем админ-панель
                    AdminPanelWindow adminWindow = new AdminPanelWindow();
                    adminWindow.Show();
                }
                else
                {
                    // Открываем дашборд клиента, передаем ID пользователя
                    ClientDashboard clientDashboard = new ClientDashboard(userId);
                    clientDashboard.Show();
                }
                
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработка клика по ссылке "Зарегистрироваться"
        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow regWindow = new RegistrationWindow();
            regWindow.Show();
            this.Close(); // Или Hide(), если хотите оставить окно в памяти
        }
    }
}
