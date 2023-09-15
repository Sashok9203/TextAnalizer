using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Windows.UIElement;

namespace TextAnalizer.Models
{
    internal class WindowModel : INotifyPropertyChanged
    {
        private enum AnalizeState
        {
            Idle,
            Started,
            Paused
        }
        
        private int? wordsCount;
        private int? sentencesCount;
        private int? symbolsCount;
        private int? qSentencesCount;
        private int? exSentencesCount;
        private bool wordsCountCheck = true;
        private bool sentencesCountCheck = true;
        private bool symbolsCountCheck = true;
        private bool qSentencesCountCheck = true;
        private bool exSentencesCountCheck = true;
        private bool fileOutputCheck = false;
        private string? text = null;
        private CancellationTokenSource tokenSource;
        private CancellationToken token;
        private ManualResetEvent pause;

        private List<Task> tasks;
        private Action[] actions;
      

        private AnalizeState state = AnalizeState.Idle;

        private AnalizeState State
        {
            get => state;
            set 
            {
                state = value;
                OnPropertyChanged("PRButtonName");
                OnPropertyChanged("SSButtonName");
                OnPropertyChanged("TextEnable");
            }
        }

        private void resetResult()
        {
            WordsCount = null;
            SentencesCount = null;
            SymbolsCount = null;
            QSentencesCount = null;
            ExSentencesCount = null;
        }

        private async void startStop()
        {
            if (State == AnalizeState.Idle)
            {
                tokenSource = new();
                token = tokenSource.Token;
                resetResult();
                State = AnalizeState.Started;
                bool[] analizators = new bool[5] { WordsCountCheck, SentencesCountCheck, SymbolsCountCheck, QSentencesCountCheck, ExSentencesCountCheck };
                tasks.Clear();
                for (int i = 0; i < analizators.Length; i++)
                    if (analizators[i]) tasks.Add(Task.Factory.StartNew(actions[i]));

                foreach (var item in tasks)
                    try { await item.WaitAsync(token); } catch { }

                State = AnalizeState.Idle;
            }
            else
            {
                tokenSource.Cancel();
                if (State == AnalizeState.Paused)
                    pause.Set();
                State = AnalizeState.Idle;
                tokenSource?.Dispose();                                                                    
            }
        }
        private int charCount(string text,char chr)
        {
            int count = 0;
            foreach (var ch in text)
            {
                pause.WaitOne();
                if (token.IsCancellationRequested) return -1;
                if (ch == chr) count++;
                Thread.Sleep(1);
            }
            return count;
        }
        private void pauseResume()
        {
            switch (State)
            {
                case AnalizeState.Started:
                    pause.Reset();
                    State = AnalizeState.Paused;
                    break; 
                case AnalizeState.Paused:
                    pause.Set();
                    State = AnalizeState.Started;
                    break;
                default:
                    Application.Current.Shutdown();
                    break;
            }
        }

        public bool TextEnable => State == AnalizeState.Idle;
        
        public bool WordsCountCheck 
        {
            get => wordsCountCheck;
            set 
            {
                wordsCountCheck = value;
                if (wordsCountCheck) WordsCountVisibility = Visibility.Visible;
                else WordsCountVisibility = Visibility.Collapsed;
                OnPropertyChanged("WordsCountVisibility");
                resetResult();
            }
        }
        public bool SentencesCountCheck
        {
            get => sentencesCountCheck;
            set
            {
                sentencesCountCheck = value;
                if (sentencesCountCheck) SentencesCountVisibility = Visibility.Visible;
                else SentencesCountVisibility = Visibility.Collapsed;
                OnPropertyChanged("SentencesCountVisibility");
                resetResult();
            }
        }
        public bool SymbolsCountCheck
        {
            get => symbolsCountCheck;
            set
            {
                symbolsCountCheck = value;
                if (symbolsCountCheck) SymbolsCountVisibility = Visibility.Visible;
                else SymbolsCountVisibility = Visibility.Collapsed;
                OnPropertyChanged("SymbolsCountVisibility");
                resetResult();
            }
        }
        public bool QSentencesCountCheck
        {
            get => qSentencesCountCheck;
            set
            {
                qSentencesCountCheck = value;
                if (qSentencesCountCheck) QSentencesCountVisibility = Visibility.Visible;
                else QSentencesCountVisibility = Visibility.Collapsed;
                OnPropertyChanged("QSentencesCountVisibility");
                resetResult();
            }
        }
        public bool ExSentencesCountCheck
        {
            get => exSentencesCountCheck;
            set
            {
                exSentencesCountCheck = value;
                if (exSentencesCountCheck) ExSentencesCountVisibility = Visibility.Visible;
                else ExSentencesCountVisibility = Visibility.Collapsed;
                OnPropertyChanged("ExSentencesCountVisibility");
                resetResult();
            }
        }
        public bool FileOutputCheck
        {
            get => fileOutputCheck;
            set
            {
                fileOutputCheck = value;
                if (fileOutputCheck) StatisticVisibility = Visibility.Collapsed;
                else StatisticVisibility = Visibility.Visible;
                OnPropertyChanged("StatisticVisibility");
            }
        }

