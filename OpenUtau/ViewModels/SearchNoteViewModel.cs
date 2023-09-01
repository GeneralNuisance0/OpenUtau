﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.MusicTheory;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpenUtau.App.ViewModels {
    class SearchNoteViewModel : ViewModelBase {
        [Reactive] public string SearchWord { get; set; } = "";
        [Reactive] public int Count { get; private set; }
        public ReactiveCommand<string, Unit> SelectCommand { get; }

        NotesViewModel notesViewModel { get; }
        List<UNote> notes = new List<UNote>();
        int selection = -1;
        static string searchWord = "";

        public SearchNoteViewModel(NotesViewModel notesViewModel) {
            this.notesViewModel = notesViewModel;
            SearchWord = searchWord;

            this.WhenAnyValue(x => x.SearchWord)
                .Subscribe(s => {
                    Search();
                });
            SelectCommand = ReactiveCommand.Create<string>(select => {
                switch (select) {
                    case "prev":
                        Prev();
                        break;
                    case "next":
                        Next();
                        break;
                    case "all":
                        SelectAll();
                        break;
                }
            });
    }

        void Search() {
            if (!string.IsNullOrEmpty(SearchWord) && notesViewModel.Part != null) {
                notes = notesViewModel.Part.notes.Where(note => note.lyric.Contains(SearchWord)).ToList();
                Count = notes.Count();
            } else {
                notes.Clear();
                Count = 0;
            }
            selection = -1;
        }

        public void Prev() {
            if (notes.Count() == 0) {
                selection = -1;
            } else if (notes.Count() == 1) {
                selection = 0;
            } else if (selection <= 0 || selection > notes.Count()) {
                selection = notes.Count() - 1;
            } else {
                selection--;
            }
            if (selection >= 0) {
                var note = notes[selection];
                notesViewModel.SelectNote(note);
                if (notesViewModel.Part != null) {
                    DocManager.Inst.ExecuteCmd(new FocusNoteNotification(notesViewModel.Part, note));
                }
            }
        }

        public void Next() {
            if(notes.Count() == 0) {
                selection = -1;
            } else if (notes.Count() == 1) {
                selection = 0;
            } else if (selection + 1 >= notes.Count()) {
                selection = 0;
            } else {
                selection++;
            }
            if (selection >= 0) {
                var note = notes[selection];
                notesViewModel.SelectNote(note);
                if (notesViewModel.Part != null) {
                    DocManager.Inst.ExecuteCmd(new FocusNoteNotification(notesViewModel.Part, note));
                }
            }
        }

        public void SelectAll() {
            notesViewModel.Selection.SelectNone();
            foreach (var note in notes) {
                notesViewModel.Selection.Add(note);
            }
            MessageBus.Current.SendMessage(new NotesSelectionEvent(notesViewModel.Selection));
        }

        public void OnClose() {
            searchWord = SearchWord;
        }
    }
}
