using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Komax_Kaefer.Backend;

namespace Komax_Kaefer
{
    public partial class PatternDialogue : Window
    {
        public StartPattern SelectedPattern { get; private set; } = StartPattern.Empty;

        public PatternDialogue()
        {
            InitializeComponent();
            PreviewEmpty.Source = GeneratePreview(StartPattern.Empty);
            PreviewChessboard.Source = GeneratePreview(StartPattern.Chessboard);
            PreviewRandom.Source = GeneratePreview(StartPattern.Random);
        }

        // Generates a bitmap preview for the given pattern
        private ImageSource GeneratePreview(StartPattern pattern)
        {
            int size = 20;
            bool[,] previewGrid = new bool[size, size];
            var random = new Random(42); // Fixed seed for consistent preview
            switch (pattern)
            {
                case StartPattern.Chessboard:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            previewGrid[x, y] = (x + y) % 2 == 0;
                    break;
                case StartPattern.Random:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            previewGrid[x, y] = random.Next(2) == 0;
                    break;
                case StartPattern.Empty:
                default:
                    break;
            }
            // Generate preview bitmap
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

        private void Empty_Click(object sender, RoutedEventArgs e)
        {
            SelectedPattern = StartPattern.Empty;
            DialogResult = true;
        }
        private void Chessboard_Click(object sender, RoutedEventArgs e)
        {
            SelectedPattern = StartPattern.Chessboard;
            DialogResult = true;
        }
        private void Random_Click(object sender, RoutedEventArgs e)
        {
            SelectedPattern = StartPattern.Random;
            DialogResult = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
