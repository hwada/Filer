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
    internal class CommandRepository : IniRepository
    {
        private static CommandRepository _instance;

        /// <summary>
        /// Singletonインスタンスを取得する
        /// </summary>
        public static CommandRepository Instance
        {
            get
            {
                _instance ??= new CommandRepository();
                return _instance;
            }
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
            return Path.Combine(dir, "commands.txt");
        }
    }
}
