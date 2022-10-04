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

namespace Filer
{
    /// <summary>
    /// ファイルリストペインのVM
    /// </summary>
    public class FileListViewModel : BindableBase, IDisposable
    {
        private CompositeDisposable _disposables = new();
        private List<FileItemViewModel> _files = new();

        /// <summary>
        /// 隣のペイン
        /// </summary>
        public FileListViewModel NextPane { get; set; }

        /// <summary>
        /// 現在位置
        /// </summary>
        public ReactiveProperty<string> Path { get; set; } = new("");

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

            // Path.Value = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
                    var current = new DirectoryInfo(Path.Value);
                    if (current.Parent != null)
                    {
                        MoveDirectory(current.Parent.FullName);
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (SelectedIndex.Value > 0)
                    {
                        SelectedIndex.Value -= 1;
                    }
                    e.Handled = true;
                    break;
                case Key.Down:
                    if (SelectedIndex.Value < Files.Count - 1)
                    {
                        SelectedIndex.Value += 1;
                    }
                    e.Handled = true;
                    break;
                case Key.Space:
                    if (SelectedItem.Value != null)
                    {
                        SelectedItem.Value.IsMarked.Value = !SelectedItem.Value.IsMarked.Value;
                    }
                    if (SelectedIndex.Value < Files.Count - 1)
                    {
                        SelectedIndex.Value += 1;
                    }
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
                    MoveDirectory(NextPane.Path.Value);
                    e.Handled = true;
                    break;
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
            var fullPath = System.IO.Path.Combine(Path.Value, directory);
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
                var previousDir = Path.Value;
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

                Path.Value = dir;
                foreach (var item in Files)
                {
                    item.Dispose();
                }
                Files.Clear();
                Files.AddRangeOnScheduler(children);

                ResetItems(children);

                HistoryRepository.Instance.Add(dir);
                UpdateFooter();

                var prev = children.FirstOrDefault(x => x.Info.FullName == previousDir);
                if (prev != null)
                {
                    SelectedItem.Value = prev;
                }
                else if (children.Count > 0)
                {
                    SelectedItem.Value = children.First();
                }
                SelectedIndex.Value = children.IndexOf(SelectedItem.Value);
            }
            catch
            {
            }
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
            var drive = new DriveInfo(System.IO.Path.GetPathRoot(Path.Value) ?? "C");
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
    }
}
