using System.ComponentModel;
using System.Runtime.CompilerServices;
//using System.Windows;

using ScreenSaver;

namespace ScreenSaverWpfClient.Model
{
    /// <summary>
    /// Модель отображаемого прямоугольника
    /// </summary>
    internal class RectangleModel : INotifyPropertyChanged
    {
        private RectanglePoint _coordinate;

        public int Id { get; set; }

        public RectangleSize Size { get; set; }

        public RectanglePoint Coordinate
        {
            get => _coordinate;
            set
            {
                _coordinate = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged contract implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); 
        #endregion
    }
}