using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace ConsoleApp1.ListOfDealMy {

    public partial class GeneralXpObject1 {
        public GeneralXpObject1(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
