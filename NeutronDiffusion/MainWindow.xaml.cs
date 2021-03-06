﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        public static readonly int NumberOfSectorsDefaultValue = 10;

        public Enviroment Enviroment { get; private set; } = new Enviroment(SigmaSDefaultValue, SigmaADefaultValue, CosFiDefaultValue) {NeutronNums = NeutronNumsDefaultValue };

        public PlotModel SimulateOnePlotModel { get; private set; }
        
        public PlotModel SimulateBatchMeanFreePathPlotModel { get; private set; }
        public PlotModel SimulateBatchMeanPathPlotModel { get; private set; }
        public PlotModel SimulateBatchDistributionPlotModel { get; private set; }
        public int NumberOfSectors { get; set; } = NumberOfSectorsDefaultValue;
        public List<Material> Materials = new List<Material>(); 
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitMaterialList();
            InitSimulateOneTab();
            InitSimulateBatchTab();
            if (!File.Exists("theory.html"))
            {
                helpTabItem.Visibility = Visibility.Hidden;
                return;
            }
            string curDir = Directory.GetCurrentDirectory();
            wb.Source = new Uri(String.Format("file:///{0}/theory.html", curDir));
        }

        public class Material
        {
            public string Caption { get; set; }
            public double SigmaS { get; set; }
            public double SigmaA { get; set; }
            public double CosFi { get; set; }

            public Material(string caption, double sigmaA, double sigmaS, double cosFi)
            {
                this.Caption = caption;
                this.SigmaA = sigmaA;
                this.SigmaS = sigmaS;
                this.CosFi = cosFi;
            }

            public override string ToString()
            {
                return this.Caption;
            }
        }

        private void InitMaterialList()
        {
            if (!File.Exists("materials.txt"))
                return;
            var materials = File.ReadLines("materials.txt");
            foreach (var material in materials)
            {
                var tmp = material.Split(';');
                try
                {
                    Materials.Add(new Material(tmp[0], convertToDouble(tmp[1]), convertToDouble(tmp[2]), convertToDouble(tmp[3])));
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            MaterialList.ItemsSource = Materials;
        }

        public void InitSimulateOneTab()
        {

            SimulateOnePlotModel = new PlotModel
            {
                Title = "Траектории нейтронов",
                //LegendTitle = "Legend",
                //LegendOrientation = LegendOrientation.Vertical,
                //LegendPlacement = LegendPlacement.Outside,
                //LegendPosition = LegendPosition.RightMiddle,
                //LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                //LegendBorder = OxyColors.Black,
                Axes =
                {
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "X, см"
                    },
                    new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Y, см"
                    }
                }
            };
        }

        public void InitSimulateBatchTab()
        {
            SimulateBatchMeanFreePathPlotModel = new PlotModel
            {
                Title = "Зависимость средней длины свободного пробега до рассеяния от количества итераций",
                //LegendTitle = "Legend",
                //LegendOrientation = LegendOrientation.Vertical,
                //LegendPlacement = LegendPlacement.Inside,
                //LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                //LegendBorder = OxyColors.Black,
                Axes =
                {
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Количество судеб нейтронов, ед"
                    },
                    new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Средняя длина свободного\nпробега до рассеяния, см"
                    }
                }
            };
            SimulateBatchMeanPathPlotModel = new PlotModel
            {
                Title = "Зависимость средней длины свободного пробега до поглощения от количества итераций",
                //LegendTitle = "Legend",
                //LegendOrientation = LegendOrientation.Vertical,
                //LegendPlacement = LegendPlacement.Inside,
                //LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                //LegendBorder = OxyColors.Black,
                Axes =
                {
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Количество итераций, ед"
                    },
                    new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Средняя длина свободного\nпробега до поглощения, см"
                    }
                }
            };
            SimulateBatchDistributionPlotModel = new PlotModel
            {
                Title = "Распределение плотности\nпотока нейтронов",
                Axes =
                {
                    new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = "Плотность потока нейтронов,\nед/(см^2*с)"
                    },
                    new CategoryAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "Растояние от точечного источника, см"
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
                //Title = $"Neutron {(SimulateOnePlotModel.Series.Count + 1).ToString("E2")}; MeanFreeLenght: {(neutron.AverageFreePathLength).ToString("E2")}; TotalLenght: {(neutron.PathLength).ToString("E2")}",
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
        
        private void StartSimulateBatchResetScales_Click(object sender, RoutedEventArgs e)
        {
            SimulateBatchResetScales();
        }

        private void DrawNeutronMeanFreePathAndMeanPath(List<Neutron> neutrons)
        {
            SimulateBatchMeanFreePathPlotModel.Series.Clear();
            SimulateBatchMeanPathPlotModel.Series.Clear();
            SimulateBatchDistributionPlotModel.Series.Clear();
            
            var meanFreePathSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 0,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                //Title = @"Practical mean free path",
                Smooth = false,
            };
            var freePathSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 0,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                //Title = @"Theoretical mean free path",
                Smooth = false,
            };
            var meanPathSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 0,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                //Title = @"Practical mean path",
                Smooth = false,
            };
            var pathSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 0,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                //Title = @"Theoretical free path",
                Smooth = false,
            };
            var neutronsDistributionSeries = new BarSeries();
			var neutronsDistributionSeriesTheoretical = new LineSeries
			{
				StrokeThickness = 2,
				MarkerSize = 0,
				CanTrackerInterpolatePoints = false,
				Smooth = true,
			};

			var i = 0;
            var maxMeanFreePath = 0d;
            var minMeanFreePath = 0d;
            var maxMeanPath = 0d;
            var minMeanPath = 0d;
            var totalSteps = 0L;
            var totalPath = 0d;
            var theoreticalFreePath = 1 / Enviroment.SigmaS;
            var theoreticalPath = 1 / Enviroment.SigmaA;
            var r = 0d;
            var neutronDistanceList = new List<double>(neutrons.Count);
            freePathSeries.Points.Add(new DataPoint(i, theoreticalFreePath));
            pathSeries.Points.Add(new DataPoint(i, theoreticalPath));
            neutrons.ForEach(neutron =>
            {
                ++i;
                totalSteps += neutron.FreePathLength.Count - 1;
                totalPath += neutron.PathLength;
                var distance = neutron.CollisionPoint[neutron.CollisionPoint.Count - 1].DistanceTo(neutron.CollisionPoint[0]);
                neutronDistanceList.Add(distance);
                if (distance > r) r = distance;
                var meanFreePath = totalPath / totalSteps;
                if (meanFreePath > maxMeanFreePath) maxMeanFreePath = meanFreePath;
                else if (meanFreePath < minMeanFreePath) minMeanFreePath = meanFreePath;
                var meanPath = totalPath / i;
                if (meanPath > maxMeanPath) maxMeanPath = meanFreePath;
                else if (meanPath < minMeanPath) minMeanPath = meanPath;
                meanFreePathSeries.Points.Add(new DataPoint(i, meanFreePath));
                meanPathSeries.Points.Add(new DataPoint(i, meanPath));
            });
            freePathSeries.Points.Add(new DataPoint(i, theoreticalFreePath));
            pathSeries.Points.Add(new DataPoint(i, theoreticalPath));
            var sectorWidth = r / NumberOfSectors;
            if (sectorWidth < double.Epsilon) sectorWidth = 0.001;
            var getSectorNumber = new Func<double, double, int>(delegate (double distance, double widthOfSecor)
            {
                var sectorNumber = 0;
                var sectorRadius = widthOfSecor;
                while (distance + double.Epsilon > sectorRadius)
                {
                    sectorRadius += widthOfSecor;
                    ++sectorNumber;
                }
                return sectorNumber;
            });
            for (var sectorNumber = 0; sectorNumber < NumberOfSectors; ++sectorNumber)
            {
                neutronsDistributionSeries.Items.Add(new BarItem(0, sectorNumber));
            }
            neutronDistanceList.ForEach(distance =>
            {
                var neutronSectorNumber = getSectorNumber(distance, sectorWidth);
                if (neutronSectorNumber < NumberOfSectors) neutronsDistributionSeries.Items[neutronSectorNumber].Value += 1;
            });
            for (var k = 0; k < neutronsDistributionSeries.Items.Count; ++k)
            {
                neutronsDistributionSeries.Items[k].Value /= Math.PI*(Math.Pow(sectorWidth * (k + 1), 2) - Math.Pow((sectorWidth * k),2));
	            double y = sectorWidth*(k + 1);
	            double x = Enviroment.NeutronsDistribution(y);
                neutronsDistributionSeriesTheoretical.Points.Add(new DataPoint(x, k));
			}

            SimulateBatchMeanFreePathTextBlock.Text = meanFreePathSeries.Points[i - 1].Y.ToString("E2");
            SimulateBatchMeanPathTextBlock.Text = meanPathSeries.Points[i - 1].Y.ToString("E2");

            SimulateBatchMeanFreePathPlotModel.Series.Add(meanFreePathSeries);
            SimulateBatchMeanFreePathPlotModel.Series.Add(freePathSeries);

            SimulateBatchMeanPathPlotModel.Series.Add(meanPathSeries);
            SimulateBatchMeanPathPlotModel.Series.Add(pathSeries);

            SimulateBatchDistributionPlotModel.Series.Add(neutronsDistributionSeries);
			SimulateBatchDistributionPlotModel.Series.Add(neutronsDistributionSeriesTheoretical);

            SimulateBatchDistributionPlotModel.Axes[1] =
                new CategoryAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Растояние от точечного источника",
                    LabelFormatter = value => ((value + 1) * sectorWidth).ToString("E2")
                };

            SimulateBatchResetScales();
        }

        private void SimulateBatchResetScales()
        {

            SimulateBatchMeanFreePathPlot.ResetAllAxes();
            SimulateBatchMeanPathPlot.ResetAllAxes();
            SimulateBatchDistributionPlotModel.ResetAllAxes();

            SimulateBatchMeanFreePathPlot.InvalidatePlot();
            SimulateBatchMeanPathPlot.InvalidatePlot();
            SimulateBatchDistributionPlotModel.InvalidatePlot(true);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Confirm_Close(object sender, CancelEventArgs e)
        {
            if (MessageBoxResult.Yes != MessageBox.Show("Вы точно хотите выйти?", "Подтверждение выхода", MessageBoxButton.YesNo))
            {
                e.Cancel = true;
            }
        }

        private void MaterialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var material = (Material)e.AddedItems[0];
            SigmaAValue.Text = material.SigmaA.ToString();
            Enviroment.SigmaA = material.SigmaA;
            SigmaSValue.Text = material.SigmaS.ToString();
            Enviroment.SigmaS = material.SigmaS;
            //CosFiValue.Text = material.CosFi.ToString();
            //Enviroment.CosFi = material.CosFi;
            SimulateBatchTheoreticalMeanFreePathTextBlock.Text = (1d / material.SigmaS).ToString();
            SimulateBatchTheoreticalMeanPathTextBlock.Text = (1d / material.SigmaA).ToString();
        }

        private double convertToDouble(string doubleStr)
        {
            return Convert.ToDouble(doubleStr.Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator == "," ? "." : ",", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
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

    public class NonNegativeIntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var getDefaultNumber = new Func<int>(() => 100);
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

    public static class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html",
            typeof(string),
            typeof(BrowserBehavior),
            new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser wb = d as WebBrowser;
            if (wb != null)
                wb.NavigateToString(e.NewValue as string);
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
