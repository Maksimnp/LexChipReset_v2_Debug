using System;
using System.Windows;

namespace LexChipReset_v2
{
    /// <summary>
    /// Interaction logic for Box.xaml
    /// </summary>
    public partial class Box : Window
    {
        public Exception ex;
        public Box()
        {
            InitializeComponent();

        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        public void SetText(string ex_message)
        {
            Textbar.Text = ex_message;
            LabelEvent.Content = " ";
        }
    }
}
