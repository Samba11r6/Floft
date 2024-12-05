using CefSharp.WinForms;
using CefSharp;
using EasyTabs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace mif
{
    namespace CefSharpHandlers
    {

        internal static class Program
        {
            /// <summary>
            /// アプリケーションのメイン エントリ ポイントです。
            /// </summary>
            [STAThread]
            static void Main()
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // EasyTabs のコンテナを作成
                AppContainer container = new AppContainer();

                // 新しいタブを追加
                var mainForm = new frmMain();
                var tab = new TitleBarTab(container)
                {
                    Content = mainForm
                };

                // タブのタイトルはページタイトルが変更されるたびに更新される
                container.Tabs.Add(tab);
                container.SelectedTabIndex = 0;

                // アプリケーションコンテキストを作成して開始
                TitleBarTabsApplicationContext applicationContext = new TitleBarTabsApplicationContext();
                applicationContext.Start(container);

                // CefSharp 設定
                CefSettings settings = new CefSettings()
                {
                    AcceptLanguageList = "ja,en-US;q=0.9,en;q=0.8",
                    CachePath = @"c:\temp\cache",
                    UserAgent = @"Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36 SimpleBroser/1.0.0.0",
                    Locale = "ja",
                    LogSeverity = LogSeverity.Verbose
                };

                // カメラにアクセスできるようにする
                settings.CefCommandLineArgs.Add("enable-media-stream", "1");

                // アプリケーション実行
                Application.Run(applicationContext);
            }
        }
    }
}