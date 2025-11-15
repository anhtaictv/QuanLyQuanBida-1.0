using CommunityToolkit.Mvvm.ComponentModel;
using QuanLyQuanBida.Core.Entities;
using System;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class TableViewModel : ObservableObject
    {
        public Table Table { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Status))]
        [NotifyPropertyChangedFor(nameof(IsSessionActive))]
        [NotifyPropertyChangedFor(nameof(ElapsedTime))] 
        private Session? _currentSession;

        [ObservableProperty]
        private string _elapsedTime = "00:00:00";

        public string Name => Table.Name;

        public string Status => CurrentSession != null ? "Occupied" : Table.Status;

        public string Zone => Table.Zone ?? "Khu vực chung";

        public bool IsSessionActive => CurrentSession != null;

        public TableViewModel(Table table)
        {
            Table = table;
        }

        public void UpdateElapsedTime()
        {
            if (CurrentSession != null)
            {
                var duration = DateTime.UtcNow - CurrentSession.StartAt;

                if (CurrentSession.PauseAt.HasValue)
                {
                    if (!CurrentSession.ResumeAt.HasValue || CurrentSession.ResumeAt < CurrentSession.PauseAt)
                    {
                        duration = CurrentSession.PauseAt.Value - CurrentSession.StartAt;
                    }
                    else if (CurrentSession.ResumeAt.HasValue)
                    {
                        duration = DateTime.UtcNow - CurrentSession.StartAt;
                    }
                }

                ElapsedTime = duration.ToString(@"hh\:mm\:ss");
            }
            else
            {
                ElapsedTime = "00:00:00";
            }
        }
    }
}