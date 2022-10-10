using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Filer.Repositories
{
    /// <summary>
    /// ディレクトリ履歴を管理するクラス
    /// </summary>
    internal class HistoryRepository : IniRepository
    {
        private static HistoryRepository _instance;

        /// <summary>
        /// Singletonインスタンスを取得する
        /// </summary>
        public static HistoryRepository Instance
        {
            get
            {
                _instance ??= new HistoryRepository();
                return _instance;
            }
        }

        /// <summary>
        /// 履歴ディレクトリが存在しているかどうか確認
        /// </summary>
        /// <param name="item">履歴パス</param>
        /// <returns>itemが存在していればtrue</returns>
        protected override bool CheckItem(string item)
        {
            return Directory.Exists(item);
        }

        /// <summary>
        /// 履歴ファイルのパスを取得する
        /// </summary>
        /// <returns></returns>
        protected override string GetIniFilePath()
        {
            // Program Filesに置く予定はないので、EXEと同じフォルダに配置してしまう
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.Assert(dir != null);
            return Path.Combine(dir, "history.txt");
        }
    }
}
