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
using System.Windows.Shapes;

namespace EduCenterWPF
{
    public partial class ClientDashboard : Window
    {
        private int _clientId = 1;  // Идентификатор клиента, например, 1

        public ClientDashboard(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
            
            // Получаем имя пользователя с помощью метода GetUserInfo
            txtClientName.Text = DatabaseHelper.GetUserInfo(clientId);
            
            // Загрузим заявки клиента напрямую из DatabaseHelper
            LoadClientApplications();
        }

        // Загрузка заявок клиента
        private void LoadClientApplications()
        {
            // Получаем все заявки
            var allApplications = DatabaseHelper.GetApplications();
            
            // Фильтруем заявки для текущего клиента (в реальности это должно делаться на стороне БД)
            // В нашем случае мы не можем фильтровать по ID, так как у нас нет этого поля в ApplicationModel
            // и мы показываем все заявки
            dgClientApplications.ItemsSource = allApplications;
        }

        // Обработчик нажатия кнопки "Подать новую заявку"
        private void btnNewApplication_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно подачи заявки, передавая Id клиента
            var applicationWindow = new ApplicationWindow(_clientId);
            applicationWindow.ShowDialog();
            // После закрытия окна можно обновить список заявок
            LoadClientApplications();
        }

        // Обработчик кнопки "Выйти"
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Например, возвращаемся на окно входа
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}