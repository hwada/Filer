using System;
using System.IO;
using System.Reactive.Disposables;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Filer
{
    /// <summary>
    /// ファイル情報
    /// </summary>
    public class FileItemViewModel : BindableBase, IDisposable
    {
        private CompositeDisposable _disposables = new();

        /// <summary>
        /// ファイル情報
        /// </summary>
        public FileSystemInfo Info { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        public string Name => Parent ? ".." : Info.Name;

        /// <summary>
        /// 最終更新日時
        /// TODO: XAML側でConverterかます方が綺麗
        /// </summary>
        public string LastWriteTime => Info.LastWriteTime.ToString("yy/MM/dd HH:mm:ss");

        /// <summary>
        /// ファイルサイズを文字列化したもの
        /// </summary>
        public string FileSize
        {
            get
            {
                if (Info is DirectoryInfo)
                {
                    return "<DIR>";
                }
                return Util.GetFileSize(((FileInfo)Info).Length);
            }
        }

        /// <summary>
        /// ファイルが選択中?
        /// </summary>
        public ReactiveProperty<bool> IsSelected { get; set; } = new(false);

        /// <summary>
        /// ファイルにマーキングされている?
        /// </summary>
        public ReactiveProperty<bool> IsMarked { get; set; } = new(false);

        /// <summary>
        /// 親ディレクトリへの参照?
        /// </summary>
        public bool Parent { get; set; } = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        public FileItemViewModel(string path)
        {
            if (Directory.Exists(path))
            {
                Info = new DirectoryInfo(path);
            }
            else
            {
                Info = new FileInfo(path);
            }

            IsSelected.AddTo(_disposables);
        }

        /// <summary>
        /// オブジェクトを解放する
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
