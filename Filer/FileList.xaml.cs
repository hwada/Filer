using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        /// キーダウンイベント
        /// </summary>
        /// <param name="sender">イベント送信元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            //TODO: コードビハインド書かずにやりくりする
            if (DataContext is FileListViewModel vm)
            {
                vm.OnKeyDown(e);
            }
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
