﻿using DevExpress.Data;
using DevExpress.Entity.ProjectModel;
using DevExpress.Xpf.Grid;
using ListOfDeal.Classes.XPO;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ListOfDeal {
    public interface IMainViewModel {
        ObservableCollection<MyProject> Projects { get; set; }

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
            //CreateNewProject(null);
            //CreateNewAction();

        }

        //private void CreateNewProject(int? oldTypeId) {
        //    CurrentProject = new MyProject();
        //    CurrentProject.Name = "newlycreatedpojerct";
        //    CurrentProject.DateCreated= DateTime.Now;
        //    CurrentProject.Status = ProjectStatusEnum.InWork;
        //    CurrentProject.IsSimpleProject = true;
        //    if (oldTypeId == null)
        //        CurrentProject.ProjectType = DataProvider.GetProjectTypeById( 7);
        //    else
        //        CurrentProject.ProjectType = DataProvider.GetProjectTypeById(oldTypeId.Value);
        //}
        //private void CreateNewAction() {
        //    CurrentAction = new MyAction();
        //    CurrentAction.Status = ActionsStatusEnum.Delay;
        //}
        private void AddProject() {
            if (string.IsNullOrEmpty(NewProjectName))
                return;
            GridControlManagerService.ClearFilterAndSearchString();

            var newProject = new MyProject();
            newProject.Name = NewProjectName;
            newProject.ProjectType = SelectedProjectType;
            newProject.Status = ProjectStatusEnum.InWork;
            newProject.DateCreated = DateTime.Now;
            newProject.IsSimpleProject = true;
            // CurrentProject.Save();
            //if (CurrentProject.IsSimpleProject) {
            MyAction act = new MyAction();
            act.Name = newProject.Name;
            act.Status = ActionsStatusEnum.InWork;
            newProject.AddAction(act);
            //}


            Projects.Add(newProject);
            NewProjectName = string.Empty;
            Dispatcher.CurrentDispatcher.BeginInvoke((System.Action)(() => {
                SelectedProject = newProject;
                SaveChanges();
                // CreateNewProject(typeId.Id);
                // CreateNewAction();
                GridControlManagerService.ScrollToSeveralRows();
                GridControlManagerService.ExpandFocusedMasterRow();
            }), DispatcherPriority.Input);
        }
        private void OnSelectedActionChanged() {
            if (SelectedAction != null) {
                SelectedProjectType = SelectedAction.ProjectType;
            }
        }
        private void OnSelectedProjectChanged() {
            if (SelectedProject != null)
                SelectedProjectType = SelectedProject.ProjectType;
        }

        void OnFocusedRowHandleChanged(ProjectType typeId) {
            if (typeId != null)
                SelectedProjectType = typeId;
        }
        private void AddAction() {
            if (string.IsNullOrEmpty(NewActionName))
                return;
            MyProject projForSave = null;
            if (SelectedProject != null) {
                projForSave = SelectedProject;
            } else {
                if (SelectedAction != null) {
                    projForSave = GetProjectById(SelectedAction.ProjectId);
                }
            }
            if (projForSave == null)
                return;
            if (projForSave.IsSimpleProject) {
                projForSave.IsSimpleProject = false;
                projForSave.Actions[0].Name = NewActionName;
                projForSave.Actions[0].DateCreated = DateTime.Now;
            } else {
                var newAction = new MyAction();
                newAction.Name = NewActionName;
                newAction.Status = newActionStatus;
                newAction.DateCreated = DateTime.Now;
                projForSave.AddAction(newAction);
            }
            SaveChanges();
            projForSave.RaisePropertyChanged("ActionsList");
            NewActionName = string.Empty;

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

        private void ProvideActions() {
            var allActions = Projects.Where(x => x.Status == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.Status == ActionsStatusEnum.InWork);
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
            catch (Exception e) {
                Console.WriteLine(e.Message);
                //foreach (var eve in e.EntityValidationErrors) {
                //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                //    foreach (var ve in eve.ValidationErrors) {
                //        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                //            ve.PropertyName, ve.ErrorMessage);
                //    }
                //}

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
                var lst = ReturnActiveActionsFromProjectList(Projects);
                obj.TotalValue = string.Format("Actions count={0}", lst.Count);
            }
        }
        public static List<MyAction> ReturnActiveActionsFromProjectList(IEnumerable<MyProject> list) {
            var lst = list.Where(x => x.Status == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.Status == ActionsStatusEnum.InWork).ToList();
            return lst;
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
            foreach (ActionXP a in allActEnt) {
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
            if (p.Actions.Where(x => x.Status == ActionsStatusEnum.InWork || x.Status == ActionsStatusEnum.Delay).Count() == 0)
                return true;
            return false;
        }
        private void CustomColumnSort(CustomColumnSortEventArgs e) {
            if (e.Value1 == null || e.Value2 == null) {
                return;
            }
            var v1 = ((ProjectType)e.Value1).Id;
            var v2 = ((ProjectType)e.Value2).Id;

            var ordNum1 = (int)this.ProjectTypes.Where(x => x.Id == v1).First().OrderNumber;
            var ordNum2 = (int)this.ProjectTypes.Where(x => x.Id == v2).First().OrderNumber;

            e.Result = Comparer<int>.Default.Compare(ordNum1, ordNum2);
            e.Handled = true;
        }

    }


}
