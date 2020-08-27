using System;
using System.Windows.Forms;

namespace SCTVObjects
{
    public partial class AddNewItem : Form
    {
        public string ItemToAdd
        {
            get { return txtAdd.Text; }
        }

        public AddNewItem()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;

            this.Close();
        }
    }
}