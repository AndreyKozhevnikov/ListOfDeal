﻿using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    public class WLProcessor {
        List<MyAction> allActions;
        List<WLTask> allTasks;
        IWLConnector wlConnector;
#if !DEBUG
        public int MyListId = 262335124;
        public int MySchedId = 262630772;
        public int MyBuyId = 263773374;
#endif
#if DEBUG
        public int MyListId = 263984253;
        public int MySchedId = 263984274;
        public int MyBuyId = 263984295;
#endif
        public event WLEventHandler Logged;
        //public void PopulateActions(List<MyAction> _actions) {
        //    allActions = _actions;
        //}
        IMainViewModel parentVM;
        public WLProcessor(IMainViewModel _parentVM) {
            parentVM = _parentVM;
        }
        public void CreateWlConnector(IWLConnector _conn) {
            wlConnector = _conn;
            UpdateData();

              // (wlConnector as WLConnector).Start();
        }

        private void UpdateData() {
            allActions = GetActiveActions();
            allTasks = GetAllActiveTasks();
        }

        public void CreateWlTasks() {
            RaiseLog("===========Start creating tasks==========");
            MainViewModel.SaveChanges();
            var emptyActions = allActions.Where(x => x.WLId == null);
            var v = emptyActions.Count();
            RaiseLog(string.Format("new task count={0}", v));
            if (v == 0)
                return;
            //   emptyActions = emptyActions.Take(1);
            foreach (var act in emptyActions) {
                string title = act.GetWLTitle();
                WLTask wlTask;
                int targetListId = -1;
                targetListId = MyListId;
                if (act.Status == ActionsStatusEnum.Scheduled) {//scheduled action
                    wlTask = wlConnector.CreateTask(title, MySchedId, act.ScheduledTime);
                }
                else {
                    if (act.ProjectType == 10) { //to buy action
                        targetListId = MyBuyId;
                    }
                    else {
                        targetListId = MyListId;
                    }
                    wlTask = wlConnector.CreateTask(title, targetListId);
                }
                act.WLId = wlTask.id;
                act.WLTaskRevision = wlTask.revision;
                //  act.parentEntity.WLTaskStatus = 1;
                string message = string.Format("title={0}, list id - {1}, new task's id={2}", wlTask.title, wlTask.list_id, wlTask.id);
                RaiseLog(message);
            }
            MainViewModel.SaveChanges();
        }

        public void HandleCompletedWLTasks() {
            RaiseLog("==========Start handle completed  WLtasks==========");
            MainViewModel.SaveChanges();
            allTasks = GetAllActiveTasks();
            var actionsWithTasks = allActions.Where(x => x.WLId != null).ToList();
            var lstwlIdinLod = actionsWithTasks.Select(x => (int)x.WLId);
            var lstwlIdInWL = allTasks.Select(x => x.id);
            var diff = lstwlIdinLod.Except(lstwlIdInWL);
            RaiseLog(string.Format("wlId in LOD - {0}, wlId in WL-{1}", lstwlIdinLod.Count(), lstwlIdInWL.Count()));
            foreach (int tskId in diff) {
                Debug.Print(tskId.ToString());
                var act = actionsWithTasks.Where(x => x.WLId == tskId).First();
                act.WLId = null;
                act.WLTaskStatus = WLTaskStatusEnum.UpToDateWLTask;
                act.Status = ActionsStatusEnum.Completed;
                RaiseLog(string.Format("complete action - {0} {1}", act.Name, act.parentEntity.Id));
            }
            MainViewModel.SaveChanges();

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
            var lst = parentVM.Projects.Where(x => x.Status == ProjectStatusEnum.InWork).SelectMany(x => x.Actions).Where(x => x.IsActive).ToList();
            return lst;
        }


        public void HandleCompletedLODActions() {
            RaiseLog("==========Start handle completed  LODActions==========");
            MainViewModel.SaveChanges();
            var lst = MainViewModel.generalEntity.Actions.Where(x => x.WLTaskStatus == 2).ToList(); //improve?
            RaiseLog(string.Format("amount actions - {0}", lst.Count));
            foreach (var act in lst) {
                var wlId = (int)act.WLId;
                wlConnector.CompleteTask(wlId);
                act.WLId = null;
                act.WLTaskStatus = 0;
                RaiseLog(string.Format("complete task of actions {0} {1}", act.Name, act.Id));
            }
            MainViewModel.SaveChanges();
        }

        public void HandleChangedLODActions() {
            RaiseLog("==========Start HandleChangedLODActions==========");
            var changedActions = allActions.Where(x => x.WLTaskStatus == WLTaskStatusEnum.UpdateNeeded).ToList();
            RaiseLog(string.Format("There are {0} actions to change", changedActions.Count));
            allTasks = GetAllActiveTasks();
            foreach (var act in changedActions) {
                string wlTitle = act.GetWLTitle();
                var wlTask = allTasks.Where(x => x.id == act.WLId).First();
                WLTask resTask = null;

                if (wlTitle != wlTask.title) {
                    resTask = wlConnector.ChangeTitleOfTask(wlTask.id, wlTitle);
                    RaiseLog(string.Format("change name to  {0}", wlTitle));
                }
                if (act.ScheduledTime != null) {
                    string currDT = WLConnector.ConvertToWLDate(act.ScheduledTime.Value);
                    if (currDT != wlTask.due_date) {
                        resTask= wlConnector.ChangeScheduledTime(wlTask.id, currDT);
                        RaiseLog("action {0} change scheduled time to {1}", act.Name, currDT);
                    }
                }

                if (resTask != null) {
                    act.WLTaskRevision = resTask.revision;
                }
                else {
                    RaiseLog("!There are no changes in the act: " + act.Name);
                }
                act.WLTaskStatus = WLTaskStatusEnum.UpToDateWLTask;
            }
        }

        public void HandleChangedWLTask() {
            RaiseLog("==========Start HandleChangedWLTask==========");
            var actionsWithTasks = allActions.Where(x => x.WLId != null).ToList();
            foreach (var act in actionsWithTasks) {
                var tsk = allTasks.Where(x => x.id == act.WLId).First();
                if (act.WLTaskRevision != tsk.revision) {
                    RaiseLog(string.Format("act has old revision {0}", act.Name));
                    if (act.Name != tsk.title) {
                        RaiseLog(string.Format("act is changing name from {0} to {1}", act.Name, tsk.title));
                        act.Name = tsk.title;
                        act.WLTaskStatus = WLTaskStatusEnum.UpToDateWLTask;
                    }
                    act.WLTaskRevision = tsk.revision;
                }
            }
            RaiseLog("==========End HandleChangedWLTask==========");
        }
        internal void RaiseLog(string st) {
            if (Logged != null)
                Logged(new WLEventArgs(st));

        }
        internal void RaiseLog(string format, params object[] par) {
            string st = string.Format(format, par);
            if (Logged != null)
                Logged(new WLEventArgs(st));
        }
    }



}
