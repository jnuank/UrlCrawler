using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UrlCrawlerForm
{
    public partial class UrlCrawlerForm : Form
    {
        public UrlCrawlerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 実行ボタンイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnApply_ClickAsync(object sender, EventArgs e)
        {
            string url = "https://www.amazon.co.jp/";
            string search = "カート";

            var logic = new UrlCrawlerLogic();

            // Task化
            var task = Task.Run(() => logic.SearchUrl(url, search, 2));

            bool isFound = await task;

            if (isFound)
            {
                txtResult.Text = logic.PrintUrlPath();
            }
            else
            {
                txtResult.Text = "見つかりませんでした";
            }
        }
    }
}
