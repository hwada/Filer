using Filer.Repositories;
using Microsoft.Xaml.Behaviors.Media;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace Filer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CompositeDisposable _disposables = new();

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

            LeftPaneViewModel.MoveDirectory(GetDirectory(Settings.Default.LeftDirectory));
            RightPaneViewModel.MoveDirectory(GetDirectory(Settings.Default.RightDirectory));

            LeftPaneViewModel.NextPane = RightPaneViewModel;
            RightPaneViewModel.NextPane = LeftPaneViewModel;
        }

        /// <summary>
        /// 引数のdirが存在しなければMyDocumentのパスを返す
        /// </summary>
        private static string GetDirectory(string dir)
        {
            return Directory.Exists(dir) ? dir : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
            HistoryRepository.Instance.Save();
            CommandRepository.Instance.Save();

            Settings.Default.WindowLeft = (int)Left;
            Settings.Default.WindowTop = (int)Top;
            Settings.Default.WindowWidth = (int)Width;
            Settings.Default.WindowHeight = (int)Height;
            Settings.Default.LeftDirectory = LeftPaneViewModel.FullPath.Value;
            Settings.Default.RightDirectory = RightPaneViewModel.FullPath.Value;
            Settings.Default.Save();

            _disposables.Dispose();
        }
    }
}
