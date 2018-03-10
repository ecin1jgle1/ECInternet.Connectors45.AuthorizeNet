namespace TesterUI
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
			this.textboxCCNumber = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.textboxAmount = new System.Windows.Forms.TextBox();
			this.buttonAuthorize = new System.Windows.Forms.Button();
			this.textboxResults = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// textboxCCNumber
			// 
			this.textboxCCNumber.Location = new System.Drawing.Point(167, 120);
			this.textboxCCNumber.Name = "textboxCCNumber";
			this.textboxCCNumber.Size = new System.Drawing.Size(233, 23);
			this.textboxCCNumber.TabIndex = 0;
			this.textboxCCNumber.Text = "4111111111111111";
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(167, 150);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(46, 23);
			this.textBox2.TabIndex = 1;
			this.textBox2.Text = "0119";
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(167, 180);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(46, 23);
			this.textBox3.TabIndex = 2;
			this.textBox3.Text = "123";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(83, 128);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(33, 15);
			this.label1.TabIndex = 3;
			this.label1.Text = "CC #";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(83, 158);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(59, 15);
			this.label2.TabIndex = 4;
			this.label2.Text = "Expiration";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(83, 188);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(30, 15);
			this.label3.TabIndex = 5;
			this.label3.Text = "CCV";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(83, 218);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(51, 15);
			this.label4.TabIndex = 6;
			this.label4.Text = "Amount";
			// 
			// textboxAmount
			// 
			this.textboxAmount.Location = new System.Drawing.Point(167, 210);
			this.textboxAmount.Name = "textboxAmount";
			this.textboxAmount.Size = new System.Drawing.Size(100, 23);
			this.textboxAmount.TabIndex = 7;
			// 
			// buttonAuthorize
			// 
			this.buttonAuthorize.Location = new System.Drawing.Point(167, 265);
			this.buttonAuthorize.Name = "buttonAuthorize";
			this.buttonAuthorize.Size = new System.Drawing.Size(75, 23);
			this.buttonAuthorize.TabIndex = 8;
			this.buttonAuthorize.Text = "Authorize";
			this.buttonAuthorize.UseVisualStyleBackColor = true;
			this.buttonAuthorize.Click += new System.EventHandler(this.buttonAuthorize_Click);
			// 
			// textboxResults
			// 
			this.textboxResults.Location = new System.Drawing.Point(86, 332);
			this.textboxResults.Multiline = true;
			this.textboxResults.Name = "textboxResults";
			this.textboxResults.ReadOnly = true;
			this.textboxResults.Size = new System.Drawing.Size(466, 448);
			this.textboxResults.TabIndex = 9;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(986, 805);
			this.Controls.Add(this.textboxResults);
			this.Controls.Add(this.buttonAuthorize);
			this.Controls.Add(this.textboxAmount);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBox3);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textboxCCNumber);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textboxCCNumber;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textboxAmount;
		private System.Windows.Forms.Button buttonAuthorize;
		private System.Windows.Forms.TextBox textboxResults;
	}
}

