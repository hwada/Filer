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

namespace Filer
{
    /// <summary>
    /// InputBox.xaml の相互作用ロジック
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            TextBox.Focus();
        }

        public string InputText
        {
            get => TextBox.Text;
            set => TextBox.Text = value;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    DialogResult = false;
                    break;
                case Key.Enter:
                    DialogResult = true;
                    break;
            }
        }
    }
}
