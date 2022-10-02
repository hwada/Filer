using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Filer
{
    /// <summary>
    /// HistoryWindowのVM
    /// </summary>
    internal class HistoryViewModel : BindableBase, IDisposable
    {
        private CompositeDisposable _disposables = new();

        /// <summary>
        /// 履歴の一覧
        /// </summary>
        public ReactiveCollection<FileItemViewModel> Directories { get; set; } = new();

        /// <summary>
        /// 選択中の行
        /// </summary>
        public ReactiveProperty<int> SelectedIndex { get; set; } = new(-1);

        /// <summary>
        /// 1行上に移動
        /// </summary>
        public ReactiveCommand MoveUpCommand { get; set; } = new();

        /// <summary>
        /// 1行下に移動
        /// </summary>
        public ReactiveCommand MoveDownCommand { get; set; } = new();

        /// <summary>
        /// 検索文字列
        /// </summary>
        public ReactiveProperty<string> SearchText { get; set; } = new("");

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HistoryViewModel()
        {
            Directories.AddTo(_disposables);
            SelectedIndex.AddTo(_disposables);
            MoveUpCommand.Subscribe(OnMoveUp).AddTo(_disposables);
            MoveDownCommand.Subscribe(OnMoveDown).AddTo(_disposables);
            SearchText.Subscribe(HistorySearch).AddTo(_disposables);
        }

        /// <summary>
        /// オブジェクトを解放する
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// 選択中のディレクトリを取得する
        /// </summary>
        /// <returns></returns>
        public string GetSelectedDirectory()
        {
            return SelectedIndex.Value < 0 ? "" : Directories[SelectedIndex.Value].Info.FullName;
        }

        /// <summary>
        /// 1行上に移動
        /// </summary>
        private void OnMoveUp()
        {
            if (SelectedIndex.Value > 0)
            {
                SelectedIndex.Value -= 1;
            }
        }

        /// <summary>
        /// 1行下に移動
        /// </summary>
        private void OnMoveDown()
        {
            if (SelectedIndex.Value < Directories.Count - 1)
            {
                SelectedIndex.Value += 1;
            }
        }

        /// <summary>
        /// 履歴を検索、絞り込む
        /// </summary>
        /// <param name="text"></param>
        private void HistorySearch(string text)
        {
            // ファジィ検索にしたいけど、ファイラならこれで十分かも
            // TODO: migemo対応
            Directories.Clear();
            foreach (var item in HistoryRepository.Instance.Directories.Where(x => x.Contains(text)).Select(x => new FileItemViewModel(x)))
            {
                Directories.Add(item);
            }

            if (Directories.Count > 0)
            {
                SelectedIndex.Value = 0;
            }
        }
    }
}
