using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogaSample
{
    public class Worker : INotifyPropertyChanged
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        private String fio;
        private String job;
        private String address;
        private String phone;
        private double? salary;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String property = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public String Fio
        {
            get { return this.fio; }
            set
            {
                this.fio = value;
                OnPropertyChanged("Fio");
            }
        }

        public String Job
        {
            get { return this.job; }
            set
            {
                this.job = value;
                OnPropertyChanged("Job");
            }
        }

        public String Address
        {
            get { return this.address; }
            set
            {
                this.address = value;
                OnPropertyChanged("Address");
            }
        }

        public String Phone
        {
            get { return this.phone; }
            set
            {
                this.phone = value;
                OnPropertyChanged("Phone");
            }
        }

        public double? Salary
        {
            get { return this.salary; }
            set
            {
                this.salary = value;
                OnPropertyChanged("Salary");
            }
        }
    }
}
