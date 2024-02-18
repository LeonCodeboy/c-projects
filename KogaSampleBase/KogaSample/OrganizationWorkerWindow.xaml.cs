using System.Collections.Generic;
using System.Windows;

namespace KogaSample
{
    /// <summary>
    /// Логика взаимодействия для OrganizationWorkerWindow.xaml
    /// </summary>
    public partial class OrganizationWorkerWindow : Window
    {
        public OrganizationWorkerWindow(IEnumerable<Organization> orgListData,
            IEnumerable<Worker> workerListData)
        {
            InitializeComponent();

            this.orgList.DataContext = orgListData;
            this.workerList.DataContext = workerListData;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
