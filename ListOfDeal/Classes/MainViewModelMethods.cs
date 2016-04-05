﻿using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public partial class MainViewModel {
        public MainViewModel() {
            InitializeData();
        }
        void InitializeData() {
            ConnectToDataBase();
            Projects = new ObservableCollection<MyProject>();
            foreach (var p in generalEntity.Projects) {
                Projects.Add(new MyProject(p));
            }
            ProjectTypes = new ObservableCollection<ProjectType>(generalEntity.ProjectTypes);
            ProjectStatuses = new ObservableCollection<ProjectStatus>(generalEntity.ProjectStatuses);
            ActionTriggers = new ObservableCollection<ActionTrigger>(generalEntity.ActionTriggers);
            ActionStatuses = new ObservableCollection<ActionStatus>(generalEntity.ActionStatuses);
            DelegatePersons = new ObservableCollection<DelegatePerson>(generalEntity.DelegatePersons);
            CreateNewProject();
            CreateNewAction();

        }



        private void ConnectToDataBase() {

            string machineName = System.Environment.MachineName;
            if (machineName == "KOZHEVNIKOV-W8") {
                generalEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesWork");
            }
            else {
                generalEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesHome");
            }
        }

        private void CreateNewProject() {
            CurrentProject = new MyProject();
            CurrentProject.TypeId = 1;
            CurrentProject.StatusId = 1;
        }
        private void CreateNewAction() {
            CurrentAction = new MyAction();
            CurrentAction.StatusId = 1;
            CurrentAction.IsActive = false;


        }
        private void AddNewProject() {
            if (string.IsNullOrEmpty(CurrentProject.Name))
                return;

            CurrentProject.DateCreated = DateTime.Now;
            CurrentProject.Save();
            Projects.Add(CurrentProject);
            generalEntity.SaveChanges();
            CreateNewProject();
        }
        private void AddAction() {
            CurrentAction.DateCreated = DateTime.Now;
            SelectedProject.AddAction(CurrentAction);

            generalEntity.SaveChanges();
            SelectedProject.RaisePropertyChanged("ActionsList");
            CreateNewAction();
        }
        internal void Test() {
            //var a = new MyAction();
            //a.Name = "testaction";
            //a.DateCreated = DateTime.Now;
            //a.StatusId = 1;
            //a.TriggerId = 1;
            //Projects[0].Actions.Add(a);
            var v = generalEntity.Projects.ToList();
            generalEntity.SaveChanges();
        }
        private void OpenEditProject() {
            EditProject ed = new EditProject();
            ed.DataContext = this;
            ed.ShowDialog();
        }
        private void OpenNewInfo(string st) {
            CreateNewInfoWindow wnd = new CreateNewInfoWindow();
            switch (st) {
                case "Trigger":
                    ActionTrigger trig = generalEntity.ActionTriggers.Create();
                    wnd.DataContext = trig;
                    wnd.ShowDialog();
                    if (!string.IsNullOrEmpty(trig.Name)) {
                        generalEntity.ActionTriggers.Add(trig);
                        generalEntity.SaveChanges();
                        ActionTriggers.Add(trig);
                    }
                    break;
                case "ProjectType":
                    ProjectType tp = generalEntity.ProjectTypes.Create();
                    wnd.DataContext = tp;
                    wnd.ShowDialog();
                    if (!string.IsNullOrEmpty(tp.Name)) {
                        generalEntity.ProjectTypes.Add(tp);
                        generalEntity.SaveChanges();
                        ProjectTypes.Add(tp);
                    }
                    break;

            }
        }
        private void ProvideActiveActions() {
            var v = Projects.Where(x => x.StatusId == 1).SelectMany(x => x.Actions).ToList();
            ActiveActions = new ObservableCollection<MyAction>(v);
        }
        private void CustomRowFilter(RowFilterEventArgs e) {
            GridControl gc = e.Source as GridControl;
            int needIndex = -1;
            switch (gc.Tag.ToString()) {
                case "WaitedActionsGrid":
                    needIndex = 1;
                    break;
                case "ScheduledActionsGrid":
                    needIndex = 2;
                    break;
                case "DelegatedActionsGrid":
                    needIndex = 3;
                    break;

            }
            var li = e.ListSourceRowIndex;
            var act = gc.GetRowByListIndex(li) as MyAction;
            if (act.IsActive && act.StatusId == needIndex)
                e.Visible = true;
            else
                e.Visible = false;
            e.Handled = true;
        }
    }
}
