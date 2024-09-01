namespace TTGVoxModifier
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            exportBtn = new Button();
            importBtn = new Button();
            inputTB = new TextBox();
            outputTB = new TextBox();
            inputFolderLabel = new Label();
            outputFolderLabel = new Label();
            button1 = new Button();
            button2 = new Button();
            listBox1 = new ListBox();
            gamelistCB = new ComboBox();
            label1 = new Label();
            progressBar1 = new ProgressBar();
            SuspendLayout();
            // 
            // exportBtn
            // 
            exportBtn.Location = new Point(182, 187);
            exportBtn.Name = "exportBtn";
            exportBtn.Size = new Size(75, 23);
            exportBtn.TabIndex = 0;
            exportBtn.Text = "Extract";
            exportBtn.UseVisualStyleBackColor = true;
            exportBtn.Click += button1_Click;
            // 
            // importBtn
            // 
            importBtn.Location = new Point(338, 187);
            importBtn.Name = "importBtn";
            importBtn.Size = new Size(75, 23);
            importBtn.TabIndex = 1;
            importBtn.Text = "Import";
            importBtn.UseVisualStyleBackColor = true;
            importBtn.Click += button2_Click;
            // 
            // inputTB
            // 
            inputTB.Location = new Point(182, 86);
            inputTB.Name = "inputTB";
            inputTB.Size = new Size(424, 23);
            inputTB.TabIndex = 2;
            // 
            // outputTB
            // 
            outputTB.Location = new Point(182, 145);
            outputTB.Name = "outputTB";
            outputTB.Size = new Size(424, 23);
            outputTB.TabIndex = 3;
            // 
            // inputFolderLabel
            // 
            inputFolderLabel.AutoSize = true;
            inputFolderLabel.Location = new Point(98, 89);
            inputFolderLabel.Name = "inputFolderLabel";
            inputFolderLabel.Size = new Size(72, 15);
            inputFolderLabel.TabIndex = 4;
            inputFolderLabel.Text = "Input folder:";
            // 
            // outputFolderLabel
            // 
            outputFolderLabel.AutoSize = true;
            outputFolderLabel.Location = new Point(88, 148);
            outputFolderLabel.Name = "outputFolderLabel";
            outputFolderLabel.Size = new Size(82, 15);
            outputFolderLabel.TabIndex = 5;
            outputFolderLabel.Text = "Output folder:";
            // 
            // button1
            // 
            button1.Location = new Point(630, 86);
            button1.Name = "button1";
            button1.Size = new Size(27, 23);
            button1.TabIndex = 6;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // button2
            // 
            button2.Location = new Point(630, 145);
            button2.Name = "button2";
            button2.Size = new Size(27, 23);
            button2.TabIndex = 7;
            button2.Text = "...";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click_1;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(24, 277);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(633, 274);
            listBox1.TabIndex = 8;
            // 
            // gamelistCB
            // 
            gamelistCB.DropDownStyle = ComboBoxStyle.DropDownList;
            gamelistCB.FormattingEnabled = true;
            gamelistCB.Location = new Point(182, 39);
            gamelistCB.Name = "gamelistCB";
            gamelistCB.Size = new Size(475, 23);
            gamelistCB.TabIndex = 9;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(40, 42);
            label1.Name = "label1";
            label1.Size = new Size(130, 15);
            label1.TabIndex = 10;
            label1.Text = "Game's encryption key:";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(24, 229);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(633, 23);
            progressBar1.TabIndex = 11;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(685, 567);
            Controls.Add(progressBar1);
            Controls.Add(label1);
            Controls.Add(gamelistCB);
            Controls.Add(listBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(outputFolderLabel);
            Controls.Add(inputFolderLabel);
            Controls.Add(outputTB);
            Controls.Add(inputTB);
            Controls.Add(importBtn);
            Controls.Add(exportBtn);
            MaximizeBox = false;
            Name = "Form1";
            Text = "Telltale's vox modifier";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button exportBtn;
        private Button importBtn;
        private TextBox inputTB;
        private TextBox outputTB;
        private Label inputFolderLabel;
        private Label outputFolderLabel;
        private Button button1;
        private Button button2;
        private ListBox listBox1;
        private ComboBox gamelistCB;
        private Label label1;
        private ProgressBar progressBar1;
    }
}
