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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YP.Pages
{
    /// <summary>
    /// Логика взаимодействия для Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        public Register()
        {
            InitializeComponent();
        }
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string displayName = txtDisplayName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;
            string passwordConfirm = txtPasswordConfirm.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(passwordConfirm))
            {
                MessageBox.Show("Все поля обязательны для заполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (password != passwordConfirm)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Core.Context.Users.Any(u => u.Login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Core.Context.Users.Any(u => u.Email == email))
            {
                MessageBox.Show("Пользователь с таким email уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var newUser = new Users
            {
                Login = login,
                Password = password,
                DisplayName = displayName,
                Email = email,
                RoleId = 1,
                IsFrozen = false
            };
            Core.Context.Users.Add(newUser);
            Core.Context.SaveChanges();
            MessageBox.Show("Регистрация прошла успешно! Теперь войдите.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.Navigate(new LoginPage());
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
}
