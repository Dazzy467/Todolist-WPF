using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.Serialization.Formatters.Binary;

namespace Todolist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ObservableCollection<TodoItem>? Todos { get; set; }

        public bool EditMode { get; set; }

        public string filePath { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Todos = new ObservableCollection<TodoItem>();
            filePath = string.Empty;
            EditMode = false;
            TodoListView.ItemsSource = Todos;
        }

        private void btnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            if (!EditMode) {
                if (inputTodo.Text.Length > 0)
                {
                    Todos?.Add(new TodoItem { isChecked = false, todoText = inputTodo.Text });
                    inputTodo.Clear();
                }
                else MessageBox.Show("Task must be entered", "No task", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (inputTodo.Text.Length > 0 && Todos?.Count > 0)
                {
                    Todos[TodoListView.SelectedIndex].todoText = inputTodo.Text;
                }
                else MessageBox.Show("Task need to be entered to edit", "No task", MessageBoxButton.OK, MessageBoxImage.Warning);



                System.Diagnostics.Debug.WriteLine($"Selected item idx: {TodoListView.SelectedIndex}\nEdited to {Todos[TodoListView.SelectedIndex].todoText}");
            }
        }

        private void TodoListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Todos?.Count > 0 && TodoListView.SelectedIndex != -1)
                Todos?.RemoveAt(TodoListView.SelectedIndex);
        }

        private void CheckBox_PreviewDclick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CheckBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < Todos?.Count; i++)
            {
                if (Todos[i].isChecked == true)
                    System.Diagnostics.Debug.WriteLine($"Item at {i} Checked:");

            }
        }

        private void TodoListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the element that was clicked
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // Check if the clicked element is a ListViewItem or its content
            bool isListViewItem = false;
            while (dep != null && !isListViewItem)
            {
                if (dep is ListViewItem)
                {
                    isListViewItem = true;
                    EditMode = true;
                    btnAddTodo.Content = "Edit";
                }
                else
                {
                    dep = VisualTreeHelper.GetParent(dep);
                    btnAddTodo.Content = "Add";
                    EditMode = false;
                }
            }

            // If the clicked element is not a ListViewItem, deselect any selected item
            if (!isListViewItem)
            {
                TodoListView.SelectedItem = null;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;

            if (item.Name == "Menu_Clearlist")
            {
                if (Todos?.Count > 0)
                    Todos?.Clear();

                if (EditMode)
                {
                    EditMode = false;
                    btnAddTodo.Content = "Add";
                }
            }
            else if (item.Name == "Menu_Open")
            {
                OpenFileMethod();
            }
            else if (item.Name == "Menu_Save")
            {
                SaveMethod();
            }
            else if (item.Name == "Menu_Saveas")
            {
                SaveAsMethod();
            }
            


            System.Diagnostics.Debug.WriteLine($"Clicked menu: {item.Name}");
        }


        private void SaveAsMethod()
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Title = "Save Todofile";
            fileDialog.Filter = "Todofile (*.tdf)|*.tdf";
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (fileDialog.ShowDialog() == true)
            {
                // Save file, serialize it first before writing it

                byte[] serializedDatas;

                using (MemoryStream memStream = new MemoryStream()) { 
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(memStream, Todos.ToArray());
                    serializedDatas = memStream.ToArray();
                }
                filePath = fileDialog.FileName;

                File.WriteAllBytes(fileDialog.FileName, serializedDatas);
            }

        }

        private void SaveMethod()
        {
            byte[] serializedDatas;

            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memStream, Todos.ToArray());
                serializedDatas = memStream.ToArray();
            }

            if (filePath != string.Empty)
                File.WriteAllBytes(filePath, serializedDatas);
            else
                SaveAsMethod();
            
            

        }

        private void OpenFileMethod()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Open a Todofile";
            fileDialog.Filter = "Todofile (*.tdf)|*.tdf";

            TodoItem[] TempTodoItem = Array.Empty<TodoItem>();

            if (fileDialog.ShowDialog() == true)
            {
                // Open the file
                using (FileStream fstream = new FileStream(fileDialog.FileName, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    TempTodoItem = (TodoItem[])formatter.Deserialize(fstream);
                }

                filePath = fileDialog.FileName;
            }


            if (Todos?.Count > 0)
                Todos.Clear();

            if (TempTodoItem.Length > 0)
            {
                for (int i = 0; i < TempTodoItem.Length; i++)
                {
                    Todos.Insert(i, TempTodoItem[i]);
                }
            }

            foreach (TodoItem item in Todos)
            {
                System.Diagnostics.Debug.WriteLine($"Readed: {item.isChecked}, {item.todoText}");
            }
            
        }
    }

    [Serializable]
    public class TodoItem : INotifyPropertyChanged
    {
        private bool _isChecked; // Private field for the isChecked property
        public bool isChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value; // Access the private field here
                    OnPropertyChanged(nameof(isChecked));
                }
            }
        }

        private string? _todoText; // Private field for the todoText property
        public string? todoText
        {
            get { return _todoText; }
            set
            {
                if (_todoText != value)
                {
                    _todoText = value; // Access the private field here
                    OnPropertyChanged(nameof(todoText));
                }
            }
        }

        [field:  NonSerialized]
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
