using System.Diagnostics;

namespace UPSMonitor
{
    public partial class HistoryForm : Form
    {
        private List<Message> messages = Program.MessageHistory.ToList();

        public HistoryForm()
        {
            InitializeComponent();

            if (messages.Count == 0)
            {
                lstMessages.Items.Add("(no messages)");
                lstMessages.Enabled = false;
            }
            else
            {
                messages.Reverse();

                foreach (var msg in messages)
                    lstMessages.Items.Add(msg.Timestamp.ToString("yyyy-dd-MM hh:mm:ss tt"));

                lstMessages.SelectedIndex = 0;
            }
        }

        private void lstMessages_SelectedIndexChanged(object sender, EventArgs e)
            => txtDetails.Text = messages[lstMessages.SelectedIndex].Content
                .Replace("\n", "\r\n")
                .Replace(Program.SeparatorControlCode, "\r\n");
    }
}
