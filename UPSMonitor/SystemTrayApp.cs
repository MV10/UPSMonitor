namespace UPSMonitor
{
    internal class SystemTrayApp : ApplicationContext
    {
        private readonly NotifyIcon systemTrayIcon;

        public SystemTrayApp()
        {
            systemTrayIcon = new NotifyIcon()
            {
                Icon = new Icon("battery.ico"),
                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items =
                    {
                        new ToolStripMenuItem("History", null, new EventHandler(History), "History"),
                        new ToolStripMenuItem("Exit", null, new EventHandler(Exit), "Exit")
                    }
                },
                Visible = true
            };
        }

        private void History(object sender, EventArgs e)
        {
            // Dispose is automatic when Show is used
            new HistoryForm().Show();
        }

        private void Exit(object sender, EventArgs e)
        {
            systemTrayIcon.Visible = false;
            systemTrayIcon.Dispose();
            Program.Exit();
        }
    }
}
