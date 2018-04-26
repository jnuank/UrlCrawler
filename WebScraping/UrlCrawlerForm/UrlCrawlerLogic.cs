using AngleSharp;
using AngleSharp.Parser.Html;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace UrlCrawlerForm
{
    /// <summary>
    /// URLを探索するクラス
    /// </summary>
    public class UrlCrawlerLogic
    {
        /// <summary>
        /// 検索する文字列
        /// </summary>
        private string searchString;

        /// <summary>
        /// 探索済みのURLリスト
        /// </summary>
        private List<string> crawledUrlList = new List<string>();

        /// <summary>
        /// URL経路のスタック
        /// </summary>
        private Stack<string> urlPathStack = new Stack<string>();

        /// <summary>
        /// 検索をする最大深度
        /// </summary>
        private int limitDepth;

        /// <summary>
        /// 対象のURLが見つかったか
        /// </summary>
        private bool isFound;

        /// <summary>
        /// 検索結果のURL
        /// </summary>
        private string foundUrl;


        /// <summary>
        /// 起点URLから、対象の文字列を含むURLを探索する。
        /// </summary>
        /// <param name="rootUrl">起点URL</param>
        /// <param name="searchString">検索する文字列</param>
        /// <param name="limitDepth">最大深度</param>
        /// <returns>true：searchStringを含んだURLが見つかった。false：見つからなかった</returns>
        public bool SearchUrl(string rootUrl, string searchString, int limitDepth)
        {
            // 何階層まで調べるかを設定する
            this.limitDepth = limitDepth;

            // 検索対象の文字列を設定する
            this.searchString = searchString;

            // 検索開始(深度は0スタート)
            this.SearchLinks(rootUrl, 0);

            // 見つかったかどうかの結果を返す
            return isFound;
        }

        /// <summary>
        /// URLからHTMLを取得し、指定した文字列を含むURLがあるかどうか調べる
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="currentDepth">現在の深度（何回目の再帰呼び出しか）</param>
        private void SearchLinks(string url, int currentDepth)
        {
            // 探索済みにする
            crawledUrlList.Add(url);

            // 最大深度を超えたらリターンする
            if (currentDepth > limitDepth)
                return;

            // 最初に指定されたURLに文字列があるかチェック
            if (url.Contains(searchString))
            {
                isFound = true;
                foundUrl = url;
                return;
            }

            WebClient client = new WebClient();
            string htmlString;

            try
            {
                // 指定したURLから、HTMLデータを文字列として取得する
                htmlString = client.DownloadString(url);
            }
            catch (WebException ex)
            {
                // 指定したURLにはアクセスできなかったのでリターンする
                return;
            }

            // 取得したHtmlの文字列をパースする
            var parser = new HtmlParser();
            var document = parser.Parse(htmlString);

            List<string> linkList = new List<string>();
            string hrefValue = string.Empty;
            Url baseUrl = new Url(url);

            // ページ内のリンクをすべて取得する
            foreach (var item in document.QuerySelectorAll("a"))
            {
                hrefValue = item.Attributes["href"]?.Value;

                // href属性が存在していなかったら追加しない
                if (hrefValue == null)
                    continue;

                // すでに追加済みなら追加しない
                if (linkList.Contains(hrefValue))
                    continue;

                // すでに調査済みなら追加しない
                if (crawledUrlList.Contains(new Url(baseUrl, hrefValue).ToString()))
                    continue;

                linkList.Add(item.Attributes["href"].Value);
            }

            // リンクが無いなら、リターンする。
            if (!linkList.Any())
                return;

            // 取得したリンクのURL内に、目的の文字列が含まれているか
            //（ searchStringが日本語の場合を考慮して、Urlインスタンスを作成し、URLエンコードしている）
            string str = linkList.Where(x => x.Contains(new Url(searchString).ToString())).FirstOrDefault();

            // 一致するものが見つかる
            if (str != null)
            {
                // 経路スタックにurlをpushする
                urlPathStack.Push(url);

                // 検索結果を保存
                foundUrl = new Url(baseUrl, str).ToString();
                isFound = true;

                return;
            }
            else
            {
                // 一致するものが見つからなかったので、Link先を辿っていく
                string subLink;
                foreach (var link in linkList)
                {
                    subLink = new Url(baseUrl, link).ToString();

                    // すでに探索済み
                    if (crawledUrlList.Contains(subLink))
                        continue;

                    // urlを変更して再帰呼び出し
                    SearchLinks(new Url(baseUrl, subLink).ToString(), (currentDepth + 1));

                    // 見つかったらスタックに入れて終了
                    if (isFound)
                    {
                        urlPathStack.Push(url);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// URL経路を出力する
        /// </summary>
        /// <returns></returns>
        public string PrintUrlPath()
        {
            StringBuilder builder = new StringBuilder();
            // 検索結果を設定する
            builder.AppendLine(foundUrl);
            // 検索結果→起点URLの経路を出力するので、要素を反転する
            var revercedStack = urlPathStack.Reverse();

            foreach (var item in revercedStack)
            {
                builder.AppendLine(item);
            }
            return builder.ToString();
        }
    }
}
