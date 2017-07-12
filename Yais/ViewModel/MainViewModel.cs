using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private async void SearchAsync()
        {
            _isSearching = true;
            RaisePropertyChanged(nameof(SearchCommand));
            FoundItems.Clear();

            var search = new Model.Search();
            var job = search.CreateSearchJob(_searchWords, 3);

            var result = await search.SearchAsync(job);
            result.Items.ForEach(FoundItems.Add);
            RaisePropertyChanged(nameof(FoundItems));
            Parallel.ForEach(result.SubJobs, async x =>
            {
                var abc = await search.SearchAsync(x);
                //abc.Items.ForEach(FoundItems.Add);
                //RaisePropertyChanged(nameof(FoundItems));
            });
            _isSearching = false;
        }

        public ObservableCollection<ImpressumItem> FoundItems { get; }

    }
}