using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EduCenterWPF
{
    public partial class EditApplicationWindow : Window
    {
        private ApplicationModel _application;

        // Конструктор для редактирования существующей заявки
        public EditApplicationWindow(ApplicationModel application)
        {
            InitializeComponent();
            
            // Сохраняем ссылку на редактируемую заявку
            _application = application;
            
            // Заполняем поля формы данными заявки
            LoadApplicationData();
        }

        // Заполнение полей формы данными заявки
        private void LoadApplicationData()
        {
            if (_application != null)
            {
                // Заполняем поля
                txtApplicationID.Text = _application.ApplicationID.ToString();
                txtClientName.Text = _application.ClientName;
                txtComments.Text = _application.Comments;
                
                // Выбираем соответствующие элементы в ComboBox для курса
                SelectComboBoxItem(cmbCourse, _application.Course);
                
                // Выбираем соответствующие элементы в ComboBox для статуса
                SelectComboBoxItem(cmbStatus, _application.Status);
            }
        }
        
        // Вспомогательный метод для выбора элемента в ComboBox
        private void SelectComboBoxItem(ComboBox comboBox, string itemContent)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == itemContent)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
            
            // Если ничего не нашли и нет выбранного элемента, добавим новый и выберем его
            if (comboBox.SelectedItem == null && !string.IsNullOrEmpty(itemContent))
            {
                ComboBoxItem newItem = new ComboBoxItem { Content = itemContent };
                comboBox.Items.Add(newItem);
                comboBox.SelectedItem = newItem;
            }
        }

        // Обработчик нажатия кнопки "Сохранить"
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что все необходимые поля заполнены
            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                MessageBox.Show("Пожалуйста, введите ФИО клиента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (cmbCourse.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите курс.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите статус заявки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Обновляем данные заявки
            string clientName = txtClientName.Text.Trim();
            string course = (cmbCourse.SelectedItem as ComboBoxItem).Content.ToString();
            string status = (cmbStatus.SelectedItem as ComboBoxItem).Content.ToString();
            string comments = txtComments.Text.Trim();
            
            // Сохраняем изменения в заявке
            DatabaseHelper.UpdateApplication(_application.ApplicationID, clientName, course, status, comments);
            
            // Закрываем окно с результатом "OK"
            this.DialogResult = true;
            this.Close();
        }

        // Обработчик нажатия кнопки "Отмена"
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем окно с результатом "Cancel"
            this.DialogResult = false;
            this.Close();
        }
    }
} 