using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Komax_Kaefer;

namespace Komax_Kaefer
{
    public partial class MainWindow : Window
    {
        private const int GridWidth = 200;
        private const int GridHeight = 200;
        private bool[,] grid;
        private Kafer kafer;
        private int zoom = 4;
        private WriteableBitmap bitmap;
        private CancellationTokenSource cts;
        private Task simulationTask;

        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            zoom = (int)ZoomSlider.Value;
            ZoomValueText.Text = $"{zoom} px";
            InitGrid();
            DrawGrid();
        }

        // Initialisiert das Grid, den Käfer und das Bitmap für die Anzeige
        private void InitGrid()
        {
            grid = new bool[GridWidth, GridHeight];
            kafer = new Kafer(GridWidth / 2, GridHeight / 2, Direction.Up, grid);
            bitmap = new WriteableBitmap(GridWidth * zoom, GridHeight * zoom, 96, 96, PixelFormats.Bgra32, null);
            GridImage.Source = bitmap;
        }

        // Zeichnet das Grid und den Käfer auf das Bitmap
        private void DrawGrid()
        {
            int width = GridWidth * zoom;
            int height = GridHeight * zoom;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];

            if (kafer == null)
                return;

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    byte r, g, b;
                    if (kafer.X == x && kafer.Y == y)
                    {
                        r = 0; g = 255; b = 0; // Grün für Käfer
                    }
                    else if (grid[x, y])
                    {
                        r = g = b = 0; // Schwarz
                    }
                    else
                    {
                        r = g = b = 255; // Weiß
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

        //Aktualisiert Zoom und Darstellung.
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            zoom = (int)ZoomSlider.Value;
            if (ZoomValueText != null)
                ZoomValueText.Text = $"{zoom} px";
            bitmap = new WriteableBitmap(GridWidth * zoom, GridHeight * zoom, 96, 96, PixelFormats.Bgra32, null);
            if (GridImage != null)
                GridImage.Source = bitmap;
            DrawGrid();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            ZoomSlider.IsEnabled = false;
            cts = new CancellationTokenSource();
            simulationTask = Task.Run(() => RunSimulation(cts.Token));
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            cts?.Cancel();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            ZoomSlider.IsEnabled = true;
        }

        private void RunSimulation(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (kafer == null)
                        continue;
                    kafer.Schritt();
                    Dispatcher.Invoke(DrawGrid);
                    Thread.Sleep(10);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}