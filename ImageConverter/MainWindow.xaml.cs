using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Drawing.Imaging;
using System.Drawing;
using System.Diagnostics;

namespace ImageConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            var dlg = new Microsoft.Win32.OpenFileDialog
                          {
                              DefaultExt = ".jpeg",
                              Filter =
                                  "JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|TiFF Files (*.tiff)|*.tiff"
                          };


            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result != true) return;

            // Open document 
            var filename = dlg.FileName;
            textImage.Text = filename;
        }


        /// <summary>
        /// http://code.msdn.microsoft.com/windowsdesktop/CSTiffImageConverter-92ac2358/sourcecode?fileId=25010&pathId=199825375
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ConvertTiffToJpeg(string fileName)
        {
            using (var imageFile = Image.FromFile(fileName))
            {
                var frameDimensions =
                    new FrameDimension(imageFile.FrameDimensionsList[0]);

                // Gets the number of pages from the tiff image (if multipage) 
                var frameNum =
                    imageFile.GetFrameCount(frameDimensions);

                var jpegPath = "";


                imageFile.SelectActiveFrame(frameDimensions, 0);
                using (var bmp = new Bitmap(imageFile))
                {
                    jpegPath =
                        String.Format("{0}\\{1}{2}.jpg",
                                      Path.GetDirectoryName(fileName),
                                      Path.GetFileNameWithoutExtension(fileName),
                                      0);

                    bmp.Save(jpegPath, ImageFormat.Jpeg);
                }

                //for (int frame = 0; frame < frameNum; frame++)
                //{
                //    // Selects one frame at a time and save as jpeg. 
                //    imageFile.SelectActiveFrame(frameDimensions, frame);
                //    using (var bmp = new Bitmap(imageFile))
                //    {
                //        jpegPaths[frame] = String.Format("{0}\\{1}{2}.jpg",
                //            Path.GetDirectoryName(fileName),
                //            Path.GetFileNameWithoutExtension(fileName),
                //            frame);

                //        bmp.Save(jpegPaths[frame], ImageFormat.Jpeg);
                //    }
                //}

                return jpegPath;
            }
        }

        private bool FormValidation()
        {
            //validation
            var w = 0;
            if (!int.TryParse(width.Text, out w))
            {
                MessageBox.Show("Width must be an integer values", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(height.Text, out w))
            {
                MessageBox.Show("Height must be an integer values", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(dpi.Text, out w))
            {
                MessageBox.Show("Dpi must be an integer values", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                var image =
                    Image.FromFile(textImage.Text);
                image.Dispose();
            }
            catch (Exception)
            {
                MessageBox.Show("An error occured while trying to load the image. Please try again.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (!FormValidation())
            {
                return;
            }

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            var imagePath = textImage.Text;

            //var ext = Path.GetExtension(imagePath);

            using (var bmThumb = new Bitmap(imagePath))
            {
                var imageDimensions =
                    GetDimensions(bmThumb.Width, bmThumb.Height, double.Parse(width.Text),
                                  double.Parse(height.Text));

                var newFilePath = String.Format("{0}\\{1}_converted.jpg",
                            Path.GetDirectoryName(imagePath),
                            Path.GetFileNameWithoutExtension(imagePath));
                ResizeImage(imageDimensions.Item1, imageDimensions.Item2, imagePath, newFilePath);

                stopwatch.Stop();
                MessageBox.Show("Succesfully converted (" + stopwatch.ElapsedMilliseconds + " ms)", "Message",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        public string ResizeImage(int newWidth, int newHeight, string filePath, string toFileName)
        {
            try
            {
                var image = System.Drawing.Image.FromFile(filePath);
                var thumbnailBitmap = new Bitmap(newWidth, newHeight);

                var thumbnailGraph = Graphics.FromImage(thumbnailBitmap);
                thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbnailGraph.DrawImage(image, imageRectangle);

                thumbnailBitmap.SetResolution(int.Parse(dpi.Text), int.Parse(dpi.Text));
                thumbnailBitmap.Save(toFileName, ImageFormat.Jpeg);

                thumbnailGraph.Dispose();
                //thumbnailBitmap.Dispose();
                image.Dispose();
                thumbnailBitmap.Dispose();
                return toFileName;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private Tuple<int, int> GetDimensions(double imgWidth, double imgHeight, double toWidth, double toHeight)
        {
            var xScale = 0.0;
            var yScale = 0.0;
            var newWidth = 0.0;
            var newHeight = 0.0;

            //Calculate scale
            if (toWidth > 0)
            {
                xScale = imgWidth / toWidth;
            }
            if (toHeight > 0)
            {
                yScale = imgHeight / toHeight;
            }

            // Calculate new width and height
            if (toHeight > 0)
            {
                if (yScale > xScale)
                {
                    newWidth = Math.Round(imgWidth * (1 / yScale));
                    newHeight = Math.Round(imgHeight * (1 / yScale));
                }
                else
                {
                    newWidth = Math.Round(imgWidth * (1 / xScale));
                    newHeight = Math.Round(imgHeight * (1 / xScale));
                }
            }
            else
            {
                newWidth = Math.Round(imgWidth * (1 / xScale));
                newHeight = Math.Round(imgHeight * (1 / xScale));
            }

            if (toWidth > imgWidth && toHeight > imgHeight)
            {
                return new Tuple<int, int>((int)Math.Round(imgWidth), (int)Math.Round(imgHeight));
            }
            return new Tuple<int, int>((int)Math.Round(newWidth), (int)Math.Round(newHeight));
        }
    }
}
