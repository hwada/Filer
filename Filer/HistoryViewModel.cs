using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Filer
{
    /// <summary>
    /// 履歴リスト
    /// </summary>
    internal class HistoryViewModel : CommandPaletteViewModel
    {
        /// <summary>
        /// 履歴を検索、絞り込む
        /// </summary>
        /// <param name="text">検索文字列</param>
        protected override void CommandSearch(string text)
        {
            // ファジィ検索にしたいけど、ファイラならこれで十分かも
            // TODO: migemo対応
            Commands.Clear();
            foreach (var item in HistoryRepository.Instance.Directories.Where(x => x.Contains(text)).Select(x => new FileItemViewModel(x)))
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
