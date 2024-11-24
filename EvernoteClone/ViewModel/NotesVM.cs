using EvernoteClone.Model;
using EvernoteClone.ViewModel.Commands;
using EvernoteClone.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EvernoteClone.ViewModel {
    public class NotesVM : INotifyPropertyChanged {

        public ObservableCollection<Notebook> Notebooks { get; set; }
        private Notebook selectedNotebook;
        public Notebook SelectedNotebook {

            get {

                return selectedNotebook;
            }
            set {

                selectedNotebook = value;
                OnPropertyChanged("SelectedNotebook");
                GetNotes();
            }
        }
        private Note selectedNote;
        public Note SelectedNote {

            get { return selectedNote; }
            set { 

                selectedNote = value;
                OnPropertyChanged("SelectedNote");
                OnSelectedNoteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public ObservableCollection<Note> Notes { get; set; }

        private Visibility isVisible;
        public Visibility IsVisible {

            get { return isVisible; }
            set {

                isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        public NewNotebookCommand NewNotebookCommand { get; set; }
        public NewNoteCommand NewNoteCommand { get; set; }
        public EditCommand EditCommand { get; set; }
        public EndEditingCommand EndEditingCommand { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler OnSelectedNoteChanged;


        public NotesVM() {

            NewNotebookCommand = new NewNotebookCommand(this);
            NewNoteCommand = new NewNoteCommand(this);
            EditCommand = new EditCommand(this);
            EndEditingCommand = new EndEditingCommand(this);

            Notebooks = new ObservableCollection<Notebook>();
            Notes = new ObservableCollection<Note>();

            IsVisible = Visibility.Collapsed;

            GetNotebooks();
        }


        public void CreateNotebook() {

            Notebook newNotebook = new Notebook() {

                Name = "Notebook",
            };
            DatabaseHelper.Insert(newNotebook);
            GetNotebooks();
        }
        public void CreateNote(int notebookId) {

            Note newNote = new Note() {

                NotebookId = notebookId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Title = $"Note for {DateTime.Now.ToString()}"
            };
            DatabaseHelper.Insert(newNote);
            GetNotes();
        }

        private void GetNotebooks() {

            var notebooks = DatabaseHelper.Read<Notebook>();
            Notebooks.Clear();
            foreach (var notebook in notebooks) {

                Notebooks.Add(notebook);
            }
        }

        private void GetNotes() {

            if (SelectedNotebook != null) {

                var notes = DatabaseHelper.Read<Note>().
                    Where(n => n.NotebookId == SelectedNotebook.Id).
                    ToList();
                Notes.Clear();
                foreach (var note in notes) {

                    Notes.Add(note);
                }
            }
        }

        private void OnPropertyChanged(string propertyName) {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StartEditing() {

            IsVisible = Visibility.Visible;
        }

        public void StopEditing(Notebook notebook) {

            IsVisible = Visibility.Collapsed;
            DatabaseHelper.Update(notebook);
            GetNotebooks();
        }
    }
}
