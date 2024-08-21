using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LowEndViet.com_VPS_Tool;

public class ForcePasswordChange : Form
{
	private IContainer components = null;

	private Label label1;

	private Label label2;

	private TextBox txtNewPassword;

	private Button btnForceChangePassword;

	private Label label3;

	private Label lblPasswordStrength;

	public string newPassword { get; set; }

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ClassStyle |= 512;
			return createParams;
		}
	}

	public ForcePasswordChange()
	{
		InitializeComponent();
	}

	private void btnForceChangePassword_Click(object sender, EventArgs e)
	{
		submitPassword();
	}

	private void txtNewPassword_TextChanged(object sender, EventArgs e)
	{
		if (isStrongPassword(txtNewPassword.Text))
		{
			lblPasswordStrength.Text = "Good";
			lblPasswordStrength.ForeColor = Color.Green;
		}
		else
		{
			lblPasswordStrength.Text = "Weak";
			lblPasswordStrength.ForeColor = Color.Red;
		}
	}

	private void txtNewPassword_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			submitPassword();
		}
	}

	private bool isStrongPassword(string password)
	{
		string[] array = null;
		try
		{
			array = File.ReadAllText("C:\\Users\\Public\\LEV\\bad_pw.txt").Split(',');
		}
		catch
		{
		}
		if (password.Length >= 8 && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit) && (array == null || !array.Contains(password)))
		{
			return true;
		}
		return false;
	}

	private void submitPassword()
	{
		if (!isStrongPassword(txtNewPassword.Text))
		{
			MessageBox.Show("Password must be:\r\n- More than 8 characters\r\n- Contains UPPER CASE leters (A-Z)\r\n- Contains lower case letter (a-z)\r\n- Contains number (0-9)");
			return;
		}
		newPassword = txtNewPassword.Text;
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
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.txtNewPassword = new System.Windows.Forms.TextBox();
		this.btnForceChangePassword = new System.Windows.Forms.Button();
		this.label3 = new System.Windows.Forms.Label();
		this.lblPasswordStrength = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 17);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(259, 39);
		this.label1.TabIndex = 0;
		this.label1.Text = "You have to change you password for the first log on.\r\nYour password must have at least 8 characters and\r\ncontain both numbers and letters.";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 68);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(106, 13);
		this.label2.TabIndex = 0;
		this.label2.Text = "Your NEW password";
		this.txtNewPassword.Location = new System.Drawing.Point(118, 65);
		this.txtNewPassword.Name = "txtNewPassword";
		this.txtNewPassword.Size = new System.Drawing.Size(167, 20);
		this.txtNewPassword.TabIndex = 1;
		this.txtNewPassword.TextChanged += new System.EventHandler(txtNewPassword_TextChanged);
		this.txtNewPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(txtNewPassword_KeyDown);
		this.btnForceChangePassword.Location = new System.Drawing.Point(118, 119);
		this.btnForceChangePassword.Name = "btnForceChangePassword";
		this.btnForceChangePassword.Size = new System.Drawing.Size(75, 23);
		this.btnForceChangePassword.TabIndex = 2;
		this.btnForceChangePassword.Text = "Change";
		this.btnForceChangePassword.UseVisualStyleBackColor = true;
		this.btnForceChangePassword.Click += new System.EventHandler(btnForceChangePassword_Click);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(12, 93);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(47, 13);
		this.label3.TabIndex = 0;
		this.label3.Text = "Strength";
		this.lblPasswordStrength.AutoSize = true;
		this.lblPasswordStrength.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lblPasswordStrength.ForeColor = System.Drawing.Color.Red;
		this.lblPasswordStrength.Location = new System.Drawing.Point(115, 93);
		this.lblPasswordStrength.Name = "lblPasswordStrength";
		this.lblPasswordStrength.Size = new System.Drawing.Size(48, 16);
		this.lblPasswordStrength.TabIndex = 0;
		this.lblPasswordStrength.Text = "Weak";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(297, 154);
		base.Controls.Add(this.btnForceChangePassword);
		base.Controls.Add(this.txtNewPassword);
		base.Controls.Add(this.lblPasswordStrength);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Name = "ForcePasswordChange";
		this.Text = "Force Password Change";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
