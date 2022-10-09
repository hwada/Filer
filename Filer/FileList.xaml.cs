using Filer.WinAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

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
        public FileListViewModel ViewModel => (FileListViewModel)DataContext;

        /// <summary>
        /// ファイルリストにフォーカスを持たせる
        /// </summary>
        public void Activate()
        {
            FileListBox.Focus();
        }

        /// <summary>
        /// リストボックス上でのキーダウンイベント
        /// </summary>
        /// <param name="sender">イベント送信元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            //TODO: コードビハインド書かずにやりくりする

            switch (e.Key)
            {
                case Key.Escape:
                    ViewModel.IsSearchMode.Value = false;
                    ViewModel.SearchText.Value = "";
                    FileListBox.Focus();
                    e.Handled = true;
                    return;
                case Key.F:
                    ViewModel.IsSearchMode.Value = true;
                    SearchTextBox.Focus();
                    e.Handled = true;
                    return;
                case Key.OemPeriod:
                    ShowContextMenu();
                    e.Handled = true;
                    return;
                case Key.K:
                    CreateNewDirectory();
                    e.Handled = true;
                    break;
            }

            ViewModel.OnKeyDown(e);
        }

        /// <summary>
        /// 新しいディレクトリを作成する
        /// </summary>
        private void CreateNewDirectory()
        {
            var window = new InputBox { Owner = Window.GetWindow(this) };
            if (window.ShowDialog() == true)
            {
                ViewModel.CreateNewDirectory(window.InputText);
            }
        }

        /// <summary>
        /// 選択したオブジェクトに対応するコンテキストメニューを表示する
        /// </summary>
        private void ShowContextMenu()
        {
            if (ViewModel.SelectedItem.Value == null)
            {
                return;
            }

            IShellFolder shellFolder;
            var idl = NativeMethods.ILCreateFromPath(ViewModel.SelectedItem.Value.Info.FullName);
            NativeMethods.SHBindToParent(idl, new Guid(ShellIIDGuid.IShellFolder), out shellFolder, out var idlRelative);
            if (shellFolder != null)
            {
                var contextMenu = NativeMethods.CreatePopupMenu();
                var idlList = new IntPtr[] { idlRelative };
                uint reserved = 0;
                shellFolder.GetUIObjectOf(IntPtr.Zero, 1, idlList, new Guid(ShellIIDGuid.IContextMenu), ref reserved, out var unknown);
                if (unknown is IContextMenu2 contextMenu2)
                {
                    var parent = new WindowInteropHelper(Window.GetWindow(this));
                    var item = (ListBoxItem)(FileListBox.ItemContainerGenerator.ContainerFromIndex(FileListBox.SelectedIndex));
                    var pos = item.PointToScreen(new Point(0, item.ActualHeight));

                    contextMenu2.QueryContextMenu(contextMenu, 0, 1, 0xffff, 0);
                    NativeMethods.TrackPopupMenu(contextMenu, 0x100, (int)pos.X, (int)pos.Y, 0, parent.Handle, IntPtr.Zero);

                    Marshal.ReleaseComObject(contextMenu2);
                }
                Marshal.ReleaseComObject(shellFolder);
            }
            NativeMethods.CoTaskMemFree(idl);
        }

        /// <summary>
        /// 検索ボックス上でのキーダウンイベント
        /// </summary>
        /// <param name="sender">イベント送信元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                case Key.Enter:
                    ViewModel.IsSearchMode.Value = false;
                    ViewModel.SearchText.Value = "";
                    e.Handled = true;
                    break;
                case Key.Up:
                    ViewModel.OnKeyDown(e);
                    break;
                case Key.Down:
                    ViewModel.OnKeyDown(e);
                    break;
            }
        }

        /// <summary>
        /// リストボックスにフォーカスがきたとき
        /// </summary>
        /// <param name="sender">イベント送信元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void FileListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.IsActive.Value = true;
        }
    }
}
