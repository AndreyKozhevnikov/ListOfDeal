using System;
using System.Collections.Generic;
using System.Linq;

namespace WlConnectionLibrary {
    public class Data {
        public List<WLList> lists { get; set; }
        public List<WLTask> tasks { get; set; }
        public List<Reminder> reminders { get; set; }
        public List<Subtask> subtasks { get; set; }
        public List<WLNote> notes { get; set; }
        public List<TaskPosition> task_positions { get; set; }
        public List<SubtaskPosition> subtask_positions { get; set; }
    }
}



