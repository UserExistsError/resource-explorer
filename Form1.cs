using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            this.resourceListView.ItemDrag += ResourceListView_ItemDrag;
        }

        private void ResourceListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // need to copy the resource to a temp file first
            ListView view = (ListView)sender;
            ListViewItem item = view.SelectedItems[0];
            Resource resource = this.resourceList[int.Parse(item.SubItems[0].Text)];
            string tempFile = Path.Combine(Path.GetTempPath(), resource.GetDefaultExportName());
            using (System.IO.FileStream file = new System.IO.FileStream(tempFile, FileMode.Create))
            {
                ResourceReader reader = resource.GetReader();
                while (reader.Remaining() > 0)
                {
                    byte[] data = reader.Read(65536);
                    file.Write(data, 0, data.Length);
                }
            }
            string[] paths = { tempFile };
            this.resourceListView.DoDragDrop(new DataObject(DataFormats.FileDrop, paths), DragDropEffects.Move);
            // cleanup in case file isn't dropped/moved
            File.Delete(tempFile);
        }

        public void DisplayStatus(string message)
        {
            this.statusLabel.Text = message;
        }

        private void ResetUi()
        {
            this.previewPanel.Controls.Clear();
            this.resourceListView.Items.Clear();
            this.openFileLabel.Text = "";
            DisplayStatus("");
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
            DisplayStatus("");
            this.previewPanel.Controls.Clear();
            ListView view = (ListView)sender;
            ListView.SelectedListViewItemCollection selectedItems = view.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return;
            }
            ListViewItem item0 = selectedItems[0];
            Resource resource = this.resourceList[int.Parse(item0.SubItems[0].Text)];
            if (resource.IsImageType())
            {
                PictureBox pic = new PictureBox();
                try
                {
                    pic.Image = resource.GetImage();
                    pic.SizeMode = PictureBoxSizeMode.AutoSize;
                    this.previewPanel.Controls.Add(pic);
                }
                catch (ArgumentException)
                {
                    DisplayStatus("Failed to get image preview");
                }
            }
            else if (resource.GetDisplayType() == "STRING" ||
                resource.GetDisplayType() == "HTML" ||
                resource.GetDisplayType() == "MANIFEST" ||
                resource.GetDisplayType() == "VERSION")
            {
                byte[] data = resource.GetReader().Read(4096);
                TextBox box = new TextBox();
                if (resource.GetDisplayType() == "VERSION")
                {
                    box.Text = Encoding.Unicode.GetString(data).Substring(3).Replace('\u0000', ' ');
                }
                else
                {
                    box.Text = resource.GetEncoding().GetString(data);
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
                byte[] data = resource.GetReader().Read(2048);
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
            byte[] data = resource.GetReader().Read();
            if (data == null)
            {
                DisplayStatus("Failed to read resource");
                return;
            }
            this.saveFileDialog1.FileName = resource.GetDefaultExportName();
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
                DisplayStatus("Failed to process file: " + filename);
                return;
            }
            this.openFileLabel.Text = filename;
            this.resourceList = currentFile.GetResourceList();
            for (int i = 0; i < this.resourceList.Count; i++) {
                Resource r = this.resourceList[i];
                ListViewItem item = new ListViewItem(i.ToString());
                item.SubItems.Add(r.name);
                item.SubItems.Add(r.GetDisplayType());
                item.SubItems.Add(r.size.ToString());
                item.SubItems.Add(r.GetHexPreview());
                item.SubItems.Add(r.GetContextualPreview());
                this.resourceListView.Items.Add(item);
            }
            // resize columns (very slow)
            //this.resourceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //this.resourceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
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