        public int? WordsCount 
        { 
            get => wordsCount;
            set
            {
                wordsCount = value;
                OnPropertyChanged();
            }
        }
        public int? SentencesCount
        {
            get => sentencesCount;
            set
            {
                sentencesCount = value;
                OnPropertyChanged();
            }
        }
        public int? SymbolsCount
        {
            get => symbolsCount;
            set
            {
                symbolsCount = value;
                OnPropertyChanged();
            }
        }
        public int? QSentencesCount
        {
            get => qSentencesCount;
            set
            {
                qSentencesCount = value;
                OnPropertyChanged();
            }
        }
        public int? ExSentencesCount
        {
            get => exSentencesCount;
            set
            {
                exSentencesCount = value;
                OnPropertyChanged();
            }
        }
        public string? Text
        {
            get => text;
            set 
            {
                text = value;
                OnPropertyChanged();
            }
        }

        public string SSButtonName => state == AnalizeState.Idle ? "Старт" :"Стоп";
        public string PRButtonName => state == AnalizeState.Paused ? "Продовжити" : state == AnalizeState.Started ? "Пауза" : "Вихід";
     

        public Visibility WordsCountVisibility { get; set; }
        public Visibility SentencesCountVisibility { get; set; }
        public Visibility SymbolsCountVisibility { get; set; }
        public Visibility QSentencesCountVisibility { get; set; }
        public Visibility ExSentencesCountVisibility { get; set; }
        public Visibility StatisticVisibility { get; set; }

        public WindowModel()
        {
            pause = new(true);
            tasks = new();
            actions = new Action[5] 
            {
                new(()=>   //Words count
                {
                    int wordsCount = 0;
                    bool wordStart = false;
                    foreach (var chr in Text)
                    {
                        pause.WaitOne();
                        if(token.IsCancellationRequested) return;
                        if (Char.IsLetter(chr) || Char.IsDigit(chr)) wordStart = true;
                        else if (wordStart)
                        {
                            wordStart = false;
                            wordsCount++;
                        }
                        Thread.Sleep(1);
                    }
                    if (wordStart) wordsCount++;
                    Application.Current.Dispatcher.Invoke( new Action(()=> WordsCount = wordsCount  ));
                }),
                new(()=>   //Sentences count
                {
                    int sentencesCount = charCount(Text,'.');
                    if(sentencesCount == -1) return;
                    Application.Current.Dispatcher.Invoke( new Action(()=> SentencesCount = sentencesCount  ));
                }),
                new(()=> //Symbols count
                {
                    int symbolsCount = 0;
                    foreach (var ch in Text)
                    {
                       pause.WaitOne();
                       if(token.IsCancellationRequested) return;
                       if(Char.IsLetter(ch)) symbolsCount++;
                       Thread.Sleep(1);
                    }
                     Application.Current.Dispatcher.Invoke( new Action(()=> SymbolsCount = symbolsCount  ));
                }),
                new(()=> //Interrogative sentences count
                { 
                    int qSentencesCount = charCount(Text,'?');
                     if(qSentencesCount == -1) return;
                    Application.Current.Dispatcher.Invoke( new Action(()=> QSentencesCount = qSentencesCount  ));
                }),
                new(()=> //Exclamatory sentences count
                {
                    int exSentencesCount = charCount(Text,'!');
                    if(exSentencesCount == -1) return;
                    Application.Current.Dispatcher.Invoke( new Action(()=> ExSentencesCount = exSentencesCount  ));
                })
            };
        }


        public RelayCommand StartStop => new((o) => { startStop(); },(o)=> !string.IsNullOrEmpty(Text) && (WordsCountCheck || SentencesCountCheck || SymbolsCountCheck || QSentencesCountCheck || ExSentencesCountCheck) );
        public RelayCommand PauseResume => new((o) => { pauseResume(); });

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
