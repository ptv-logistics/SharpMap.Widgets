using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ToursAndStops
{
    /// <summary>
    /// Interaction logic for Balloon.xaml
    /// </summary>
    public partial class Balloon : UserControl
    {
        public Balloon()
        {
            InitializeComponent();
        }

        #region dependency property Color 
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(Balloon), 
            new PropertyMetadata(Colors.Black, new PropertyChangedCallback(OnColorPropertyChanged)));

        // synchronize the (external) Color property to the internal path object
        private static void OnColorPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((Balloon)obj).ballonPath.Fill = new SolidColorBrush((Color)args.NewValue);
        }
        #endregion

        #region dependency property Text
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Balloon), 
            new PropertyMetadata("", new PropertyChangedCallback(OnTextPropertyChanged)));

        // synchronize the (external) Text property to the internal text object
        private static void OnTextPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((Balloon)obj).textBox.Text = (string)args.NewValue;
        }

        #endregion
    }
}
