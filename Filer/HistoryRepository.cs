using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Filer
{
    /// <summary>
    /// ディレクトリ履歴を管理するクラス
    /// </summary>
    internal class HistoryRepository
    {
        private static HistoryRepository? _instance;

        /// <summary>
        /// 履歴の一覧
        /// </summary>
        public List<string> Directories { get; set; } = new();

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
        /// コンストラクタ
        /// </summary>
        private HistoryRepository()
        {
            var path = GetHistoryFilePath();
            if (!File.Exists(path))
            {
                return;
            }

            foreach (var item in File.ReadLines(path))
            {
                if (Directory.Exists(item))
                {
                    Directories.Add(item);
                }
            }
        }

        /// <summary>
        /// 履歴にディレクトリを追加する
        /// </summary>
        /// <param name="dir"></param>
        public void Add(string dir)
        {
            var index = Directories.IndexOf(dir);
            if (index == 0)
            {
                return;
            }
            if (index > 0)
            {
                Directories.RemoveAt(index);
            }

            Directories.Insert(0, dir);

            //TODO: 履歴保持数の上限、必要？
        }

        /// <summary>
        /// 履歴をファイルに保存する
        /// </summary>
        public void Save()
        {
            File.WriteAllLines(GetHistoryFilePath(), Directories);
        }

        /// <summary>
        /// 履歴ファイルのパスを取得する
        /// </summary>
        /// <returns></returns>
        private static string GetHistoryFilePath()
        {
            // Program Filesに置く予定はないので、EXEと同じフォルダに配置してしまう
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.Assert(dir != null);
            return Path.Combine(dir, "history.txt");
        }
    }
}
