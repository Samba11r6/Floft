using EasyTabs;
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
using System.Windows.Controls;
using System.Windows.Forms;

namespace mif
{
    public partial class AppContainer : TitleBarTabs
    {
        public AppContainer()
        {
            InitializeComponent();

            AeroPeekEnabled = true;
            TabRenderer = new ChromeTabRenderer(this);
            Icon = floft.Properties.Resources.logo;
        }

        public override TitleBarTab CreateTab()
        {
            var frm = new frmMain(); // frmMain のインスタンスを作成
            var tab = new TitleBarTab(this)
            {
                Content = frm
            };

            // frmMain のタイトルを設定
            frm.Text = "New Tab"; // 初期タイトルを設定

            return tab;
        }
    }
}