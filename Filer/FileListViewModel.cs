using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    internal class FileListViewModel : BindableBase, IDisposable
    {
        private CompositeDisposable _disposables = new();

        /// <summary>
        /// 現在位置
        /// </summary>
        public ReactiveProperty<string> Path { get; set; } = new("");

        /// <summary>
        /// 現在のディレクトリ下にあるディレクトリとファイルのリスト
        /// </summary>
        public ReactiveCollection<FileItemViewModel> Files { get; set; } = new();

        /// <summary>
        /// 選択中のオブジェクト
        /// </summary>
        public ReactiveProperty<FileItemViewModel> SelectedItem { get; set; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FileListViewModel()
        {
            // TODO: 前回のフォルダを覚える
            //MoveDirectory(Directory.GetCurrentDirectory());
            MoveDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            Files.AddTo(_disposables);
            SelectedItem.AddTo(_disposables);

        }

        /// <summary>
        /// オブジェクトを解放する
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// キーダウンイベント
        /// </summary>
        /// <param name="e">押下されたキー</param>
        public void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (SelectedItem.Value != null)
                    {
                        if (SelectedItem.Value.Info is DirectoryInfo info)
                        {
                            MoveDirectory(info.FullName);
                        }
                    }
                    break;
                case Key.Back:
                    var current = new DirectoryInfo(Path.Value);
                    if (current.Parent != null)
                    {
                        MoveDirectory(current.Parent.FullName);
                    }
                    break;
            }
        }

        /// <summary>
        /// ディレクトリを移動する
        /// </summary>
        /// <param name="dir">移動先のディレクトリ</param>
        private void MoveDirectory(string dir)
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

                var prev = children.FirstOrDefault(x => x.Info.FullName == previousDir);
                if (prev != null)
                {
                    SelectedItem.Value = prev;
                }
                else if (children.Count > 0)
                {
                    SelectedItem.Value = children.First();
                }
            }
            catch
            {
            }
        }
    }
}
