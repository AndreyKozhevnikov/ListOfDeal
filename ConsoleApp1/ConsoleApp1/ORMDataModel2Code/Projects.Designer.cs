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

    public partial class ProjectsMy : XPLiteObject {
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
        ProjectTypesMy fTypeId;
        [Association(@"ProjectsReferencesProjectTypes")]
        public ProjectTypesMy TypeId {
            get { return fTypeId; }
            set { SetPropertyValue<ProjectTypesMy>(nameof(TypeId), ref fTypeId, value); }
        }
        DateTime fDateCreated;
        public DateTime DateCreated {
            get { return fDateCreated; }
            set { SetPropertyValue<DateTime>(nameof(DateCreated), ref fDateCreated, value); }
        }
        int fStatusId;
        public int StatusId {
            get { return fStatusId; }
            set { SetPropertyValue<int>(nameof(StatusId), ref fStatusId, value); }
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
        string fDesiredResult;
        [Size(SizeAttribute.Unlimited)]
        public string DesiredResult {
            get { return fDesiredResult; }
            set { SetPropertyValue<string>(nameof(DesiredResult), ref fDesiredResult, value); }
        }
        bool fIsSimpleProject;
        public bool IsSimpleProject {
            get { return fIsSimpleProject; }
            set { SetPropertyValue<bool>(nameof(IsSimpleProject), ref fIsSimpleProject, value); }
        }
        [Association(@"ActionsReferencesProjects")]
        public XPCollection<ActionsMy> ActionsCollection { get { return GetCollection<ActionsMy>(nameof(ActionsCollection)); } }
    }

}
