using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogaSample
{
    public class OrganizationWorkerC
    {
        private int id;
        private String worker_Name;
        private String organization_Name;

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                this.id = value;
            }
        }

        public String Worker_Name
        {
            get
            {
                return worker_Name;
            }
            set
            {
                this.worker_Name = value;
            }
        }

        public String Organization_Name
        {
            get
            {
                return organization_Name;
            }
            set
            {
                this.organization_Name = value;
            }
        }
    }
}
