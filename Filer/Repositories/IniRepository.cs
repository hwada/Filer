using System.IO;
using System.Collections.Generic;

namespace Filer.Repositories
{
    /// <summary>
    /// 何らかの履歴を管理するクラス
    /// </summary>
    internal abstract class IniRepository
    {
        /// <summary>
        /// 履歴の一覧
        /// </summary>
        public List<string> Items { get; set; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected IniRepository()
        {
            var path = GetIniFilePath();
            if (!File.Exists(path))
            {
                return;
            }

            foreach (var item in File.ReadLines(path))
            {
                if (CheckItem(item))
                {
                    Items.Add(item);
                }
            }
        }

        protected virtual bool CheckItem(string item)
        {
            return true;
        }

        /// <summary>
        /// 履歴にディレクトリを追加する
        /// </summary>
        /// <param name="dir"></param>
        public void Add(string dir)
        {
            var index = Items.IndexOf(dir);
            if (index == 0)
            {
                return;
            }
            if (index > 0)
            {
                Items.RemoveAt(index);
            }

            Items.Insert(0, dir);

            //TODO: 履歴保持数の上限、必要？
        }

        /// <summary>
        /// 履歴をファイルに保存する
        /// </summary>
        public void Save()
        {
            File.WriteAllLines(GetIniFilePath(), Items);
        }

        /// <summary>
        /// 履歴を保存するファイルパスを取得する
        /// </summary>
        /// <returns></returns>
        protected abstract string GetIniFilePath();
    }
}
