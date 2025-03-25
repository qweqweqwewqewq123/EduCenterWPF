using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
using Dapper;
using MySqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using System.Security.Cryptography;
using System.Configuration;

namespace EduCenterWPF
{
    public class DatabaseHelper
    {
        // Получение строки подключения из конфигурации
        private static readonly string connectionString = 
            "Server=localhost;Database=educenterdb;User ID=root;Password=qwe135;Allow User Variables=True;SslMode=None;AllowPublicKeyRetrieval=true;DefaultAuthenticationPlugin=mysql_native_password;";
        
        // Флаг, указывающий на использование фиктивных данных
        private static bool useMockData = false;
        
        // Используем фиктивные данные вместо подключения к БД
        private static List<ApplicationModel> mockApplications = new List<ApplicationModel>
        {
            new ApplicationModel
            {
                ApplicationID = 1,
                ClientName = "Иванов Иван",
                Course = "Программирование C#",
                SubmissionDate = DateTime.Now.AddDays(-5),
                Status = "На рассмотрении",
                Comments = "Хочу научиться программировать"
            },
            new ApplicationModel
            {
                ApplicationID = 2,
                ClientName = "Петров Петр",
                Course = "Веб-дизайн",
                SubmissionDate = DateTime.Now.AddDays(-3),
                Status = "Подтверждена",
                Comments = "Есть опыт в HTML и CSS"
            },
            new ApplicationModel
            {
                ApplicationID = 3,
                ClientName = "Сидорова Анна",
                Course = "Английский язык",
                SubmissionDate = DateTime.Now.AddDays(-1),
                Status = "Отклонена",
                Comments = "Для работы за границей"
            }
        };
        
        private static int nextApplicationId = 4;
        
