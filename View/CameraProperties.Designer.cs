﻿namespace View
{
    partial class CameraProperties
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
            this.objectProperties1 = new View.ObjectProperties();
            this.SuspendLayout();
            // 
            // objectProperties1
            // 
            this.objectProperties1.Location = new System.Drawing.Point(13, 13);
            this.objectProperties1.MainUserControl = null;
            this.objectProperties1.Model = null;
            this.objectProperties1.Name = "objectProperties1";
            this.objectProperties1.Size = new System.Drawing.Size(340, 237);
            this.objectProperties1.TabIndex = 0;
            // 
            // CameraProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(366, 262);
            this.Controls.Add(this.objectProperties1);
            this.Name = "CameraProperties";
            this.Text = "CameraProperties";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CameraProperties_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private ObjectProperties objectProperties1;
    }
}