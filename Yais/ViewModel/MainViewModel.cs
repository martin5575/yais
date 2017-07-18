using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Yais.Model;
using System.Collections.Generic;
using System.Windows;

namespace Yais.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _searchWords = "Lohnunternehmer";
            _depth = 4;
            FoundItems = new ObservableCollection<ImpressumItem>();
        }

        private string _searchWords;
        public string SearchWords
        {
            get { return _searchWords; }
            set
            {
                _searchWords = value;
                RaisePropertyChanged();
            }
        }

        private int _depth;
        public int Depth
        {
            get { return _depth; }
            set
            {
                _depth = value;
                RaisePropertyChanged();
            }
        }

        private int _queueLength;
        public int QueueLength
        {
            get { return _queueLength; }
            set
            {
                _queueLength = value;
                RaisePropertyChanged();
            }
        }


        public ICommand SearchCommand => new RelayCommand(SearchAsync, SearchIsEnabled);

        private bool _isSearching = false;
        private bool SearchIsEnabled()
        {
            return !_isSearching;
        }

        private readonly BlockingCollection<SearchJob> _queue = new BlockingCollection<SearchJob>();
        private readonly Model.SearchEngine _search = new SearchEngine();
        private const string FoundItemsName = nameof(FoundItems);

        private async void SearchAsync()
        {
            _isSearching = true;
            RaisePropertyChanged(nameof(SearchCommand));
            FoundItems.Clear();

            var job = _search.CreateSearchJob(_searchWords, _depth);
            _queue.Add(job);
            await Task.Factory.StartNew(Consume);
        }

        private void Consume()
        {
            var tasks = new Task[3];
            for (int i = 0; i < 3; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    await ConsumeAsync();
                });
            }
            try
            {
                Task.WaitAll(tasks);
            }
            finally
            {
                foreach (var task in tasks)
                    task.Dispose();
            }
            _isSearching = false;
        }

        private async Task ConsumeAsync()
        {
            try
            {
                while (true)
                {
                    var job = _queue.Take();
                    QueueLength = _queue.Count;
                    var result = await _search.SearchAsync(job);

                    result.Items.ForEach(x => AddOnUI(FoundItems, x));
                    //RaisePropertyChanged(FoundItemsName);

                    if (result.SubJobs.Any())
                        result.SubJobs.ForEach(_queue.Add);
                    else if (!_queue.Any())
                        _queue.CompleteAdding();
                    QueueLength = _queue.Count;
                }
            }
            catch (InvalidOperationException)
            {
                // An InvalidOperationException means that Take() was called on a completed collection
            }
        }

        public static void AddOnUI<T>(ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        public ObservableCollection<ImpressumItem> FoundItems { get; }

    }
}