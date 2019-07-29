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
        List<Label> data_controls = new List<Label>();
        KeyHook keyHook;
        int nowElement = -1;
        string searchText = "";
        const int tile_width = 180;

        public WIN()
        {
            InitializeComponent();
            ConfigPath = $"{PropsPath}\\SmartWIN";
            this.lastStartedApps = ConfigLoader.LoadFavoritesConfig(ConfigPath);
            this.Width = SystemInformation.PrimaryMonitorSize.Width;
            this.MaximumSize = this.MinimumSize = new Size(this.Width, 22);
            this.BackColor = Theme.FormBackColor;
            this.Shown += delegate {
                this.Hide();
                //TODO: REMOVE WIN StartBTN
                Process shellStart = new Process();
                shellStart.StartInfo.FileName = "explorer";
                shellStart.Start();
            };

            //Spawn controls
            Label temp;
            searchBox.Location = new Point(50, 1);
            searchBox.Font = Theme.TextFont;
            searchBox.ForeColor = Theme.TextColor;
            searchBox.BackColor = Theme.BackColor;
            for (int pos_x = 280; pos_x + 202 < this.Width - 30; pos_x += tile_width)
            {
                temp = new Label();
                temp.Location = new Point(pos_x, 0);
                temp.Size = new Size(tile_width, 25);
                temp.TextAlign = ContentAlignment.MiddleCenter;
                temp.Font = Theme.TextFont;
                temp.BackColor = Theme.BackColor;
                temp.ForeColor = Theme.TextColor;
                this.Controls.Add(temp);
                data_controls.Add(temp);
            }
            //Start KeyHook
            keyHook = new KeyHook(this);
            keyHook.LeftArrowEvent += LeftArrClick;
            keyHook.RightArrowEvent += RightArrClick;
            keyHook.EnterClickEvent += EnterClick;
            keyHook.OpenCloseEvent += ShowHideEvent;
            keyHook.KeyClickEvent += KeyClick;

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
                    data.AddRange(get.Count > 10 ? get.Take(10) : get);
                    if (data.Count >= 10)
                    {
                        return data.Take(10);
                    }
                }
            }

            return data.Count == 0? null : data;
        }

        private void DrawApps()
        {
            ClearSelection();
            if (data == null)
            {
                data = new List<App>(lastStartedApps.ToArray());
            }
            int i = 0;
            for (; i < data_controls.Count && i < data.Count; i++)
            {
                data_controls[i].Text = data[data.Count - i - 1].name;
            }
            for (; i < data_controls.Count; i++)
            {
                data_controls[i].Text = "";
            }
        }

        private void ClearDataControls()
        {
            for (int i = 0; i < data_controls.Count; i++)
            {
                data_controls[i].Text = "";
            }
        }

        private void ClearSelection()
        {
            if (nowElement != -1)
            {
                data_controls[nowElement].BackColor = Theme.BackColor;
                data_controls[nowElement].ForeColor = Theme.TextColor;
                nowElement = -1;
            }
        }

        private void ClearAll()
        {
            searchBox.Text = "";
            ClearDataControls();
            ClearSelection();
        }

        //-------------------------------FORM EVENTS-------------------------------

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

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            foreach (IFinder finder in finders)
            {
                finder.Save();
            }
        }
        private void SystemEvents_SessionEnded(object sender, Microsoft.Win32.SessionEndedEventArgs e)
        {
            foreach (IFinder finder in finders)
            {
                finder.Save();
            }
        }

    //-------------------------------OWN EVENTS-------------------------------

    private void ShowHideEvent()
        {
            if (this.Visible)
            {
                if (data != null && nowElement != -1 && nowElement < data.Count)
                {
                    data_controls[nowElement].BackColor = Theme.BackColor;
                    data_controls[nowElement].ForeColor = Theme.TextColor;
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
            data_controls[nowElement].BackColor = Theme.BackColor;
            data_controls[nowElement].ForeColor = Theme.TextColor;
            nowElement--;
            data_controls[nowElement].BackColor = Theme.SelectBackColor;
            data_controls[nowElement].ForeColor = Theme.SelectTextColor;
        }

        private void RightArrClick()
        {
            if (nowElement + 2 > data_controls.Count) return;
            if (data == null || nowElement + 2 > data.Count || data[nowElement + 1].name == "") return;
            if (nowElement != -1)
            {
                data_controls[nowElement].BackColor = Theme.BackColor;
                data_controls[nowElement].ForeColor = Theme.TextColor;
            }
            else if (data.Count > 1)
            {
                nowElement++;
            }
            nowElement++;
            data_controls[nowElement].BackColor = Theme.SelectBackColor;
            data_controls[nowElement].ForeColor = Theme.SelectTextColor;
        }

        private void EnterClick()
        {
            if (data == null) return;
            if (data.Count > nowElement || nowElement == -1)
            {
                var app = data[nowElement == -1 ? 0 : data.Count - nowElement - 1];
                app.StartProcess();
                lastStartedApps.Enqueue(app);
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