        // Проверка соединения с базой данных при загрузке приложения
        static DatabaseHelper()
        {
            // По умолчанию используем мок-данные
            useMockData = false;
            
            try
            {
                // Пробуем подключиться к БД
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // Проверяем, что таблицы существуют и доступны
                    string testQuery = "SELECT COUNT(*) FROM Applications";
                    var cmd = new MySqlCommand(testQuery, connection);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    // Успешное подключение, используем базу данных
                    useMockData = false;
                }
            }
            catch (MySqlException ex)
            {
                // Ошибка связана с MySQL
                string errorMessage = $"Ошибка подключения к базе данных MySQL:\n{ex.Message}";
                
                if (ex.Message.Contains("Unknown database"))
                {
                    errorMessage += "\n\nВозможно, база данных не создана. Необходимо создать базу educenterdb.";
                }
                else if (ex.Message.Contains("Access denied"))
                {
                    errorMessage += "\n\nНеверные учетные данные. Проверьте имя пользователя и пароль MySQL.";
                }
                else if (ex.Message.Contains("Unable to connect"))
                {
                    errorMessage += "\n\nНе удалось подключиться к серверу MySQL. Убедитесь, что сервер запущен.";
                }
                
                errorMessage += "\n\nПриложение будет работать с тестовыми данными.";
                
                MessageBox.Show(errorMessage, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                useMockData = true;
            }
            catch (Exception ex)
            {
                // Другие ошибки
                MessageBox.Show($"Ошибка при инициализации: {ex.Message}\n\nПриложение будет работать с тестовыми данными.", 
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                useMockData = true;
            }
        }
        
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        
        // Регистрация пользователя
        public static bool RegisterUser(string username, string password)
        {
            if (useMockData)
            {
                // Фиктивная реализация
                MessageBox.Show($"Регистрация пользователя '{username}' успешно выполнена! (тестовый режим)", 
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            
            try
            {
                string hashedPassword = HashPassword(password);
                
                using (IDbConnection db = new MySqlConnection(connectionString))
                {
                    // Сначала проверим соединение
                    db.Open();
                    
                    // Затем проверим, существует ли уже пользователь с таким именем
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                    int userExists = db.ExecuteScalar<int>(checkQuery, new { username });
                    
                    if (userExists > 0)
                    {
                        MessageBox.Show($"Пользователь с именем '{username}' уже существует.", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    
                    // Если пользователя нет, добавляем его
                    string insertQuery = "INSERT INTO users (username, password_hash) VALUES (@username, @password_hash)";
                    db.Execute(insertQuery, new { username, password_hash = hashedPassword });
                    
                    MessageBox.Show($"Пользователь '{username}' успешно зарегистрирован!", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                string errorMessage = $"Ошибка при регистрации: {ex.Message}";
                
                // Добавляем дополнительную информацию в зависимости от кода ошибки
                switch (ex.Number)
                {
                    case 1042: // Unable to connect to server
                        errorMessage += "\n\nНе удалось подключиться к серверу MySQL. Проверьте, запущен ли сервер.";
                        break;
                    case 1045: // Access denied for user
                        errorMessage += "\n\nДоступ запрещен. Проверьте имя пользователя и пароль MySQL.";
                        break;
                    case 1049: // Unknown database
                        errorMessage += "\n\nБаза данных не существует. Необходимо создать базу данных 'educenterdb'.";
                        break;
                    case 1054: // Unknown column
                        errorMessage += "\n\nНеизвестное поле в таблице. Возможно, структура базы данных некорректна.";
                        break;
                    case 1146: // Table doesn't exist
                        errorMessage += "\n\nТаблица не существует. Запустите SQL-скрипт для создания базы данных.";
                        break;
                }
                
                MessageBox.Show(errorMessage, "Ошибка MySQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка при регистрации: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Авторизация пользователя
        public static (bool isAuthenticated, bool isAdmin, int userId) AuthenticateUser(string username, string password)
        {
            if (useMockData)
            {
                // Фиктивная реализация
                if (username == "admin" && password == "password")
                    return (true, true, 1);  // Администратор
                else if (username == "user" && password == "password" || username == "testuser" && password == "password")
                    return (true, false, 2); // Обычный пользователь
                else
                    return (false, false, 0); // Аутентификация не удалась
            }
            
            try
            {
                string hashedPassword = HashPassword(password);
                
                using (IDbConnection db = new MySqlConnection(connectionString))
                {
                    string query = @"
                        SELECT user_id, is_admin 
                        FROM users 
                        WHERE username = @username AND password_hash = @password_hash";
                        
                    var user = db.QueryFirstOrDefault<dynamic>(query, new { username, password_hash = hashedPassword });
                    
                    if (user != null)
                    {
                        return (true, user.is_admin, user.user_id);
                    }
                    
                    return (false, false, 0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при аутентификации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // В случае ошибки, используем фиктивную аутентификацию
                if (username == "admin" && password == "password")
                    return (true, true, 1);
                else if (username == "user" && password == "password" || username == "testuser" && password == "password")
                    return (true, false, 2);
                    
                return (false, false, 0);
            }
        }

        // Получение всех заявок
        public static List<ApplicationModel> GetApplications()
        {
            if (useMockData)
            {
                // Возвращаем фиктивные данные
                return mockApplications;
            }
            
            try
            {
                using (IDbConnection db = new MySqlConnection(connectionString))
                {
                    string query = @"
                    SELECT a.ApplicationID, c.FullName AS ClientName, a.Course, 
                           a.SubmissionDate, a.Status, a.Comments 
                    FROM Applications a 
                    JOIN Clients c ON a.ClientID = c.ClientID";
                    
                    return db.Query<ApplicationModel>(query).AsList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении заявок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return mockApplications; // В случае ошибки возвращаем фиктивные данные
            }
        }

        // Добавление новой заявки (для обратной совместимости)
        public static void AddApplication(int clientId, string course, string comments)
        {
            // Вызываем новый метод с фиктивным именем клиента
            AddApplicationWithClientName(clientId, "Клиент #" + clientId, course, comments);
        }

        // Добавление новой заявки с указанием ФИО клиента
        public static void AddApplicationWithClientName(int clientId, string clientName, string course, string comments)
        {
            if (useMockData)
            {
                // Добавляем заявку в список фиктивных данных с указанием ФИО
                mockApplications.Add(new ApplicationModel
                {
                    ApplicationID = nextApplicationId++,
                    ClientName = clientName,  // Используем переданное ФИО
                    Course = course,
                    SubmissionDate = DateTime.Now,
                    Status = "На рассмотрении",
                    Comments = comments
                });
                return;
            }
            
            try
            {
                using (IDbConnection db = new MySqlConnection(connectionString))
                {
                    // Сперва проверяем, есть ли клиент с таким ID
                    string checkClientQuery = "SELECT COUNT(*) FROM Clients WHERE ClientID = @ClientID";
                    int clientExists = db.ExecuteScalar<int>(checkClientQuery, new { ClientID = clientId });
                    
                    // Если клиента нет, создаем его
                    if (clientExists == 0)
                    {
                        string createClientQuery = @"
                        INSERT INTO Clients (ClientID, FullName) 
                        VALUES (@ClientID, @FullName)";
                        
                        db.Execute(createClientQuery, new { ClientID = clientId, FullName = clientName });
                    }
                    else
                    {
                        // Если клиент существует, обновляем его имя
                        string updateClientQuery = @"
                        UPDATE Clients SET FullName = @FullName 
                        WHERE ClientID = @ClientID";
                        
                        db.Execute(updateClientQuery, new { ClientID = clientId, FullName = clientName });
                    }
                    
                    // Затем создаем заявку
                    string insertAppQuery = @"
                    INSERT INTO Applications (ClientID, Course, SubmissionDate, Status, Comments) 
                    VALUES (@ClientID, @Course, NOW(), 'На рассмотрении', @Comments)";
                    
                    db.Execute(insertAppQuery, new { ClientID = clientId, Course = course, Comments = comments });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Добавляем в список фиктивных данных в случае ошибки
                mockApplications.Add(new ApplicationModel
                {
                    ApplicationID = nextApplicationId++,
                    ClientName = clientName,
                    Course = course,
                    SubmissionDate = DateTime.Now,
                    Status = "На рассмотрении",
                    Comments = comments
                });
            }
        }

        // Обновление статуса заявки
        public static void UpdateApplicationStatus(int applicationId, string status)
        {
            if (useMockData)
            {
                // Находим заявку и обновляем её статус
                var application = mockApplications.FirstOrDefault(a => a.ApplicationID == applicationId);
                if (application != null)
                {
                    application.Status = status;
                }
                return;
            }
            
            try
            {
                using (IDbConnection db = new MySqlConnection(connectionString))
                {
                    string query = "UPDATE Applications SET Status = @Status WHERE ApplicationID = @ApplicationID";
                    db.Execute(query, new { ApplicationID = applicationId, Status = status });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Пробуем обновить в фиктивных данных в случае ошибки
                var application = mockApplications.FirstOrDefault(a => a.ApplicationID == applicationId);
                if (application != null)
                {
                    application.Status = status;
                }
            }
        }

        // Удаление заявки
        public static void DeleteApplication(int applicationId)
        {
            if (useMockData)
            {
                // Удаляем заявку из фиктивных данных
                mockApplications.RemoveAll(a => a.ApplicationID == applicationId);
                return;
            }
            
            try
            {
                using (IDbConnection db = new MySqlConnection(connectionString))
                {
                    string query = "DELETE FROM Applications WHERE ApplicationID = @ApplicationID";
                    db.Execute(query, new { ApplicationID = applicationId });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Удаляем из фиктивных данных в случае ошибки
                mockApplications.RemoveAll(a => a.ApplicationID == applicationId);
            }
        }

        // Получение информации о пользователе по ID
        public static string GetUserInfo(int userId)
        {
            if (useMockData)
            {
                // Для тестовых данных возвращаем заглушку
                if (userId == 1) return "Администратор";
                if (userId == 2) return "Тестовый Пользователь";
                return "Пользователь #" + userId;
            }
            
            try
            {
                using (IDbConnection db = new MySqlConnection(connectionString))
                {
                    // Проверяем, есть ли для этого пользователя запись в таблице Clients
                    string query = @"
                        SELECT c.FullName 
                        FROM Clients c
                        WHERE c.user_id = @userId";
                        
                    string fullName = db.QueryFirstOrDefault<string>(query, new { userId });
                    
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        return fullName;
                    }
                    
                    // Если в таблице Clients нет информации, получаем хотя бы имя пользователя из users
                    string usernameQuery = "SELECT username FROM users WHERE user_id = @userId";
                    string username = db.QueryFirstOrDefault<string>(usernameQuery, new { userId });
                    
                    if (!string.IsNullOrEmpty(username))
                    {
                        return username;
                    }
                    
                    return "Пользователь #" + userId;
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем идентификатор пользователя
                return "Пользователь #" + userId;
            }
        }

        // Обновление всех данных заявки
        public static void UpdateApplication(int applicationId, string clientName, string course, string status, string comments)
        {
            if (useMockData)
            {
                // Находим заявку и обновляем её данные
                var application = mockApplications.FirstOrDefault(a => a.ApplicationID == applicationId);
                if (application != null)
                {
                    application.ClientName = clientName;
                    application.Course = course;
                    application.Status = status;
                    application.Comments = comments;
                }
                return;
            }
            
            try
            {
                using (IDbConnection db = new MySqlConnection(connectionString))
                {
                    // Сначала получаем ClientID для этой заявки
                    string getClientIdQuery = "SELECT ClientID FROM Applications WHERE ApplicationID = @ApplicationID";
                    int clientId = db.ExecuteScalar<int>(getClientIdQuery, new { ApplicationID = applicationId });
                    
                    // Обновляем имя клиента
                    string updateClientQuery = "UPDATE Clients SET FullName = @FullName WHERE ClientID = @ClientID";
                    db.Execute(updateClientQuery, new { ClientID = clientId, FullName = clientName });
                    
                    // Обновляем заявку
                    string updateAppQuery = @"
                    UPDATE Applications 
                    SET Course = @Course, Status = @Status, Comments = @Comments 
                    WHERE ApplicationID = @ApplicationID";
                    
                    db.Execute(updateAppQuery, new { 
                        ApplicationID = applicationId,
                        Course = course,
                        Status = status,
                        Comments = comments
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Обновляем в фиктивных данных в случае ошибки
                var application = mockApplications.FirstOrDefault(a => a.ApplicationID == applicationId);
                if (application != null)
                {
                    application.ClientName = clientName;
                    application.Course = course;
                    application.Status = status;
                    application.Comments = comments;
                }
            }
        }
    }

    // Модель данных для заявки
    public class ApplicationModel
    {
        public int ApplicationID { get; set; }  // Идентификатор заявки
        public string ClientName { get; set; }   // Имя клиента
        public string Course { get; set; }       // Курс
        public DateTime SubmissionDate { get; set; }  // Дата подачи заявки
        public string Status { get; set; }      // Статус заявки
        public string Comments { get; set; }    // Комментарии
    }
}