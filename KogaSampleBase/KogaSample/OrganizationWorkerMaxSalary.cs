using System;

namespace KogaSample
{
    public class OrganizationWorkerMaxSalary
    {
        private String worker_Name;
        private String organization_Name;
        private int salary;

        public int Salary
        {
            get
            {
                return salary;
            }
            set
            {
                this.salary = value;
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
