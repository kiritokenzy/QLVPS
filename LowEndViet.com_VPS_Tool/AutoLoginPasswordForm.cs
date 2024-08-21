using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LowEndViet.com_VPS_Tool;

public class AutoLoginPasswordForm : Form
{
	private IContainer components = null;

	private TextBox txtAutoLoginPassword;

	private Label label1;

	private Button btnSubmitAutoLoginPassword;

	private Label label2;

	public string autoLoginPassword { get; set; }

	public AutoLoginPasswordForm()
	{
		InitializeComponent();
		MaximumSize = base.Size;
		MinimumSize = base.Size;
	}

	private void btnSubmitAutoLoginPassword_Click(object sender, EventArgs e)
	{
		submitPassword();
	}

	private void txtAutoLoginPassword_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			submitPassword();
		}
	}

	private void submitPassword()
	{
		autoLoginPassword = txtAutoLoginPassword.Text;
		base.DialogResult = DialogResult.OK;
		Close();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.txtAutoLoginPassword = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.btnSubmitAutoLoginPassword = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.txtAutoLoginPassword.Location = new System.Drawing.Point(96, 55);
		this.txtAutoLoginPassword.Name = "txtAutoLoginPassword";
		this.txtAutoLoginPassword.Size = new System.Drawing.Size(176, 20);
		this.txtAutoLoginPassword.TabIndex = 0;
		this.txtAutoLoginPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(txtAutoLoginPassword_KeyDown);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 58);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(77, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Your password";
		this.btnSubmitAutoLoginPassword.Location = new System.Drawing.Point(96, 90);
		this.btnSubmitAutoLoginPassword.Name = "btnSubmitAutoLoginPassword";
		this.btnSubmitAutoLoginPassword.Size = new System.Drawing.Size(89, 23);
		this.btnSubmitAutoLoginPassword.TabIndex = 2;
		this.btnSubmitAutoLoginPassword.Text = "OK";
		this.btnSubmitAutoLoginPassword.UseVisualStyleBackColor = true;
		this.btnSubmitAutoLoginPassword.Click += new System.EventHandler(btnSubmitAutoLoginPassword_Click);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 9);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(248, 26);
		this.label2.TabIndex = 3;
		this.label2.Text = "In order for this program to run at startup, you have \r\nto enter the CURRENT password.";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(284, 123);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.btnSubmitAutoLoginPassword);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.txtAutoLoginPassword);
		base.Name = "AutoLoginPasswordForm";
		this.Text = "Password";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
