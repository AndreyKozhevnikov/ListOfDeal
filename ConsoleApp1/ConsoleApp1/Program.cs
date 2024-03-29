﻿using ConsoleApp1.ListOfDealBase;
using ConsoleApp1.ListOfDealMy;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1 {
    internal class Program {
        static int k = 0;
        static int prCount = 0;
        static int prCountAll = 0;
        static string prTypeName = "";
        static void Main(string[] args) {
            ConnectionHelperMYsql.Connect(DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            ConnectionHelperLoc.Connect(DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            var uowLoc = new UnitOfWork(ConnectionHelperLoc.MyDataLayer);
            var uowMy = new UnitOfWork(ConnectionHelperMYsql.MyDataLayer);


            //var testP = new ProjectTypesMy(uowMy);
            //testP.Name = "тестовое имя";
            //uowMy.CommitChanges();

            var prTypes = new XPCollection<ProjectTypes>(uowLoc);

            foreach (var typeOld in prTypes) {
                prTypeName = typeOld.Name;
                var newType = new ProjectTypesMy(uowMy);
                CheckK(uowMy);
                newType.Name = typeOld.Name;
                newType.OrderNumber = typeOld.OrderNumber;
                prCountAll = typeOld.ProjectsCollection.Count;
                prCount = 0;
                foreach (var projectOld in typeOld.ProjectsCollection) {
                    if (projectOld.StatusId == 3 || projectOld.StatusId == 4) {
                        continue;
                    }
                    prCount++;
                    var newProject = new ProjectsMy(uowMy);
                    CheckK(uowMy);
                    CloneProperties(newProject, projectOld);
                    newProject.TypeId = newType;

                    foreach (var actionOld in projectOld.ActionsCollection) {
                        if (actionOld.StatusId == 2 || actionOld.StatusId == 3) {
                            continue;
                        }
                        var newAction = new ActionsMy(uowMy);
                        CheckK(uowMy);
                        CloneProperties(newAction, actionOld);
                        newAction.ProjectId = newProject;
                        foreach (var wlRecOld in actionOld.WeekRecordsCollection) {
                            var newWL = new WeekRecordsMy(uowMy);
                            CheckK(uowMy);
                            CloneProperties(newWL, wlRecOld);
                            newWL.ActionId = newAction;
                        }
                    }

                }

            }


            uowMy.CommitChanges();
        }
        static void CheckK(UnitOfWork uow) {
            k++;
            if (k == 100) {
                k = 0;
                uow.CommitChanges();
                Console.WriteLine(string.Format("{0} - {1} - {2}", prCount, prCountAll, prTypeName));
            }
        }
        static void CloneProperties(WeekRecordsMy newWl, WeekRecords oldWl) {
            newWl.DateAdd = oldWl.DateAdd;
            newWl.Comment = oldWl.Comment;
            newWl.WeekId = oldWl.WeekId;
            newWl.IsCompletedInWeek = oldWl.IsCompletedInWeek;
        }
        static void CloneProperties(ActionsMy newA, Actions oldA) {
            newA.Name = oldA.Name;
            newA.DateCreated = oldA.DateCreated;
            newA.ScheduledTime = oldA.ScheduledTime;
            newA.Comment = oldA.Comment;
            newA.CompleteTime = oldA.CompleteTime;
            newA.OrderNumber = oldA.OrderNumber;
            newA.WLId = oldA.WLId;
            newA.WLTaskRevision = oldA.WLTaskRevision;
            newA.WLTaskStatus = oldA.WLTaskStatus;
            newA.ToBuy = oldA.ToBuy;
            newA.IsMajor = oldA.IsMajor;
            newA.StatusId = oldA.StatusId;
        }
        static void CloneProperties(ProjectsMy newP, Projects oldP) {
            newP.Name = oldP.Name;
            newP.DateCreated = oldP.DateCreated;
            newP.StatusId = oldP.StatusId;
            newP.Comment = oldP.Comment;
            newP.CompleteTime = oldP.CompleteTime;
            newP.DesiredResult = oldP.DesiredResult;
            newP.IsSimpleProject = oldP.IsSimpleProject;
        }
    }


}
