using AutoUpdaterDotNET;
using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;
using mif.CefSharpHandlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using EasyTabs;

namespace mif
{
public partial class frmMain : Form
    {
        public ChromiumWebBrowser Browser { get; set; }

        // frmMain.cs
        public event EventHandler<TitleChangedEventArgs> TitleChanged;

        public frmMain()
        {
            InitializeComponent();

            browser = new ChromiumWebBrowser(txtUrl.Text);
            browser.TitleChanged += Browser_TitleChanged;
        }
        private void Browser_TitleChanged(object sender, TitleChangedEventArgs e)
        {
            // UI スレッドでない場合、UI スレッドで処理を行う
            if (this.InvokeRequired)
            {
                // Invoke を使って UI スレッドに処理を渡す
                this.Invoke(new Action(() =>
                {
                    // タブタイトルを変更する処理
                    this.Text = e.Title;  // フォームのタイトルをページのタイトルに設定
                }));
            }
            else
            {
                // UI スレッド内で直接処理を行う
                this.Text = e.Title;
            }
        }

        private void UpdateTabTitle(string title)
        {
            try
            {
                if (!string.IsNullOrEmpty(title))
                {
                    this.Text = title;  // タブのタイトルを設定
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting title: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Cef.Shutdown();
            base.OnFormClosing(e);
        }


        private void TxtUrl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                browser.Load(txtUrl.Text);
        }

        private void BtnGo_Click(object sender, EventArgs e)
        {
            browser.Load(txtUrl.Text);
        }

        ChromiumWebBrowser browser;

        private void frmMain_Load(object sender, EventArgs e)
        {

            AutoUpdater.Mandatory = true;
            AutoUpdater.UpdateMode = Mode.Forced;

            AutoUpdater.Start("https://release.floft.app/public/autoupdate.xml");

            // イベントハンドラ登録
            browser.TitleChanged += Browser_TitleChanged;
            browser.AddressChanged += Browser_AddressChanged;
            browser.LoadingStateChanged += Browser_LoadingStateChanged;

            // Dock と表示設定
            browser.Dock = DockStyle.Fill;
            this.pContainer.Controls.Add(browser);
            browser.LifeSpanHandler = new CustomLifeSpanHandler();
        }


        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                if (e.IsLoading)
                {
                    pictureBox.Image = floft.Properties.Resources.load; // 読み込み中のGIF
                }
                else
                {
                    LoadFavicon(browser.Address); // 読み込み完了後にファビコンをロード
                }
            }));
        }

        // ファビコンを取得してPictureBoxに設定する
        private void LoadFavicon(string url)
        {
            try
            {
                // Favicon URLの推測
                Uri uri = new Uri(url);
                string faviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico";

                // Faviconのダウンロード
                using (WebClient client = new WebClient())
                {
                    byte[] data = client.DownloadData(faviconUrl);
                    using (MemoryStream stream = new MemoryStream(data))
                    {
                        using (Image downloadedImage = Image.FromStream(stream))
                        {
                            // 必要に応じて画像を拡大
                            int targetSize = 30; // 表示したいサイズ
                            Bitmap resizedImage = new Bitmap(targetSize, targetSize);

                            using (Graphics g = Graphics.FromImage(resizedImage))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.DrawImage(downloadedImage, 0, 0, targetSize, targetSize);
                            }

                            pictureBox.Image = resizedImage; // Resized Image を PictureBox にセット
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // エラー時のデフォルトアイコン設定
                pictureBox.Image = floft.Properties.Resources.default_favicon; // デフォルトアイコンをプロジェクトリソースに追加
                Console.WriteLine($"Error loading favicon: {ex.Message}");
            }
        }

        private void Browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            // URL変更時に表示する
            if (txtUrl.InvokeRequired)
            {
                txtUrl.Invoke(new Action(() =>
                {
                    txtUrl.Text = e.Address;  // URLを表示
                }));
            }
            else
            {
                txtUrl.Text = e.Address;
            }
        }

        private void versionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            browser.Load("chrome://version/");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            about f = new about();
            f.Show();
        }

        private void devToolToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }

        private void printToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            browser.Print();
        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            browser.Back();
        }

        private void forwardBtn_Click(object sender, EventArgs e)
        {
            browser.Forward();
        }

        private void reloadBtn_Click(object sender, EventArgs e)
        {
            browser.Reload();
        }

       

    }


    // カスタム LifeSpanHandler の定義
    public class CustomLifeSpanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            string targetUrl,
            string targetFrameName,
            WindowOpenDisposition targetDisposition,
            bool userGesture,
            IPopupFeatures popupFeatures,
            IWindowInfo windowInfo,
            IBrowserSettings browserSettings,
            ref bool noJavascriptAccess,
            out IWebBrowser newBrowser)
        {
            // 新しいウィンドウではなく、現在のウィンドウでリンクを開く
            chromiumWebBrowser.Load(targetUrl);  // 同じウィンドウでURLを読み込む
            newBrowser = null;
            return true;  // ポップアップをキャンセル
        }

        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser) => false;
        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
    }

}