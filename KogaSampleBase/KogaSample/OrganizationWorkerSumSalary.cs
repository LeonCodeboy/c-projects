using System;

namespace KogaSample
{
    public class OrganizationWorkerSumSalary
    {
        private String organization_Name;
        private int common_Salary;

        public int Common_Salary
        {
            get
            {
                return common_Salary;
            }
            set
            {
                this.common_Salary = value;
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
