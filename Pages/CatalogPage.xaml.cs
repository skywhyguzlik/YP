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
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : Page
    {
        private List<Books> _allBooks; // незамороженные книги
        public CatalogPage()
        {
            InitializeComponent();
            LoadData();
        }
        /// <summary>
        /// загрузка книг и заполнение списка жанров
        /// </summary>
        private void LoadData()
        {
            _allBooks = Core.Context.Books.Where(b => !b.IsFrozen).ToList(); // получаем все книги, которые не заморожены
            ListBoxBooks.ItemsSource = _allBooks;

            var genres = Core.Context.Genres.ToList();
            genres.Insert(0, new Genres { Id = 0, Name = "Все жанры" });
            CmbGenres.ItemsSource = genres;
            CmbGenres.SelectedIndex = 0;
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void CmbGenres_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        /// <summary>
        /// применение фильтров к списку книг
        /// </summary>
        private void ApplyFilters()
        {
            if (_allBooks == null) return;
            var filtered = _allBooks.AsEnumerable();
            // поиск по названию или имени автора
            string text = TxtSearch.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(text))
                filtered = filtered.Where(b => b.Title.ToLower().Contains(text) || b.Users.DisplayName.ToLower().Contains(text));
            // фильтр по жанру
            if (CmbGenres.SelectedItem is Genres genre && genre.Id != 0)
                filtered = filtered.Where(b => b.BookGenres.Any(bg => bg.GenreId == genre.Id));
            // сортировка
            switch (CmbSort.SelectedIndex)
            {
                case 1:
                    filtered = filtered.OrderBy(b => b.Title);
                    break;
                case 2:
                    filtered = filtered.OrderByDescending(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0);
                    break;
            }
            ListBoxBooks.ItemsSource = filtered.ToList();
        }
        /// <summary>
        /// переход на страницу книги
        /// </summary>
        private void ListBoxBooks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxBooks.SelectedItem is Books selectedBook)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new BookDetailPage(selectedBook));
            }
        }
        /// <summary>
        /// кнопка "добавить в список" под книгой
        /// </summary>
        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (UserData.IsFrozen)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Вы не можете добавлять книги.", "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (((Button)sender).DataContext is Books selectedBook)
            {
                var window = new WindowAddToList(selectedBook);
                window.Owner = Window.GetWindow(this);
                window.ShowDialog();
            }
        }
    }
}
