using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace KogaSample
{
    class ApplicationViewModel
    {
        AppContext dbContext;
        private TCommand addCommand_Organization;
        private TCommand editCommand_Organization;
        private TCommand deleteCommand_Organization;

        private TCommand addCommand_Worker;
        private TCommand editCommand_Worker;
        private TCommand deleteCommand_Worker;

        private TCommand addCommand_OrganizationWorker;
        private TCommand deleteCommand_OrganizationWorker;

        public IEnumerable<Organization> Organizations { get; set; }
        public IEnumerable<Worker> Workers { get; set; }
        public IEnumerable<OrganizationWorker> OrganizationWorkers { get; set; }
        public ObservableCollection<OrganizationWorkerC> OrganizationWorkersC {get; set;}

        public IEnumerable<OrganizationWorkerMaxSalary> OrganizationWorkersMaxSalary { get; set; }
        public IEnumerable<OrganizationWorkerSumSalary> OrganizationWorkersSumSalary { get; set; }

        public ApplicationViewModel()
        {
            dbContext = new AppContext();
            dbContext.Organizations.Load();
            Organizations = dbContext.Organizations.Local.ToBindingList();

            dbContext.Workers.Load();
            Workers = dbContext.Workers.Local.ToBindingList();

            dbContext.OrganizationWorkers.Load();
            OrganizationWorkers = dbContext.OrganizationWorkers.Local.ToBindingList();

            dbContext.Configuration.ValidateOnSaveEnabled = false;

            var result = from org_wrk in OrganizationWorkers
                         join org in Organizations
                            on org_wrk.Organization_id equals org.Id
                         join wrk in Workers
                            on org_wrk.Worker_id equals wrk.Id
                         select new OrganizationWorkerC { Id = org_wrk.Id, Worker_Name = wrk.Fio, Organization_Name = org.Name };

            OrganizationWorkersC = new ObservableCollection<OrganizationWorkerC>(result.ToList());

            var resultMax = from org_wrk in OrganizationWorkers
                         join org in Organizations
                            on org_wrk.Organization_id equals org.Id
                         join wrk in Workers
                            on org_wrk.Worker_id equals wrk.Id group new { wrk, org } by org.Name into r
                         select new OrganizationWorkerMaxSalary
                         { Worker_Name = r.Max(ed => ed.wrk.Fio),
                             Organization_Name = r.Max(ed => ed.org.Name),
                             Salary = (int) r.Max(ed => ed.wrk.Salary) };


            OrganizationWorkersMaxSalary = new ObservableCollection<OrganizationWorkerMaxSalary>(resultMax.ToList());

            var resultSum = from org_wrk in OrganizationWorkers
                            join org in Organizations
                               on org_wrk.Organization_id equals org.Id
                            join wrk in Workers
                               on org_wrk.Worker_id equals wrk.Id
                            group new { wrk, org } by org_wrk.Organization_id into r
                            select new OrganizationWorkerSumSalary
                            {
                                Organization_Name = r.Max(ed => ed.org.Name),
                                Common_Salary = (int)r.Sum(ed => ed.wrk.Salary)
                            };


            OrganizationWorkersSumSalary = new ObservableCollection<OrganizationWorkerSumSalary>(resultSum.ToList());
        }

        public TCommand AddCommand_Organization
        {
            get
            {
                addCommand_Organization = new TCommand((o) =>
                {
                    OrganizationWindow wnd = new OrganizationWindow(new Organization());
                    if (wnd.ShowDialog() == true)
                    {
                        Organization org = wnd.Organization;
                        dbContext.Organizations.Add(org);
                        
                        dbContext.SaveChanges();
                    }
                });

                return addCommand_Organization;
            }
        }

        public TCommand  DeleteCommand_Organization
        {
            get
            {
                deleteCommand_Organization = new TCommand((selected) =>
                {
                    if (selected == null) return;
                    Organization org = selected as Organization;
                    dbContext.Organizations.Remove(org);
                    dbContext.SaveChanges();
                });

                return deleteCommand_Organization;
            }
        }

        public TCommand EditCommand_Organization
        {
            get
            {
                editCommand_Organization = new TCommand((selected) =>
                {
                    if (selected == null) return;
                    Organization org = selected as Organization;

                    Organization vm = new Organization()
                    {
                        Id = org.Id,
                        Name = org.Name,
                        Description = org.Description,
                    };
                    OrganizationWindow wnd = new OrganizationWindow(vm);

                    if (wnd.ShowDialog() == true)
                    {
                        org = dbContext.Organizations.Find(wnd.Organization.Id);
                        if (org != null)
                        {
                            org.Name = wnd.Organization.Name;
                            org.Description = wnd.Organization.Description;
                            dbContext.Entry(org).State = EntityState.Modified;
                            dbContext.SaveChanges();
                        }
                    }
                });

                return editCommand_Organization;
            }
        }

        public TCommand AddCommand_Worker
        {
            get
            {
                addCommand_Worker = new TCommand((o) =>
                {
                    WorkerWindow wnd = new WorkerWindow(new Worker());
                    if (wnd.ShowDialog() == true)
                    {
                        Worker wrk = wnd.Worker;
                        dbContext.Workers.Add(wrk);
                        dbContext.SaveChanges();
                    }
                });

                return addCommand_Worker;
            }
        }

        public TCommand DeleteCommand_Worker
        {
            get
            {
                deleteCommand_Worker = new TCommand((selected) =>
                {
                    if (selected == null) return;
                    Worker wrk = selected as Worker;
                    dbContext.Workers.Remove(wrk);
                    dbContext.SaveChanges();
                });

                return deleteCommand_Worker;
            }
        }

        public TCommand EditCommand_Worker
        {
            get
            {
                editCommand_Worker = new TCommand((selected) =>
                {
                    if (selected == null) return;
                    Worker wrk = selected as Worker;

                    Worker vm = new Worker()
                    {
                        Id = wrk.Id,
                        Fio = wrk.Fio,
                        Job = wrk.Job,
                        Address = wrk.Address,
                        Phone = wrk.Phone,
                        Salary = wrk.Salary,
                    };
                    WorkerWindow wnd = new WorkerWindow(vm);

                    if (wnd.ShowDialog() == true)
                    {
                        wrk = dbContext.Workers.Find(wnd.Worker.Id);
                        if (wrk != null)
                        {
                            wrk.Fio = wnd.Worker.Fio;
                            wrk.Job = wnd.Worker.Job;
                            wrk.Address = wnd.Worker.Address;
                            wrk.Phone = wnd.Worker.Phone;
                            wrk.Salary = wnd.Worker.Salary;
                            dbContext.Entry(wrk).State = EntityState.Modified;
                            dbContext.SaveChanges();
                        }
                    }
                });

                return editCommand_Worker;
            }
        }

        public TCommand DeleteCommand_OrganizationWorker
        {
            get
            {
                deleteCommand_OrganizationWorker = new TCommand((selected) =>
                {
                    if (selected == null) return;
                    
                    OrganizationWorkerC excepted = selected as OrganizationWorkerC;
                    OrganizationWorkersC.Remove(excepted);

                    // get connection id
                    PropertyInfo prop = selected.GetType().GetProperty("Id");
                    int conn_id = (int) prop.GetValue(selected, null);

                    int idx = OrganizationWorkers.ToList().FindIndex(x => x.Id == conn_id);
                    OrganizationWorker org_wrk = OrganizationWorkers.ElementAt(idx);

                    dbContext.OrganizationWorkers.Remove(org_wrk);
                    dbContext.SaveChanges();
                });

                return deleteCommand_OrganizationWorker;
            }
        }
        
        public TCommand AddCommand_OrganizationWorker
        {
            get
            {
                addCommand_OrganizationWorker = new TCommand((o) =>
                {
                    List<Organization> cloneOrganizations = new List<Organization> ();
                    List<Worker> cloneWorkers = new List<Worker>();
                    foreach (Organization org in Organizations)
                    {
                        cloneOrganizations.Add(new Organization { Id = org.Id, Name = org.Name });
                    }
                    foreach (Worker wrk in Workers)
                    {
                        cloneWorkers.Add(new Worker { Id = wrk.Id, Fio = wrk.Fio });
                    }
                    OrganizationWorkerWindow wnd = new OrganizationWorkerWindow(cloneOrganizations, cloneWorkers);
                    if (wnd.ShowDialog() == true)
                    {
                        Organization newOrg = wnd.orgList.SelectedItem as Organization;
                        Worker newWrk = wnd.workerList.SelectedItem as Worker;
                        
                        OrganizationWorker org_wrk = new OrganizationWorker { Organization_id = newOrg.Id,
                                                                Worker_id = newWrk.Id };
                        dbContext.OrganizationWorkers.Add(org_wrk);
                        dbContext.SaveChanges();

                        int lastInserted = org_wrk.Id;
                        OrganizationWorkerC org_wrk_c = new OrganizationWorkerC
                        {
                            Id = lastInserted,
                            Organization_Name = newOrg.Name,
                            Worker_Name = newWrk.Fio
                        };
                        OrganizationWorkersC.Add(org_wrk_c);
                    }
                });

                return addCommand_OrganizationWorker;
            }
        }
    }
}
