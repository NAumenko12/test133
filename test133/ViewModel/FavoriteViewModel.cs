using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using test133.Model;
using test133.View;

namespace test133.ViewModel
{
    public class FavoriteViewModel : INotifyPropertyChanged
    {
        private readonly DataBase dataBase = new DataBase();
        private ObservableCollection<Товар> _favorites;

        public ObservableCollection<Товар> Favorites
        {
            get => _favorites;
            set
            {
                _favorites = value;
                OnPropertyChanged(nameof(Favorites));
            }
        }

        public ICommand NavigateToShopCommand { get; }

        public FavoriteViewModel(string login)
        {
            LoadFavorites(login);
            NavigateToShopCommand = new RelayCommand(obj => NavigateToShop(login));
        }

        private void LoadFavorites(string login)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dataBase.connectionString))
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
                            Категории c ON t.Категория = c.Id_Категории
                        INNER JOIN 
                            Избранные_товары f ON t.Id_Товар = f.Товар
                        INNER JOIN 
                            Клиент k ON f.Клиент = k.Id_Клиент
                        WHERE 
                            k.Логин = @Login";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", login);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            ObservableCollection<Товар> favorites = new ObservableCollection<Товар>();

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

                                favorites.Add(product);
                            }

                            Favorites = favorites;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке избранных товаров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
