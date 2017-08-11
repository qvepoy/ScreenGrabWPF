﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace screengrab.Windows
{
    public partial class EditWindow : Window
    {
        Image image = new Image();
        PaintType paintType;
        public EditWindow(Image image)
        {
            InitializeComponent();
            this.image = image;
            editCanvas.Children.Add(this.image);
            ConfigurateWindowSize(this.image);
            paintType = PaintType.Pencil;
        }

        void ConfigurateWindowSize(Image image) {
            // Added pixels for windows border and fields
            int addedHeight = 78, addedWidth = 56;
            if (image.Source.Height < 175) {
                this.Height = 175 + addedHeight;
            } else if (image.Source.Height > 700) {
                this.Height = 700 + addedHeight;
            } else {
                this.Height = image.Source.Height + addedHeight;
            }

            if (image.Source.Width < 270) {
                this.Width = 270 + addedWidth;
            } else if (image.Source.Width > 1000) {
                this.Width = 1000 + addedWidth;
            } else {
                this.Width = image.Source.Width + addedWidth;
            }

            editCanvas.Height = image.Source.Height;
            editCanvas.Width = image.Source.Width;
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetImage(GetImageFromCanvas(editCanvas,1).Frames[0]);
            this.Close();
        }

        private void SaveImageAs_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "image"; // Default file name
            saveFileDialog.DefaultExt = ".png"; // Default file extension
            saveFileDialog.Filter = "Png (*.png)|*.png|JPG(*.jpg)|*.jpg|BMP (*.bmp)|*.bmp";
            if (saveFileDialog.ShowDialog() == true) {
                BitmapEncoder enc = GetImageFromCanvas(editCanvas, saveFileDialog.FilterIndex);
                using (var stm = System.IO.File.Create(saveFileDialog.FileName)) {
                    enc.Save(stm);
                }
            }
        }
        
        public BitmapEncoder GetImageFromCanvas( Canvas surface, int format) {
           
            // Get the size of canvas
            Size size = new Size(surface.Width, surface.Height);
            
            var scale = 1;//100/96d;
            surface.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var sz = surface.DesiredSize;
            var rect = new Rect(sz);
            surface.Arrange(rect);
            var bmp = new RenderTargetBitmap((int)(scale * (rect.Width)),
                                             (int)(scale * (rect.Height)),
                                              scale * 96,
                                              scale * 96,
                                              PixelFormats.Default);
            bmp.Render(surface);
            
            BitmapEncoder enc = null;
            switch (format) {
                case 1: // PNG
                    enc = new PngBitmapEncoder();
                    break;
                case 2: // JPG
                    enc = new JpegBitmapEncoder();
                    break;
                case 3: // BMP
                    enc = new BmpBitmapEncoder();
                    break;
            }

            enc.Frames.Add(BitmapFrame.Create(bmp));
            return enc;
        }

        // For painting by pencil
        Point currentPoint = new Point();

        // For painting by rectangle
        Point firstPoint = new Point();
        Rectangle rect;

        public enum PaintType
        {
            Pencil,
            Rect
        }
        
        private void editCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed) {
                currentPoint = e.GetPosition(this);
            }
            switch (paintType) {
                case PaintType.Pencil:
                    
                    break;

                case PaintType.Rect:
                    firstPoint = e.GetPosition(this);

                    rect = new Rectangle() {
                        StrokeThickness = 2,
                        Stroke = Brushes.Red
                    };

                    Canvas.SetLeft(rect, firstPoint.X);
                    Canvas.SetTop(rect, firstPoint.Y);

                    editCanvas.Children.Add(rect);
                    break;
            }
        }

        private void editCanvas_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                switch(paintType) {
                    case PaintType.Pencil:

                        Line line = new Line();

                        line.Stroke = new SolidColorBrush(Colors.Red);
                        line.StrokeThickness = 3;
                        line.X1 = currentPoint.X;
                        line.Y1 = currentPoint.Y;
                        line.X2 = e.GetPosition(this).X;
                        line.Y2 = e.GetPosition(this).Y;

                        currentPoint = e.GetPosition(this);

                        editCanvas.Children.Add(line);

                        break;

                    case PaintType.Rect:
                        if (e.LeftButton == MouseButtonState.Released || rect == null)
                            return;

                        currentPoint = e.GetPosition(this);

                        var x = Math.Min(currentPoint.X, firstPoint.X);
                        var y = Math.Min(currentPoint.Y, firstPoint.Y);

                        var w = Math.Max(currentPoint.X, firstPoint.X) - x;
                        var h = Math.Max(currentPoint.Y, firstPoint.Y) - y;
                        
                        rect.Width = w;
                        rect.Height = h;

                        Canvas.SetLeft(rect, x);
                        Canvas.SetTop(rect, y);

                        break;
                }
            }
        }

        private void editCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
            rect = null;
        }

        private void ButtonPaintPen_Click(object sender, RoutedEventArgs e) {
            paintType = PaintType.Pencil;
        }

        private void ButtonPaintRect_Click(object sender, RoutedEventArgs e) {
            paintType = PaintType.Rect;
        }

        
    }
}
