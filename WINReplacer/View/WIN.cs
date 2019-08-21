using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WINReplacer
{
    public partial class WIN : Form
    {
        readonly string[] StartMenuPathes = new string[] { Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) };
        readonly string PropsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        readonly string ConfigPath;
        readonly IFinder[] finders;
        FixedSizedQueue<App> lastStartedApps;
        List<App> data;
        List<Label> dataControls = new List<Label>();
        public static int ControlsCount { get; private set; }
        KeyHook keyHook;
        int nowElement = -1;
        string searchText = "";
        const int tileWidth = 180;

        public WIN()
        {
            InitializeComponent();
            ConfigPath = $"{PropsPath}\\SmartWIN";
            this.lastStartedApps = ConfigLoader.LoadHistoryConfig(ConfigPath);
            this.Width = SystemInformation.PrimaryMonitorSize.Width;
            this.MaximumSize = this.MinimumSize = new Size(this.Width, 22);
            this.BackColor = Theme.FormBackColor;
            this.Shown += delegate
            {
                this.Hide();
                Process shellStart = new Process();
                shellStart.StartInfo.FileName = "explorer";
                shellStart.Start();
                WINFix.HideWinBtn();
            };

            //Spawn controls
            searchBox.Location = new Point(50, 1);
            searchBox.Font = Theme.TextFont;
            searchBox.ForeColor = Theme.TextColor;
            searchBox.BackColor = Theme.BackColor;
            for (int posX = 280; posX + 202 < this.Width - 30; posX += tileWidth)
            {
                Label temp = new Label();
                temp.Location = new Point(posX, 0);
                temp.Size = new Size(tileWidth, 25);
                temp.TextAlign = ContentAlignment.MiddleCenter;
                temp.Font = Theme.TextFont;
                temp.BackColor = Theme.BackColor;
                temp.ForeColor = Theme.TextColor;
                this.Controls.Add(temp);
                dataControls.Add(temp);
            }
            ControlsCount = dataControls.Count;
            //Start KeyHook
            keyHook = new KeyHook(this);
            keyHook.LeftArrowEvent += LeftArrClick;
            keyHook.RightArrowEvent += RightArrClick;
            keyHook.EnterClickEvent += EnterClick;
            keyHook.OpenCloseEvent += ShowHideEvent;
            keyHook.KeyClickEvent += KeyClick;
            keyHook.ShutdownClickEvent += ShutdownClick;

            //TODO: Finder by standard folders
            finders = new IFinder[] {
                new StartMenuHandler(ConfigPath, StartMenuPathes)
            };
        }

        private IEnumerable<App> FindApps()
        {
            data = new List<App>();
            foreach (var finder in finders)
            {
                List<App> get;
                if ((get = finder.FindByName(searchBox.Text)) != null)
                {
                    data.AddRange(get.Count > ControlsCount ? get.Take(ControlsCount) : get);
                    if (data.Count >= ControlsCount)
                    {
                        return data.Take(ControlsCount);
                    }
                }
            }

            return data.Count == 0 ? null : data;
        }

        private void DrawApps()
        {
            ClearSelection();
            if (data == null)
            {
                data = new List<App>(lastStartedApps.ToArray());
            }
            int i = 0;
            for (; i < dataControls.Count && i < data.Count; i++)
            {
                dataControls[i].Text = data[data.Count - i - 1].name;
            }
            for (; i < dataControls.Count; i++)
            {
                dataControls[i].Text = "";
            }
        }

        private void ClearDataControls()
        {
            for (int i = 0; i < dataControls.Count; i++)
            {
                dataControls[i].Text = "";
            }
        }

        private void ClearSelection()
        {
            if (nowElement != -1)
            {
                dataControls[nowElement].BackColor = Theme.BackColor;
                dataControls[nowElement].ForeColor = Theme.TextColor;
                nowElement = -1;
            }
        }

        private void ClearAll()
        {
            searchBox.Text = "";
            ClearDataControls();
            ClearSelection();
        }

        //FORM EVENTS

        private void WIN_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (searchBox.Text == searchText) return;
            data = FindApps()?.ToList();
            DrawApps();
        }

        //OWN EVENTS

        private void ShowHideEvent()
        {
            if (this.Visible)
            {
                if (data != null && nowElement != -1 && nowElement < data.Count)
                {
                    dataControls[nowElement].BackColor = Theme.BackColor;
                    dataControls[nowElement].ForeColor = Theme.TextColor;
                }
                nowElement = -1;
                this.Hide();
                ClearAll();
            }
            else
            {
                data = null;
                DrawApps();
                this.Show();
            }
        }

        private void LeftArrClick()
        {
            if (nowElement - 1 < 0) return;
            if (data == null || data[nowElement - 1].name == "") return;
            dataControls[nowElement].BackColor = Theme.BackColor;
            dataControls[nowElement].ForeColor = Theme.TextColor;
            nowElement--;
            dataControls[nowElement].BackColor = Theme.SelectBackColor;
            dataControls[nowElement].ForeColor = Theme.SelectTextColor;
        }

        private void RightArrClick()
        {
            if (nowElement + 2 > dataControls.Count) return;
            if (data == null || nowElement + 2 > data.Count || data[nowElement + 1].name == "") return;
            if (nowElement != -1)
            {
                dataControls[nowElement].BackColor = Theme.BackColor;
                dataControls[nowElement].ForeColor = Theme.TextColor;
            }
            else if (data.Count > 1)
            {
                nowElement++;
            }
            nowElement++;
            dataControls[nowElement].BackColor = Theme.SelectBackColor;
            dataControls[nowElement].ForeColor = Theme.SelectTextColor;
        }

        private void ShutdownClick()
        {
            data = new List<App>() {
                new App("shutdown", "hibernation", "/h /t 0", DateTime.Now),
                new App("shutdown", "reboot", "/r /t 0", DateTime.Now),
                new App("shutdown", "shutdown", "/s /t 0", DateTime.Now)
            };

            DrawApps();
            if (!this.Visible)
            {
                this.Show();
            }

            ConfigLoader.SaveLastStartedConfig(ConfigPath, lastStartedApps);

            foreach (IFinder finder in finders)
            {
                finder.Save();
            }
        }

        private void EnterClick()
        {
            if (data == null) return;
            if (data.Count > nowElement || nowElement == -1)
            {
                var app = data[nowElement == -1 ? data.Count - 1 : data.Count - nowElement - 1];
                if (!lastStartedApps.Contains(app))
                {
                    lastStartedApps.Enqueue(app);
                }
                app.StartProcess();
            }
            ShowHideEvent();
        }

        private void KeyClick(Keys key)
        {
            if ((int)key >= (int)Keys.A && (int)key <= (int)Keys.Z)
            {
                searchBox.Text += key.ToString().ToLower();
            }
            if (key == Keys.Back && searchBox.Text.Length > 0)
            {
                searchBox.Text = searchBox.Text.Remove(searchBox.Text.Length - 1);
            }
        }
    }
}
