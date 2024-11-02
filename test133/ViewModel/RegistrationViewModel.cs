using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using test133.View;

namespace test133.ViewModel
{
    public class RegistrationViewModel : INotifyPropertyChanged
    {
        private readonly DataBase dataBase = new DataBase();

        public string LoginText { get; set; }
        public string PasswordText { get; set; }
        public string NameText { get; set; }
        public string MobileText { get; set; }
        public string AdressText { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand RegisterCommand { get; set; }
        public ICommand NavigateToLoginCommand { get; set; }

        public RegistrationViewModel()
        {
            RegisterCommand = new RelayCommand(Register);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
        }

        private void Register(object obj)
        {
            if (string.IsNullOrWhiteSpace(NameText) || string.IsNullOrWhiteSpace(MobileText) ||
                string.IsNullOrWhiteSpace(AdressText) || string.IsNullOrWhiteSpace(LoginText) ||
                string.IsNullOrWhiteSpace(PasswordText))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка наличия аккаунта в базе данных
            if (dataBase.SqlSelect("select * from [dbo].[Клиент] where [Логин] = '" + LoginText + "'").Rows.Count > 0)
            {
                MessageBox.Show("Аккаунт с таким логином уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Вставка данных в базу данных
            string insertQuery = $"insert into [dbo].[Клиент] (Имя, Телефон, Адрес, Логин, Пароль) values ('{NameText}', '{MobileText}', '{AdressText}', '{LoginText}', '{PasswordText}')";
            dataBase.SqlInsert(insertQuery);

            MessageBox.Show("Регистрация прошла успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            // Переход на страницу ShopWindow
            NavigateToShop();
        }

        private void NavigateToShop()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContent.Content = new ShopWindow(LoginText);
            }
        }
        private void NavigateToLogin(object obj)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContent.Content = new LoginWindow();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
