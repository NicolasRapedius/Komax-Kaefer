using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Komax_Kaefer
{
    public partial class MusterDialog : Window
    {
        public Startmuster GewaehltesMuster { get; private set; } = Startmuster.Leer;

        public MusterDialog()
        {
            InitializeComponent();
            PreviewLeer.Source = ErzeugeVorschau(Startmuster.Leer);
            PreviewSchachbrett.Source = ErzeugeVorschau(Startmuster.Schachbrett);
            PreviewZufall.Source = ErzeugeVorschau(Startmuster.Zufall);
        }

        // Erzeugt eine Bitmap-Vorschau für das angegebene Muster
        private ImageSource ErzeugeVorschau(Startmuster muster)
        {
            int size = 20;
            bool[,] previewGrid = new bool[size, size];
            var random = new Random(42); // Fester Seed für konsistente Vorschau
            switch (muster)
            {
                case Startmuster.Schachbrett:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            previewGrid[x, y] = (x + y) % 2 == 0;
                    break;
                case Startmuster.Zufall:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            previewGrid[x, y] = random.Next(2) == 0;
                    break;
                case Startmuster.Leer:
                default:
                    break;
            }
            // erzeugt Vorschau-Bitmap
            WriteableBitmap bmp = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
            int stride = size * 4;
            byte[] pixels = new byte[size * size * 4];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int idx = y * stride + x * 4;
                    byte val = previewGrid[x, y] ? (byte)0 : (byte)255;
                    pixels[idx + 0] = val;
                    pixels[idx + 1] = val;
                    pixels[idx + 2] = val;
                    pixels[idx + 3] = 255;
                }
            }
            bmp.WritePixels(new Int32Rect(0, 0, size, size), pixels, stride, 0);
            return bmp;
        }

        private void Leer_Click(object sender, RoutedEventArgs e)
        {
            GewaehltesMuster = Startmuster.Leer;
            DialogResult = true;
        }
        private void Schachbrett_Click(object sender, RoutedEventArgs e)
        {
            GewaehltesMuster = Startmuster.Schachbrett;
            DialogResult = true;
        }
        private void Zufall_Click(object sender, RoutedEventArgs e)
        {
            GewaehltesMuster = Startmuster.Zufall;
            DialogResult = true;
        }
        private void Abbrechen_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
