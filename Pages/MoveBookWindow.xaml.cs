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
    /// Логика взаимодействия для MoveBookWindow.xaml
    /// </summary>
    public partial class MoveBookWindow : Window
    {
        private ReadingLists _entry; //запись для изменения

        public MoveBookWindow(ReadingLists entry)
        {
            InitializeComponent();
            _entry = entry; //сохраняем
            DataContext = _entry; // для отображения названия книги
            var statuses = Core.Context.StatusBooks.ToList();
            CmbStatuses.ItemsSource = statuses; //устанавливаем список
            CmbStatuses.SelectedItem = statuses.FirstOrDefault(s => s.Id == _entry.StatusId); //текущий как выбранный
        }
        /// <summary>
        /// кнопочка "Переместить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            if (CmbStatuses.SelectedItem is StatusBooks newStatus)
            {
                if (newStatus.Id == _entry.StatusId)
                {
                    MessageBox.Show("Книга уже находится в этом статусе.");
                    return;
                }
                var dbEntry = Core.Context.ReadingLists.Find(_entry.Id);//ищем запись по id
                if (dbEntry != null)
                {
                    dbEntry.StatusId = newStatus.Id;
                    dbEntry.UpdatedAt = DateTime.Now;
                    Core.Context.SaveChanges();
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }
    }
}
