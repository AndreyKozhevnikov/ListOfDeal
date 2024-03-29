﻿using ListOfDeal.Classes.XPO;
using NUnit.Framework;
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
        public ActionXP parentEntity;
        public MyAction(ActionXP _parentEntity) {
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
        public int Id {
            get {
                return parentEntity.Id;
            }
            set {
                parentEntity.Id = value;
            }
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
        public ActionsStatusEnum Status {
            get {
                return (ActionsStatusEnum)parentEntity.StatusId;
            }
            set {
                if (parentEntity.StatusId == (int)value)
                    return;
                parentEntity.StatusId = (int)value;
                if(value != ActionsStatusEnum.InWork && this.ScheduledTime != null) {
                    this.ScheduledTime = null;
                }
                if (value == ActionsStatusEnum.Done || value == ActionsStatusEnum.Rejected || value == ActionsStatusEnum.Delay) {
                    this.CompleteTime = DateTime.Now;
                    this.OrderNumber = -1;
                    SetDeleteTaskIfNeeded();
                }
                else {
                    this.CompleteTime = null;
                }
               
                RaisePropertyChanged("Status");
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
                    this.Status = ActionsStatusEnum.InWork;
                }
                HandlePropertyChanges("ScheduledTime");
            }
        }

        public void SetWLStatusUpdatedIfNeeded() {
            if (WLId != null &&WLTaskStatus!=WLTaskStatusEnum.DeletingNeeded)
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
                return parentEntity.ProjectId.Name;
            }
        }
        public ProjectType ProjectType {
            get {
                return parentEntity.ProjectId.TypeId;
            }
        }
        public int ProjectId {
            get {
                return parentEntity.ProjectId.Id;
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
            this.Status = act.Status;
            this.ScheduledTime = act.ScheduledTime;
            this.Comment = act.Comment;
            this.DateCreated = act.DateCreated;
        }

        internal string GetWLTitle() {
            string title;
            if (parentEntity.ProjectId.IsSimpleProject)
                title = this.Name;

            else
                title = string.Format("{0} - {1}", this.Name, this.ProjectName);
            return title;
        }

        public override string ToString() {
            return string.Format("Action {0} {1}", GetWLTitle(), parentEntity.Id);
        }
    }




}
