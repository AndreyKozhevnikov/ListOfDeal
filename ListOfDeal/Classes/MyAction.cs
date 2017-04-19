﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListOfDeal {
    [DebuggerDisplay("Name = {Name}")]
    public class MyAction : MyBindableBase {
        public Action parentEntity;
        public MyAction(Action _parentEntity) {
            parentEntity = _parentEntity;
            changedProperties = new List<string>();
        }
        public List<string> changedProperties;
        void HandlePropertyChanges(string st) {
            if (!changedProperties.Contains(st))
                changedProperties.Add(st);
            SetWLStatusUpdatedIfNeeded();
            RaisePropertyChanged(st);
        }
        public MyAction() {
            var v = MainViewModel.DataProvider.CreateAction();
            parentEntity = v;
            DateCreated = DateTime.Now;
            changedProperties = new List<string>();
        }
        public string Name {
            get {
                return parentEntity.Name;
            }
            set {
                parentEntity.Name = value;
                HandlePropertyChanges("Name");
            }
        }
        public ActionsStatusEnum2 Status2 {
            get {
                return (ActionsStatusEnum2)parentEntity.StatusId;
            }
            set {
                if (parentEntity.StatusId == (int)value)
                    return;
                parentEntity.StatusId = (int)value;
                if (value == ActionsStatusEnum2.Done || value == ActionsStatusEnum2.Rejected) {
                    this.CompleteTime = DateTime.Now;
                    this.OrderNumber = -1;
                    SetDeleteTaskIfNeeded();
                }
                else {
                    this.CompleteTime = null;
                }
                if (value != ActionsStatusEnum2.InWork && this.ScheduledTime != null) {
                    this.ScheduledTime = null;
                }
                RaisePropertyChanged("Status2");
            }
        }
        public DateTime DateCreated {
            get {
                return parentEntity.DateCreated;
            }
            set {
                parentEntity.DateCreated = value;
                RaisePropertyChanged("DateCreated");
            }
        }

        public DateTime? ScheduledTime {
            get {
                return parentEntity.ScheduledTime;
            }
            set {
                if (value == parentEntity.ScheduledTime)
                    return;
                parentEntity.ScheduledTime = value;
                if (value.HasValue) {
                    this.Status2 = ActionsStatusEnum2.InWork;
                }
                else {
                    if (this.parentEntity.Project.StatusId == (int)ProjectStatusEnum.Delayed) {
                        WLTaskStatus = WLTaskStatusEnum.DeletingNeeded;
                        return;
                    }
                }
                HandlePropertyChanges("ScheduledTime");
            }
        }

        public void SetWLStatusUpdatedIfNeeded() {
            if (WLId != null)
                WLTaskStatus = WLTaskStatusEnum.UpdateNeeded;
        }


        public string Comment {
            get {
                return parentEntity.Comment;
            }
            set {
                parentEntity.Comment = value;
                HandlePropertyChanges("Comment");
            }
        }
        public int OrderNumber {
            get {
                return parentEntity.OrderNumber;
            }
            set {
                parentEntity.OrderNumber = value;
                RaisePropertyChanged("OrderNumber");
            }
        }
        public bool ToBuy {
            get {
                return parentEntity.ToBuy;
            }
            set {
                parentEntity.ToBuy = value;
                HandlePropertyChanges("ToBuy");
            }
        }

        public string ProjectName {
            get {
                return parentEntity.Project.Name;
            }
        }
        public int ProjectType {
            get {
                return parentEntity.Project.TypeId;
            }
        }
        public int ProjectId {
            get {
                return parentEntity.Project.Id;
            }
        }


        public DateTime? CompleteTime {
            get {
                return parentEntity.CompleteTime;
            }
            set {
                parentEntity.CompleteTime = value;
            }
        }
        public int? WLTaskRevision {
            get {
                return parentEntity.WLTaskRevision;
            }
            set {
                parentEntity.WLTaskRevision = value;
            }
        }
        public string WLId {
            get {
                return parentEntity.WLId;
            }
            set {
                parentEntity.WLId = value;
            }
        }
        public WLTaskStatusEnum WLTaskStatus {
            get {
                return (WLTaskStatusEnum)parentEntity.WLTaskStatus;
            }
            set {
                if (WLId != null)
                    parentEntity.WLTaskStatus = (int)value;
            }
        }
        public bool IsMajor {
            get {
                return parentEntity.IsMajor;
            }
            set {
                parentEntity.IsMajor = value;
                HandlePropertyChanges("IsMajor");
            }
        }


        public void SetDeleteTaskIfNeeded() {
            if (WLId != null)
                this.WLTaskStatus = WLTaskStatusEnum.DeletingNeeded;
        }
        internal void CopyProperties(MyAction act) {
            this.Name = act.Name;
            this.Status2 = act.Status2;
            this.ScheduledTime = act.ScheduledTime;
            this.Comment = act.Comment;
            this.DateCreated = act.DateCreated;
        }

        internal string GetWLTitle() {
            string title;
            if (parentEntity.Project.IsSimpleProject)
                title = this.Name;

            else
                title = string.Format("{0} - {1}", this.ProjectName, this.Name);
            return title;
        }

        public override string ToString() {
            return string.Format("Action {0} {1}", GetWLTitle(), parentEntity.Id);
        }
    }




}
