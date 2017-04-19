using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ListOfDeal {
    public class WLProcessor {
        List<MyAction> allActions;
        List<WLTask> allTasks;
        IWLConnector wlConnector;
#if Release
        public static int MyListId = 262335124;
        public static int MySchedId = 262630772;
        public static int MyBuyId = 263773374;
#endif
#if (DEBUG || DebugTest)
        public static int MyListId = 263984253;
        public static int MySchedId = 263984274;
        public static int MyBuyId = 263984295;
#endif
        public static int MyDiarId = 289882019;
        public static int RejectedListId = 299386783;
        public event System.Action<string> Logged;
        IMainViewModel parentVM;
        public WLProcessor(IMainViewModel _parentVM) {
            parentVM = _parentVM;
        }
        public void CreateWlConnector(IWLConnector _conn) {
            wlConnector = _conn;
            UpdateData();
            RaiseLog("WLProcessor", "Created");
        }

        public void UpdateData() {
            allActions = GetActiveActions();
            allTasks = GetAllActiveTasks();
        }
        public Action<string> SetClipboardText;

        async public void CreateWlTasks() {
            RaiseLog("Process creating tasks", "Started");
            MainViewModel.SaveChanges();
            var emptyActions = allActions.Where(x => x.WLId == null);
            var v = emptyActions.Count();
            RaiseLog("Count of new tasks", v.ToString());
            if (v == 0)
                return;
            foreach (var act in emptyActions) {
#if !DebugTest
                Task t = Task.Run(() => Thread.Sleep(20));
                await t;
#endif
                string title = act.GetWLTitle();
                WLTask wlTask;
                int targetListId = MyListId;
                string lstName = "MyList";
                if (act.ScheduledTime.HasValue) {//scheduled action
                    targetListId = MySchedId;
                    lstName = "MySched";
                }
                if (act.ToBuy) { //to buy action
                    targetListId = MyBuyId;
                    lstName = "MyBuy";
                }
                wlTask = wlConnector.CreateTask(title, targetListId, act.ScheduledTime, act.IsMajor);
                act.WLId = wlTask.id;
                act.WLTaskRevision = wlTask.revision;
                if (!string.IsNullOrEmpty(act.Comment)) {
                    wlConnector.CreateNote(act.WLId, act.Comment);
                }
                RaiseLog(wlTask, "created", string.Format("list id - {0}({1}), new task's id={2}", lstName, targetListId.ToString(), wlTask.id));
            }
            MainViewModel.SaveChanges();
        }
#if DebugTest
        public void HandleCompletedWLTasks() {
#else
        public async void HandleCompletedWLTasks() {
#endif
            RaiseLog("Handling completing WLtasks", "Started");
            MainViewModel.SaveChanges();
            var rejectedTasks = wlConnector.GetTasksForList(WLProcessor.RejectedListId).Select(x => x.id);
            allTasks = GetAllActiveTasks();
            var actionsWithTasks = allActions.Where(x => x.WLId != null).ToList();
            var lstwlIdinLod = actionsWithTasks.Select(x => x.WLId);
            var lstwlIdInWL = allTasks.Select(x => x.id);
            var diff = lstwlIdinLod.Except(lstwlIdInWL);
            RaiseLog("", string.Format("wlId in LOD - {0}, wlId in WL-{1}", lstwlIdinLod.Count(), lstwlIdInWL.Count()));
            foreach (string tskId in diff) {
#if !DebugTest
                Task t = Task.Run(() => Thread.Sleep(20));
                await t;
#endif
                Debug.Print(tskId.ToString());
                var tsk = wlConnector.GetTask(tskId);
                var act = actionsWithTasks.Where(x => x.WLId == tskId).First();
                var notes = wlConnector.GetNodesForTask(tskId);
                if (notes.Count > 0) {
                    act.Comment = notes[0].content;
                }
                if (rejectedTasks.Contains(act.WLId)) {
                    act.Status = ActionsStatusEnum.Rejected;
                    wlConnector.CompleteTask(act.WLId);
                }
                else {
                    act.Status = ActionsStatusEnum.Done;
                    string dtSt = tsk.completed_at.Split('T')[0];
                    DateTime completedTime = DateTime.Parse(dtSt);
                    act.CompleteTime = completedTime;
                }
                act.WLTaskStatus = WLTaskStatusEnum.UpToDateWLTask;
                act.WLId = null;
                RaiseLog(act, "completed");
            }
            MainViewModel.SaveChanges();
            RaiseLog("Handling completing WLtasks", "Finished");

        }
        List<WLTask> GetAllActiveTasks() {
            var v0 = wlConnector.GetTasksForList(MyListId);
            var v1 = wlConnector.GetTasksForList(MySchedId);
            var v2 = wlConnector.GetTasksForList(MyBuyId);
            var v4 = v0.Concat(v1);
            var v5 = v4.Concat(v2).ToList();
            return v5;
        }
        List<MyAction> GetActiveActions() {
            var lst = parentVM.Projects.Where(x => x.Status == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.Status == ActionsStatusEnum.InWork).ToList();
            var scheduledList = parentVM.Projects.Where(x => x.Status == ProjectStatusEnum.Delayed).SelectMany(x => x.Actions).Where(x => x.ScheduledTime.HasValue).ToList();
            var finalList = lst.Concat(scheduledList).ToList();
            return finalList;
        }


        public void HandleCompletedLODActions() {
            RaiseLog("Handling completed LOD Actions", "Started");
            MainViewModel.SaveChanges();
            var lst = parentVM.Projects.SelectMany(x => x.Actions).Where(x => x.WLTaskStatus == WLTaskStatusEnum.DeletingNeeded).ToList();
            RaiseLog("Amount of actions", lst.Count.ToString());
            foreach (var act in lst) {
                var wlId = act.WLId;
                wlConnector.CompleteTask(wlId);
                act.WLTaskStatus = WLTaskStatusEnum.UpToDateWLTask;
                act.WLId = null;
                RaiseLog(act, "Task completed");
            }
            MainViewModel.SaveChanges();
            RaiseLog("Handling completed LOD Actions", "Completed");
        }

        public void HandleChangedLODActions() {
            RaiseLog("Handling changed LOD actions", "Started");
            var changedActions = allActions.Where(x => x.WLTaskStatus == WLTaskStatusEnum.UpdateNeeded).ToList();
            RaiseLog("Actions to change", changedActions.Count.ToString());
            allTasks = GetAllActiveTasks();
            foreach (var act in changedActions) {
                var wlTask = allTasks.Where(x => x.id == act.WLId).First();
                var changedPropertiesCount = act.changedProperties.Count - 1;
                for (int i = changedPropertiesCount; i >= 0; i--) {
                    string propertyName = act.changedProperties[i];
                    switch (propertyName) {
                        case "IsMajor":
                            if (act.IsMajor != wlTask.starred) {
                                wlTask = wlConnector.ChangeStarredOfTask(wlTask.id, act.IsMajor, wlTask.revision);
                                RaiseLog(wlTask, "new IsMajor", act.IsMajor);
                            }
                            break;
                        case "Name":
                            string wlTitle = act.GetWLTitle();
                            if (wlTitle != wlTask.title) {
                                wlTask = wlConnector.ChangeTitleOfTask(wlTask.id, wlTitle, wlTask.revision);
                                RaiseLog(wlTask, "change name", wlTitle);
                            }
                            break;
                        case "Comment":
                            var wlNotes = wlConnector.GetNodesForTask(wlTask.id);
                            var wlNote = wlNotes.Count == 0 ? null : wlNotes[0];
                            var wlNoteValue = wlNote == null ? null : wlNote.content;
                            if (act.Comment != wlNoteValue) {
                                if (act.Comment != null) {
                                    if (wlNoteValue == null) {
                                        wlConnector.CreateNote(wlTask.id, act.Comment);
                                    }
                                    else {
                                        wlConnector.UpdateNoteContent(wlNote.id, wlNote.revision, act.Comment);
                                    }
                                }
                                else {
                                    wlConnector.DeleteNote(wlNote.id, wlNote.revision);
                                }
                                wlTask = wlConnector.GetTask(wlTask.id);
                                RaiseLog(wlTask, "new comment", act.Comment == null ? "null" : act.Comment);
                            }
                            break;
                        case "ScheduledTime":
                            if (act.ScheduledTime.HasValue) {
                                string currDT = WLConnector.ConvertToWLDate(act.ScheduledTime.Value);
                                if (wlTask.list_id != WLProcessor.MySchedId) {
                                    RaiseLog(wlTask, "moved", "MyShed -" + WLProcessor.MySchedId);
                                    wlTask = wlConnector.ChangeListOfTask(wlTask.id, WLProcessor.MySchedId, wlTask.revision);
                                }
                                if (currDT != wlTask.due_date) {
                                    wlTask = wlConnector.ChangeScheduledTime(wlTask.id, currDT, wlTask.revision);
                                    RaiseLog(wlTask, "changed scheduled time", currDT);
                                }
                            }
                            else {
                                if (wlTask.list_id == WLProcessor.MySchedId) {
                                    RaiseLog(wlTask, "moved", "MyList  -  " + WLProcessor.MyListId);
                                    wlTask = wlConnector.ChangeListOfTask(wlTask.id, WLProcessor.MyListId, wlTask.revision);
                                    RaiseLog(wlTask, "changed scheduled time", "null");
                                    wlTask = wlConnector.ChangeScheduledTime(wlTask.id, "null", wlTask.revision);
                                }
                            }
                            break;
                        case "ToBuy":
                            if (act.ToBuy && wlTask.list_id != WLProcessor.MyBuyId) {
                                wlTask = wlConnector.ChangeListOfTask(wlTask.id, WLProcessor.MyBuyId, wlTask.revision);
                            }
                            if (!act.ToBuy) {
                                if (act.ScheduledTime.HasValue) {
                                    wlTask = wlConnector.ChangeListOfTask(wlTask.id, WLProcessor.MySchedId, wlTask.revision);
                                }
                                else {
                                    wlTask = wlConnector.ChangeListOfTask(wlTask.id, WLProcessor.MyListId, wlTask.revision);
                                }
                            }
                            break;
                    }
                    act.changedProperties.Remove(propertyName);
                }
                act.WLTaskRevision = wlTask.revision;
                act.WLTaskStatus = WLTaskStatusEnum.UpToDateWLTask;

            }
            RaiseLog("Handling changed LOD actions", "Finished");
        }

        public void HandleChangedWLTask() {
            RaiseLog("Handling changed WLTasks", "Started");
            var actionsWithTasks = allActions.Where(x => x.WLId != null).ToList();
            foreach (var act in actionsWithTasks) {
                var tsk = allTasks.Where(x => x.id == act.WLId).FirstOrDefault();
                if (tsk == null) {
                    RaiseLog(act, "There are no tasks");
                    continue;
                }
                if (act.WLTaskRevision != tsk.revision) {
                    var actNameForWL = act.GetWLTitle();
                    RaiseLog(act, "has old revision");
                    if (actNameForWL != tsk.title) {
                        string nameFromTitle = tsk.title;
                        if (!act.parentEntity.Project.IsSimpleProject) {
                            var parsedTitle = tsk.title.Split('-');
                            if (parsedTitle.Count() == 2) {
                                nameFromTitle = parsedTitle[1].Trim();
                            }
                        }
                        RaiseLog(act, "changed name", string.Format("from {0} to {1}", act.Name, nameFromTitle));
                        act.Name = nameFromTitle;
                    }
                    DateTime? wlDateTime = null;
                    DateTime tmpDateTime;
                    if (DateTime.TryParse(tsk.due_date, out tmpDateTime))
                        wlDateTime = tmpDateTime;
                    if (act.ScheduledTime != wlDateTime) {
                        if (act.ScheduledTime.HasValue) {
                            if (wlDateTime.HasValue) {
                                RaiseLog(act, "changed time", string.Format("from {0} to {1}", act.ScheduledTime.Value.ToString("yyy-MM-dd"), tsk.due_date));
                                var newDate = DateTime.Parse(tsk.due_date);
                                act.ScheduledTime = newDate;
                            }
                            else {
                                act.ScheduledTime = null;
                                RaiseLog(act, "delete time");
                            }
                        }
                        else {
                            if (wlDateTime.HasValue) {
                                RaiseLog(act, "set time", wlDateTime.Value.ToString("yyy-MM-dd"));
                                act.ScheduledTime = wlDateTime;
                            }
                        }
                    }
                    var wlNotes = wlConnector.GetNodesForTask(tsk.id);
                    var wlNote = wlNotes.Count == 0 ? null : wlNotes[0];
                    var wlNoteValue = wlNote == null ? null : wlNote.content;
                    if (wlNoteValue != act.Comment) {
                        act.Comment = wlNoteValue;
                        RaiseLog(act, "new comment", act.Comment == null ? "null" : act.Comment);
                    }
                    if (tsk.starred != act.IsMajor) {
                        act.IsMajor = tsk.starred;
                        RaiseLog(act, "new IsMajor", act.IsMajor);
                    }
                    act.WLTaskStatus = WLTaskStatusEnum.UpToDateWLTask;
                    act.WLTaskRevision = tsk.revision;
                }
            }
            RaiseLog("Handling changed WLTasks", "Finished");
        }

        internal void RaiseLog(object subject, string description, object newValue = null) {
            string st = CreateLogString(subject.ToString(), description, newValue?.ToString());
            if (Logged != null)
                Logged(st);
        }

        public string CreateLogString(string subject, string description, string newValue = null) {
            string dateString = DateTime.Now.ToString("dd-MMM-yy HH:mm");
            string subjectString = subject.PadRight(95).Substring(0, 95);
            string descriptionString = description.PadRight(20);
            string newValueString = (newValue != null ? newValue : "").PadRight(58);
            string result = string.Format("| {0} | {1} | {2} | {3} |", dateString, subjectString, descriptionString, newValueString);
            return result;
        }
        public void Test() {

            //var lst = this.wlConnector.GetAllLists();
            //foreach(var l in lst) {
            //    var res = this.wlConnector.GetTasksForList(l.id);
            //}
            //return lst;

            //var lst = wlConnector.GetAllLists();
            var t = wlConnector.GetTask("2709210017");
            var lst = wlConnector.GetTasksForList(WLProcessor.RejectedListId);
        }
        public void PasteDiaryEntries() {
            var lst = wlConnector.GetTasksForList(MyDiarId);
            var titles = lst.Select(x => x.title).ToList();
            string result = string.Join(System.Environment.NewLine, titles);
            SetClipboardText(result);
            RaiseLog("diary entries", "copied");
        }
    }

    public static class ClipboardHelper {
        public static void ClipboardSetText(string st) {
            Clipboard.SetText(st);
        }
    }

}
