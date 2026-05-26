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
    /// Логика взаимодействия для WindowReadText.xaml
    /// </summary>
    public partial class WindowReadText : Window
    {
        /// <summary>
        /// конструктор окна, принимает текст книги
        /// </summary>
        public WindowReadText(string text)
        {
            InitializeComponent();
            // напрямую присваиваем текст
            TxtContent.Text = text;
        }
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
