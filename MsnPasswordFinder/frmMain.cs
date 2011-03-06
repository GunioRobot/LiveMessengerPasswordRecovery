using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

/*
Copyright 2009 Oguz Kartal

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

namespace MsnPasswordFinder
{
    public partial class frmMain : Form
    {
        LanguageSupport LangMgr = new LanguageSupport();
        LiveMessegerPasswordFinder MsnAccountFinder = null;

        public frmMain()
        {
            DialogResult Chooise = DialogResult.None;
            string WantToUse = "";

            frmLanguageSelection LangDlg = new frmLanguageSelection();
            LangDlg.ShowDialog();

            if (LangDlg.UseDefault)
            {
                LanguageSupport.Language = LanguageSupport.GetNativeLanguage();
            }
            else
            {
                LanguageSupport.Language = LangDlg.SelectedLanguage == 0 ? "Turkish" : "English";
            }



            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            MsnAccountFinder = new LiveMessegerPasswordFinder();
            LiveIdInformation Account;

            if (MsnAccountFinder.GetAccountInformations())
            {
                while (MsnAccountFinder.Read(out Account))
                {
                    lstAccounts.Items.Add(new ListViewItem(new string[] { Account.LiveId, Account.Password }));
                }

                MsnAccountFinder.Release();

            }
            else
            {
                MessageBox.Show(LangMgr["Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lnkGoTb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.oguzkartal.net");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void kopyalasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstAccounts.SelectedItems.Count == 0)
            {
                MessageBox.Show(LangMgr["SelectItemWarn"], "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ListViewItem Selected = lstAccounts.SelectedItems[0];
            string SelectedData = string.Format("{0} = {1}", Selected.SubItems[0].Text, Selected.SubItems[1].Text);
            Clipboard.SetText(SelectedData);
        }

        private void tümünüKopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem Selected = null;
            string SelectedData = "";

            for (int i = 0; i < lstAccounts.Items.Count; i++)
            {
                Selected = lstAccounts.Items[i];
                SelectedData += string.Format("{0} = {1}\r\n", Selected.SubItems[0].Text, Selected.SubItems[1].Text);
            }

            Clipboard.SetText(SelectedData);
        }
    }
}
