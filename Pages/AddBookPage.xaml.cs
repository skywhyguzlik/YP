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
    /// Логика взаимодействия для AddBookPage.xaml
    /// </summary>
    public partial class AddBookPage : Page
    {
        private List<Genres> _selectedGenres;// выбранные жанры
        private List<Genres> _availableGenres;// жанры, доступные
        public AddBookPage()
        {
            InitializeComponent();
            _selectedGenres = new List<Genres>();
            RefreshGenres();
        }
        /// <summary>
        /// обновляет список доступных/выбранных жанров
        /// </summary>
        private void RefreshGenres()
        {
            var allGenres = Core.Context.Genres.OrderBy(g => g.Name).ToList();
            // доступные жанры = все жанры минус уже выбранные
            _availableGenres = allGenres.Where(g => !_selectedGenres.Any(sg => sg.Id == g.Id)).ToList();
            CmbGenre.ItemsSource = _availableGenres;
            CmbGenre.SelectedIndex = _availableGenres.Any() ? 0 : -1;
            ListBoxGenres.ItemsSource = _selectedGenres.ToList();
        }
        /// <summary>
        /// добавляет выбранный жанр в список
        /// </summary>
        private void BtnAddGenre_Click(object sender, RoutedEventArgs e)
        {
            if (CmbGenre.SelectedItem is Genres selectedGenre)
            {
                if (_selectedGenres.Any(g => g.Id == selectedGenre.Id))
                {
                    MessageBox.Show("Этот жанр уже выбран.");
                    return;
                }
                _selectedGenres.Add(selectedGenre);
                RefreshGenres();
            }
        }
        /// <summary>
        /// удаляет жанр
        /// </summary>
        private void BtnDeleteGenre_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var genre = btn.DataContext as Genres;
            if (genre == null) return;

            _selectedGenres.Remove(genre);
            RefreshGenres();
        }
        /// <summary>
        /// создаёт новую книгу и сохраняет
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string coverPath = TxtCoverPath.Text.Trim();
            string title = TxtTitle.Text.Trim();
            string description = TxtDescription.Text.Trim();
            string content = TxtContent.Text.Trim();
            if (string.IsNullOrWhiteSpace(coverPath) ||
                string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Все поля обязательны для заполнения.");
                return;
            }
            try
            {
                var newBook = new Books
                {
                    CoverPath = coverPath,
                    Title = title,
                    Description = description,
                    Content = content,
                    AuthorId = UserData.CurrentUser.Id,
                    IsFrozen = false
                };
                Core.Context.Books.Add(newBook);
                Core.Context.SaveChanges();
                if (_selectedGenres.Any())
                {
                    foreach (var genre in _selectedGenres)
                    {
                        Core.Context.BookGenres.Add(new BookGenres { BookId = newBook.Id, GenreId = genre.Id });
                    }
                    Core.Context.SaveChanges();
                }

                MessageBox.Show("Книга успешно добавлена!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении книги: {ex.Message}");
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            bool hasData = !string.IsNullOrWhiteSpace(TxtCoverPath.Text) || !string.IsNullOrWhiteSpace(TxtTitle.Text) ||
                   !string.IsNullOrWhiteSpace(TxtDescription.Text) || !string.IsNullOrWhiteSpace(TxtContent.Text) ||
                   _selectedGenres.Count > 0;
            if (hasData)
            {
                MessageBoxResult result = MessageBox.Show("Все введённые данные будут потеряны, закрыть страницу?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                    return;
            }
            NavigationService?.GoBack();
        }
    }
}
