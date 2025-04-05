using Frosty.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MeshVariationDbPlugin.Windows
{
    /// <summary>
    /// Interaction logic for ReplaceShadersWindow.xaml
    /// </summary>
    public partial class ReplaceShadersWindow : FrostyDockableWindow
    {
        public Guid Guid { get; private set; } = Guid.Empty;
        public bool isAll { get; private set; } = true;
        public bool isMesh { get; private set; } = false;
        public bool isMaterial { get; private set; } = false;
        public ReplaceShadersWindow()
        {
            InitializeComponent();

            guidTextBox.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void guidTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValid = Guid.TryParse(guidTextBox.Text, out _);

            if (isValid)
            {
                doneButton.IsEnabled = true;
                statusImage.Source = new BitmapImage(new Uri("/MeshVariationDbPlugin;component/Images/AlphaStatusHappy.png", UriKind.Relative));
            }
            else
            {
                doneButton.IsEnabled = false;
                statusImage.Source = new BitmapImage(new Uri("/MeshVariationDbPlugin;component/Images/AlphaStatusSad.png", UriKind.Relative));
            }
        }

        private void doneButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            if (isMesh || isMaterial)
            {
                Guid = Guid.Parse(guidTextBox.Text);
            }

            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AllRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (guidTextBox != null)
            {
                doneButton.IsEnabled = true;

                guidTextBox.Text = "";
                guidTextBox.IsEnabled = false;
                guidTextBox.Visibility = System.Windows.Visibility.Collapsed;

                statusImage.Source = new BitmapImage(new Uri("/MeshVariationDbPlugin;component/Images/AlphaStatusHappy.png", UriKind.Relative));

                isAll = true;
                isMesh = false;
                isMaterial = false;
            }
        }

        private void MeshRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (guidTextBox != null)
            {
                doneButton.IsEnabled = false;

                guidTextBox.Text = "";
                guidTextBox.IsEnabled = true;
                guidTextBox.Visibility = System.Windows.Visibility.Visible;

                statusImage.Source = new BitmapImage(new Uri("/MeshVariationDbPlugin;component/Images/AlphaStatusSad.png", UriKind.Relative));

                isAll = false;
                isMesh = true;
                isMaterial = false;
            }
        }

        private void MaterialRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (guidTextBox != null)
            {
                doneButton.IsEnabled = false;

                guidTextBox.Text = "";
                guidTextBox.IsEnabled = true;
                guidTextBox.Visibility = System.Windows.Visibility.Visible;

                statusImage.Source = new BitmapImage(new Uri("/MeshVariationDbPlugin;component/Images/AlphaStatusSad.png", UriKind.Relative));

                isAll = false;
                isMesh = false;
                isMaterial = true;
            }
        }
    }
}
