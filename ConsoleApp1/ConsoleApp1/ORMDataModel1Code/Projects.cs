﻿using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace ConsoleApp1.ListOfDealBase {

    public partial class Projects {
        public Projects(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}