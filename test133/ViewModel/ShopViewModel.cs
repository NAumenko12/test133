using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using test133.Model;
using test133.View;
using System.Data.Entity;

namespace test133.ViewModel
{
    internal class ShopViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Товар> _products;
        private bool _isUserAuthenticated;
        private string _loginText;
        private readonly DataBase dataBase = new DataBase();

        public bool IsUserAuthenticated
        {
            get => _isUserAuthenticated;
            set
            {
                _isUserAuthenticated = value;
                OnPropertyChanged(nameof(IsUserAuthenticated));
            }
        }

        public string LoginText
        {
            get => _loginText;
            set
            {
                _loginText = value;
                OnPropertyChanged(nameof(LoginText));
            }
        }

        public ICommand NavigateProfileCommand { get; }
        public ICommand NavigateFavoritesCommand { get; }
        public ICommand GoHomeNavigateCommand { get; }
        public ICommand AddToFavoritesCommand { get; }


        public ObservableCollection<Товар> Products
        {
            get { return _products; }
            set
            {
                if (_products != value)
                {
                    _products = value;
                    OnPropertyChanged(nameof(Products));
                }
            }
        }

        public ShopViewModel(string loginText)
        {
            _loginText = loginText;
            Products = new ObservableCollection<Товар>();
            LoadProducts();

            NavigateProfileCommand = new RelayCommand(NavigateProfile);
            NavigateFavoritesCommand = new RelayCommand(NavigateFavorites);
            GoHomeNavigateCommand = new RelayCommand(NavigateHome);
            AddToFavoritesCommand = new RelayCommand(AddToFavorites);
        }

        private void NavigateProfile(object obj)
        {
            if (IsUserAuthenticated)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // Используем свойство LoginText для передачи логина
                    string login = LoginText;
                    mainWindow.MainContent.Content = new ProfileWindow(login);
                }
            }
            else
            {
                MessageBox.Show("Для доступа к профилю необходимо зарегистрироваться или войти в аккаунт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NavigateFavorites(object obj)
        {
            if (IsUserAuthenticated)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    string login = LoginText;
                    mainWindow.MainContent.Content = new FavoriteWindow(login);
                }
            }
            else
            {
                MessageBox.Show("Для доступа к избранным товарам необходимо зарегистрироваться или войти в аккаунт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NavigateHome(object obj)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContent.Content = new LoginWindow();
            }
        }
        private void AddToFavorites(object obj)
        {
            if (obj is Товар product)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(dataBase.connectionString))
                    {
                        connection.Open();
                        string query = @"
                            INSERT INTO Избранные_товары (Клиент, Товар)
                            SELECT 
                                k.Id_Клиент, 
                                @Товар
                            FROM 
                                Клиент k
                            WHERE 
                                k.Логин = @Логин";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Товар", product.Id_Товар);
                            command.Parameters.AddWithValue("@Логин", LoginText);
                            command.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Товар добавлен в избранное", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении товара в избранное: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadProducts()
        {
            DataBase myDbContext = new DataBase();

            try
            {
                using (SqlConnection connection = new SqlConnection(myDbContext.connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            t.Id_Товар, 
                            t.Артикул, 
                            t.Название, 
                            t.Категория, 
                            t.Бренд, 
                            t.Животное, 
                            t.Описание, 
                            t.Состав, 
                            t.Колличество_за_ед, 
                            t.Единица, 
                            t.Стоимость, 
                            c.Название AS НазваниеКатегории 
                        FROM 
                            Товар t
                        INNER JOIN 
                            Категории c ON t.Категория = c.Id_Категории";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            ObservableCollection<Товар> products = new ObservableCollection<Товар>();

                            while (reader.Read())
                            {
                                Товар product = new Товар
                                {
                                    Id_Товар = reader.GetInt32(0),
                                    Артикул = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    Название = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    Категория = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                                    Бренд = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                    Животное = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                    Описание = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                    Состав = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                    Колличество_за_ед = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                                    Единица = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                                    Стоимость = reader.IsDBNull(10) ? (decimal?)null : reader.GetDecimal(10)
                                };

                                // Добавляем название категории в объект Категории
                                product.Категории = new Категории
                                {
                                    Id_Категории = product.Категория ?? 0,
                                    Название = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
                                };

                                products.Add(product);
                            }

                            Products = products;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке товаров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
