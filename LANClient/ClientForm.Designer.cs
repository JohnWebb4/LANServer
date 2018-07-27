namespace LANServer.Client
{
    partial class ClientForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientForm));
            this.rTBConsole = new System.Windows.Forms.RichTextBox();
            this.tbInput = new System.Windows.Forms.TextBox();
            this.bttnSend = new System.Windows.Forms.Button();
            this.pnlSend = new System.Windows.Forms.Panel();
            this.bWorkStart = new System.ComponentModel.BackgroundWorker();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.bWorkRead = new System.ComponentModel.BackgroundWorker();
            this.pnlSend.SuspendLayout();
            this.SuspendLayout();
            // 
            // rTBConsole
            // 
            this.rTBConsole.BackColor = System.Drawing.Color.White;
            this.rTBConsole.Dock = System.Windows.Forms.DockStyle.Top;
            this.rTBConsole.Location = new System.Drawing.Point(0, 0);
            this.rTBConsole.Name = "rTBConsole";
            this.rTBConsole.ReadOnly = true;
            this.rTBConsole.Size = new System.Drawing.Size(284, 233);
            this.rTBConsole.TabIndex = 0;
            this.rTBConsole.TabStop = false;
            this.rTBConsole.Text = "";
            this.rTBConsole.TextChanged += new System.EventHandler(this.rTBConsole_TextChanged);
            // 
            // tbInput
            // 
            this.tbInput.BackColor = System.Drawing.Color.White;
            this.tbInput.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbInput.Location = new System.Drawing.Point(0, 0);
            this.tbInput.Name = "tbInput";
            this.tbInput.Size = new System.Drawing.Size(209, 20);
            this.tbInput.TabIndex = 1;
            // 
            // bttnSend
            // 
            this.bttnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.bttnSend.Location = new System.Drawing.Point(209, 0);
            this.bttnSend.Name = "bttnSend";
            this.bttnSend.Size = new System.Drawing.Size(75, 22);
            this.bttnSend.TabIndex = 2;
            this.bttnSend.Text = "Send";
            this.bttnSend.UseVisualStyleBackColor = true;
            this.bttnSend.Click += new System.EventHandler(this.bttnSend_Click);
            // 
            // pnlSend
            // 
            this.pnlSend.Controls.Add(this.bttnSend);
            this.pnlSend.Controls.Add(this.tbInput);
            this.pnlSend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSend.Location = new System.Drawing.Point(0, 239);
            this.pnlSend.Name = "pnlSend";
            this.pnlSend.Size = new System.Drawing.Size(284, 22);
            this.pnlSend.TabIndex = 3;
            // 
            // bWorkStart
            // 
            this.bWorkStart.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bWorkStart_DoWork);
            // 
            // timerUpdate
            // 
            this.timerUpdate.Enabled = true;
            this.timerUpdate.Interval = 1000;
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // bWorkRead
            // 
            this.bWorkRead.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bWorkRead_DoWork);
            // 
            // ClientForm
            // 
            this.AcceptButton = this.bttnSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.pnlSend);
            this.Controls.Add(this.rTBConsole);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "ClientForm";
            this.Text = "LAN Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientForm_FormClosing);
            this.Shown += new System.EventHandler(this.ClientForm_Shown);
            this.SizeChanged += new System.EventHandler(this.ClientForm_SizeChanged);
            this.pnlSend.ResumeLayout(false);
            this.pnlSend.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rTBConsole;
        private System.Windows.Forms.TextBox tbInput;
        private System.Windows.Forms.Button bttnSend;
        private System.Windows.Forms.Panel pnlSend;
        private System.ComponentModel.BackgroundWorker bWorkStart;
        private System.Windows.Forms.Timer timerUpdate;
        private System.ComponentModel.BackgroundWorker bWorkRead;
    }
}