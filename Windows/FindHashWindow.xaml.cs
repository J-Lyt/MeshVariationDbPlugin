using Frosty.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MeshVariationDbPlugin.Windows
{
    /// <summary>
    /// Interaction logic for FindHashWindow.xaml
    /// </summary>
    public partial class FindHashWindow : FrostyDockableWindow
    {
        public uint NameHash { get; private set; } = 0;

        public FindHashWindow()
        {
            InitializeComponent();

            doneButton.IsEnabled = false;
        }

        private void nameHashTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValid = UInt32.TryParse(nameHashTextBox.Text, out _);

            if (isValid)
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

            NameHash = UInt32.Parse(nameHashTextBox.Text);
            
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
