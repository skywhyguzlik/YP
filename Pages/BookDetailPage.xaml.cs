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
    /// Логика взаимодействия для BookDetailPage.xaml
    /// </summary>
    public partial class BookDetailPage : Page
    {
        private Books _book; // тек книга
        public BookDetailPage(Books book)
        {
            InitializeComponent();
            _book = book;
            DataContext = _book;// для привязок в xaml
            double avgRating = _book.Reviews.Any() ? _book.Reviews.Average(r => r.Rating) : 0;
            TxtRating.Text = avgRating.ToString("0");
            LoadReviews();
            SetButtonsVisibility();
        }
        /// <summary>
        /// управление видимостью кнопок в зависимости от роли и авторства
        /// </summary>
        private void SetButtonsVisibility()
        {
            BtnFreezeBook.Visibility = UserData.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

            bool isAuthor = UserData.CurrentUser?.Id == _book.AuthorId;
            if (isAuthor)
            {
                BtnComplaintBook.Visibility = Visibility.Collapsed;
                BtnComplaintAuthor.Visibility = Visibility.Collapsed;
                // Скрываем весь блок отправки отзыва
                ReviewPanel.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// загрузка незамороженных отзывов к книге
        /// </summary>
        private void LoadReviews()
        {
            ReviewsList.Items.Clear();
            var reviews = Core.Context.Reviews.Where(r => r.BookId == _book.Id && !r.IsFrozen).Include("Users").ToList();
            foreach (var review in reviews)
            {
                ReviewsList.Items.Add(review);
            }
        }
        /// <summary>
        /// кнопка "заморозить отзыв" только для администратора
        /// </summary>
        private void BtnFreezeReview_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserData.IsAdmin)
                ((Button)sender).Visibility = Visibility.Visible;
        }
        /// <summary>
        /// кнопка "жалоба" не должна быть видна у автора отзыва
        /// </summary>
        private void BtnComplaintReview_Loaded(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var review = btn.DataContext as Reviews;
            if (review != null && review.UserId == UserData.CurrentUser?.Id)
            {
                btn.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// открыть окно чтения текста книги
        /// </summary>
        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_book.Content))
            {
                WindowReadText window = new WindowReadText(_book.Content);
                window.Owner = Window.GetWindow(this);
                window.ShowDialog();
            }
        }
        /// <summary>
        /// добавить книгу в список чтения
        /// </summary>
        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (UserData.IsFrozen)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Вы не можете добавлять книги в список.", "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var window = new WindowAddToList(_book);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
        /// <summary>
        /// подать жалобу на книгу (тип 1)
        /// </summary>
        private void BtnComplaintBook_Click(object sender, RoutedEventArgs e)
        {
            if (UserData.IsFrozen)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Вы не можете подавать жалобы.", "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ComplaintWindow window = new ComplaintWindow(1, _book.Title, _book.Id);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
        /// <summary>
        /// подать жалобу на отзыв (тип 2)
        /// </summary>
        private void BtnComplaintReview_Click(object sender, RoutedEventArgs e)
        {
            if (UserData.IsFrozen)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Вы не можете подавать жалобы.", "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Button btn = (Button)sender;
            if (btn.Tag != null)
            {
                int reviewId = Convert.ToInt32(btn.Tag);
                ComplaintWindow window = new ComplaintWindow(2, $"Отзыв #{reviewId}", _book.Id, reviewId);
                window.Owner = Window.GetWindow(this);
                window.ShowDialog();
            }
        }
        /// <summary>
        /// подать жалобу на автора (тип 3)
        /// </summary>  
        private void BtnComplaintAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (UserData.IsFrozen)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Вы не можете подавать жалобы.", "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string authorName = "Автор";
            if (_book.Users != null && _book.Users.DisplayName != null)
            {
                authorName = _book.Users.DisplayName;
            }
            ComplaintWindow window = new ComplaintWindow(3, authorName, _book.Id);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
        /// <summary>
        /// новый отзыв к книге
        /// </summary>
        private void BtnSubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (UserData.IsFrozen)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Вы не можете оставлять отзывы.", "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string text = TxtNewReview.Text.Trim();
            if (text == "")
            {
                MessageBox.Show("Введите текст отзыва.");
                return;
            }

            bool alreadyReviewed = Core.Context.Reviews.Any(r => r.BookId == _book.Id && r.UserId == UserData.CurrentUser.Id);
            if (alreadyReviewed)
            {
                MessageBox.Show("Вы уже оставляли отзыв на эту книгу.", "Ограничение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ComboBoxItem selectedItem = (ComboBoxItem)CmbRating.SelectedItem;
            int rating = int.Parse(selectedItem.Content.ToString());
            Reviews newReview = new Reviews
            {
                BookId = _book.Id,
                UserId = UserData.CurrentUser.Id,
                Text = text,
                Rating = rating,
                CreatedAt = DateTime.Now,
                IsFrozen = false
            };
            Core.Context.Reviews.Add(newReview);
            Core.Context.SaveChanges();
            TxtNewReview.Clear(); // очищаем форму и обновляем список отзывов
            CmbRating.SelectedIndex = 0;
            LoadReviews();
            UpdateAverageRating(); // обновляем рейтинг
        }
        /// <summary>
        /// пересчёт и отображение среднего рейтинга книги
        /// </summary>
        private void UpdateAverageRating()
        {
            double avgRating = 0;
            if (_book.Reviews.Count > 0)
            {
                double sum = 0;
                foreach (var review in _book.Reviews)
                {
                    sum += review.Rating;
                }
                avgRating = sum / _book.Reviews.Count;
            }
            TxtRating.Text = Math.Round(avgRating).ToString();
        }
        /// <summary>
        /// заморозить книгу (только для админа)
        /// </summary>
        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show($"Заморозить книгу «{_book.Title}»?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _book.IsFrozen = true;
                Core.Context.SaveChanges();
                MessageBox.Show("Книга заморожена.");
            }
        }
        /// <summary>
        /// заморозить отзыв (для админа)
        /// </summary>
        private void BtnFreezeReview_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int reviewId = (int)btn.Tag;
            var review = Core.Context.Reviews.FirstOrDefault(r => r.Id == reviewId);

            if (review == null)
            {
                MessageBox.Show("Отзыв не найден.");
                return;
            }
            if (review.IsFrozen)
            {
                MessageBox.Show("Этот отзыв уже заморожен.");
                return;
            }
            MessageBoxResult result = MessageBox.Show("Заморозить этот отзыв?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                review.IsFrozen = true;
                Core.Context.SaveChanges();
                LoadReviews();
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
