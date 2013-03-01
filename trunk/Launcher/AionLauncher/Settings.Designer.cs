namespace AionLauncher
{
    partial class Settings
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
            this.title = new System.Windows.Forms.Label();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.Drag = new Microsoft.VisualBasic.PowerPacks.RectangleShape();
            this.CloseBtn = new Glass.GlassButton();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.title.Location = new System.Drawing.Point(12, 9);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(93, 15);
            this.title.TabIndex = 0;
            this.title.Text = "Configuration";
            this.title.Click += new System.EventHandler(this.title_Click);
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.Drag});
            this.shapeContainer1.Size = new System.Drawing.Size(395, 365);
            this.shapeContainer1.TabIndex = 1;
            this.shapeContainer1.TabStop = false;
            // 
            // Drag
            // 
            this.Drag.BackColor = System.Drawing.Color.Transparent;
            this.Drag.BorderColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Drag.BorderWidth = 2;
            this.Drag.FillColor = System.Drawing.Color.Transparent;
            this.Drag.FillGradientColor = System.Drawing.Color.Transparent;
            this.Drag.Location = new System.Drawing.Point(1, 1);
            this.Drag.Name = "Drag";
            this.Drag.SelectionColor = System.Drawing.Color.Transparent;
            this.Drag.Size = new System.Drawing.Size(393, 363);
            this.Drag.MouseDown += new System.Windows.Forms.MouseEventHandler(this.drag_MouseDown);
            // 
            // CloseBtn
            // 
            this.CloseBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.CloseBtn.ForeColor = System.Drawing.Color.Transparent;
            this.CloseBtn.GlowColor = System.Drawing.Color.Silver;
            this.CloseBtn.Image = global::AionLauncher.Properties.Resources.close;
            this.CloseBtn.InnerBorderColor = System.Drawing.Color.Transparent;
            this.CloseBtn.Location = new System.Drawing.Point(366, 12);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.OuterBorderColor = System.Drawing.Color.Transparent;
            this.CloseBtn.ShineColor = System.Drawing.Color.Transparent;
            this.CloseBtn.Size = new System.Drawing.Size(17, 18);
            this.CloseBtn.TabIndex = 2;
            this.CloseBtn.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.ClientSize = new System.Drawing.Size(395, 365);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.title);
            this.Controls.Add(this.shapeContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.RectangleShape Drag;
        private Glass.GlassButton CloseBtn;
    }
}