using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using test133.Model;
using test133.View;

namespace test133.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly DataBase dataBase = new DataBase();
        private string _loginText;
        private string _passwordText;

        public event PropertyChangedEventHandler PropertyChanged;

        public string LoginText
        {
            get => _loginText;
            set
            {
                _loginText = value;
                OnPropertyChanged(nameof(LoginText));
            }
        }

        public string PasswordText
        {
            get => _passwordText;
            set
            {
                _passwordText = value;
                OnPropertyChanged(nameof(PasswordText));
            }
        }

        public ICommand AuthorizeCommand { get; }
        public ICommand NavigateToRegistrationCommand { get; }
        public ICommand NavigateToShopCommand { get; }
        public ICommand NavigateToShopWithoutAuthCommand { get; }

        public LoginViewModel()
        {
            AuthorizeCommand = new RelayCommand(Authorize);
            NavigateToRegistrationCommand = new RelayCommand(NavigateToRegistration);
            NavigateToShopCommand = new RelayCommand(obj => NavigateToShop(obj, LoginText));
            NavigateToShopWithoutAuthCommand = new RelayCommand(NavigateToShopWithoutAuth);
        }

        private void Authorize(object obj)
        {
            if (string.IsNullOrWhiteSpace(LoginText) || string.IsNullOrWhiteSpace(PasswordText))
            {
                MessageBox.Show("Вы не ввели логин или пароль", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (dataBase.SqlSelect("select * from [dbo].[Клиент] where [Логин] = '" + LoginText + "' and [Пароль] = '" + PasswordText + "'").Rows.Count > 0)
            {
                MessageBox.Show("Пользователь", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigateToShop(true, LoginText); // Передаем true и логин
            }
            else
            {
                MessageBox.Show("Данные введены неккоректно", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NavigateToRegistration(object obj)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContent.Content = new RegistrationWindow();
            }
        }

        private void NavigateToShop(object obj, string loginText)
        {
            var isAuthenticated = (bool)obj;
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                var shopViewModel = new ShopViewModel(loginText);
                shopViewModel.IsUserAuthenticated = isAuthenticated;
                mainWindow.MainContent.Content = new ShopWindow(loginText) { DataContext = shopViewModel };
            }
        }

        private void NavigateToShopWithoutAuth(object obj)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                var shopViewModel = new ShopViewModel(string.Empty);
                shopViewModel.IsUserAuthenticated = false;
                mainWindow.MainContent.Content = new ShopWindow(string.Empty) { DataContext = shopViewModel };
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
