using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            DataContext = UserData.CurrentUser; // данные админа в заголовке
            LoadData();
        }
        /// <summary>
        /// загрузка всех разделов админки
        /// </summary>
        private void LoadData()
        {
            LoadComplaints();
            LoadUnfreezeRequests();
            LoadAuthorRequests();
            LoadFrozenObjects();
            LoadUsers();
        }
        /// <summary>
        /// загрузка необработанных жалоб
        /// </summary>
        private void LoadComplaints()
        {
            ListBoxComplaints.ItemsSource = Core.Context.Complaints.Where(x => x.IsConfirmed == null).Include(x => x.Users).Include(x => x.ComplaintReasons).Include(x => x.ComplaintTargetTypes).Include(x => x.Books).ToList();
        }
        /// <summary>
        /// загрузка необработанных заявок на разморозку
        /// </summary>
        private void LoadUnfreezeRequests()
        {
            ListBoxUnfreeze.ItemsSource = Core.Context.UnfreezeApplications.Where(x => x.IsConfirmed == null).Include(x => x.Users).Include(x => x.Books).ToList();
        }
        /// <summary>
        /// загрузка заявок на роль автора
        /// </summary>
        private void LoadAuthorRequests()
        {
            ListBoxAuthorApps.ItemsSource = Core.Context.AuthorApplications.Where(x => x.StatusId == 1).Include(x => x.Users).ToList();
        }
        /// <summary>
        /// загрузка всех замороженных объектов
        /// </summary>
        private void LoadFrozenObjects()
        {
            ListBoxFrozenBooks.ItemsSource = Core.Context.Books.Where(x => x.IsFrozen).Include(x => x.Users).ToList();
            ListBoxFrozenUsers.ItemsSource = Core.Context.Users.Where(x => x.IsFrozen).Include(x => x.Roles).ToList();
            ListBoxFrozenReviews.ItemsSource = Core.Context.Reviews.Where(x => x.IsFrozen).Include(x => x.Users).Include(x => x.Books).ToList();
        }
        /// <summary>
        /// загрузка всех пользователей
        /// </summary>
        private void LoadUsers()
        {
            ListBoxAllUsers.ItemsSource = Core.Context.Users.Include(x => x.Roles).Where(x => x.Roles.RoleName != "Администратор").ToList();
        }
        /// <summary>
        /// принять жалобу и заморозить
        /// </summary>
        private void BtnAcceptComplaint_Click(object sender, RoutedEventArgs e)
        {
            var complaint = (sender as Button).DataContext as Complaints;
            if (complaint == null) return;

            complaint.IsConfirmed = true;

            if (complaint.TargetTypeId == 1) // книга
            {
                var book = Core.Context.Books.FirstOrDefault(x => x.Id == complaint.BookId);
                if (book != null) book.IsFrozen = true;
            }

            if (complaint.TargetTypeId == 3) // автор
            {
                var book = Core.Context.Books.FirstOrDefault(x => x.Id == complaint.BookId);
                if (book != null)
                {
                    var user = Core.Context.Users.FirstOrDefault(x => x.Id == book.AuthorId);
                    if (user != null) user.IsFrozen = true;
                }
            }

            if (complaint.TargetTypeId == 2 && complaint.TargetId != null) // отзыв
            {
                var review = Core.Context.Reviews.FirstOrDefault(x => x.Id == complaint.TargetId);
                if (review != null) review.IsFrozen = true;
            }

            Core.Context.SaveChanges();
            LoadComplaints();
            LoadFrozenObjects();
        }
        /// <summary>
        /// отклонить жалобу
        /// </summary>
        private void BtnRejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            var complaint = (sender as Button).DataContext as Complaints;
            if (complaint == null)
                return;
            complaint.IsConfirmed = false;
            Core.Context.SaveChanges();
            LoadComplaints();
        }
        /// <summary>
        /// принять заявку на разморозку и снять заморозку
        /// </summary>
        private void BtnAcceptUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            var request = (sender as Button).DataContext as UnfreezeApplications;
            if (request == null)
                return;
            request.IsConfirmed = true;
            if (request.BookId != null)
            {
                var book = Core.Context.Books.FirstOrDefault(x => x.Id == request.BookId);
                if (book != null)
                    book.IsFrozen = false;
            }
            else
            {
                var user = Core.Context.Users.FirstOrDefault(x => x.Id == request.UserId);
                if (user != null)
                    user.IsFrozen = false;
            }
            Core.Context.SaveChanges();
            LoadUnfreezeRequests();
            LoadFrozenObjects();
        }
        /// <summary>
        /// отклонить заявку на разморозку
        /// </summary>
        private void BtnRejectUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            var request = (sender as Button).DataContext as UnfreezeApplications;
            if (request == null)
                return;
            request.IsConfirmed = false;
            Core.Context.SaveChanges();
            LoadUnfreezeRequests();
        }
        /// <summary>
        /// одобрить заявку на роль автора
        /// </summary>
        private void BtnAcceptAuthorApp_Click(object sender, RoutedEventArgs e)
        {
            var app = (sender as Button).DataContext as AuthorApplications;
            if (app == null)
                return;
            app.StatusId = 2;
            var user = Core.Context.Users.FirstOrDefault(x => x.Id == app.UserId);
            if (user != null)
                user.RoleId = 2;
            Core.Context.SaveChanges();
            LoadAuthorRequests();
            LoadUsers();
        }
        /// <summary>
        /// отклонить заявку на роль автора
        /// </summary>
        private void BtnRejectAuthorApp_Click(object sender, RoutedEventArgs e)
        {
            var app = (sender as Button).DataContext as AuthorApplications;
            if (app == null)
                return;
            app.StatusId = 3;
            Core.Context.SaveChanges();
            LoadAuthorRequests();
        }
        /// <summary>
        /// загрузка списка ролей
        /// </summary>
        private void CmbRole_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.Items.Count == 0)
            {
                comboBox.Items.Add("Читатель");
                comboBox.Items.Add("Автор");
                comboBox.Items.Add("Администратор");
            }
            Users user = comboBox.DataContext as Users;
            if (user != null)
                comboBox.SelectedItem = user.Roles.RoleName;
        }
        /// <summary>
        /// смена роли пользователя
        /// </summary>
        private void CmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null || comboBox.SelectedItem == null)
                return;
            Users user = comboBox.DataContext as Users;
            if (user == null)
                return;
            string selectedRole = comboBox.SelectedItem.ToString();
            int roleId = 1;
            if (selectedRole == "Автор")
                roleId = 2;
            if (selectedRole == "Администратор")
                roleId = 3;
            if (user.RoleId == roleId)
                return;
            user.RoleId = roleId;
            user.Roles = Core.Context.Roles.FirstOrDefault(x => x.Id == roleId);
            Core.Context.SaveChanges();
            LoadUsers();
        }
        /// <summary>
        /// окно смены пароля для выбранного пользователя
        /// </summary>
        private void BtnOpenPasswordWindow_Click(object sender, RoutedEventArgs e)
        {
            Users user = (sender as Button).DataContext as Users;
            if (user == null)
                return;
            ChangePasswordWindow window = new ChangePasswordWindow(user);
            window.ShowDialog();
        }
        /// <summary>
        /// разморозить книгу напрямую
        /// </summary>
        private void BtnUnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            Books book = (sender as Button).DataContext as Books;
            if (book == null)
                return;
            book.IsFrozen = false;
            Core.Context.SaveChanges();
            LoadFrozenObjects();
        }
        /// <summary>
        /// разморозить пользователя напрямую
        /// </summary>
        private void BtnUnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            Users user = (sender as Button).DataContext as Users;
            if (user == null)
                return;
            user.IsFrozen = false;
            Core.Context.SaveChanges();
            LoadFrozenObjects();
        }
        /// <summary>
        /// разморозить отзыв напрямую
        /// </summary>
        private void BtnUnfreezeReview_Click(object sender, RoutedEventArgs e)
        {
            Reviews review = (sender as Button).DataContext as Reviews;
            if (review == null)
                return;
            review.IsFrozen = false;
            Core.Context.SaveChanges();
            LoadFrozenObjects();
        }
    }
}
