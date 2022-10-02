using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Filer
{
    /// <summary>
    /// ファイルリストペイン
    /// </summary>
    public sealed partial class FileList : UserControl
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FileList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// VMを取得する
        /// </summary>
        internal FileListViewModel ViewModel => (FileListViewModel)DataContext;

        /// <summary>
        /// キーダウンイベント
        /// </summary>
        /// <param name="sender">イベント送信元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            //TODO: コードビハインド書かずにやりくりする

            switch (e.Key)
            {
                case Key.H:
                    {
                        // 何か別のウィンドウ使う時のプラクティスがあったような
                        var window = new HistoryWindow() { Owner = Window.GetWindow(this) };
                        if (window.ShowDialog() == true)
                        {
                            var dir = window.ViewModel.GetSelectedDirectory();
                            if (Directory.Exists(dir))
                            {
                                ViewModel.MoveDirectory(dir);
                            }
                        }
                    }
                    e.Handled = true;
                    return;
            }

            ViewModel.OnKeyDown(e);
        }

        /// <summary>
        /// ファイルリストにフォーカスを持たせる
        /// </summary>
        public void Activate()
        {
            FileListBox.Focus();
        }
    }
}
