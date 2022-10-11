using System;
using System.Collections.Generic;
using System.Linq;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.IO;
using System.Reactive.Disposables;
using System.Windows.Input;
using Prism.Mvvm;
using Reactive.Bindings.Extensions;
using System.Windows.Threading;
using System.Windows;
using Filer.Repositories;
using System.Diagnostics;

namespace Filer
{
    /// <summary>
    /// ファイルリストペインのVM
    /// </summary>
    public class FileListViewModel : BindableBase, IDisposable
    {
        private CompositeDisposable _disposables = new();
        private List<FileItemViewModel> _files = new();
        private string _selectedItemName = ""; //TODO: もうちょっとスマートにできないか?
        private FileSystemWatcher _watcher = new(@"C:\");

        /// <summary>
        /// 隣のペイン
        /// </summary>
        public FileListViewModel NextPane { get; set; }

        /// <summary>
        /// 現在位置
        /// </summary>
        public ReactiveProperty<string> FullPath { get; set; } = new("");

        /// <summary>
        /// このペインのステータス
        /// </summary>
        public ReactiveProperty<string> Footer { get; set; } = new("");

        /// <summary>
        /// このペインがフォーカスを持っているかどうか
        /// </summary>
        public ReactiveProperty<bool> IsActive { get; set; } = new(false);

        /// <summary>
        /// 現在のディレクトリ下にあるディレクトリとファイルのリスト
        /// </summary>
        public ReactiveCollection<FileItemViewModel> Files { get; set; } = new();

        /// <summary>
        /// 選択中のオブジェクト
        /// </summary>
        public ReactiveProperty<FileItemViewModel> SelectedItem { get; set; } = new();

        /// <summary>
        /// 選択行
        /// </summary>
        public ReactiveProperty<int> SelectedIndex { get; set; } = new(-1);

        /// <summary>
        /// 検索モードかどうか
        /// </summary>
        public ReactiveProperty<bool> IsSearchMode { get; set; } = new(false);

        /// <summary>
        /// 検索文字列
        /// </summary>
        public ReactiveProperty<string> SearchText { get; set; } = new("");

        /// <summary>
        /// コンストラクタ
        /// </summary>
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        public FileListViewModel()
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        {
            Files.AddTo(_disposables);
            SelectedItem.AddTo(_disposables);
            IsSearchMode.AddTo(_disposables);
            SearchText.Subscribe(OnSearchText).AddTo(_disposables);

            IsActive.Where(x => x).Subscribe(_ =>
            {
                NextPane.IsActive.Value = false;
            }).AddTo(_disposables);

            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
            _watcher.Created += OnDirectoryUpdated;
            _watcher.Changed += OnDirectoryUpdated;
            _watcher.Renamed += OnDirectoryUpdated;
            _watcher.Deleted += OnDirectoryUpdated;
            _watcher.IncludeSubdirectories = false;
            _watcher.EnableRaisingEvents = true;
            _watcher.Filter = "*";

            Files.CollectionChangedAsObservable().Subscribe(e =>
            {
                if (e.NewItems?.Count > 0)
                {
                    if (_selectedItemName.Length == 0 && SelectedItem.Value == null)
                    {
                        SelectedItem.Value = (FileItemViewModel)(e.NewItems[0]!);
                    }
                    else
                    {
                        foreach (FileItemViewModel item in e.NewItems)
                        {
                            if (item.Info.FullName == _selectedItemName)
                            {
                                _selectedItemName = "";
                                SelectedItem.Value = item;
                                break;
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// オブジェクトを解放する
        /// </summary>
        public void Dispose()
        {
            foreach (var item in _files)
            {
                item.Dispose();
            }
            _disposables.Dispose();
        }

        /// <summary>
        /// キーダウンイベント
        /// </summary>
        /// <param name="e">押下されたキー</param>
        public void OnKeyDown(KeyEventArgs e)
        {
            // InputBindingを使っていないのはキーバインドを変更できるようにしたい、と少しだけ考えているため
            switch (e.Key)
            {
                case Key.Enter:
                    if (SelectedItem.Value != null)
                    {
                        if (SelectedItem.Value.Info is DirectoryInfo info)
                        {
                            MoveDirectory(info.FullName);
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.Back:
                    var current = new DirectoryInfo(FullPath.Value);
                    if (current.Parent != null)
                    {
                        MoveDirectory(current.Parent.FullName);
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    OffsetSelect(-1);
                    e.Handled = true;
                    break;
                case Key.Down:
                    OffsetSelect(+1);
                    e.Handled = true;
                    break;
                case Key.Space:
                    if (SelectedItem.Value != null)
                    {
                        SelectedItem.Value.IsMarked.Value = !SelectedItem.Value.IsMarked.Value;
                    }
                    OffsetSelect(+1);
                    e.Handled = true;
                    break;
                case Key.A:
                    foreach (var item in Files)
                    {
                        item.IsMarked.Value = !item.IsMarked.Value;
                    }
                    e.Handled = true;
                    break;
                case Key.O:
                    MoveDirectory(NextPane.FullPath.Value);
                    e.Handled = true;
                    break;
                case Key.C:
                    CopyItems();
                    e.Handled = true;
                    break;
                case Key.M:
                    MoveItems();
                    e.Handled = true;
                    break;
                case Key.R:
                    RenameItems();
                    e.Handled = true;
                    break;
                case Key.H:
                    ShowHistoryPalette();
                    e.Handled = true;
                    break;
                case Key.F1:
                    ShowCommandPalette();
                    e.Handled = true;
                    break;
                case Key.Q:
                    QuitApplication();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// 履歴ウィンドウを表示
        /// </summary>
        private void ShowHistoryPalette()
        {
            // 何か別のウィンドウ使う時のプラクティスがあったような
            var vm = new CommandPaletteViewModel(HistoryRepository.Instance);
            var window = new CommandWindow()
            {
                Owner = App.Current.MainWindow,
                DataContext = vm
            };
            if (window.ShowDialog() == true)
            {
                MoveDirectory(vm.GetSelectedCommandItem());
            }
        }

        /// <summary>
        /// コマンドパレットを表示する
        /// </summary>
        private void ShowCommandPalette()
        {
            try
            {
                var vm = new CommandPaletteViewModel(CommandRepository.Instance);
                var window = new CommandWindow()
                {
                    Owner = App.Current.MainWindow,
                    DataContext = vm
                };
                if (window.ShowDialog() == true)
                {
                    var command = vm.GetSelectedCommandItem();
                    var path = SelectedItem.Value?.Info?.FullName ?? FullPath.Value;
                    Process.Start(new ProcessStartInfo(command, path)
                    {
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WorkingDirectory = FullPath.Value,
                    });
                    CommandRepository.Instance.Add(command);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// 選択行を移動する
        /// </summary>
        /// <param name="offset">移動量</param>
        private void OffsetSelect(int offset)
        {
            var next = SelectedIndex.Value + offset;
            if (next >= 0 && next < Files.Count)
            {
                SelectedIndex.Value = next;
            }
        }

        /// <summary>
        /// アプリを終了する
        /// </summary>
        private void QuitApplication()
        {
            App.Current.MainWindow.Close();
        }

        /// <summary>
        /// マークしているアイテムをリネームする
        /// </summary>
        private void RenameItems()
        {
            var markItems = Files.Where(x => x.IsSelected.Value || x.IsMarked.Value).ToArray();
            if (markItems.Length == 1)
            {
                var inputBox = new InputBox { Owner = App.Current.MainWindow, InputText = markItems[0].Name };
                if (inputBox.ShowDialog() == true)
                {
                    var dstPath = Path.Combine(FullPath.Value, inputBox.InputText);
                    _selectedItemName = dstPath;
                    markItems[0].Rename(dstPath);
                }

            }
            // TODO
        }

        /// <summary>
        /// 隣接ペインに選択したアイテムをコピーする
        /// </summary>
        private void CopyItems()
        {
            foreach (var item in Files.Where(x => x.IsMarked.Value))
            {
                NextPane.CopyFrom(item);
            }
        }

        /// <summary>
        /// 隣接ペインに選択したアイテムを移動する
        /// </summary>
        private void MoveItems()
        {
            foreach (var item in Files.Where(x => x.IsMarked.Value))
            {
                NextPane.MoveFrom(item);
            }
        }

        /// <summary>
        /// 現在のディレクトリにitemをコピーします
        /// </summary>
        /// <param name="item">コピー元のアイテム</param>
        public void CopyFrom(FileItemViewModel item)
        {
            if (item.Info is FileInfo fileInfo)
            {
                Util.CopyFile(fileInfo.FullName, Path.Combine(FullPath.Value, fileInfo.Name));
            }
            else if (item.Info is DirectoryInfo directoryInfo)
            {
                Util.CopyDirectory(directoryInfo.FullName, FullPath.Value);
            }
        }

        /// <summary>
        /// 現在のディレクトリにitemを移動します
        /// </summary>
        /// <param name="item">移動するのアイテム</param>
        public void MoveFrom(FileItemViewModel item)
        {
            if (item.Info is FileInfo fileInfo)
            {
                Util.MoveFile(fileInfo.FullName, Path.Combine(FullPath.Value, fileInfo.Name));
            }
            else if (item.Info is DirectoryInfo directoryInfo)
            {
                Util.MoveDirectory(directoryInfo.FullName, FullPath.Value);
            }
        }

        /// <summary>
        /// 新しいディレクトリを作成する
        /// </summary>
        /// <param name="directory">ディレクトリ名</param>
        public void CreateNewDirectory(string directory)
        {
            if (directory.Length == 0)
            {
                return;
            }
            var fullPath = System.IO.Path.Combine(FullPath.Value, directory);
            if (Directory.Exists(fullPath))
            {
                return;
            }

            Directory.CreateDirectory(fullPath);
            MoveDirectory(fullPath);
        }

        /// <summary>
        /// ディレクトリを移動する
        /// </summary>
        /// <param name="dir">移動先のディレクトリ</param>
        public void MoveDirectory(string dir)
        {
            try
            {
                var previousDir = FullPath.Value;

                UpdateItems(dir);
                FullPath.Value = dir;
                _watcher.Path = dir;

                HistoryRepository.Instance.Add(dir);
                UpdateFooter();

                if (_files.Any(x => x.Info.FullName == previousDir))
                {
                    _selectedItemName = previousDir;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 指定ディレクトリ以下のアイテムを取得する
        /// </summary>
        /// <param name="dir">ディレクトリパス</param>
        private void UpdateItems(string dir)
        {
            var children = new List<FileItemViewModel>();
            var current = new DirectoryInfo(dir);
            if (current.Parent != null)
            {
                children.Add(new FileItemViewModel(current.Parent.FullName) { Parent = true });
            }

            foreach (var item in Directory.EnumerateDirectories(dir))
            {
                children.Add(new FileItemViewModel(item));
            }
            foreach (var item in Directory.EnumerateFiles(dir))
            {
                children.Add(new FileItemViewModel(item));
            }

            foreach (var item in Files)
            {
                item.Dispose();
            }
            Files.ClearOnScheduler();
            Files.AddRangeOnScheduler(children);
            ResetItems(children);
        }

        /// <summary>
        /// ファイルリストを更新する
        /// </summary>
        /// <param name="items">新しいディレクトリのファイルリスト</param>
        private void ResetItems(List<FileItemViewModel> items)
        {
            foreach (var item in _files)
            {
                item.Dispose();
            }

            _files.Clear();
            _files.AddRange(items);
        }

        /// <summary>
        /// ステータス情報を更新する
        /// </summary>
        private void UpdateFooter()
        {
            var drive = new DriveInfo(System.IO.Path.GetPathRoot(FullPath.Value) ?? "C");
            if (drive.IsReady)
            {
                Footer.Value = $"{Util.GetFileSize(drive.AvailableFreeSpace)} / {Util.GetFileSize(drive.TotalSize)}";
            }
            else
            {
                Footer.Value = $"";
            }
        }

        /// <summary>
        /// ファイルリストの絞り込みを行う
        /// </summary>
        /// <param name="text">検索文字列</param>
        private void OnSearchText(string text)
        {
            var words = text.Split(" ").Where(x => x.Length > 0).Select(x => x.ToLower()).ToArray();
            Files.Clear();
            foreach (var item in _files.Where(x => words.All(w => x.Name.ToLower().Contains(w))))
            {
                Files.Add(item);
            }
        }

        /// <summary>
        /// フォルダ内の更新イベント
        /// </summary>
        /// <param name="sender">イベント送信元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void OnDirectoryUpdated(object sender, FileSystemEventArgs e)
        {
            UpdateItems(FullPath.Value);
        }
    }
}
