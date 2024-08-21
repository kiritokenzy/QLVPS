using System.Windows.Forms;

namespace LowEndViet.com_VPS_Tool;

public class LevCheckbox
{
	public CheckBox checkBox { get; set; }

	public string status { get; set; }

	public string command { get; set; }

	public string softwareURL { get; set; }

	public string setupFileName { get; set; }

	public string remark { get; set; }

	public LevCheckbox(CheckBox chk, string command)
	{
		checkBox = chk;
		this.command = command;
		status = checkBox.Text + " >>> Waiting.....";
	}

	public LevCheckbox(CheckBox chk, string url, string fileName, string command, string remark = null)
	{
		checkBox = chk;
		this.command = command;
		softwareURL = url;
		setupFileName = fileName;
		status = checkBox.Text + " >>> Xin chờ.....";
		this.remark = remark;
	}

	public void updateResultStatus(int exitCode)
	{
		if (exitCode == 0)
		{
			status = checkBox.Text + " >>> Thành công!";
		}
		else
		{
			status = checkBox.Text + " >>> Có lỗi xảy ra!";
		}
		if (remark != null)
		{
			status += remark;
		}
	}

	public void updateInstallingStatus()
	{
		status = checkBox.Text + " >>> Đang cài đặt...";
	}

	public void updateDownloadingStatus()
	{
		status = checkBox.Text + " >>> Đang tải...";
	}
}
