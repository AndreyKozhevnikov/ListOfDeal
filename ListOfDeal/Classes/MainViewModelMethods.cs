using DevExpress.Data;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ListOfDeal {
    public partial class MainViewModel {
        public MainViewModel() {
            InitializeData();
        }
        void InitializeData() {
            ConnectToDataBase();

            ProjectTypes = new ObservableCollection<ProjectType>(generalEntity.ProjectTypes);
            ProjectStatuses = new ObservableCollection<ProjectStatus>(generalEntity.ProjectStatuses);
            ActionTriggers = new ObservableCollection<ActionTrigger>(generalEntity.ActionTriggers);
            ActionStatuses = new ObservableCollection<ActionStatus>(generalEntity.ActionStatuses);
            DelegatePersons = new ObservableCollection<DelegatePerson>(generalEntity.DelegatePersons);
            Projects = new ObservableCollection<MyProject>();
            foreach (var p in generalEntity.Projects) {
                Projects.Add(new MyProject(p));
            }
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
            CurrentProject.TypeId = 11;
            CurrentProject.StatusId = 2;
        }
        private void CreateNewAction() {
            CurrentAction = new MyAction();
            CurrentAction.StatusId = 1;
            CurrentAction.IsActive = false;


        }
        private void AddProject() {
            if (string.IsNullOrEmpty(CurrentProject.Name))
                return;

            CurrentProject.DateCreated = DateTime.Now;
            CurrentProject.Save();
            if (CurrentProject.IsSimpleProject) {
                MyAction act = new MyAction();
                act.Name = CurrentProject.Name;
                act.StatusId = 1;
                act.IsActive = true;
                CurrentProject.AddAction(act);
            }


            Projects.Add(CurrentProject);
            SelectedProject = CurrentProject;
            SaveChanges();
            CreateNewProject();
        }
        private void AddAction() {
            if (string.IsNullOrEmpty(CurrentAction.Name))
                return;
            CurrentAction.DateCreated = DateTime.Now;
            SelectedProject.AddAction(CurrentAction);

            SaveChanges();
            SelectedProject.RaisePropertyChanged("ActionsList");
            CreateNewAction();
        }
        internal void Test() {
            var v = Projects.Where(x => x.Actions.Count == 0);
            foreach (MyProject p in v) {
                MyAction act = new MyAction();
                act.Name = p.Name;
                act.StatusId = 1;
                act.DateCreated = DateTime.Now;
                p.AddAction(act);
            }
            SaveChanges();
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
                        SaveChanges();
                        ActionTriggers.Add(trig);
                    }
                    break;
                case "ProjectType":
                    ProjectType tp = generalEntity.ProjectTypes.Create();
                    wnd.DataContext = tp;
                    wnd.ShowDialog();
                    if (!string.IsNullOrEmpty(tp.Name)) {
                        generalEntity.ProjectTypes.Add(tp);
                        SaveChanges();
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

        private void ExportWaitedGrid() {
            ExportToExcelService.Export();
        }


        public static void SaveChanges() {
            try {
                // Your code...
                // Could also be before try if you know the exception occurs in SaveChanges

                generalEntity.SaveChanges();
            }
            catch (DbEntityValidationException e) {
                foreach (var eve in e.EntityValidationErrors) {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors) {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }

            }
        }
        private void PreviewKeyProjects(KeyEventArgs e) {

            if (IfCtrlEnterIsPressed(e)) {
                AddProject();
            }
        }
        private void PreviewKeyActions(KeyEventArgs e) {
            if (IfCtrlEnterIsPressed(e)) {
                AddAction();
            }
        }

        private static bool IfCtrlEnterIsPressed(KeyEventArgs e) {
            return (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)));
        }
        private void CustomSummary(CustomSummaryEventArgs obj) {
            if (obj.SummaryProcess == CustomSummaryProcess.Finalize && Projects != null) {
                var v = Projects.SelectMany(x => x.Actions).ToList();
                obj.TotalValue =string.Format("Actions count={0}", v.Count);
            }
        }
        private void GoToParentProject(MyAction obj) {
            SelectedTabIndex = 0;
            MyProject p = Projects.Where(x => x.Id == obj.ProjectId).First();
            SelectedProject = p;
        }
    }
}
