using System;
using System.Windows.Forms;

namespace SCTVObjects
{
    public partial class internetConnectionDown : Form
    {
        public internetConnectionDown()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (chbDontTellAgain.Checked)
                this.DialogResult = DialogResult.Ignore;
            else
                this.DialogResult = DialogResult.Retry;

            this.Close();
        }
    }
}