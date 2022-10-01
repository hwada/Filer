using System;
using System.Windows;
using System.Windows.Input;

namespace Filer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初期化、ウィンドウを描画した後のイベント
        /// </summary>
        /// <param name="e">イベント送信元オブジェクト</param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            LeftPane.Activate();
        }

        /// <summary>
        /// キーダウンイベント
        /// </summary>
        /// <param name="sender">イベント送信元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    LeftPane.Activate();
                    e.Handled = true;
                    break;
                case Key.Right:
                    RightPane.Activate();
                    e.Handled = true;
                    break;
            }
        }
    }
}
