using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ScreenSaver;

using ScreenSaverWpfClient.DataAccess;

namespace ScreenSaverWpfClient.Model
{
    /// <summary>
    /// Модель отображаемого прямоугольника
    /// </summary>
    public class RectangleModel
    {
        private RectanglePoint _prevCoordinate;
        private readonly DoubleAnimation _animationX;
        private readonly DoubleAnimation _animationY;
        private Rectangle _rectangle;
        private readonly RectangleDataProvider _data;
        private TranslateTransform _transform;

        public RectangleModel()
        {
            _data = new RectangleDataProvider();
            StartAnimate = new AsyncRelayCommand<object>(OnStartAnimate);
            _animationX = new DoubleAnimation();
            _animationY = new DoubleAnimation();
            _animationX.Completed += Animation_Completed;
        }


        public int Id { get; set; }

        public RectangleSize Size { get; set; }

        public ICommand StartAnimate { get; }

        public RectanglePoint Coordinate { get; set; }

        private async Task ConfigureAnimation()
        {
            Duration duration = await CalculateDuration();

            _animationX.From = _prevCoordinate.X;
            _animationX.To = Coordinate.X;
            _animationX.Duration = duration;
            _animationY.From = _prevCoordinate.Y;
            _animationY.To = Coordinate.Y;
            _animationY.Duration = duration;
        }

        public async Task OnStartAnimate(object parameter)
        {
            _rectangle = (Rectangle)parameter;
            _transform = (TranslateTransform)_rectangle.Resources["transformation"];

            await MoveToNewCoordinate();
        }

        private async void Animation_Completed(object sender, EventArgs e)
        {
            await MoveToNewCoordinate();
        }

        private async Task MoveToNewCoordinate()
        {
            await UpdateCoordinate();
            await ConfigureAnimation();
            BeginAnimation();
        }

        private void BeginAnimation()
        {
            _transform.BeginAnimation(TranslateTransform.XProperty, _animationX);
            _transform.BeginAnimation(TranslateTransform.YProperty, _animationY);
        }

        private async Task UpdateCoordinate()
        {
            _prevCoordinate = Coordinate;

            Coordinate = await _data.GetNewCoordinate(Id);
        }

        private async Task<Duration> CalculateDuration()
        {
            double timeShift = await CalculateDistance() / 90;

            return new Duration(TimeSpan.FromSeconds(timeShift));
        }

        private async Task<double> CalculateDistance() =>
            await Task.Run(() => Math.Sqrt(2) * Math.Abs(_prevCoordinate.X - Coordinate.X));
    }
}