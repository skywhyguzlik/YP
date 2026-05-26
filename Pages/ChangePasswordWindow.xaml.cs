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

namespace YP.Pages
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        private Users _user;
        public ChangePasswordWindow(Users user)
        {
            InitializeComponent();
            _user = user;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string password1 = PasswordBoxNew.Password.Trim();
            string password2 = PasswordBoxRepeat.Password.Trim();
            if (string.IsNullOrWhiteSpace(password1))
            {
                MessageBox.Show("Введите пароль.");
                return;
            }
            if (password1 != password2)
            {
                MessageBox.Show("Пароли не совпадают.");
                return;
            }
            _user.Password = password1;
            Core.Context.SaveChanges();
            MessageBox.Show("Пароль успешно изменён.");
            Close();
        }
    }
}