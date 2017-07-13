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


        public ICommand SearchCommand => new RelayCommand(SearchAsync, SearchIsEnabled);

        private bool _isSearching = false;
        private bool SearchIsEnabled()
        {
            return !_isSearching;
        }

        private readonly BlockingCollection<SearchJob> _queue = new BlockingCollection<SearchJob>();
        private readonly Model.Search _search = new Search();
        private const string FoundItemsName = nameof(FoundItems);

        private async void SearchAsync()
        {
            _isSearching = true;
            RaisePropertyChanged(nameof(SearchCommand));
            FoundItems.Clear();

            var job = _search.CreateSearchJob(_searchWords, 3);
            _queue.Add(job);
            await Task.Factory.StartNew(Consume);
        }

        private void Consume()
        {
            using (Task task = Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (true)
                    {
                        var job = _queue.Take();
                        var result = await _search.SearchAsync(job);

                        result.Items.ForEach(FoundItems.Add);
                        RaisePropertyChanged(FoundItemsName);

                        if (result.SubJobs.Any())
                            result.SubJobs.ForEach(_queue.Add);
                        else if (!_queue.Any())
                            _queue.CompleteAdding();
                    }
                }
                catch (InvalidOperationException)
                {
                    // An InvalidOperationException means that Take() was called on a completed collection
                    _isSearching = false;
                }
            }))
                Task.WaitAll(task);
        }

        public ObservableCollection<ImpressumItem> FoundItems { get; }

    }
}