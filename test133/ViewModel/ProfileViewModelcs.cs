using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using test133.Model;
using test133.View;

namespace test133.ViewModel
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly DataBase dataBase = new DataBase();
        private Клиент _user;

        public Клиент User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged(nameof(User));
            }
        }

        public ICommand SaveChangesCommand { get; }
        public ICommand NavigateToShopCommand { get; }

        public ProfileViewModel(string login)
        {
            LoadUserProfile(login);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            NavigateToShopCommand = new RelayCommand(obj => NavigateToShop(login));
        }

        private void LoadUserProfile(string login)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dataBase.connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            Id_Клиент, 
                            Имя, 
                            Телефон, 
                            Адрес, 
                            Логин, 
                            Пароль 
                        FROM 
                            Клиент 
                        WHERE 
                            Логин = @Login";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", login);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                User = new Клиент
                                {
                                    Id_Клиент = reader.GetInt32(0),
                                    Имя = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    Телефон = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    Адрес = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                    Логин = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                    Пароль = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке профиля пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveChanges(object obj)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dataBase.connectionString))
                {
                    connection.Open();
                    string query = @"
                        UPDATE 
                            Клиент 
                        SET 
                            Имя = @Имя, 
                            Телефон = @Телефон, 
                            Адрес = @Адрес, 
                            Пароль = @Пароль 
                        WHERE 
                            Логин = @Логин";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Имя", User.Имя);
                        command.Parameters.AddWithValue("@Телефон", User.Телефон);
                        command.Parameters.AddWithValue("@Адрес", User.Адрес);
                        command.Parameters.AddWithValue("@Пароль", User.Пароль);
                        command.Parameters.AddWithValue("@Логин", User.Логин);
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Изменения сохранены", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToShop(string login)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContent.Content = new ShopWindow(login);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
