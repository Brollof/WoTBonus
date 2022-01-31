﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using HtmlAgilityPack;

namespace WoTBonus
{
    public partial class Form1 : Form
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
        private List<string> prevTitles = null;

        public Form1()
        {
            InitializeComponent();
            Init();
        }

        private List<string> GetTitles()
        {
            
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(RYKOSZET_URL);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//h2[@class='post-title entry-title']");

            if (nodes != null)
            {
                return nodes.Select(n => n.InnerText.Trim()).ToList();
            }
            return new List<string>();
        }

        private void Init()
        {
#if DEBUG
            AllocConsole();
#endif
            ContextMenu menu = new ContextMenu();
            //menu.MenuItems.Add("Temperature chart", ShowTemperatureChart);
            menu.MenuItems.Add("Exit", ContextMenuExit);
            notifyIcon1.ContextMenu = menu;

            timerScrap.Interval = SCRAP_TIMER_PERIOD_MS;
            timerScrap.Start();

            CenterToScreen();
        }

        private void ContextMenuExit(object sender, EventArgs e)
        {
            OnExit();
        }

        private void OnExit()
        {
#if DEBUG
            FreeConsole();
#endif
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
            Application.Exit();
            Environment.Exit(0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnExit();
        }

        private void timerScrap_Tick(object sender, EventArgs e)
        {
            var newTitles = GetTitles();
            if (prevTitles == null || prevTitles[0] != newTitles[0])
            {
                if (newTitles.Any(title => title.ToLower().Contains("bonus")) == true)
                {
                    Console.WriteLine("notify");
                    notifyIcon1.ShowBalloonTip(5000, "WoT Bonus Code", "There is a new bonus code", ToolTipIcon.Info);
                }
            }
            prevTitles = newTitles;
        }
    }
}
