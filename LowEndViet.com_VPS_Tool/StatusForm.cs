using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LowEndViet.com_VPS_Tool;

public class StatusForm : Form
{
	private delegate void DelegateUpdateProgress();

	private List<LevCheckbox> levCheckboxList;

	private IContainer components = null;

	private GroupBox groupBox1;

	private Button btnClose;

	private RichTextBox rtbProgress;

	public StatusForm()
	{
		InitializeComponent();
		MaximumSize = base.Size;
		MinimumSize = base.Size;
	}

	public StatusForm(List<LevCheckbox> levCheckboxList)
	{
		InitializeComponent();
		this.levCheckboxList = levCheckboxList;
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	public void updateProgress()
	{
		if (rtbProgress.InvokeRequired)
		{
			DelegateUpdateProgress method = updateProgress;
			Invoke(method);
			return;
		}
		rtbProgress.Text = "";
		foreach (LevCheckbox levCheckbox in levCheckboxList)
		{
			if (levCheckbox.checkBox.Checked)
			{
				RichTextBox richTextBox = rtbProgress;
				richTextBox.Text = richTextBox.Text + levCheckbox.status + Environment.NewLine;
			}
		}
		rtbProgress.Update();
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
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.btnClose = new System.Windows.Forms.Button();
		this.rtbProgress = new System.Windows.Forms.RichTextBox();
		this.groupBox1.SuspendLayout();
		base.SuspendLayout();
		this.groupBox1.Controls.Add(this.rtbProgress);
		this.groupBox1.Location = new System.Drawing.Point(4, 2);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(516, 215);
		this.groupBox1.TabIndex = 0;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Progress status";
		this.btnClose.Location = new System.Drawing.Point(436, 223);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(75, 23);
		this.btnClose.TabIndex = 1;
		this.btnClose.Text = "Close";
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		this.rtbProgress.Location = new System.Drawing.Point(6, 19);
		this.rtbProgress.Name = "rtbProgress";
		this.rtbProgress.Size = new System.Drawing.Size(504, 190);
		this.rtbProgress.TabIndex = 0;
		this.rtbProgress.Text = "";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(523, 253);
		base.Controls.Add(this.btnClose);
		base.Controls.Add(this.groupBox1);
		base.Name = "StatusForm";
		this.Text = "Progress";
		this.groupBox1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
