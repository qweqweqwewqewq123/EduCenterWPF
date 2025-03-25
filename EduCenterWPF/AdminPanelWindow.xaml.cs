using System.Collections.Generic;
using System.Windows;
using System;
using System.Linq;


namespace EduCenterWPF
{
    public partial class AdminPanelWindow : Window
    {
        private List<EduCenterWPF.ApplicationModel> applications;

        public AdminPanelWindow()
        {
            InitializeComponent();
            LoadApplications();
        }

        // Загрузка заявок из MySQL
        private void LoadApplications()
        {
            applications = DatabaseHelper.GetApplications();
            dgApplications.ItemsSource = applications;
        }

        // Подтверждение заявки
        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (dgApplications.SelectedItem is ApplicationModel selectedApplication)
            {
                DatabaseHelper.UpdateApplicationStatus(selectedApplication.ApplicationID, "Подтверждена");
                LoadApplications();
                MessageBox.Show($"Заявка {selectedApplication.ApplicationID} подтверждена.");
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для подтверждения.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Отклонение заявки
        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            if (dgApplications.SelectedItem is ApplicationModel selectedApplication)
            {
                DatabaseHelper.UpdateApplicationStatus(selectedApplication.ApplicationID, "Отклонена");
                LoadApplications();
                MessageBox.Show($"Заявка {selectedApplication.ApplicationID} отклонена.");
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для отклонения.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Удаление заявки
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgApplications.SelectedItem is ApplicationModel selectedApplication)
            {
                var result = MessageBox.Show($"Удалить заявку {selectedApplication.ApplicationID}?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    DatabaseHelper.DeleteApplication(selectedApplication.ApplicationID);
                    LoadApplications();
                    MessageBox.Show("Заявка успешно удалена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        // Редактирование заявки
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgApplications.SelectedItem is ApplicationModel selectedApplication)
            {
                // Создаем окно редактирования и передаем выбранную заявку
                EditApplicationWindow editWindow = new EditApplicationWindow(selectedApplication);
                
                // Показываем окно как диалог (модальное окно)
                bool? result = editWindow.ShowDialog();
                
                // Если пользователь нажал "Сохранить" (OK), обновляем список заявок
                if (result == true)
                {
                    LoadApplications();
                    MessageBox.Show($"Заявка {selectedApplication.ApplicationID} успешно обновлена.", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для редактирования.", 
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // Обработчик кнопки "Найти"
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = txtSearch.Text;  // Получаем значение из поля поиска
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Ищем среди уже загруженных заявок
                var searchResults = applications
                    .Where(a => (a.ClientName != null && a.ClientName.ToLower().Contains(searchTerm.ToLower())) || 
                               (a.Course != null && a.Course.ToLower().Contains(searchTerm.ToLower())) ||
                               (a.Status != null && a.Status.ToLower().Contains(searchTerm.ToLower())))
                    .ToList();

                // Привязать результаты поиска к DataGrid
                dgApplications.ItemsSource = searchResults;
            }
            else
            {
                // Если поле поиска пустое, просто показываем все заявки
                dgApplications.ItemsSource = applications;
            }
        }
    }
    
    // Класс Client для устранения ошибки CS0246
    public class Client
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }
    
    public class CourseApplication  // или Application
    {
        public int Id { get; set; }
        public string Course { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; }
        // Другие свойства заявки

        // Навигационное свойство для клиента
        public Client Client { get; set; }

        // Если требуется явное указание внешнего ключа:
        public int ClientId { get; set; }
    }
}