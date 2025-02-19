﻿
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Shapes = Microsoft.UI.Xaml.Shapes;

using WinUIStreamingClient.Core.Contracts.Services;
using Microsoft.UI;
using Windows.UI;
using Windows.Foundation;

namespace WinUIStreamingClient.ViewModels;

[ObservableObject]
public partial class MainViewModel
{
    private readonly SensorClient _sensorClient;
    private CancellationTokenSource _cancellationTokenSource = new();
    private Canvas? _canvas;

    public MainViewModel(SensorClient sensorClient)
    {
        _sensorClient = sensorClient;
    }

    public void InitCanvas(Canvas canvas)
    {
        _canvas = canvas;
        Line line1 = new() { X1 = 100, Y1 = 100, X2 = 400, Y2 = 400 };
        canvas.Children.Add(line1);
    }


    [ICommand]
    private async Task StartAsync()
    {
        static PathSegmentCollection GetNewPath(Canvas canvas, Point startPoint, int ix)
        {
            Color[] colors =
            {
                Colors.Black,
                Colors.Red,
                Colors.Blue,
                Colors.Green
            };

            PathSegmentCollection pathSegments = new();
            Shapes.Path path = new();
            path.Stroke = new SolidColorBrush(colors[ix % 4]);
            path.StrokeThickness = 3;
            PathFigureCollection figures = new();
            figures.Add(new PathFigure { Segments = pathSegments, StartPoint = startPoint });
            path.Data = new PathGeometry { Figures = figures };
            canvas.Children.Add(path);
            return pathSegments;
        }

        if (_canvas is null) throw new InvalidOperationException("Initialize Canvas first");
        _canvas.Children.Clear();
        _cancellationTokenSource = new();

        try
        {
            double height = _canvas.ActualHeight;
            double multiplicator = height / 50;

            int colorNumber = 0;

            PathSegmentCollection? segments = null;
            int x = 10;
          
            await _sensorClient.StartAsync(_cancellationTokenSource.Token);
            var stream = _sensorClient.StreamAsync(_cancellationTokenSource.Token);
            await foreach (var data in stream)
            {
                double xVal = x += 4;
                double yVal = height - (data.Value * multiplicator);
                if ((segments is null) || (xVal >= _canvas.ActualWidth))
                {
                    x = 10;
                    xVal = x += 4;
                    Point startPoint = new(xVal, yVal);
                    segments = GetNewPath(_canvas, startPoint, colorNumber++);
                }
                else
                {
                    LineSegment segment = new();
                    segment.Point = new(xVal, yVal);
                    segments.Add(segment);
                }
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    [ICommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }
}
