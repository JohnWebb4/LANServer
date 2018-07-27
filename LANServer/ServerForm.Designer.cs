using System;

namespace LANServer
{
    partial class ServerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerForm));
            this.rtbConsole = new System.Windows.Forms.RichTextBox();
            this.pnlSend = new System.Windows.Forms.Panel();
            this.tbSend = new System.Windows.Forms.TextBox();
            this.bttnSend = new System.Windows.Forms.Button();
            this.bWorkStart = new System.ComponentModel.BackgroundWorker();
            this.pnlSend.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtbConsole
            // 
            this.rtbConsole.Dock = System.Windows.Forms.DockStyle.Top;
            this.rtbConsole.Location = new System.Drawing.Point(0, 0);
            this.rtbConsole.Name = "rtbConsole";
            this.rtbConsole.Size = new System.Drawing.Size(284, 235);
            this.rtbConsole.TabIndex = 20;
            this.rtbConsole.TabStop = false;
            this.rtbConsole.Text = "";
            this.rtbConsole.TextChanged += new System.EventHandler(this.rtbConsole_TextChanged);
            // 
            // pnlSend
            // 
            this.pnlSend.Controls.Add(this.tbSend);
            this.pnlSend.Controls.Add(this.bttnSend);
            this.pnlSend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSend.Location = new System.Drawing.Point(0, 241);
            this.pnlSend.Name = "pnlSend";
            this.pnlSend.Size = new System.Drawing.Size(284, 20);
            this.pnlSend.TabIndex = 0;
            // 
            // tbSend
            // 
            this.tbSend.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbSend.Location = new System.Drawing.Point(0, 0);
            this.tbSend.Name = "tbSend";
            this.tbSend.Size = new System.Drawing.Size(213, 20);
            this.tbSend.TabIndex = 1;
            // 
            // bttnSend
            // 
            this.bttnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.bttnSend.Location = new System.Drawing.Point(211, 0);
            this.bttnSend.Name = "bttnSend";
            this.bttnSend.Size = new System.Drawing.Size(73, 20);
            this.bttnSend.TabIndex = 2;
            this.bttnSend.Text = "Send";
            this.bttnSend.UseVisualStyleBackColor = true;
            this.bttnSend.Click += new System.EventHandler(this.bttnSend_Click);
            // 
            // bWorkStart
            // 
            this.bWorkStart.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bWorkStart_DoWork);
            this.bWorkStart.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bWorkStart_RunWorkerCompleted);
            // 
            // ServerForm
            // 
            this.AcceptButton = this.bttnSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.pnlSend);
            this.Controls.Add(this.rtbConsole);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ServerForm";
            this.Text = "ServerForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerForm_FormClosing);
            this.Shown += new System.EventHandler(this.ServerForm_Shown);
            this.SizeChanged += new System.EventHandler(this.ServerForm_SizeChanged);
            this.pnlSend.ResumeLayout(false);
            this.pnlSend.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbConsole;
        private System.Windows.Forms.Panel pnlSend;
        private System.Windows.Forms.TextBox tbSend;
        private System.Windows.Forms.Button bttnSend;
        private System.ComponentModel.BackgroundWorker bWorkStart;
    }
}