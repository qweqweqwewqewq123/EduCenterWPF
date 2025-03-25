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
    // Окно заявки
    public partial class ApplicationWindow : Window
    {
        private int _clientId;

        public ApplicationWindow(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
            // Если нужно, можно загрузить данные клиента и установить их в форму
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Получаем ФИО клиента
            string clientName = txtClientName.Text.Trim();
            
            // Проверяем, что введено ФИО
            if (string.IsNullOrEmpty(clientName))
            {
                MessageBox.Show("Пожалуйста, введите ФИО клиента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Проверяем, что курс выбран
            if (cmbCourses.SelectedItem is ComboBoxItem selectedCourse)
            {
                string course = selectedCourse.Content.ToString();
                string comments = txtComments.Text.Trim();

                if (string.IsNullOrEmpty(course))
                {
                    MessageBox.Show("Пожалуйста, выберите курс.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Модифицированный метод для передачи ФИО клиента
                DatabaseHelper.AddApplicationWithClientName(_clientId, clientName, course, comments);

                MessageBox.Show($"Заявка для клиента \"{clientName}\" на курс \"{course}\" успешно отправлена!", 
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                txtClientName.Clear();      // Очистка поля ФИО
                cmbCourses.SelectedIndex = -1;  // Сброс выбранного элемента в ComboBox
                txtComments.Clear();  // Очистка комментариев
                this.Close();  // Закрываем окно после отправки заявки
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите курс.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}