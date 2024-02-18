using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogaSample
{
    public class OrganizationWorker : INotifyPropertyChanged
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        private int organization_id;
        private int worker_id;

        public int Organization_id
        {
            get { return this.organization_id; }
            set
            {
                this.organization_id = value;
                OnPropertyChanged("Organization_id");
            }
        }

        public int Worker_id
        {
            get { return this.worker_id; }
            set
            {
                this.worker_id = value;
                OnPropertyChanged("Worker_id");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String property = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
