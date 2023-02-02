using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using ScreenSaver;

using ScreenSaverWpfClient.DataAccess;
using ScreenSaverWpfClient.Model;

namespace ScreenSaverWpfClient.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Private Fields
        private int _rectangleCount;

        private CancellationTokenSource _cts = new();
        private RectangleSize _canvasSize;
        private RectangleDataProvider _data;
        private ObservableCollection<RectangleModel> _rectangleCollection;
        private readonly Dictionary<string, List<string>> _errorsByPropertyName;
        #endregion

        #region Public constractions
        public MainViewModel()
        {
            RectangleCollection = new ObservableCollection<RectangleModel>();
            StartCommand = new AsyncRelayCommand(OnStartCommand);
            ExitCommand = new RelayCommand(OnExitCommand);
            _data = new();
            _errorsByPropertyName = new Dictionary<string, List<string>>();
        }

        #endregion

        #region Public Commands
        public ICommand StartCommand { get; }
        public ICommand ExitCommand { get; }
        #endregion

        #region Public Properties
        public ObservableCollection<RectangleModel> RectangleCollection
        {
            get => _rectangleCollection;
            set
            {
                _rectangleCollection = value;
                OnPropertyChanged();
            }
        }

        public RectangleSize CanvasSize
        {
            get => IsCanvasSizeEmpty ? _canvasSize = InitilazeCanvasBoundaries() : _canvasSize;
            set
            {
                if (!_canvasSize.Equals(value))
                    _canvasSize = value;
            }
        }
        private bool IsCanvasSizeEmpty => _canvasSize is null;
        #endregion

        #region Public Methods
        public void OnErrorChanged(string propertyName)
        {
            if (propertyName != null)
                ErrorsChanged.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void GetNewCoordinate()
        {

        }
        #endregion

        #region Private Methods
        private RectangleSize InitilazeCanvasBoundaries() => Task.Run(() => _data.InitilazeCanvasBoundariesAsync()).Result;

        private async Task OnStartCommand()
        {
            await _data.GetRectangleList(RectangleCollection);
        }

        private void OnExitCommand()
        {
            _data.GenerateCancelationToken();
            System.Windows.Application.Current.Shutdown();
        }

        private void AddErrors(string propName, string error)
        {
            if (!_errorsByPropertyName.ContainsKey(propName))
                _errorsByPropertyName[propName] = new();

            if (!_errorsByPropertyName[propName].Contains(error))
            {
                _errorsByPropertyName[propName].Add(error);
                OnErrorChanged(propName);
            }
        }

        private void ClearErrors(string propName)
        {
            if (_errorsByPropertyName.ContainsKey(propName))
            {
                _errorsByPropertyName.Remove(propName);
                OnErrorChanged(propName);
            }
        }
        #endregion

        #region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors => _errorsByPropertyName.Any();

        public IEnumerable GetErrors(string propertyName) =>
            _errorsByPropertyName.Where(n => n.Key == propertyName).Select(e => e.Value);
        #endregion

        #region INotifyPropertyChanged contract implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
