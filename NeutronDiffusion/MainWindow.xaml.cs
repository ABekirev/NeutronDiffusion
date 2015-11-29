using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using NeutronDiffusion.Logic;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace NeutronDiffusion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly double SigmaSDefaultValue = 100;
        public static readonly double SigmaADefaultValue = 1;
        public static readonly double CosFiDefaultValue = 0.5;
        public static readonly int NeutronNumsDefaultValue = 1000;

        public Enviroment Enviroment { get; private set; } = new Enviroment(SigmaSDefaultValue, SigmaADefaultValue, CosFiDefaultValue) {NeutronNums = NeutronNumsDefaultValue };

        public PlotModel SimulateOnePlotModel { get; private set; }
        
        public PlotModel SimulateBatchMeanFreePathPlotModel { get; private set; }
        public PlotModel SimulateBatchMeanPathPlotModel { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitSimulateOneTab();
            InitSimulateBatchTab();
        }

        public void InitSimulateOneTab()
        {

            SimulateOnePlotModel = new PlotModel
            {
                LegendTitle = "Legend",
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightMiddle,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendBorder = OxyColors.Black,
                Axes =
                {
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "X"
                    },
                    new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Y"
                    }
                }
            };
        }

        public void InitSimulateBatchTab()
        {
            SimulateBatchMeanFreePathPlotModel = new PlotModel
            {
                LegendTitle = "Legend",
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightMiddle,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendBorder = OxyColors.Black,
                Axes =
                {
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Number of neutrons simulated"
                    },
                    new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Mean free path lenght"
                    }
                }
            };
            SimulateBatchMeanPathPlotModel = new PlotModel
            {
                LegendTitle = "Legend",
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightMiddle,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendBorder = OxyColors.Black,
                Axes =
                {
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Number of neutrons simulated"
                    },
                    new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Mean path lenght"
                    }
                }
            };
        }

        public void DrawNeutronWay(Neutron neutron)
        {
            if (neutron.CollisionPoint.Count == 0)
                return;
            
            var neutronCollisionSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                Title = $"Neutron {SimulateOnePlotModel.Series.Count + 1}; MeanFreeLenght: {neutron.AverageFreePathLength}; TotalLenght: {neutron.PathLength}",
                Smooth = false,
            };
            neutron.CollisionPoint.ForEach(p => neutronCollisionSeries.Points.Add(new DataPoint(p.X, p.Y)));
            SimulateOnePlotModel.Series.Add(neutronCollisionSeries);
            SimulateOnePlot.ResetAllAxes();
            SimulateOnePlot.InvalidatePlot();
            neutron.CollisionPoint.ForEach(p => Console.WriteLine(p.X + @"; " + p.Y));
            Console.WriteLine();
        }

        private void StartSimulateOneButton_Click(object sender, RoutedEventArgs e)
        {
            var neutron = Enviroment.SimulateOneNeutron();
            DrawNeutronWay(neutron);
        }

        private void ResetSimulateOneButton_Click(object sender, RoutedEventArgs e)
        {
            SimulateOnePlotModel.Series.Clear();
            SimulateOnePlot.ResetAllAxes();
        }

        private void StartSimulateBatchButton_Click(object sender, RoutedEventArgs e)
        {
            //Enviroment = new Enviroment(Enviroment);
            Enviroment.Neutrons.Clear();
            //var neutrons = Enviroment.SimulateBatchNeutrons();
            var neutrons = new List<Neutron>(Enviroment.NeutronNums);
            for (int i = 0; i < Enviroment.NeutronNums; ++i)
            {
                neutrons.Add(Enviroment.SimulateOneNeutron());
            }
            DrawNeutronMeanFreePathAndMeanPath(neutrons);
        }

        private void DrawNeutronMeanFreePathAndMeanPath(List<Neutron> neutrons)
        {
            SimulateBatchMeanFreePathPlotModel.Series.Clear();
            SimulateBatchMeanPathPlotModel.Series.Clear();
            
            var meanFreePathSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 0,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                Title = @"Practical mean free path",
                Smooth = false,
            };
            var freePathSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 0,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                Title = @"Theoretical mean free path",
                Smooth = false,
            };
            var meanPathSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 0,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                Title = @"Practical mean path",
                Smooth = false,
            };
            var pathSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 0,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                Title = @"Theoretical free path",
                Smooth = false,
            };
            var i = 0;
            var maxMeanFreePath = 0d;
            var minMeanFreePath = 0d;
            var maxMeanPath = 0d;
            var minMeanPath = 0d;
            var totalSteps = 0l;
            var totalPath = 0d;
            var theoreticalFreePath = 1 / Enviroment.SigmaS;
            var theoreticalPath = 1 / Enviroment.SigmaA;
            neutrons.ForEach(neutron =>
            {
                ++i;
                //var meanFreePath = neutron.AverageFreePathLength / i + (i != 1 ? meanFreePathSeries.Points[i - 2].Y * (1 - 1d / i) : 0);
                totalSteps += neutron.FreePathLength.Count - 1;
                totalPath += neutron.PathLength;
                var meanFreePath = totalPath / totalSteps;
                if (meanFreePath > maxMeanFreePath) maxMeanFreePath = meanFreePath;
                else if (meanFreePath < minMeanFreePath) minMeanFreePath = meanFreePath;
                //var meanPath = neutron.PathLength / i + (i != 1 ? meanPathSeries.Points[i - 2].Y * (1 - 1d / i) : 0);
                var meanPath = totalPath / i;
                if (meanPath > maxMeanPath) maxMeanPath = meanFreePath;
                else if (meanPath < minMeanPath) minMeanPath = meanPath;
                meanFreePathSeries.Points.Add(new DataPoint(i, meanFreePath));
                freePathSeries.Points.Add(new DataPoint(i, theoreticalFreePath));
                meanPathSeries.Points.Add(new DataPoint(i, meanPath));
                pathSeries.Points.Add(new DataPoint(i, theoreticalPath));
            });
            SimulateBatchMeanFreePathTextBlock.Text = meanFreePathSeries.Points[i - 1].Y.ToString();
            SimulateBatchMeanPathTextBlock.Text = meanPathSeries.Points[i - 1].Y.ToString();
            SimulateBatchMeanFreePathPlotModel.Series.Add(meanFreePathSeries);
            SimulateBatchMeanFreePathPlotModel.Series.Add(freePathSeries);
            SimulateBatchMeanPathPlotModel.Series.Add(meanPathSeries);
            SimulateBatchMeanPathPlotModel.Series.Add(pathSeries);
            SimulateBatchMeanFreePathPlot.ResetAllAxes();
            SimulateBatchMeanPathPlot.ResetAllAxes();
            SimulateBatchMeanFreePathPlot.InvalidatePlot();
            SimulateBatchMeanPathPlot.InvalidatePlot();

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Confirm_Close(object sender, CancelEventArgs e)
        {
            if (MessageBoxResult.Yes != MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation", MessageBoxButton.YesNo))
            {
                e.Cancel = true;
            }
        }
    }

    public class CosFiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().Replace(",", ".");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number;
            try
            {
                var strValue = ((String)value).Replace(".", ",");
                number = Double.Parse(strValue);
                if (number < -1.0)
                {
                    number = -1.0;
                }
                else if (number > 1.0)
                {
                    number = 1.0;
                }
            }
            catch (FormatException)
            {
                number = 0.0;
            }
            return number;
        }
    }

    public class SigmaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().Replace(",", ".");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var getDefaultNumber = new Func<double>(delegate
            {
                if ("SigmaA".Equals(parameter))
                {
                    return MainWindow.SigmaADefaultValue;
                }
                if ("SigmaS".Equals(parameter))
                {
                    return MainWindow.SigmaSDefaultValue;
                }
                return 1000;
            });
            var getNeededNumber = new Func<double, double>(delegate (double numb)
            {
                if (Math.Abs(numb) < double.Epsilon)
                {
                    return getDefaultNumber();
                }
                if (numb < 0)
                {
                    return -numb;
                }
                return numb;
            });
            double number;
            try
            {
                var strValue = ((string)value).Replace(".", ",");
                number = double.Parse(strValue);
                number = getNeededNumber(number);
            }
            catch (FormatException)
            {
                number = getDefaultNumber();
            }
            return number;
        }
    }

    public class NeutronNumsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var getDefaultNumber = new Func<int>(() => 1000);
            var getNeededNumber = new Func<int, int>(delegate (int numb)
            {
                if (numb < 0)
                {
                    return -numb;
                }
                return numb;
            });
            int number;
            try
            {
                var strValue = ((string)value).Replace(".", ",");
                number = int.Parse(strValue);
                number = getNeededNumber(number);
            }
            catch (FormatException)
            {
                number = getDefaultNumber();
            }
            return number;
        }
    }

    //public class DoubleRangeValidationRule : ValidationRule
    //{
    //    public double Min { get; set; }
    //    public double Max { get; set; }

    //    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    //    {
    //        ValidationResult vResult = ValidationResult.ValidResult;
    //        double parameter = 0;
    //        try
    //        {

    //            if (((string)value).Length > 0) //Check if there is a input in the textbox
    //            {
    //                parameter = Double.Parse((String)value);
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            return new ValidationResult(false, "Illegal characters or " + e.Message);
    //        }

    //        if ((parameter < this.Min) || (parameter > this.Max))
    //        {
    //            return new ValidationResult(false, "Please enter value in the range: " + this.Min + " - " + this.Max + ".");
    //        }
    //        return vResult;
    //    }

    //}
}
