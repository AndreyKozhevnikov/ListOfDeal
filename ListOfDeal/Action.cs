//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ListOfDeal
{
    using System;
    using System.Collections.Generic;
    
    public partial class Action
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> ScheduledTime { get; set; }
        public Nullable<int> DelegatedTo { get; set; }
        public string Comment { get; set; }
        public int StatusId { get; set; }
        public Nullable<int> TriggerId { get; set; }
        public Nullable<System.DateTime> CompletedTime { get; set; }
        public bool IsActive { get; set; }
        public string OrderNumber { get; set; }
    
        public virtual DelegatePerson DelegatePerson { get; set; }
        public virtual Project Project { get; set; }
        public virtual ActionStatus ActionStatus { get; set; }
        public virtual ActionTrigger ActionTrigger { get; set; }
    }
}
