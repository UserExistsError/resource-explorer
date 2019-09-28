using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ResourceExplorer
{
    public partial class ResourceExplorer : Form
    {
        Win32Resources currentFile;
        IList<Resource> resourceList;

        public ResourceExplorer()
        {
            InitializeComponent();

            this.resourceList = new List<Resource>();

            this.resourceListView.View = View.Details;
            this.resourceListView.AllowColumnReorder = true;
            this.resourceListView.GridLines = true;
            this.resourceListView.FullRowSelect = true;
            this.resourceListView.MultiSelect = false;
            this.resourceListView.Columns.Add("Index", 45); // into resourceList
            this.resourceListView.Columns.Add("Name", 80);
            this.resourceListView.Columns.Add("Type", 80);
            this.resourceListView.Columns.Add("Size", 80);
            this.resourceListView.Columns.Add("Hex", 200);
            this.resourceListView.Columns.Add("Printable", -2);
        }

        public void DisplayStatus(string message)
        {
            //XXX messagebox or inline UI element instead
            Console.WriteLine(message);
        }

        private void ResetUi()
        {
            this.previewPanel.Controls.Clear();
            this.resourceListView.Items.Clear();
        }

        private Resource[] GetSelectedResources()
        {
            Resource[] resourceArray = new Resource[this.resourceListView.SelectedItems.Count];
            for (int i = 0; i < resourceArray.Length; i++)
            {
                int index = int.Parse(this.resourceListView.SelectedItems[i].SubItems[0].Text);
                resourceArray[i] = this.resourceList[index];
            }
            return resourceArray;
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.previewPanel.Controls.Clear();
            ListView view = (ListView)sender;
            ListView.SelectedListViewItemCollection selectedItems = view.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return;
            }
            ListViewItem item0 = selectedItems[0];
            Resource resource = this.resourceList[int.Parse(item0.SubItems[0].Text)];
            if (resource.getDisplayType() == "BITMAP" || resource.getDisplayType() == "ICON")
            {
                byte[] data = this.currentFile.GetResource(resource);
                MemoryStream stream = new MemoryStream(data);
                PictureBox pic = new PictureBox();
                try
                {
                    if (resource.getDisplayType() == "BITMAP")
                        pic.Image = new Bitmap(stream);
                    else
                        pic.Image = Bitmap.FromHicon(new Icon(stream, new Size(48, 48)).Handle);
                    pic.SizeMode = PictureBoxSizeMode.AutoSize;
                    this.previewPanel.Controls.Add(pic);
                }
                catch (ArgumentException)
                {

                }
            }
            else if (resource.getDisplayType() == "STRING" ||
                resource.getDisplayType() == "HTML" ||
                resource.getDisplayType() == "MANIFEST" ||
                resource.getDisplayType() == "VERSION")
            {
                byte[] data = this.currentFile.GetResource(resource, 2048);
                TextBox box = new TextBox();
                if (resource.getDisplayType() == "VERSION")
                {
                    box.Text = Encoding.Unicode.GetString(data).Substring(3).Replace('\u0000', ' ');
                }
                else
                {
                    box.Text = resource.getEncoding().GetString(data);
                }
                box.Multiline = true;
                box.Dock = DockStyle.Fill;
                box.WordWrap = true;
                box.ScrollBars = ScrollBars.Both;
                this.previewPanel.Controls.Add(box);
            }
            else
            {
                // best effort "strings"
                byte[] data = this.currentFile.GetResource(resource, 2048);
                string printable = Resource.GetString(data);
                if (printable.Length > 3)
                {
                    TextBox box = new TextBox();
                    box.Text = printable.ToString();
                    box.Multiline = true;
                    box.Dock = DockStyle.Fill;
                    box.WordWrap = true;
                    box.ScrollBars = ScrollBars.Both;
                    this.previewPanel.Controls.Add(box);
                }
            }
        }

        private void ExportResource(Resource resource)
        {
            byte[] data = currentFile.GetResource(resource);
            if (data == null)
            {
                DisplayStatus("Failed to GetResource");
                return;
            }
            this.saveFileDialog1.FileName = resource.getDefaultExportName();
            if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream outStream = this.saveFileDialog1.OpenFile();
                if (outStream != null)
                {
                    outStream.Write(data, 0, data.Length);
                    outStream.Close();
                }
            }
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Console.WriteLine(this.openFileDialog1.FileName);
            ProcessFile(this.openFileDialog1.FileName);
        }

        private void ProcessFile(string filename)
        {
            ResetUi();
            try
            {
                currentFile = new Win32Resources(filename);
            }
            catch (Exception exc)
            {
                DisplayStatus("Failed to process file " + filename);
                return;
            }
            this.openFileLabel.Text = filename;
            this.resourceList = currentFile.GetResourceList();
            for (int i = 0; i < this.resourceList.Count; i++) {
                Resource r = this.resourceList[i];
                ListViewItem item = new ListViewItem(i.ToString());
                item.SubItems.Add(r.name);
                item.SubItems.Add(r.getDisplayType());
                item.SubItems.Add(r.size.ToString());
                item.SubItems.Add(r.getHexPreview());
                item.SubItems.Add(r.getContextualPreview());
                this.resourceListView.Items.Add(item);
            }
            // resize columns
            this.resourceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.resourceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
        }

        private void ResourceListView_DragOver(object sender, DragEventArgs e)
        {
            // change cursor during dragover
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void ResourceListView_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            ProcessFile(files[0]);
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            Resource[] resourceArray = GetSelectedResources();
            foreach (Resource r in resourceArray)
            {
                ExportResource(r);
            }
        }
    }
}
