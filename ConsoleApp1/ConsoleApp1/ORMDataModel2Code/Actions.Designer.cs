﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace ConsoleApp1.ListOfDealBase {
  
    public partial class ActionsMy : XPLiteObject {
        int fId;
        [Key(true)]
        public int Id {
            get { return fId; }
            set { SetPropertyValue<int>(nameof(Id), ref fId, value); }
        }
        string fName;
        [Size(SizeAttribute.Unlimited)]
        public string Name {
            get { return fName; }
            set { SetPropertyValue<string>(nameof(Name), ref fName, value); }
        }
        ProjectsMy fProjectId;
        [Association(@"ActionsReferencesProjects")]
        public ProjectsMy ProjectId {
            get { return fProjectId; }
            set { SetPropertyValue<ProjectsMy>(nameof(ProjectId), ref fProjectId, value); }
        }
        DateTime fDateCreated;
        public DateTime DateCreated {
            get { return fDateCreated; }
            set { SetPropertyValue<DateTime>(nameof(DateCreated), ref fDateCreated, value); }
        }
        DateTime fScheduledTime;
        public DateTime ScheduledTime {
            get { return fScheduledTime; }
            set { SetPropertyValue<DateTime>(nameof(ScheduledTime), ref fScheduledTime, value); }
        }
        string fComment;
        [Size(SizeAttribute.Unlimited)]
        public string Comment {
            get { return fComment; }
            set { SetPropertyValue<string>(nameof(Comment), ref fComment, value); }
        }
        DateTime fCompleteTime;
        public DateTime CompleteTime {
            get { return fCompleteTime; }
            set { SetPropertyValue<DateTime>(nameof(CompleteTime), ref fCompleteTime, value); }
        }
        int fOrderNumber;
        public int OrderNumber {
            get { return fOrderNumber; }
            set { SetPropertyValue<int>(nameof(OrderNumber), ref fOrderNumber, value); }
        }
        string fWLId;
        [Size(11)]
        public string WLId {
            get { return fWLId; }
            set { SetPropertyValue<string>(nameof(WLId), ref fWLId, value); }
        }
        int fWLTaskStatus;
        public int WLTaskStatus {
            get { return fWLTaskStatus; }
            set { SetPropertyValue<int>(nameof(WLTaskStatus), ref fWLTaskStatus, value); }
        }
        int fWLTaskRevision;
        public int WLTaskRevision {
            get { return fWLTaskRevision; }
            set { SetPropertyValue<int>(nameof(WLTaskRevision), ref fWLTaskRevision, value); }
        }
        bool fToBuy;
        public bool ToBuy {
            get { return fToBuy; }
            set { SetPropertyValue<bool>(nameof(ToBuy), ref fToBuy, value); }
        }
        bool fIsMajor;
        
        public bool IsMajor {
            get { return fIsMajor; }
            set { SetPropertyValue<bool>(nameof(IsMajor), ref fIsMajor, value); }
        }
        int fStatusId;
        
        public int StatusId {
            get { return fStatusId; }
            set { SetPropertyValue<int>(nameof(StatusId), ref fStatusId, value); }
        }
        [Association(@"WeekRecordsReferencesActions")]
        public XPCollection<WeekRecordsMy> WeekRecordsCollection { get { return GetCollection<WeekRecordsMy>(nameof(WeekRecordsCollection)); } }
    }

}
