using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Komax_Kaefer.Backend;

namespace Komax_Kaefer
{
    public partial class MainWindow : Window
    {
        private GridModel gridModel;
        private CancellationTokenSource cts;
        private Task simulationTask;
        private int zoom = 4;
        private WriteableBitmap bitmap;
        private bool isRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            gridModel = new GridModel(200, 200);
            AddAntToCenter();
            Loaded += MainWindow_Loaded;
            ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;
            PatternButton.Click += PatternButton_Click;
            StartButton.Click += StartButton_Click;
            StopButton.Click += StopButton_Click;
            UpdateButtonStates();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            zoom = (int)ZoomSlider.Value;
            ZoomValueText.Text = $"{zoom} px";
            DrawGrid();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) return;
            isRunning = true;
            UpdateButtonStates();
            cts = new CancellationTokenSource();
            simulationTask = Task.Run(() => RunSimulation(cts.Token));
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning) return;
            cts?.Cancel();
            isRunning = false;
            UpdateButtonStates();
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            zoom = (int)ZoomSlider.Value;
            if (ZoomValueText != null)
                ZoomValueText.Text = $"{zoom} px";
            DrawGrid();
        }

        private void PatternButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                // Pattern change is now disabled by button state, no warning needed
                return;
            }
            var dialog = new PatternDialogue { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                gridModel.SetPattern(dialog.SelectedPattern);
                gridModel.Ants.Clear();
                AddAntToCenter();
                DrawGrid();
            }
        }

        private void AddAntToCenter()
        {
            var ant = new Ant(gridModel.Width / 2, gridModel.Height / 2, Direction.Up, gridModel.Cells);
            gridModel.AddAnt(ant);
        }

        private void UpdateButtonStates()
        {
            StartButton.IsEnabled = !isRunning;
            PatternButton.IsEnabled = !isRunning;
            StopButton.IsEnabled = isRunning;
        }

        private void RunSimulation(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    gridModel.StepAll();
                    Dispatcher.Invoke(DrawGrid);
                    Thread.Sleep(10);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void DrawGrid()
        {
            int gridWidth = gridModel.Width;
            int gridHeight = gridModel.Height;
            bitmap = new WriteableBitmap(gridWidth * zoom, gridHeight * zoom, 96, 96, PixelFormats.Bgra32, null);
            GridImage.Source = bitmap;
            int width = gridWidth * zoom;
            int height = gridHeight * zoom;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];

            var grid = gridModel.Cells;
            var ants = gridModel.Ants;
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    byte r = 255, g = 255, b = 255; // Default to white
                    bool isAnt = false;
                    foreach (var ant in ants)
                    {
                        if (ant.X == x && ant.Y == y)
                        {
                            r = 0; g = 255; b = 0; // Green for ant
                            isAnt = true;
                            break;
                        }
                    }
                    if (!isAnt)
                    {
                        if (grid[x, y].Color == AntColor.Black)
                        {
                            r = g = b = 0; // Black
                        }
                    }
                    for (int dy = 0; dy < zoom; dy++)
                    {
                        for (int dx = 0; dx < zoom; dx++)
                        {
                            int px = x * zoom + dx;
                            int py = y * zoom + dy;
                            int idx = py * stride + px * 4;
                            pixels[idx + 0] = b;
                            pixels[idx + 1] = g;
                            pixels[idx + 2] = r;
                            pixels[idx + 3] = 255;
                        }
                    }
                }
            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }
    }
}
