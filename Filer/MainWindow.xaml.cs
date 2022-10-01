using System;
using System.IO;
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

            Left = Settings.Default.WindowLeft;
            Top = Settings.Default.WindowTop;
            Width = Settings.Default.WindowWidth;
            Height = Settings.Default.WindowHeight;

            if (Directory.Exists(Settings.Default.LeftDirectory))
            {
                LeftPaneViewModel.MoveDirectory(Settings.Default.LeftDirectory);
            }
            else
            {
                LeftPaneViewModel.MoveDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }

            if (Directory.Exists(Settings.Default.RightDirectory))
            {
                RightPaneViewModel.MoveDirectory(Settings.Default.RightDirectory);
            }
            else
            {
                RightPaneViewModel.MoveDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
        }

        /// <summary>
        /// 左側ペインのVM
        /// </summary>
        FileListViewModel LeftPaneViewModel => LeftPane.ViewModel;

        /// <summary>
        /// 右側のペインのVM
        /// </summary>
        FileListViewModel RightPaneViewModel => RightPane.ViewModel;

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

        /// <summary>
        /// ウィンドウを閉じる際のイベント
        /// </summary>
        /// <param name="sender">イベント送信元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            Settings.Default.WindowLeft = (int)Left;
            Settings.Default.WindowTop = (int)Top;
            Settings.Default.WindowWidth = (int)Width;
            Settings.Default.WindowHeight = (int)Height;
            Settings.Default.LeftDirectory = LeftPaneViewModel.Path.Value;
            Settings.Default.RightDirectory = RightPaneViewModel.Path.Value;
            Settings.Default.Save();
        }
    }
}
