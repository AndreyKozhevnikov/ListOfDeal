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
   public interface IMainViewModel {
      ObservableCollection<MyProject> Projects { get; set; }
    }
    public partial class MainViewModel:IMainViewModel {

        public MainViewModel() {
            InitializeData();
        }
     
        void InitializeData() {
           
            ConnectToDataBase();

            ProjectTypes = new ObservableCollection<ProjectType>(generalEntity.ProjectTypes.OrderBy(x => x.OrderNumber));
          
            ActionTriggers = new ObservableCollection<ActionTrigger>(generalEntity.ActionTriggers);
            DelegatePersons = new ObservableCollection<DelegatePerson>(generalEntity.DelegatePersons);
            Projects = new ObservableCollection<MyProject>();
            var actProjects = generalEntity.Projects.Where(x => x.StatusId != 3).OrderBy(x => x.StatusId).ThenBy(x => x.DateCreated);
            foreach (var p in actProjects) {
                Projects.Add(new MyProject(p));
            }
            CreateNewProject();
            CreateNewAction();

            WLViewModel = new WunderListViewModel(this);

        }

       

        private void ConnectToDataBase() {

            string machineName = System.Environment.MachineName;
            if (machineName == "KOZHEVNIKOV-W10") {
                generalEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesWork");
            }
            else {
                generalEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesHome");
            }
#if DEBUG
            if (machineName == "KOZHEVNIKOV-W10")
                generalEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesWork");
            else
                generalEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesHomeTest");
#endif

        }

        private void CreateNewProject() {
            CurrentProject = new MyProject();
            CurrentProject.TypeId = 11;
            CurrentProject.StatusId = ProjectStatusEnum.Delayed;
            CurrentProject.IsSimpleProject = true;
        }
        private void CreateNewAction() {
            CurrentAction = new MyAction();
            CurrentAction.StatusId =ActionsStatusEnum.Waited;
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
                act.StatusId =ActionsStatusEnum.Waited;
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

            MyProject projForSave = null;
            if (SelectedProject != null) {
                projForSave = SelectedProject;
            }
            else {
                if (SelectedAction != null) {
                    projForSave = GetProjectById(SelectedAction.ProjectId);
                }
            }
            if (projForSave == null)
                return;
            projForSave.AddAction(CurrentAction);

            SaveChanges();
            projForSave.RaisePropertyChanged("ActionsList");
            CreateNewAction();
        }
        internal void Test() {
            //var v = Projects.Where(x => x.Actions.Count == 1);
            //foreach (MyProject p in v) {
            //    var act = p.Actions[0];
            //    if (act.Name == p.Name) {
            //        p.IsSimpleProject = true;
            //    }
            //}
            //SaveChanges();
            GridControlManagerService.ExpandMasterRow(SelectedProject);
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
        private void ProvideActions() {
            var allActions = Projects.Where(x => x.StatusId == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.IsActive);
            var actActions = allActions.Where(x => x.StatusId == ActionsStatusEnum.Waited);
            var shedActions = allActions.Where(x => x.StatusId == ActionsStatusEnum.Scheduled);
            var delActions = allActions.Where(x => x.StatusId == ActionsStatusEnum.Delegated);

            WaitedActions = new ObservableCollection<MyAction>(actActions);
            ScheduledActions = new ObservableCollection<MyAction>(shedActions);
            DelegatedActions = new ObservableCollection<MyAction>(delActions);

         
        }

        private void ExportGrids() {
            ExportToExcelService.Export();
        }


        public static void SaveChanges() {
            try {
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
                var v = Projects.SelectMany(x => x.Actions).Where(y => y.StatusId != ActionsStatusEnum.Completed).ToList();
                obj.TotalValue = string.Format("Actions count={0}", v.Count);
            }
        }
        private void GoToParentProject(MyAction act) {
            SelectedTabIndex = 0;
            MyProject p = GetProjectById(act.ProjectId);
            SelectedProject = p;
            GridControlManagerService.ExpandMasterRow(p);
        }

        MyProject GetProjectById(int id) {
            var p = Projects.Where(x => x.Id == id).First();
            return p;
        }

        private void GetChartData() {
            var allActions = generalEntity.Actions.ToList();
            var minDate = allActions.Min(x => x.DateCreated).Date;
            var todayDate = DateTime.Today;
            var count = (todayDate - minDate).Days;
            var dates = Enumerable.Range(0, count + 1).Select(offset => minDate.AddDays(offset)).ToList();

            var startDates = allActions.GroupBy(x => x.DateCreated.Date).Select(d => new { dt = d.Key, cntin = d.Count() }).ToList();
            var finishDates = allActions.Where(x => x.CompleteTime.HasValue).GroupBy(x => x.CompleteTime.Value.Date).Select(d => new { dt = d.Key, cntout = d.Count() }).ToList();

            var coll1 = from dt in dates
                        join sd in startDates on
                         dt.Date equals sd.dt
                         into t
                        from rt in t.DefaultIfEmpty(new { dt = DateTime.Today, cntin = 0 })
                        select new {
                            TDate = dt.Date,
                            CountIn = rt.cntin
                        };

            var coll2 = from pd in coll1
                        join fd in finishDates on
                         pd.TDate equals fd.dt
                         into result
                        from result2 in result.DefaultIfEmpty(new { dt = DateTime.Today, cntout = 0 })
                        orderby pd.TDate
                        select new DayData() {
                            TDate = pd.TDate,
                            CountIn = pd.CountIn,
                            CountOut = result2.cntout,
                            Delta = pd.CountIn - result2.cntout
                        };

            var col3 = coll2.ToList();

            int k = 0;
            foreach (DayData d in col3) {
                d.Summary = d.Delta + k;
                k = d.Summary;
            }
            ChartMinValue = col3.Where(x => x.TDate > new DateTime(2016, 4, 10)).Min(x => x.Summary) - 10;
            col3.RemoveAt(0);
            AllDayData = new ObservableCollection<DayData>(col3);
        }
        private void GetActionsHistory() {
            var allActEnt = generalEntity.Actions;
            var allAct = new List<MyAction>();
            foreach (Action a in allActEnt) {
                allAct.Add(new MyAction(a));
            }
            var complAct = allAct.Where(x => x.CompleteTime != null);
            allAct.Concat(complAct);
            var v1 = allAct.Select(x => new HistoryActionItem { Action = x, IsCompleted = x.CompleteTime.HasValue, FinalDate = x.CompleteTime.HasValue ? x.CompleteTime : x.DateCreated });
            ActionsHistoryCollection = new ObservableCollection<HistoryActionItem>(v1);
        }
        private void ValidateColumn(GridRowValidationEventArgs e) {
            MyProject p = e.Row as MyProject;
            var v = (int)e.Value;
            if (v == 3 && p.Actions.Where(x => x.StatusId != ActionsStatusEnum.Completed).Count() > 0) {
                e.ErrorContent = "there are active actions";
                e.IsValid = false;
                e.Handled = true;
            }

        }
        private void CustomColumnSort(CustomColumnSortEventArgs e) {
            var v1 = (int)e.Value1;
            var v2 = (int)e.Value2;

            var ordNum1 = (int)this.ProjectTypes.Where(x => x.Id == v1).First().OrderNumber;
            var ordNum2 = (int)this.ProjectTypes.Where(x => x.Id == v2).First().OrderNumber;

            e.Result = Comparer<int>.Default.Compare(ordNum1, ordNum2);
            e.Handled = true;
        }

    }
}
