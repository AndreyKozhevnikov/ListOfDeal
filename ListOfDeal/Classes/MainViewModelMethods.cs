using DevExpress.Data;
using DevExpress.Xpf.Grid;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ListOfDeal {
    public interface IMainViewModel {
        ObservableCollection<MyProject> Projects { get; set; }

    }

    public interface IMainViewModelDataProvider {
        IEnumerable<ProjectType> GetProjectTypes();
        IEnumerable<Project> GetProjects();
        IListOfDealBaseEntities GeneralEntity { get; set; }
        Project CreateProject();
        void AddProject(Project p);
        void AddWeekRecord(WeekRecord wr);
        IEnumerable<Action> GetActions();
        IEnumerable<WeekRecord> GetWeekRecords();
        ProjectType CreateProjectType();
        void AddProjectType(ProjectType projectType);
        void SaveChanges();
        Action CreateAction();
        WeekRecord CreateWeekRecord();

    }
    public class MainViewModelDataProvider : IMainViewModelDataProvider {
        public MainViewModelDataProvider() {
            ConnectToDataBase();

        }
        public IListOfDealBaseEntities GeneralEntity { get; set; }

        private void ConnectToDataBase() {
            string machineName = System.Environment.MachineName;
            if (machineName == "KOZHEVNIKOV-W10") {
                GeneralEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesWork");
            }
            else {
                GeneralEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesHome");
            }
#if DEBUG
            if (machineName == "KOZHEVNIKOV-W10")
                GeneralEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesWork");
            else
                GeneralEntity = new ListOfDealBaseEntities("ListOfDealBaseEntitiesHomeTest");
#endif


        }

        public IEnumerable<ProjectType> GetProjectTypes() {
            return GeneralEntity.ProjectTypes;
        }

        public IEnumerable<Project> GetProjects() {
            return GeneralEntity.Projects;
        }
        public IEnumerable<WeekRecord> GetWeekRecords() {
            return GeneralEntity.WeekRecords;
        }
        public Project CreateProject() {
            return GeneralEntity.Projects.Create();
        }

        public void AddProject(Project p) {
            GeneralEntity.Projects.Add(p);
        }
        public void AddWeekRecord(WeekRecord wr) {
            GeneralEntity.WeekRecords.Add(wr);
        }
        public IEnumerable<Action> GetActions() {
            return GeneralEntity.Actions;
        }
        public ProjectType CreateProjectType() {
            return GeneralEntity.ProjectTypes.Create();
        }
        public void AddProjectType(ProjectType projectType) {
            GeneralEntity.ProjectTypes.Add(projectType);
        }
        public void SaveChanges() {
            GeneralEntity.SaveChanges();
        }
        public Action CreateAction() {
            return GeneralEntity.Actions.Create();
        }
        public WeekRecord CreateWeekRecord() {
            return GeneralEntity.WeekRecords.Create();
        }
    }
    public partial class MainViewModel : IMainViewModel {

        public MainViewModel() {
            InitializeData();
        }


        void InitializeData() {
            ProjectTypes = new ObservableCollection<ProjectType>(DataProvider.GetProjectTypes().OrderBy(x => x.OrderNumber));

            Projects = new ObservableCollection<MyProject>();
            var actProjects = DataProvider.GetProjects().Where(x => x.StatusId == (int)ProjectStatusEnum.InWork || x.StatusId == (int)ProjectStatusEnum.Delayed).OrderBy(x => x.StatusId).ThenBy(x => x.DateCreated);
            foreach (var p in actProjects) {
                Projects.Add(new MyProject(p));
            }
            CreateNewProject(null);
            CreateNewAction();

            WLViewModel = new WunderListViewModel(this);
          //  PathchStatus2();
        }

        void PathchStatus2() {
            var acts = DataProvider.GetActions();
            foreach( var act in acts) {
                if (act.IsActive) {
                    act.StatusId2 = (int)ActionsStatusEnum2.InWork;
                }
                else {
                    if (act.StatusId == (int)ActionsStatusEnum.Completed) {
                        act.StatusId2 = (int)ActionsStatusEnum2.Done;
                    }
                    if (act.StatusId == (int)ActionsStatusEnum.Waited) {
                        act.StatusId2 = (int)ActionsStatusEnum2.Delay;
                    }
                }
            }
        }



        private void CreateNewProject(int? oldTypeId) {
            CurrentProject = new MyProject();
            CurrentProject.Status = ProjectStatusEnum.InWork;
            CurrentProject.IsSimpleProject = true;
            if (oldTypeId == null)
                CurrentProject.TypeId = 7;
            else
                CurrentProject.TypeId = oldTypeId.Value;
        }
        private void CreateNewAction() {
            CurrentAction = new MyAction();
            CurrentAction.Status2 = ActionsStatusEnum2.Delay;
        }
        private void AddProject() {
            if (string.IsNullOrEmpty(CurrentProject.Name))
                return;
            GridControlManagerService.ClearFilterAndSearchString();
            var typeId = CurrentProject.TypeId;
            CurrentProject.DateCreated = DateTime.Now;
            CurrentProject.Save();
            if (CurrentProject.IsSimpleProject) {
                MyAction act = new MyAction();
                act.Name = CurrentProject.Name;
                act.Status2 = ActionsStatusEnum2.InWork;
                CurrentProject.AddAction(act);
            }


            Projects.Add(CurrentProject);
            Dispatcher.CurrentDispatcher.BeginInvoke((System.Action)(() => {
                SelectedProject = CurrentProject;
                SaveChanges();
                CreateNewProject(typeId);
                GridControlManagerService.ScrollToSeveralRows();
                GridControlManagerService.ExpandFocusedMasterRow();
            }), DispatcherPriority.Input);
        }
        private void OnSelectedActionChanged() {
            if (SelectedAction != null) {
                CurrentProject.TypeId = SelectedAction.ProjectType;
            }
        }
        private void OnSelectedProjectChanged() {
            if (SelectedProject != null)
                CurrentProject.TypeId = SelectedProject.TypeId;
        }

        void OnFocusedRowHandleChanged(int typeId) {
            if (typeId != -1)
                CurrentProject.TypeId = typeId;
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
        private void OpenNewType() {
            CreateNewInfoWindow wnd = new CreateNewInfoWindow();
            ProjectType tp = DataProvider.CreateProjectType();
            wnd.DataContext = tp;
            wnd.ShowDialog();
            if (!string.IsNullOrEmpty(tp.Name)) {
                DataProvider.AddProjectType(tp);
                SaveChanges();
                ProjectTypes.Add(tp);
            }

        }
        private void ProvideActions() {
            var allActions = Projects.Where(x => x.Status == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.Status2 == ActionsStatusEnum2.InWork);
            var actActions = allActions.Where(x => x.ScheduledTime == null);
            var shedActions = allActions.Where(x => x.ScheduledTime.HasValue);

            WaitedActions = new ObservableCollection<MyAction>(actActions);
            ScheduledActions = new ObservableCollection<MyAction>(shedActions);


        }

        private void ExportGrids() {
            ExportToExcelService.Export();
        }


        public static void SaveChanges() {
            try {
                DataProvider.SaveChanges();
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
                var v = Projects.SelectMany(x => x.Actions).Where(y => y.Status2 == ActionsStatusEnum2.InWork).ToList();
                obj.TotalValue = string.Format("Actions count={0}", v.Count);
            }
        }
        private void GoToParentProject(MyAction act) {
            SelectedTabIndex = 0;
            MyProject p = GetProjectById(act.ProjectId);
            SelectedProject = p;
            GridControlManagerService.ExpandMasterRow(p);
        }

        public MyProject GetProjectById(int id) {
            var p = Projects.Where(x => x.Id == id).First();
            return p;
        }

        private void GetChartData() {
            var allActions = DataProvider.GetActions().ToList();
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
            var allActEnt = DataProvider.GetActions();
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
            var isValidChange = IsNewStatusIsValid((MyProject)e.Row, (ProjectStatusEnum)e.Value);
            if (!isValidChange) { 
                e.ErrorContent = "there are active actions";
                e.IsValid = false;
                e.Handled = true;
            }

        }
        public bool IsNewStatusIsValid(MyProject p, ProjectStatusEnum newStatus) {
            if (newStatus == ProjectStatusEnum.InWork || newStatus == ProjectStatusEnum.Delayed)
                return true;
            if (p.Actions.Where(x => x.Status2 == ActionsStatusEnum2.InWork||x.Status2==ActionsStatusEnum2.Delay).Count() == 0)
                return true;
            return false;
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
