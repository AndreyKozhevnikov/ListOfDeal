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
    
    public partial class Project
    {
        public Project()
        {
            this.Actions = new HashSet<Action>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<bool> IsWork { get; set; }
        public string Comment { get; set; }
        public Nullable<int> TypeId { get; set; }
    
        public virtual ICollection<Action> Actions { get; set; }
        public virtual ProjectType ProjectType { get; set; }
    }
}
