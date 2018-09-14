using System;
using System.Collections.Generic;
using System.Linq;

namespace WlConnectionLibrary {
    public interface IWLConnector {
        WLTask ChangeTitleOfTask(string wlId, string newName, int revision);
        WLTask ChangeScheduledTime(string wlId, string dueTime, int revision);
        WLTask ChangeStarredOfTask(string wlId, bool isMajor, int revision);
        List<WLList> GetAllLists();
        List<WLTask> GetTasksForList(int listId);
        List<WLTask> GetCompletedTasksForList(int listId);
        WLTask GetTask(string taskId);
        List<WLNote> GetNodesForTask(string taskId);
        WLTask CreateTask(string title, int listId, DateTime? dueDate, bool isMajor);
        WLTask ChangeListOfTask(string wlId, int listId, int revision);
        WLTask UpdateTask(WLTask task);
        WLTask CompleteTask(string wlId);
        WLNote CreateNote(string taskId, string content);
        WLNote UpdateNoteContent(string noteId, int revision, string content);
        void DeleteNote(string noteId, int revision);
        void DeleteTask(WLTask task);
        string GetBackup();
        event EventHandler<UnhandledExceptionEventArgs> ConnectionErrorEvent;
    }
}



