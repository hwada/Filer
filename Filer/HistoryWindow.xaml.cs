using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Filer
{
    /// <summary>
    /// HistoryWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HistoryWindow()
        {
            InitializeComponent();
            SearchBox.Focus();
        }

        /// <summary>
        /// このウィンドウのVM
        /// </summary>
        internal HistoryViewModel ViewModel => (HistoryViewModel)DataContext;

        /// <summary>
        /// 画面を閉じる
        /// </summary>
        public void Ok()
        {
            DialogResult = true;
        }

        /// <summary>
        /// 画面を閉じる(キャンセル)
        /// </summary>
        public void Cancel()
        {
            DialogResult = false;
        }
    }
}
