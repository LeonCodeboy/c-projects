using System.Windows;

namespace KogaSample
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ApplicationViewModel appModel = new ApplicationViewModel();

            this.DataContext = appModel;
        }
    }
}
