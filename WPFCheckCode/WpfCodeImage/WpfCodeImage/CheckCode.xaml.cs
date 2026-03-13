using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfCodeImage
{
    /// <summary>
    /// CheckCode.xaml 的交互逻辑
    /// </summary>
    public partial class CheckCode : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(CheckCode),
            new FrameworkPropertyMetadata(null));
        /// <summary>
        /// 随机生成的验证码
        /// </summary>
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public CheckCode()
        {
            InitializeComponent();
            this.Loaded += CheckCode_Loaded;
        }

        private void CheckCode_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshCode();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);

                RefreshCode();
            }

        }

        //复用Random对象，避免重复new
        private static readonly Random _rand = new Random();
        private static string CreateCode(int strLength)
        {
            var strCode = "abcdefhkmnprstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            if (strLength <= 0 || strLength > strCode.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(strLength), "请求生成的验证码个数不合法！");
            }

            var _charArray = strCode.ToCharArray();
            var randomCode = "";

            for (int i = 0; i < strLength; i++)
            {
                int t = _rand.Next(strCode.Length);

                //去除掉重复的字符，保证验证码的唯一性
                if (!string.IsNullOrWhiteSpace(randomCode))
                {
                    while (randomCode.IndexOf(_charArray[t].ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        t = _rand.Next(strCode.Length);
                    }
                }
                randomCode += _charArray[t];
            }
            return randomCode;
        }
        private ImageSource CreateCheckCodeImage(string checkCode, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(checkCode))
                return null;
            if (width <= 0 || height <= 0)
                return null;
            DrawingVisual drawingVisual = new DrawingVisual();

            //Random random = new Random(Guid.NewGuid().GetHashCode());

            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                //1.外边框使用width和height
                dc.DrawRectangle(Brushes.White, new Pen(Brushes.Silver, 1D), new Rect(new Size(width, height)));

                //2.字体大小根据图片高度来设置，保证字体占满图片
                double fontSize = height * 0.6;

                FormattedText formattedText = new FormattedText(checkCode,
                    System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Arial"), FontStyles.Oblique, FontWeights.Bold, FontStretches.Normal),
                    fontSize, new LinearGradientBrush(Colors.Green, Colors.DarkRed, 1.2D))
                {
                    MaxLineCount = 1,
                    TextAlignment = TextAlignment.Justify,
                    Trimming = TextTrimming.CharacterEllipsis
                };

                //3.文字居中
                double textWidth = formattedText.WidthIncludingTrailingWhitespace;
                double textHeight = formattedText.Height;

                double X = (width - textWidth) / 2;
                double Y = (height - textHeight) / 2;

                dc.DrawText(formattedText, new Point(X, Y));

                //4.干扰线
                int lineCount = width / 20;
                Pen linePen = new Pen(Brushes.Silver, 0.5D);

                for (int i = 0; i < lineCount; i++)
                {
                    dc.DrawLine(linePen, new Point(_rand.Next(width), _rand.Next(height)), new Point(_rand.Next(width), _rand.Next(height)));
                }

                //5.干扰点
                int noiseCount = width * height / 60;
                for (int i = 0; i < noiseCount; i++)
                {
                    byte r = (byte)_rand.Next(0, 256);
                    byte g = (byte)_rand.Next(0, 256);
                    byte b = (byte)_rand.Next(0, 256);

                    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
                    Pen noisePen = new Pen(brush, 1D);

                    int px = _rand.Next(width);
                    int py = _rand.Next(height);

                    dc.DrawLine(noisePen, new Point(px, py), new Point(px + 1, py + 1));
                }

            }

            //6.将DrawingVisual转换成BitmapSource
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);
            return BitmapFrame.Create(renderBitmap);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RefreshCode();
        }

        private void RefreshCode()
        {
            ImageSource = CreateCheckCodeImage(CreateCode(4), (int)this.Width, (int)this.Height);
        }
    }
}
