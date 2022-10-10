using Filer.Repositories;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filer
{
    /// <summary>
    /// コマンドパレットのVM
    /// </summary>
    internal class CommandPaletteViewModel : BindableBase, IDisposable
    {
        private CompositeDisposable _disposables = new();
        private IniRepository _repository;

        /// <summary>
        /// コマンド(選択肢)の一覧
        /// </summary>
        public ReactiveCollection<string> Commands { get; set; } = new();

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
        public CommandPaletteViewModel(IniRepository repository)
        {
            _repository = repository;

            Commands.AddTo(_disposables);
            SelectedIndex.AddTo(_disposables);
            MoveUpCommand.Subscribe(OnMoveUp).AddTo(_disposables);
            MoveDownCommand.Subscribe(OnMoveDown).AddTo(_disposables);
            SearchText.Subscribe(CommandSearch).AddTo(_disposables);
        }

        /// <summary>
        /// オブジェクトを解放する
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// 選択中の文字列を取得する
        /// </summary>
        /// <returns></returns>
        public string GetSelectedCommandItem()
        {
            return SelectedIndex.Value < 0 ? SearchText.Value : Commands[SelectedIndex.Value];
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
            if (SelectedIndex.Value < Commands.Count - 1)
            {
                SelectedIndex.Value += 1;
            }
        }

        /// <summary>
        /// コマンドを検索、絞り込む
        /// </summary>
        /// <param name="text">検索文字列</param>
        private void CommandSearch(string text)
        {
            // ファジィ検索にしたいけど、ファイラならこれで十分かも
            // TODO: migemo対応
            Commands.Clear();
            foreach (var item in _repository.Items.Where(x => x.Contains(text)))
            {
                Commands.Add(item);
            }

            if (Commands.Count > 0)
            {
                SelectedIndex.Value = 0;
            }
        }
    }
}
