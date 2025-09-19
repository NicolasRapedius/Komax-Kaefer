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
        private readonly GridService gridService = new(200, 200);
        private CancellationTokenSource cts;
        private Task simulationTask;
        private int zoom = 4;
        private WriteableBitmap bitmap;
        private bool isRunning;

        public MainWindow()
        {
            InitializeComponent();
            gridService.AddAntToCenter();
            Loaded += (_, _) => { UpdateZoom(); DrawGrid(); };
            ZoomSlider.ValueChanged += (_, _) => { UpdateZoom(); DrawGrid(); };
            PatternButton.Click += PatternButton_Click;
            StartButton.Click += StartButton_Click;
            StopButton.Click += StopButton_Click;
            UpdateButtonStates();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) => StartSimulation();
        private void StopButton_Click(object sender, RoutedEventArgs e) => StopSimulation();

        private void StartSimulation()
        {
            if (isRunning) return;
            isRunning = true;
            UpdateButtonStates();
            cts = new CancellationTokenSource();
            simulationTask = Task.Run(() => RunSimulation(cts.Token));
        }

        private void StopSimulation()
        {
            if (!isRunning) return;
            cts?.Cancel();
            isRunning = false;
            UpdateButtonStates();
        }

        private void PatternButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) return;
            var dialog = new PatternDialogue { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                gridService.SetPattern(dialog.SelectedPattern);
                gridService.GridModel.Ants.Clear();
                gridService.AddAntToCenter();
                DrawGrid();
            }
        }

        private void UpdateButtonStates()
        {
            StartButton.IsEnabled = !isRunning;
            PatternButton.IsEnabled = !isRunning;
            StopButton.IsEnabled = isRunning;
        }

        private void UpdateZoom()
        {
            zoom = (int)ZoomSlider.Value;
            if (ZoomValueText != null)
                ZoomValueText.Text = $"{zoom} px";
        }

        private async void RunSimulation(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var (oldX, oldY, newX, newY, oldValue, newValue) = gridService.StepAnt();
                    Dispatcher.Invoke(DrawGrid);
                   
                        await gridService.PublishStepAsync(oldX, oldY, newX, newY, oldValue, newValue);

                    await Task.Delay(1);
                }
            }
            catch (OperationCanceledException) { }
        }

        // Draws the current grid state to the UI
        private void DrawGrid()
        {
            int width, height, stride;
            byte[] pixels = gridService.GetGridPixels(zoom, out width, out height, out stride);
            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            GridImage.Source = bitmap;
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }
    }
}
