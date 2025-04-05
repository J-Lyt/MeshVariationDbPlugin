using Frosty.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MeshVariationDbPlugin.Windows
{
    /// <summary>
    /// Interaction logic for FindAndReplaceWindow.xaml
    /// </summary>
    public partial class FindAndReplaceWindow : FrostyDockableWindow
    {
        public uint SurfaceShaderId { get; private set; } = 0;
        public uint SurfaceShaderId2 { get; private set; } = 0;
        public Guid SurfaceShaderGuid { get; private set; } = Guid.Empty;
        public Guid SurfaceShaderGuid2 { get; private set; } = Guid.Empty;
        public bool isShader { get; private set; } = true;
        public FindAndReplaceWindow()
        {
            InitializeComponent();

            randomHashButton.IsEnabled = false;

            doneButton.IsEnabled = false;
        }

        bool isValidId = false;
        bool isValidId2 = false;
        bool isValidGuid = false;
        bool isGenerated = false;

        private void surfaceShaderIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValid = UInt32.TryParse(surfaceShaderIdTextBox.Text, out _);

            if (isValid)
            {
                isValidId = true;
            }
            else
            {
                isValidId = false;
            }

            DoneButton_Enable();
        }

        private void surfaceShaderIdTextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValid = UInt32.TryParse(surfaceShaderIdTextBox2.Text, out _);

            if (isValid)
            {
                isValidId2 = true;
            }
            else
            {
                isValidId2 = false;
            }

            if (isGenerated && surfaceShaderIdTextBox2.Text == "")
            {
                surfaceShaderIdTextBox2.WatermarkText = "VariationAssetNameHash";
            }

            DoneButton_Enable();
        }

        private void surfaceShaderGuidTextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValid = Guid.TryParse(surfaceShaderGuidTextBox2.Text, out _);

            if (isValid)
            {
                isValidGuid = true;
            }
            else
            {
                isValidGuid = false;
            }

            DoneButton_Enable();
        }

        public void DoneButton_Enable()
        {
            if(isValidId && isValidId2 && isValidGuid)
            {
                doneButton.IsEnabled = true;
            }
            else if (isValidId && isValidId2 && !isShader)
            {
                doneButton.IsEnabled = true;
            }
            else
            {
                doneButton.IsEnabled = false;
            }
        }

        private void doneButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            if (isShader)
            {
                SurfaceShaderId = UInt32.Parse(surfaceShaderIdTextBox.Text);
                SurfaceShaderId2 = UInt32.Parse(surfaceShaderIdTextBox2.Text);
                SurfaceShaderGuid2 = Guid.Parse(surfaceShaderGuidTextBox2.Text);
            }
            else if (!isShader)
            {
                SurfaceShaderId = UInt32.Parse(surfaceShaderIdTextBox.Text);
                SurfaceShaderId2 = UInt32.Parse(surfaceShaderIdTextBox2.Text);
            }
            
            
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShaderRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (surfaceShaderIdTextBox != null)
            {
                surfaceShaderIdTextBox.Text = "";
                surfaceShaderIdTextBox2.Text = "";
                surfaceShaderGuidTextBox2.Text = "";

                surfaceShaderIdTextBox.WatermarkText = "SurfaceShaderId";
                surfaceShaderIdTextBox2.WatermarkText = "SurfaceShaderId";
                surfaceShaderGuidTextBox2.Visibility = System.Windows.Visibility.Visible;
                surfaceShaderGuidTextBox2.IsEnabled = true;
                randomHashButton.IsEnabled = false;

                isShader = true;
            }
        }

        private void NamehashRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (surfaceShaderIdTextBox != null)
            {
                surfaceShaderIdTextBox.Text = "";
                surfaceShaderIdTextBox2.Text = "";
                surfaceShaderGuidTextBox2.Text = "";

                surfaceShaderIdTextBox.WatermarkText = "VariationAssetNameHash";
                surfaceShaderIdTextBox2.WatermarkText = "VariationAssetNameHash";
                surfaceShaderGuidTextBox2.Visibility = System.Windows.Visibility.Collapsed;
                surfaceShaderGuidTextBox2.IsEnabled = false;
                randomHashButton.IsEnabled = true;

                isShader = false;
            }
        }

        private void RandomHashButton_Click(object sender, RoutedEventArgs e)
        {
            isGenerated = true;

            Random random = new();

            uint thirtyBits = (uint)random.Next(1 << 30);
            uint twoBits = (uint)random.Next(1 << 2);
            uint fullRange = (thirtyBits << 2) | twoBits;

            surfaceShaderIdTextBox2.WatermarkText = "";
            surfaceShaderIdTextBox2.Text = fullRange.ToString();
        }
    }
}
