using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace EguiLab2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        ObservableCollection<Book> items = new ObservableCollection<Book>();
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        Book _filters = new Book();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            DataContext = _filters;
            InitializeComponent();
            try
            {       
                StreamReader file = new StreamReader("LibraryData.txt");
                String line;
                while ((line = file.ReadLine()) != null)
                {
                    string[] itemData = line.Split(';');
                    if (itemData.Length != 3)
                        throw new IOException("File is corrupted (record length mismatch)!");
                    if((Regex.Match(itemData[2], @"[^\d]")).Success)
                        throw new IOException("File is corrupted (Year must contain number)!");
                    Book newItem = new Book(itemData[0], itemData[1], itemData[2]);
                    items.Add(newItem);
                }
                file.Close();
            }
            catch (IOException e)
            {
                MessageBox.Show("File could not be read:" + e.Message, "Error");
                Close();
            }
            BookList.ItemsSource = items;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(BookList.ItemsSource);
            view.Filter = BookFilter;
            this.Closed += new EventHandler(MainWindow_Closed);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            using (StreamWriter file = new StreamWriter("LibraryData.txt"))
            {
                foreach (Book item in items)
                {
                    file.WriteLine(item.Author + ";" + item.Title + ";" + item.Year);
                }
            }
        }

        void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null && headerClicked.Role != GridViewColumnHeaderRole.Padding)
            {
                if (headerClicked != _lastHeaderClicked)
                {
                    direction = ListSortDirection.Ascending;
                }
                else
                {
                    if (_lastDirection == ListSortDirection.Ascending)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = ListSortDirection.Ascending;
                    }
                }

                var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                Sort(sortBy, direction);

                if (direction == ListSortDirection.Ascending)
                {
                    headerClicked.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
                }
                else
                {
                    headerClicked.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;
                }

                // Remove arrow from previously sorted header
                if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                {
                    _lastHeaderClicked.Column.HeaderTemplate = null;
                }

                _lastHeaderClicked = headerClicked;
                _lastDirection = direction;
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(BookList.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private bool BookFilter(object item)
        {
            Boolean isAuthorFiltered = !String.IsNullOrEmpty(authorFilter.Text) && (item as Book).Author.IndexOf(authorFilter.Text, StringComparison.OrdinalIgnoreCase) < 0;
            Boolean isTitleFiltered = !String.IsNullOrEmpty(titleFilter.Text) && (item as Book).Title.IndexOf(titleFilter.Text, StringComparison.OrdinalIgnoreCase) < 0;
            Boolean isYearFiltered = !String.IsNullOrEmpty(yearFilter.Text) && (item as Book).Year.IndexOf(yearFilter.Text, StringComparison.OrdinalIgnoreCase) < 0;
            return !(isAuthorFiltered || isTitleFiltered || isYearFiltered);
        }

        private void AddNewBookButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new DialogVM(null);
            var dialog = new Dialog()
            {
                Title = "New Book",
                DataContext = window
            };
            if (dialog.ShowDialog() ?? false)
            {
                items.Add(window.Result);
            }
        }

        private void EditSelectedBookButton_Click(object sender, RoutedEventArgs e)
        { 
            ShowEditDialog();
        }

        private void DeleteSelectedBooksButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = BookList.SelectedItems.Cast<Object>().ToArray();
            foreach (object item in selected)
            {
                items.Remove((Book)item);
            }
        }

        private void TitleFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(BookList.ItemsSource).Refresh();
        }

        private void AuthorFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(BookList.ItemsSource).Refresh();
        }

        private void YearFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(BookList.ItemsSource).Refresh();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            authorFilter.Text = null;
            titleFilter.Text = null;
            yearFilter.Text = null;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowEditDialog();
        }

        private void ShowEditDialog()
        {

            if (BookList.SelectedItems.Count == 1)
            {
                if (BookList.SelectedItem is Book book)
                {
                    DialogVM model = new DialogVM(book);
                    Dialog dialog = new Dialog()
                    {
                        Title = "Edit Book",
                        DataContext = model
                    };
                    dialog.ShowDialog();
                }
            }
            else if (BookList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select an item to edit.", "Warning");
            }
            else
            {
                MessageBox.Show("Select only one item to edit.", "Warning");
            }
        }
    }
}
