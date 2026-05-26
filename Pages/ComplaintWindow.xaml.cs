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
    /// Логика взаимодействия для ComplaintWindow.xaml
    /// </summary>
    public partial class ComplaintWindow : Window
    {
        private int _targetTypeId; // тип цели жалобы
        private string _targetName; // название объекта
        private int _bookId;  // id книги
        private int? _targetId; // id отзыва, если жалуемся на отзыв

        public ComplaintWindow(int targetTypeId, string targetName, int bookId, int? targetId = null)
        {
            InitializeComponent();

            _targetTypeId = targetTypeId;
            _targetName = targetName;
            _bookId = bookId;
            _targetId = targetId;
            // определяем текст в зависимости от типа жалобы
            string typeText;
            switch (targetTypeId)
            {
                case 1: typeText = "книгу"; break;
                case 2: typeText = "отзыв"; break;
                case 3: typeText = "автора"; break;
                default: typeText = "объект"; break;
            }
            TxtTargetInfo.Text = $"Жалоба на {typeText}";
            TxtTargetName.Text = targetName;
            CmbReasons.ItemsSource = Core.Context.ComplaintReasons.ToList();
            CmbReasons.SelectedIndex = 0;
        }
        /// <summary>
        /// отправка жалобы
        /// </summary>
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (CmbReasons.SelectedItem == null)
            {
                MessageBox.Show("Выберите причину жалобы.");
                return;
            }
            var selectedReason = (ComplaintReasons)CmbReasons.SelectedItem;
            // нет ли уже нерассмотренной жалобы на этот объект
            bool alreadyComplained = Core.Context.Complaints.Any(c => c.UserId == UserData.CurrentUser.Id && c.TargetTypeId == _targetTypeId && c.TargetId == _targetId && c.BookId == _bookId && c.IsConfirmed == null);
            if (alreadyComplained)
            {
                MessageBox.Show("Вы уже подавали жалобу на этот объект. Ожидайте рассмотрения.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = false;
                this.Close();
                return;
            }
            var result = MessageBox.Show($"Отправить жалобу на {TxtTargetInfo.Text} «{_targetName}»?\nПричина: {selectedReason.Name}", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                var complaint = new Complaints
                {
                    UserId = UserData.CurrentUser.Id,
                    TargetTypeId = _targetTypeId,
                    ReasonId = selectedReason.Id,
                    BookId = _bookId,
                    TargetId = _targetId,
                    CreatedAt = DateTime.Now
                };

                Core.Context.Complaints.Add(complaint);
                Core.Context.SaveChanges();
                MessageBox.Show("Жалоба отправлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
