﻿namespace ResourceExplorer
{
    partial class ResourceExplorer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.resourceListView = new System.Windows.Forms.ListView();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.openFileButton = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileLabel = new System.Windows.Forms.Label();
            this.exportButton = new System.Windows.Forms.Button();
            this.previewPanel = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.statusLabel = new System.Windows.Forms.Label();
            this.currentFileLeftLabel = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // resourceListView
            // 
            this.resourceListView.AllowDrop = true;
            this.resourceListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourceListView.HideSelection = false;
            this.resourceListView.Location = new System.Drawing.Point(0, 0);
            this.resourceListView.Name = "resourceListView";
            this.resourceListView.Size = new System.Drawing.Size(776, 512);
            this.resourceListView.TabIndex = 0;
            this.resourceListView.UseCompatibleStateImageBehavior = false;
            this.resourceListView.SelectedIndexChanged += new System.EventHandler(this.ListView1_SelectedIndexChanged);
            this.resourceListView.DragDrop += new System.Windows.Forms.DragEventHandler(this.ResourceListView_DragDrop);
            this.resourceListView.DragOver += new System.Windows.Forms.DragEventHandler(this.ResourceListView_DragOver);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog1_FileOk);
            // 
            // openFileButton
            // 
            this.openFileButton.Location = new System.Drawing.Point(12, 29);
            this.openFileButton.Name = "openFileButton";
            this.openFileButton.Size = new System.Drawing.Size(75, 23);
            this.openFileButton.TabIndex = 1;
            this.openFileButton.Text = "Open";
            this.openFileButton.UseVisualStyleBackColor = true;
            this.openFileButton.Click += new System.EventHandler(this.Button1_Click);
            // 
            // openFileLabel
            // 
            this.openFileLabel.AutoSize = true;
            this.openFileLabel.Location = new System.Drawing.Point(253, 34);
            this.openFileLabel.Name = "openFileLabel";
            this.openFileLabel.Size = new System.Drawing.Size(130, 13);
            this.openFileLabel.TabIndex = 2;
            this.openFileLabel.Text = "Click or drag to open a file";
            // 
            // exportButton
            // 
            this.exportButton.Location = new System.Drawing.Point(94, 29);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 4;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // previewPanel
            // 
            this.previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewPanel.Location = new System.Drawing.Point(0, 0);
            this.previewPanel.Name = "previewPanel";
            this.previewPanel.Size = new System.Drawing.Size(776, 148);
            this.previewPanel.TabIndex = 5;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 58);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.resourceListView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.previewPanel);
            this.splitContainer1.Size = new System.Drawing.Size(776, 664);
            this.splitContainer1.SplitterDistance = 512;
            this.splitContainer1.TabIndex = 6;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(187, 9);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 13);
            this.statusLabel.TabIndex = 7;
            // 
            // currentFileLeftLabel
            // 
            this.currentFileLeftLabel.AutoSize = true;
            this.currentFileLeftLabel.Location = new System.Drawing.Point(187, 34);
            this.currentFileLeftLabel.Name = "currentFileLeftLabel";
            this.currentFileLeftLabel.Size = new System.Drawing.Size(60, 13);
            this.currentFileLeftLabel.TabIndex = 8;
            this.currentFileLeftLabel.Text = "Current file:";
            // 
            // ResourceExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 734);
            this.Controls.Add(this.currentFileLeftLabel);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.openFileLabel);
            this.Controls.Add(this.openFileButton);
            this.Name = "ResourceExplorer";
            this.Text = "ResourceExplorer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView resourceListView;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button openFileButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label openFileLabel;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Panel previewPanel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label currentFileLeftLabel;
    }
}

