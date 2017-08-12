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
using Yais.Model.Search;
using Yais.Model.Search.Priorities;

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

        private readonly PriorityQueue<SearchJob> _queue = new PriorityQueue<SearchJob>();
        private readonly Model.SearchEngine _search = new SearchEngine();
        private const string FoundItemsName = nameof(FoundItems);
        private Priority _priority;
        private HashSet<string> _visited;

        private async void SearchAsync()
        {
            _priority = new Priority();
            _visited = new HashSet<string>();
            _isSearching = true;
            RaisePropertyChanged(nameof(SearchCommand));
            FoundItems.Clear();

            var job = _search.CreateSearchJob(_searchWords, _depth);
            _queue.Enqueue(job, _priority.GetBestPrio());
            await Task.Factory.StartNew(Consume);
        }

        private static object _LockFoundHostNames = new object();
        private static object _LockVisited = new object();
        private const int _Threshold = 10000;
        private void Enqueue(SearchJob job)
        {
            var host = job.Link.Uri.Host;
            lock (_LockFoundHostNames)
            {
                if (_foundHostNames.Contains(host))
                    return;
            }

            var prio = _priority.GetPrio(job);
            var absoluteUri = job.Link.Uri.AbsoluteUri;
            if (prio > _Threshold)
                return;

            lock(_LockVisited)
            { 
                if (_visited.Contains(absoluteUri))
                    return;

                _visited.Add(absoluteUri);
                _queue.Enqueue(job, prio);
            }
        }

        private void Consume()
        {
            int count = 1;
            var tasks = new Task[count];
            for (int i = 0; i < tasks.Length; i++)
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

        private static HashSet<string> _foundHostNames = new HashSet<string>();
        private async Task ConsumeAsync()
        {
            int emptyCount = 0;
            try
            {
                while (true)
                {
                    SearchJob job;
                    if (!_queue.TryDequeue(out job))
                    {
                        await Task.Delay(500);
                        if (++emptyCount>50)
                            break;

                        continue;
                    }

                    emptyCount = 0;
                    QueueLength = _queue.Count;
                    var result = await _search.SearchAsync(job);
                    if (result == null)
                        continue;

                    lock (_LockFoundHostNames)
                    {
                        result.Items.ForEach(x => _foundHostNames.Add(x.Host));
                    }

                    result.Items.ForEach(x => AddOnUI(FoundItems, x));
                    //RaisePropertyChanged(FoundItemsName);

                    if (result.SubJobs.Any())
                        result.SubJobs.ForEach(Enqueue);
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