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
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            Loaded += ProfilePage_Loaded;
        }
        /// <summary>
        /// заполнение данных после загрузки страницы
        /// </summary>
        private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            var user = UserData.CurrentUser;

            BtnChangePassword.Visibility = UserData.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnAuthorRequest.Visibility = UserData.IsReader ? Visibility.Visible : Visibility.Collapsed;

            if (UserData.IsFrozen)
            {
                bool hasPendingUnfreeze = Core.Context.UnfreezeApplications.Any(u => u.UserId == user.Id && u.IsConfirmed == null);

                if (hasPendingUnfreeze)
                {
                    MessageBox.Show("Ваша заявка на разморозку уже отправлена и ожидает рассмотрения.",
                                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    var mainWindows = Application.Current.MainWindow as MainWindow;
                    mainWindows?.MainFrame.Navigate(new MainPage());
                    return;
                }
                string reason = "Причина не указана";
                // ищем последнюю подтверждённую жалобу на автора
                var lastComplaint = Core.Context.Complaints.Where(c => c.TargetTypeId == 3 && c.IsConfirmed == true && c.Books.AuthorId == user.Id).OrderByDescending(c => c.CreatedAt).FirstOrDefault();

                if (lastComplaint != null && lastComplaint.ComplaintReasons != null)
                    reason = lastComplaint.ComplaintReasons.Name;

                MessageBoxResult result = MessageBox.Show($"Ваш аккаунт заморожен.\nПричина: {reason}\n\nХотите подать заявку на разморозку?", "Заморожен", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var request = new UnfreezeApplications
                    {
                        UserId = user.Id,
                        Reason = "Оспаривание заморозки аккаунта",
                        StatusId = 1,
                        CreatedAt = DateTime.Now,
                        BookId = null
                    };
                    Core.Context.UnfreezeApplications.Add(request);
                    Core.Context.SaveChanges();
                    MessageBox.Show("Заявка на разморозку отправлена!");
                }
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new MainPage());
                return;
            }
            //если не заморожен показываем данные пользователя
            DataContext = user;

            var reviews = Core.Context.Reviews.Where(r => r.UserId == user.Id).Include("Books").OrderByDescending(r => r.CreatedAt).ToList();
            ListBoxReviews.ItemsSource = reviews;
        }
        /// <summary>
        /// смена пароля для администратора
        /// </summary>
        private void BtnChangePasswordProfile_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var user = btn.DataContext as Users;
            if (user == null) return;
            var window = new ChangePasswordWindow(user);
            window.ShowDialog();
        }
        /// <summary>
        /// подача заявки на роль автора
        /// </summary>
        private void BtnAuthorRequest_Click(object sender, RoutedEventArgs e)
        {
            var user = UserData.CurrentUser;
            if (user == null) return;

            // нет ли уже активной заявки 
            var existingPending = Core.Context.AuthorApplications.FirstOrDefault(a => a.UserId == user.Id && a.StatusId == 1);

            if (existingPending != null)
            {
                MessageBox.Show("Ваша заявка на роль автора уже находится на рассмотрении.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            // была ли ранее отклонённая заявка
            var existingRejected = Core.Context.AuthorApplications.FirstOrDefault(a => a.UserId == user.Id && a.StatusId == 3);

            MessageBoxResult result = MessageBox.Show("Подать заявку на получение роли «Автор»?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (existingRejected != null)
                    {
                        existingRejected.StatusId = 1;
                        existingRejected.CreatedAt = DateTime.Now;
                    }
                    else
                    {
                        var request = new AuthorApplications
                        {
                            UserId = user.Id,
                            StatusId = 1,
                            CreatedAt = DateTime.Now
                        };
                        Core.Context.AuthorApplications.Add(request);
                    }
                    Core.Context.SaveChanges();
                    MessageBox.Show("Заявка принята!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// клик по отзыву – переход на книгу, если отзыв не заморожен
        /// </summary>
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = (Border)sender;
            var review = border.DataContext as Reviews;

            if (review == null) return;
            if (review.IsFrozen)
            {
                MessageBox.Show("Этот отзыв заморожен и недоступен для просмотра.", "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (review.Books != null)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new BookDetailPage(review.Books));
            }
        }
    }
}
