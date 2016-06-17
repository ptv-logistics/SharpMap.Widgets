namespace SharpMap.WinThin
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.formsMap1 = new Ptv.XServer.Controls.Map.FormsMap();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // formsMap1
            // 
            this.formsMap1.CoordinateDiplayFormat = Ptv.XServer.Controls.Map.CoordinateDiplayFormat.Degree;
            this.formsMap1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsMap1.FitInWindow = false;
            this.formsMap1.InvertMouseWheel = false;
            this.formsMap1.Location = new System.Drawing.Point(0, 0);
            this.formsMap1.MaxZoom = 19;
            this.formsMap1.MinZoom = 0;
            this.formsMap1.MouseDoubleClickZoom = true;
            this.formsMap1.MouseDragMode = Ptv.XServer.Controls.Map.Gadgets.DragMode.SelectOnShift;
            this.formsMap1.MouseWheelSpeed = 0.5D;
            this.formsMap1.Name = "formsMap1";
            this.formsMap1.ShowCoordinates = true;
            this.formsMap1.ShowLayers = true;
            this.formsMap1.ShowMagnifier = true;
            this.formsMap1.ShowNavigation = true;
            this.formsMap1.ShowOverview = true;
            this.formsMap1.ShowScale = true;
            this.formsMap1.ShowZoomSlider = true;
            this.formsMap1.Size = new System.Drawing.Size(803, 519);
            this.formsMap1.TabIndex = 0;
            this.formsMap1.UseAnimation = true;
            this.formsMap1.UseDefaultTheme = true;
            this.formsMap1.UseMiles = false;
            this.formsMap1.XMapCopyright = "Please configure a valid copyright text!";
            this.formsMap1.XMapCredentials = "";
            this.formsMap1.XMapStyle = "";
            this.formsMap1.XMapUrl = "";
            this.formsMap1.ZoomLevel = 1D;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 123);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(200, 396);
            this.propertyGrid1.TabIndex = 1;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(803, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 519);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(0, 0);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(200, 123);
            this.checkedListBox1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.propertyGrid1);
            this.panel1.Controls.Add(this.checkedListBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(806, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 519);
            this.panel1.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 519);
            this.Controls.Add(this.formsMap1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Windows Demo";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Ptv.XServer.Controls.Map.FormsMap formsMap1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Panel panel1;
    }
}

