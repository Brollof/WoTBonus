using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using HtmlAgilityPack;
#if DEBUG
using System.Runtime.InteropServices;
#endif

namespace WoTBonus
{
    public partial class WotBonusAppContext : ApplicationContext
    {
#if DEBUG
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();
#endif

        private readonly int SCRAP_TIMER_PERIOD_MS = 5000;
        private readonly string RYKOSZET_URL = "https://rykoszet.info/category/wot/";
        private readonly NotifyIcon trayIcon = new NotifyIcon();
        private readonly Timer timerScrap = new Timer();
        private List<string> prevTitles = null;

        public WotBonusAppContext()
        {
            Init();
        }

        private void Init()
        {
#if DEBUG
            AllocConsole();
#endif
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("rykoszet.info", OpenRykoszetWebsite);
            menu.MenuItems.Add("Exit", ContextMenuExit);
            trayIcon.ContextMenu = menu;
            trayIcon.Icon = Properties.Resources.AppIcon;
            trayIcon.Visible = true;

            timerScrap.Tick += TimerScrap_Tick;
            timerScrap.Interval = SCRAP_TIMER_PERIOD_MS;
            timerScrap.Start();
        }

        private List<string> GetTitles()
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(RYKOSZET_URL);
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//h2[@class='post-title entry-title']");

                if (nodes != null)
                {
                    return nodes.Select(n => n.InnerText.Trim()).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Web parsing error: {0}", ex.Message);
            };

            return new List<string>();
        }

        private void OnExit()
        {
#if DEBUG
            FreeConsole();
#endif
            trayIcon.Visible = false;
            trayIcon.Dispose();
            Application.Exit();
            Environment.Exit(0);
        }

        private void TimerScrap_Tick(object sender, EventArgs e)
        {
            var newTitles = GetTitles();
            if (prevTitles == null || prevTitles[0] != newTitles[0])
            {
                if (newTitles.Any(title => title.ToLower().Contains("bonus")) == true)
                {
                    Console.WriteLine("notify");
                    trayIcon.ShowBalloonTip(5000, "WoT Bonus Code", "There is a new bonus code", ToolTipIcon.Info);
                }
            }
            prevTitles = newTitles;
        }

        private void OpenRykoszetWebsite(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(RYKOSZET_URL);
        }

        private void ContextMenuExit(object sender, EventArgs e)
        {
            OnExit();
        }
    }
}
