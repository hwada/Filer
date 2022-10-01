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
    internal class FileListViewModel : BindableBase, IDisposable
    {
        private CompositeDisposable _disposables = new();

        public ReactiveProperty<string> Path { get; set; } = new("");

        public ReactiveCollection<FileItemViewModel> Files { get; set; } = new();

        public ReactiveProperty<FileItemViewModel> SelectedItem { get; set; } = new();

        public FileListViewModel()
        {
            // TODO: 前回のフォルダを覚える
            //MoveDirectory(Directory.GetCurrentDirectory());
            MoveDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            Files.AddTo(_disposables);
            SelectedItem.AddTo(_disposables);

        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SelectedItem.Value != null)
                {
                    if (SelectedItem.Value.Info is DirectoryInfo info)
                    {
                        MoveDirectory(info.FullName);
                    }
                }
            }
        }

        private void MoveDirectory(string dir)
        {
            try
            {
                var children = new List<FileItemViewModel>();
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
                SelectedItem.Value = children.First();
            }
            catch
            {
            }
        }
    }
}
