using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LowEndViet.com_VPS_Tool.Properties;
using Microsoft.Win32;

namespace LowEndViet.com_VPS_Tool;

public class form_LowEndVietFastVPSConfig : Form
{
	public class DNSConfig
	{
		public string serverName { get; set; }

		public string DNS1 { get; set; }

		public DNSConfig(string serverName, string DNS1)
		{
			this.serverName = serverName;
			this.DNS1 = DNS1;
		}

		public override string ToString()
		{
			return serverName + " | " + DNS1;
		}
	}

	private static readonly string APPNAME = "VM QuickConfig";

	public readonly string VERSION = "2022";

	private static readonly string GITNAME = "VM QuickConfig";

	private static readonly string GITHOME = "https://github.com/chieunhatnang/VM-QuickConfig";

	private static readonly string REG_STARTUP = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\";

	private static readonly string REG_LEV = "Software\\LEV\\VMQuickConfig";

	private static readonly string REG_RDP_PORT = "SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp";

	private static readonly string LEV_DIR = "C:\\Users\\Public\\LEV\\";

	private static readonly string DISKPART_CONFIG_PATH = LEV_DIR + "diskpartconfig.txt";

	private static readonly string NETWORK_CONFIG_PATH = LEV_DIR + "networkconfig.txt";

	public static List<DNSConfig> DNSServerList;

	public List<LevCheckbox> levCheckbox4WindowsList;

	public List<LevCheckbox> levCheckbox4Software;

	private RegistryKey LEVStartupKey;

	private RegistryKey LEVRegKey;

	private string currentUsername;

	private const uint SPI_SETDESKWALLPAPER = 20u;

	private const uint SPIF_UPDATEINIFILE = 1u;

	private IContainer components = null;

	private Button bntConfigNetwork;

	private ComboBox cmbDNS;

	private Label label5;

	private Label label3;

	private Label label2;

	private Label label1;

	private TextBox txtCustomDNS;

	private TextBox txtGateway;

	private TextBox txtNetmask;

	private TextBox txtIP;

	private Button btnChangePassword;

	private TextBox txtNewPassword;

	private Label label4;

	private Button btnConfigWindows;

	private CheckBox chkPerformanceRDP;

	private CheckBox chkDisableDriverSig;

	private CheckBox chkDisableRecovery;

	private CheckBox chkDisableHiberfil;

	private CheckBox chkDisableSleep;

	private CheckBox chkSmallTaskbar;

	private CheckBox chkShowTrayIcon;

	private CheckBox chkDisableUpdate;

	private CheckBox chkTurnoffESC;

	private CheckBox chkDisableUAC;

	private Button btnInstall;

	private CheckBox chkNotepad;

	private CheckBox chkUnikey;

	private CheckBox chk7zip;

	private CheckBox chkOpera;

	private CheckBox chkCoccoc;

	private CheckBox chkFirefox;

	private CheckBox chkChrome;

	private TextBox txtFirefoxVer;

	private Label label6;

	private CheckBox chkDeleteTempFolder;

	private CheckBox chkNET48;

	private CheckBox chkCcleaner;

	private RadioButton rdDHCP;

	private RadioButton rdStatic;

	private CheckBox chkCheckAll;

	private CheckBox chkStartUp;

	private CheckBox chkAutoLogin;

	private CheckBox chkForceChangePass;

	private ToolTip ttAutologin;

	private CheckBox chkBrave;

	private CheckBox chkTor;

	private CheckBox chkWinRAR;

	private Button btnChangeAdminAcc;

	private Button btnChangeRDPPort;

	private TextBox txtAdminAcc;

	private TextBox txtRDPPort;

	private Label label13;

	private Label label14;

	private Label label100;

	private Label lblCurrentRDPPort;

	private CheckBox ckhMicroE;

	private Label label7;

	private Label label8;

	private Label lbIPV4;

	private Label lbGateW;

	private Button button1;

	private PictureBox pictureBox1;

	private PictureBox pictureBox2;

	public form_LowEndVietFastVPSConfig(string[] args)
	{
		InitializeComponent();
		MaximumSize = base.Size;
		MinimumSize = base.Size;
		Text = Text + " Version " + VERSION;
		ttAutologin.SetToolTip(label8, "If you check this box, your VPS will be automatically login when it is started.\r\nIt allows you to reset your password over Web console in case you forget the password.");
		initCheckbox();
		initRegistry();
		initLEVDir();
		foreach (DNSConfig dNSServer in DNSServerList)
		{
			cmbDNS.Items.Add(dNSServer);
		}
		cmbDNS.DropDownWidth = 200;
		if (File.Exists(NETWORK_CONFIG_PATH))
		{
			loadNetworkConfigFile(NETWORK_CONFIG_PATH);
		}
		else
		{
			foreach (DriveInfo item in from d in DriveInfo.GetDrives()
				where d.DriveType == DriveType.CDRom
				select d)
			{
				if (item.IsReady)
				{
					string text = item.RootDirectory.ToString() + "config.txt";
					if (File.Exists(text))
					{
						loadNetworkConfigFile(text);
					}
				}
			}
		}
		string name = WindowsIdentity.GetCurrent().Name;
		if (name.Contains("\\"))
		{
			currentUsername = name.Split('\\')[name.Split('\\').Length - 1];
		}
		else
		{
			currentUsername = name;
		}
		txtAdminAcc.Text = currentUsername;
		lblCurrentRDPPort.Text = Registry.LocalMachine.OpenSubKey(REG_RDP_PORT).GetValue("PortNumber").ToString();
	}

	public static string GetLocalIPAddress()
	{
		IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
		IPAddress[] addressList = hostEntry.AddressList;
		foreach (IPAddress iPAddress in addressList)
		{
			if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				return iPAddress.ToString();
			}
		}
		throw new Exception("No network adapters with an IPv4 address in the system!");
	}

	public static string GetLocalGatewayAddress()
	{
		IEnumerable<string> source = from nics in NetworkInterface.GetAllNetworkInterfaces()
			from props in nics.GetIPProperties().GatewayAddresses
			where nics.OperationalStatus == OperationalStatus.Up
			select props.Address.ToString();
		return source.Last();
	}

	private void form_LowEndVietFastVPSConfig_Load(object sender, EventArgs e)
	{
		pictureBox2.Image.Save(Path.GetTempPath() + "Background.png", ImageFormat.Png);
		lbIPV4.Text = getLocalIPAddress();
		lbGateW.Text = GetLocalGatewayAddress();
		if (LEVRegKey.GetValue("ForceChangePassword").ToString() == "1")
		{
			executeCommand("taskkill /IM explorer.exe /F", sync: true);
			string text = "";
			ForcePasswordChange forcePasswordChange = new ForcePasswordChange();
			DialogResult dialogResult = forcePasswordChange.ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				executeCommand("explorer.exe");
				text = forcePasswordChange.newPassword;
				changePassword("Administrator", text);
				setupAutoLogin(text);
				LEVRegKey.SetValue("ForceChangePassword", 0);
				chkForceChangePass.Checked = false;
			}
		}
	}

	private void rdDHCP_CheckedChanged(object sender, EventArgs e)
	{
		if (rdStatic.Checked)
		{
			txtIP.Enabled = true;
			txtNetmask.Enabled = true;
			txtGateway.Enabled = true;
		}
		if (rdDHCP.Checked)
		{
			txtIP.Enabled = false;
			txtNetmask.Enabled = false;
			txtGateway.Enabled = false;
		}
	}

	private void bntConfigNetwork_Click(object sender, EventArgs e)
	{
		if (rdStatic.Checked)
		{
			setStaticIP(txtIP.Text, txtNetmask.Text, txtGateway.Text, txtCustomDNS.Text);
			string contents = txtIP.Text + Environment.NewLine + txtNetmask.Text + Environment.NewLine + txtGateway.Text + Environment.NewLine + txtCustomDNS.Text + Environment.NewLine;
			try
			{
				File.WriteAllText(NETWORK_CONFIG_PATH, contents);
			}
			catch
			{
			}
		}
		if (rdDHCP.Checked)
		{
			setDHCP(txtCustomDNS.Text);
		}
		MessageBox.Show("Successfully set your network configuration!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void cmbDNS_SelectedIndexChanged(object sender, EventArgs e)
	{
		txtCustomDNS.Text = ((DNSConfig)cmbDNS.SelectedItem).DNS1;
		if (((DNSConfig)cmbDNS.SelectedItem).DNS1 == "")
		{
			txtCustomDNS.Enabled = true;
		}
		else
		{
			txtCustomDNS.Enabled = false;
		}
	}

	private void btnChangePassword_Click(object sender, EventArgs e)
	{
		string text = txtAdminAcc.Text;
		DialogResult dialogResult = MessageBox.Show("Bạn đang thay đổi mật khẩu của tên người dùng \"" + text + "\"\r\nNếu \"" + text + "\" không phải là tài khoản Quản trị viên của bạn, vui lòng nhập tài khoản Quản trị viên của bạn vào hộp văn bản \"Thay đổi User VPS.\" ở trên.\r\nLần tiếp theo, hãy yêu cầu đăng nhập bằng thông tin đăng nhập mới: \r\n\r\n" + text + "\r\n" + txtNewPassword.Text, "Thay đổi mật khẩu cho " + text + " ?", MessageBoxButtons.OKCancel);
		changePassword(text, txtNewPassword.Text);
		if (chkAutoLogin.Checked)
		{
			setupAutoLogin(txtNewPassword.Text);
		}
	}

	private void btnChangeAdminAcc_Click(object sender, EventArgs e)
	{
		string text = txtAdminAcc.Text;
		DialogResult dialogResult = MessageBox.Show("Bạn đang đổi tên tài khoản của mình thành " + text + ". Bạn có thể cần phải khởi động lại để áp dụng thay đổi. Vui lòng đăng nhập lại bằng tên người dùng mới:\r\n\r\n" + text, "Đổi tên tài khoản thành " + text + " ? ", MessageBoxButtons.OKCancel);
		if (dialogResult == DialogResult.OK)
		{
			executeCommand("wmic useraccount where name='" + currentUsername + "' call rename name='" + txtAdminAcc.Text + "'");
			DialogResult dialogResult2 = MessageBox.Show("Đã đổi tên thành công tài khoản của bạn thành " + text + ". Bạn có muốn RESTART ngay bây giờ không?", "Thành công!", MessageBoxButtons.YesNo);
			if (dialogResult2 == DialogResult.Yes)
			{
				executeCommand("shutdown /r /t 5");
			}
		}
	}

	private void btnChangeRDPPort_Click(object sender, EventArgs e)
	{
		string text = txtRDPPort.Text;
		DialogResult dialogResult = MessageBox.Show("Bạn đang thay đổi RDP port đến " + text + ". Sau khi nhấn OK, Bạn sẽ bị Ngắt Kết Nối!!!\r\nVui lòng kết nối đến địa chỉ sau:\r\n\r\n" + txtIP.Text + ":" + text, "Thay đổi cổng từ xa thành " + text + " ?", MessageBoxButtons.OKCancel);
		if (dialogResult == DialogResult.OK)
		{
			string text2 = "0x" + int.Parse(text).ToString("X");
			executeCommand("reg add \"HKEY_LOCAL_MACHINE\\" + REG_RDP_PORT + "\" /v PortNumber /t REG_DWORD /d " + text2 + " /f");
			executeCommand("netsh advfirewall firewall add rule name = \"Secure RDP on port " + text + "\" dir =in action = allow protocol = TCP localport = " + text);
			executeCommand("net stop \"TermService\" /y && net start \"TermService\"");
			MessageBox.Show("Thay đổi thành công cổng RDP!", "Thành công!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			lblCurrentRDPPort.Text = text;
		}
	}

	private void btnExtendDisk_Click(object sender, EventArgs e)
	{
		extendDisk();
		MessageBox.Show("Successfully extend your disk to maximum capacity!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void btnCheckAll_CheckedChanged(object sender, EventArgs e)
	{
		foreach (LevCheckbox levCheckbox4Windows in levCheckbox4WindowsList)
		{
			levCheckbox4Windows.checkBox.Checked = chkCheckAll.Checked;
		}
	}

	private void btnConfigWindows_Click(object sender, EventArgs e)
	{
		StatusForm statusForm = new StatusForm(levCheckbox4WindowsList);
		statusForm.Show();
		Thread thread = new Thread((ThreadStart)delegate
		{
			foreach (LevCheckbox levCheckbox4Windows in levCheckbox4WindowsList)
			{
				if (levCheckbox4Windows.checkBox.Checked)
				{
					levCheckbox4Windows.updateResultStatus(executeCommand(levCheckbox4Windows.command, sync: true));
					statusForm.updateProgress();
				}
			}
			executeCommand("taskkill /IM explorer.exe /F & explorer.exe", sync: true);
		});
		thread.Start();
	}

	private void btnInstall_Click(object sender, EventArgs e)
	{
		if (txtFirefoxVer.Text == "Latest")
		{
			levCheckbox4Software.Add(new LevCheckbox(chkFirefox, "https://download.mozilla.org/?product=firefox-latest&os=win&lang=en-US", "FirefoxLatest.exe", "FirefoxLatest.exe /S"));
		}
		else
		{
			levCheckbox4Software.Add(new LevCheckbox(chkFirefox, "https://ftp.mozilla.org/pub/firefox/releases/" + txtFirefoxVer.Text + ".0/win32/en-US/Firefox%20Setup%20" + txtFirefoxVer.Text + ".0.exe", "FirefoxSetup.exe", "FirefoxSetup.exe /S"));
		}
		StatusForm statusForm = new StatusForm(levCheckbox4Software);
		statusForm.Show();
		WebClient wc = new WebClient();
		Task task = new Task(delegate
		{
			foreach (LevCheckbox item in levCheckbox4Software)
			{
				if (item.checkBox.Checked)
				{
					ServicePointManager.Expect100Continue = true;
					ServicePointManager.DefaultConnectionLimit = 9999;
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
					ServicePointManager.ServerCertificateValidationCallback = (object _003Cp0_003E, X509Certificate _003Cp1_003E, X509Chain _003Cp2_003E, SslPolicyErrors _003Cp3_003E) => true;
					item.updateDownloadingStatus();
					statusForm.updateProgress();
					try
					{
						wc.DownloadFile(item.softwareURL, Path.GetTempPath() + item.setupFileName);
						if (ckhMicroE.Checked)
						{
							try
							{
								ZipFile.ExtractToDirectory(Path.GetTempPath() + "MicroEmulator.zip", "MicroEmulator");
							}
							catch
							{
							}
						}
					}
					catch (Exception ex)
					{
						item.remark = ex.Message;
					}
					finally
					{
						item.updateInstallingStatus();
					}
					statusForm.updateProgress();
					item.updateResultStatus((!(item.checkBox.Name == "ckhMicroE")) ? executeCommand(Path.GetTempPath() + item.command, sync: true) : 0);
					statusForm.updateProgress();
				}
			}
			wc.Dispose();
		});
		task.Start();
	}

	private void chkStartUp_CheckedChanged(object sender, EventArgs e)
	{
		if (chkStartUp.Checked)
		{
			LEVStartupKey.SetValue(APPNAME, Application.ExecutablePath);
		}
		else
		{
			LEVStartupKey.DeleteValue(APPNAME, throwOnMissingValue: false);
		}
	}

	private void chkUpdate_CheckedChanged(object sender, EventArgs e)
	{
	}

	private void chkForceChangePass_CheckedChanged(object sender, EventArgs e)
	{
		if (chkForceChangePass.Checked)
		{
			LEVRegKey.SetValue("ForceChangePassword", "1");
		}
		else
		{
			LEVRegKey.SetValue("ForceChangePassword", "0");
		}
	}

	private void lnkGit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		Process.Start(GITHOME);
	}

	private void form_LowEndVietFastVPSConfig_FormClosed(object sender, FormClosedEventArgs e)
	{
		LEVStartupKey.Close();
		LEVRegKey.Close();
	}

	private void initCheckbox()
	{
		DNSServerList = new List<DNSConfig>(new DNSConfig[4]
		{
			new DNSConfig("Google DNS", "8.8.8.8"),
			new DNSConfig("Cloudflare DNS", "1.1.1.1"),
			new DNSConfig("Cisco OpenDNS", "208.67.222.222"),
			new DNSConfig("Custom DNS", "")
		});
		levCheckbox4WindowsList = new List<LevCheckbox>(new LevCheckbox[11]
		{
			new LevCheckbox(chkDisableUAC, "reg ADD HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System /v EnableLUA /t REG_DWORD /d 0 /f"),
			new LevCheckbox(chkDisableHiberfil, "powercfg.exe /hibernate off"),
			new LevCheckbox(chkTurnoffESC, "REG ADD \"HKLM\\SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}\" /v IsInstalled /t REG_DWORD /d 00000000 /f&& REG ADD \"HKLM\\SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}\" /v IsInstalled /t REG_DWORD /d 00000000 /f"),
			new LevCheckbox(chkDisableRecovery, "bcdedit /set {default} bootstatuspolicy ignoreallfailures && bcdedit /set {default} recoveryenabled No"),
			new LevCheckbox(chkDisableUpdate, "reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\" /v AUOptions /t REG_DWORD /d 1 /f"),
			new LevCheckbox(chkDisableDriverSig, "bcdedit -set loadoptions DISABLE_INTEGRITY_CHECKS"),
			new LevCheckbox(chkShowTrayIcon, "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\" /v EnableAutoTray /t REG_DWORD /d 0 /f"),
			new LevCheckbox(chkPerformanceRDP, "reg add \"HKEY_USERS\\.DEFAULT\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects\" /v VisualFXSetting /t REG_DWORD /d 2 /f"),
			new LevCheckbox(chkSmallTaskbar, "reg add \"HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\" /v TaskbarSmallIcons /t REG_DWORD /d 1 /f"),
			new LevCheckbox(chkDeleteTempFolder, "DEL /F /S /Q %TEMP%"),
			new LevCheckbox(chkDisableSleep, "powercfg /x -standby-timeout-ac 0")
		});
		if (Environment.Is64BitOperatingSystem)
		{
			levCheckbox4Software = new List<LevCheckbox>(new LevCheckbox[12]
			{
				new LevCheckbox(chkChrome, "https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7B162F372C-537B-5D4B-4170-3A63D3FA265F%7D%26lang%3Den%26browser%3D4%26usagestats%3D0%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-stable-statsdef_1%26installdataindex%3Ddefaultbrowser/chrome/install/ChromeStandaloneSetup64.exe", "ChromeSetup64.exe", "ChromeSetup64.exe /silent /install"),
				new LevCheckbox(ckhMicroE, "https://drive.google.com/u/0/uc?id=1X16guEOq4Ov6_oqNIMYjo7diU3sFcX7C&export=download&confirm=t", "MicroEmulator.zip", ""),
				new LevCheckbox(chkCoccoc, "http://files.coccoc.com/browser/coccoc_standalone_vi.exe", "CocCocSetup.exe", "CocCocSetup.exe /silent /install"),
				new LevCheckbox(chkUnikey, "http://file.lowendviet.com/Software/UniKey42RC.exe", "UniKey42RC.exe", " & copy " + Path.GetTempPath() + "UniKey42RC.exe " + Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
				new LevCheckbox(chkNotepad, "https://notepad-plus-plus.org/repository/7.x/7.5.6/npp.7.5.6.Installer.exe", "npp.exe", "npp.exe /S"),
				new LevCheckbox(chkOpera, "https://ftp.opera.com/pub/opera/desktop/69.0.3686.77/win/Opera_69.0.3686.77_Setup_x64.exe", "OperaSetup.exe", "OperaSetup.exe /silent /install"),
				new LevCheckbox(chkCcleaner, "https://download.ccleaner.com/ccsetup542.exe", "CCleaner.exe", "CCleaner.exe /S"),
				new LevCheckbox(chk7zip, "https://www.7-zip.org/a/7z1900-x64.exe", "7zSetup.exe", "7zSetup.exe /S"),
				new LevCheckbox(chkNET48, "https://download.visualstudio.microsoft.com/download/pr/014120d7-d689-4305-befd-3cb711108212/0fd66638cde16859462a6243a4629a50/ndp48-x86-x64-allos-enu.exe", "net48.exe", "net48.exe /q /norestart"),
				new LevCheckbox(chkBrave, "https://laptop-updates.brave.com/latest/winx64", "Brave.exe", "Brave.exe /silent /install"),
				new LevCheckbox(chkTor, "https://www.torproject.org/dist/torbrowser/9.5.1/torbrowser-install-win64-9.5.1_en-US.exe", "Tor.exe", "Tor.exe /S"),
				new LevCheckbox(chkWinRAR, "https://www.rarlab.com/rar/winrar-x64-58b2.exe", "WinRAR.exe", "WinRAR.exe /s")
			});
		}
		else
		{
			levCheckbox4Software = new List<LevCheckbox>(new LevCheckbox[12]
			{
				new LevCheckbox(chkChrome, "https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7B6895D2F5-C00B-C0C3-5A9F-9F5A2D9AE003%7D%26lang%3Den%26browser%3D4%26usagestats%3D0%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26installdataindex%3Ddefaultbrowser/update2/installers/ChromeSetup.exe", "ChromeSetup.exe", "ChromeSetup.exe /silent /install"),
				new LevCheckbox(ckhMicroE, "https://drive.google.com/u/0/uc?id=1X16guEOq4Ov6_oqNIMYjo7diU3sFcX7C&export=download&confirm=t", "MicroEmulator.zip", ""),
				new LevCheckbox(chkCoccoc, "http://files.coccoc.com/browser/coccoc_standalone_vi.exe", "CocCocSetup.exe", "CocCocSetup.exe /silent /install"),
				new LevCheckbox(chkUnikey, "http://file.lowendviet.com/Software/UniKey42RC.exe", "UniKey42RC.exe", " & copy " + Path.GetTempPath() + "UniKey42RC.exe " + Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
				new LevCheckbox(chkNotepad, "https://notepad-plus-plus.org/repository/7.x/7.5.6/npp.7.5.6.Installer.exe", "npp.exe", "npp.exe /S"),
				new LevCheckbox(chkOpera, "https://ftp.opera.com/pub/opera/desktop/69.0.3686.77/win/Opera_69.0.3686.77_Setup.exe", "OperaSetup.exe", "OperaSetup.exe /silent /install"),
				new LevCheckbox(chkCcleaner, "https://download.ccleaner.com/ccsetup542.exe", "CCleaner.exe", "CCleaner.exe /S"),
				new LevCheckbox(chk7zip, "https://www.7-zip.org/a/7z1900.exe", "7zSetup.exe", "7zSetup.exe /S"),
				new LevCheckbox(chkNET48, "https://download.visualstudio.microsoft.com/download/pr/014120d7-d689-4305-befd-3cb711108212/0fd66638cde16859462a6243a4629a50/ndp48-x86-x64-allos-enu.exe", "net48.exe", "net48.exe /q /norestart"),
				new LevCheckbox(chkBrave, "https://laptop-updates.brave.com/latest/winx64", "Brave.exe", "Brave.exe /silent /install"),
				new LevCheckbox(chkTor, "https://www.torproject.org/dist/torbrowser/9.5.1/torbrowser-install-9.5.1_en-US.exee", "Tor.exe", "Tor.exe /S"),
				new LevCheckbox(chkWinRAR, "https://www.rarlab.com/rar/wrar58b2.exe", "WinRAR.exe", "WinRAR.exe /s")
			});
		}
	}

	private void initRegistry()
	{
		LEVStartupKey = Registry.LocalMachine.OpenSubKey(REG_STARTUP, writable: true);
		LEVRegKey = Registry.CurrentUser.OpenSubKey(REG_LEV, writable: true);
		if (LEVRegKey == null || LEVRegKey.GetValue("ForceChangePassword") == null || LEVRegKey.GetValue("AutoUpdate") == null || LEVRegKey.GetValue("Version") == null)
		{
			LEVRegKey = Registry.CurrentUser.CreateSubKey(REG_LEV);
			LEVRegKey.SetValue("ForceChangePassword", "0");
			LEVRegKey.SetValue("AutoUpdate", "1");
			LEVStartupKey.SetValue(APPNAME, Application.ExecutablePath);
		}
		LEVRegKey.SetValue("Version", VERSION);
		if (LEVRegKey.GetValue("ForceChangePassword").ToString() == "1")
		{
			chkForceChangePass.Checked = true;
		}
		if (LEVStartupKey.GetValue(APPNAME) != null)
		{
			chkStartUp.Checked = true;
		}
	}

	private void initLEVDir()
	{
		try
		{
			if (!Directory.Exists(LEV_DIR))
			{
				Directory.CreateDirectory(LEV_DIR);
			}
		}
		catch (Exception)
		{
		}
	}

	private void loadNetworkConfigFile(string filePath)
	{
		try
		{
			string text = File.ReadAllText(filePath);
			text = text.TrimEnd('\r', '\n');
			RegexOptions options = RegexOptions.None;
			Regex regex = new Regex("[\r, \n]{1,}", options);
			text = regex.Replace(text, "|");
			string[] array = text.Split('|');
			txtIP.Text = array[0];
			txtNetmask.Text = array[1];
			txtGateway.Text = array[2];
			if (array.Length > 3)
			{
				cmbDNS.SelectedIndex = 3;
				txtCustomDNS.Text = array[3];
			}
			else
			{
				cmbDNS.SelectedIndex = 0;
			}
		}
		catch
		{
		}
	}

	private void setupAutoLogin(string autoLoginPassword)
	{
		executeCommand("REG ADD \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\" /v AutoAdminLogon /t REG_SZ /d 1 /f");
		executeCommand("REG ADD \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\" /v DefaultUserName /t REG_SZ /d Administrator /f");
		executeCommand("REG ADD \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\" /v DefaultPassword /t REG_SZ /d " + autoLoginPassword + " /f");
	}

	private void changePassword(string account, string newPassword)
	{
		if (newPassword.Length < 8 || !newPassword.Any(char.IsUpper) || !newPassword.Any(char.IsLower) || !newPassword.Any(char.IsDigit))
		{
			MessageBox.Show("New password must have at least 8 character, with both UPPERCASE, lowercase and number!", "Error!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand);
			return;
		}
		executeCommand("net user " + account + " \"" + newPassword + "\"", sync: true);
		MessageBox.Show("Successfully change Windows password!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private static void setStaticIP(string ip, string netmask, string gateway, string dns)
	{
		ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
		ManagementObjectCollection instances = managementClass.GetInstances();
		foreach (ManagementObject item in instances)
		{
			if ((bool)item["IPEnabled"])
			{
				try
				{
					ManagementBaseObject methodParameters = item.GetMethodParameters("EnableStatic");
					methodParameters["IPAddress"] = new string[1] { ip };
					methodParameters["SubnetMask"] = new string[1] { netmask };
					ManagementBaseObject managementBaseObject = item.InvokeMethod("EnableStatic", methodParameters, null);
					ManagementBaseObject methodParameters2 = item.GetMethodParameters("SetGateways");
					methodParameters2["DefaultIPGateway"] = new string[1] { gateway };
					methodParameters2["GatewayCostMetric"] = new int[1] { 1 };
					ManagementBaseObject managementBaseObject2 = item.InvokeMethod("SetGateways", methodParameters2, null);
					ManagementBaseObject methodParameters3 = item.GetMethodParameters("SetDNSServerSearchOrder");
					methodParameters3["DNSServerSearchOrder"] = dns.Split(',');
					ManagementBaseObject managementBaseObject3 = item.InvokeMethod("SetDNSServerSearchOrder", methodParameters3, null);
				}
				catch (Exception)
				{
					throw;
				}
			}
		}
	}

	private static void setDHCP(string dns)
	{
		ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
		ManagementObjectCollection instances = managementClass.GetInstances();
		foreach (ManagementObject item in instances)
		{
			if ((bool)item["IPEnabled"])
			{
				try
				{
					ManagementBaseObject managementBaseObject = item.InvokeMethod("EnableDHCP", null, null);
					ManagementBaseObject methodParameters = item.GetMethodParameters("SetDNSServerSearchOrder");
					methodParameters["DNSServerSearchOrder"] = dns.Split(',');
					ManagementBaseObject managementBaseObject2 = item.InvokeMethod("SetDNSServerSearchOrder", methodParameters, null);
				}
				catch (Exception)
				{
					throw;
				}
			}
		}
	}

	public static bool CheckForInternetConnection()
	{
		try
		{
			using WebClient webClient = new WebClient();
			using (webClient.OpenRead("https://google.com"))
			{
				return true;
			}
		}
		catch
		{
			return false;
		}
	}

	private static string getLocalIPAddress()
	{
		IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
		IPAddress[] addressList = hostEntry.AddressList;
		foreach (IPAddress iPAddress in addressList)
		{
			if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				return iPAddress.ToString();
			}
		}
		return "";
	}

	private void extendDisk()
	{
		string contents = "SELECT DISK 0" + Environment.NewLine + "RESCAN" + Environment.NewLine + "SELECT PARTITION 2" + Environment.NewLine + "EXTEND" + Environment.NewLine + "EXIT";
		File.WriteAllText(DISKPART_CONFIG_PATH, contents);
		executeCommand("diskpart.exe /s " + DISKPART_CONFIG_PATH, sync: true);
		File.Delete(DISKPART_CONFIG_PATH);
	}

	private static int executeCommand(string commnd, bool sync = false, int timeout = 200000)
	{
		ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", "/C" + commnd)
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			WorkingDirectory = "C:\\"
		};
		Process process = Process.Start(startInfo);
		if (sync)
		{
			process.WaitForExit(timeout);
			int exitCode = process.ExitCode;
			process.Close();
			return exitCode;
		}
		return 0;
	}

	private void txtGateway_TextChanged(object sender, EventArgs e)
	{
	}

	private void form_LowEndVietFastVPSConfig_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Location.X > 0 && e.Location.Y > 0 && e.Location.X < 300 && e.Location.Y < 100)
		{
			Process.Start("https://viecloud.asia");
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

	public static void SetDesktopWallpaper(string path)
	{
		SystemParametersInfo(20u, 0u, path, 1u);
	}

	private void button1_Click(object sender, EventArgs e)
	{
		SetDesktopWallpaper(Path.GetTempPath() + "Background.png");
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
            this.components = new System.ComponentModel.Container();
            this.rdDHCP = new System.Windows.Forms.RadioButton();
            this.rdStatic = new System.Windows.Forms.RadioButton();
            this.bntConfigNetwork = new System.Windows.Forms.Button();
            this.cmbDNS = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCustomDNS = new System.Windows.Forms.TextBox();
            this.txtGateway = new System.Windows.Forms.TextBox();
            this.txtNetmask = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.btnChangeAdminAcc = new System.Windows.Forms.Button();
            this.btnChangeRDPPort = new System.Windows.Forms.Button();
            this.txtAdminAcc = new System.Windows.Forms.TextBox();
            this.btnChangePassword = new System.Windows.Forms.Button();
            this.txtRDPPort = new System.Windows.Forms.TextBox();
            this.chkAutoLogin = new System.Windows.Forms.CheckBox();
            this.lblCurrentRDPPort = new System.Windows.Forms.Label();
            this.label100 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.txtNewPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnConfigWindows = new System.Windows.Forms.Button();
            this.chkCheckAll = new System.Windows.Forms.CheckBox();
            this.chkDeleteTempFolder = new System.Windows.Forms.CheckBox();
            this.chkPerformanceRDP = new System.Windows.Forms.CheckBox();
            this.chkDisableDriverSig = new System.Windows.Forms.CheckBox();
            this.chkDisableRecovery = new System.Windows.Forms.CheckBox();
            this.chkDisableHiberfil = new System.Windows.Forms.CheckBox();
            this.chkDisableSleep = new System.Windows.Forms.CheckBox();
            this.chkSmallTaskbar = new System.Windows.Forms.CheckBox();
            this.chkShowTrayIcon = new System.Windows.Forms.CheckBox();
            this.chkDisableUpdate = new System.Windows.Forms.CheckBox();
            this.chkTurnoffESC = new System.Windows.Forms.CheckBox();
            this.chkDisableUAC = new System.Windows.Forms.CheckBox();
            this.btnInstall = new System.Windows.Forms.Button();
            this.txtFirefoxVer = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkTor = new System.Windows.Forms.CheckBox();
            this.chkBrave = new System.Windows.Forms.CheckBox();
            this.chkFirefox = new System.Windows.Forms.CheckBox();
            this.chkOpera = new System.Windows.Forms.CheckBox();
            this.chkChrome = new System.Windows.Forms.CheckBox();
            this.chkCoccoc = new System.Windows.Forms.CheckBox();
            this.chkUnikey = new System.Windows.Forms.CheckBox();
            this.chkNET48 = new System.Windows.Forms.CheckBox();
            this.chkNotepad = new System.Windows.Forms.CheckBox();
            this.chkCcleaner = new System.Windows.Forms.CheckBox();
            this.chkWinRAR = new System.Windows.Forms.CheckBox();
            this.chk7zip = new System.Windows.Forms.CheckBox();
            this.chkForceChangePass = new System.Windows.Forms.CheckBox();
            this.chkStartUp = new System.Windows.Forms.CheckBox();
            this.ttAutologin = new System.Windows.Forms.ToolTip(this.components);
            this.ckhMicroE = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lbIPV4 = new System.Windows.Forms.Label();
            this.lbGateW = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // rdDHCP
            // 
            this.rdDHCP.AutoSize = true;
            this.rdDHCP.BackColor = System.Drawing.Color.Transparent;
            this.rdDHCP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdDHCP.ForeColor = System.Drawing.Color.Lime;
            this.rdDHCP.Location = new System.Drawing.Point(1104, 213);
            this.rdDHCP.Margin = new System.Windows.Forms.Padding(4);
            this.rdDHCP.Name = "rdDHCP";
            this.rdDHCP.Size = new System.Drawing.Size(71, 21);
            this.rdDHCP.TabIndex = 1;
            this.rdDHCP.Text = "DHCP";
            this.rdDHCP.UseVisualStyleBackColor = false;
            this.rdDHCP.CheckedChanged += new System.EventHandler(this.rdDHCP_CheckedChanged);
            // 
            // rdStatic
            // 
            this.rdStatic.AutoSize = true;
            this.rdStatic.BackColor = System.Drawing.Color.Transparent;
            this.rdStatic.Checked = true;
            this.rdStatic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdStatic.ForeColor = System.Drawing.Color.Lime;
            this.rdStatic.Location = new System.Drawing.Point(1104, 162);
            this.rdStatic.Margin = new System.Windows.Forms.Padding(4);
            this.rdStatic.Name = "rdStatic";
            this.rdStatic.Size = new System.Drawing.Size(70, 21);
            this.rdStatic.TabIndex = 0;
            this.rdStatic.TabStop = true;
            this.rdStatic.Text = "Static";
            this.rdStatic.UseVisualStyleBackColor = false;
            // 
            // bntConfigNetwork
            // 
            this.bntConfigNetwork.BackColor = System.Drawing.Color.Red;
            this.bntConfigNetwork.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bntConfigNetwork.ForeColor = System.Drawing.Color.Lavender;
            this.bntConfigNetwork.Location = new System.Drawing.Point(919, 308);
            this.bntConfigNetwork.Margin = new System.Windows.Forms.Padding(4);
            this.bntConfigNetwork.Name = "bntConfigNetwork";
            this.bntConfigNetwork.Size = new System.Drawing.Size(192, 46);
            this.bntConfigNetwork.TabIndex = 7;
            this.bntConfigNetwork.Text = "Cấu hình mạng";
            this.bntConfigNetwork.UseVisualStyleBackColor = false;
            this.bntConfigNetwork.Click += new System.EventHandler(this.bntConfigNetwork_Click);
            // 
            // cmbDNS
            // 
            this.cmbDNS.FormattingEnabled = true;
            this.cmbDNS.Location = new System.Drawing.Point(920, 276);
            this.cmbDNS.Margin = new System.Windows.Forms.Padding(4);
            this.cmbDNS.Name = "cmbDNS";
            this.cmbDNS.Size = new System.Drawing.Size(164, 24);
            this.cmbDNS.TabIndex = 5;
            this.cmbDNS.SelectedIndexChanged += new System.EventHandler(this.cmbDNS_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Lime;
            this.label5.Location = new System.Drawing.Point(832, 281);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 17);
            this.label5.TabIndex = 2;
            this.label5.Text = "DNS";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Lime;
            this.label3.Location = new System.Drawing.Point(832, 240);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Gateway";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Lime;
            this.label2.Location = new System.Drawing.Point(832, 201);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Netmask";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Lime;
            this.label1.Location = new System.Drawing.Point(835, 159);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(22, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP";
            // 
            // txtCustomDNS
            // 
            this.txtCustomDNS.Enabled = false;
            this.txtCustomDNS.Location = new System.Drawing.Point(1111, 276);
            this.txtCustomDNS.Margin = new System.Windows.Forms.Padding(4);
            this.txtCustomDNS.Name = "txtCustomDNS";
            this.txtCustomDNS.Size = new System.Drawing.Size(133, 22);
            this.txtCustomDNS.TabIndex = 6;
            // 
            // txtGateway
            // 
            this.txtGateway.Location = new System.Drawing.Point(920, 235);
            this.txtGateway.Margin = new System.Windows.Forms.Padding(4);
            this.txtGateway.Name = "txtGateway";
            this.txtGateway.Size = new System.Drawing.Size(164, 22);
            this.txtGateway.TabIndex = 4;
            this.txtGateway.TextChanged += new System.EventHandler(this.txtGateway_TextChanged);
            // 
            // txtNetmask
            // 
            this.txtNetmask.Location = new System.Drawing.Point(920, 196);
            this.txtNetmask.Margin = new System.Windows.Forms.Padding(4);
            this.txtNetmask.Name = "txtNetmask";
            this.txtNetmask.Size = new System.Drawing.Size(164, 22);
            this.txtNetmask.TabIndex = 3;
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(920, 154);
            this.txtIP.Margin = new System.Windows.Forms.Padding(4);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(164, 22);
            this.txtIP.TabIndex = 2;
            // 
            // btnChangeAdminAcc
            // 
            this.btnChangeAdminAcc.BackColor = System.Drawing.Color.DodgerBlue;
            this.btnChangeAdminAcc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChangeAdminAcc.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnChangeAdminAcc.Location = new System.Drawing.Point(713, 75);
            this.btnChangeAdminAcc.Margin = new System.Windows.Forms.Padding(4);
            this.btnChangeAdminAcc.Name = "btnChangeAdminAcc";
            this.btnChangeAdminAcc.Size = new System.Drawing.Size(87, 28);
            this.btnChangeAdminAcc.TabIndex = 1;
            this.btnChangeAdminAcc.Text = "OK";
            this.btnChangeAdminAcc.UseVisualStyleBackColor = false;
            this.btnChangeAdminAcc.Click += new System.EventHandler(this.btnChangeAdminAcc_Click);
            // 
            // btnChangeRDPPort
            // 
            this.btnChangeRDPPort.BackColor = System.Drawing.Color.DodgerBlue;
            this.btnChangeRDPPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChangeRDPPort.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnChangeRDPPort.Location = new System.Drawing.Point(713, 165);
            this.btnChangeRDPPort.Margin = new System.Windows.Forms.Padding(4);
            this.btnChangeRDPPort.Name = "btnChangeRDPPort";
            this.btnChangeRDPPort.Size = new System.Drawing.Size(87, 28);
            this.btnChangeRDPPort.TabIndex = 1;
            this.btnChangeRDPPort.Text = "OK";
            this.btnChangeRDPPort.UseVisualStyleBackColor = false;
            this.btnChangeRDPPort.Click += new System.EventHandler(this.btnChangeRDPPort_Click);
            // 
            // txtAdminAcc
            // 
            this.txtAdminAcc.Location = new System.Drawing.Point(593, 76);
            this.txtAdminAcc.Margin = new System.Windows.Forms.Padding(4);
            this.txtAdminAcc.Name = "txtAdminAcc";
            this.txtAdminAcc.Size = new System.Drawing.Size(115, 22);
            this.txtAdminAcc.TabIndex = 0;
            this.txtAdminAcc.Text = "Administrator";
            // 
            // btnChangePassword
            // 
            this.btnChangePassword.BackColor = System.Drawing.Color.DodgerBlue;
            this.btnChangePassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChangePassword.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnChangePassword.Location = new System.Drawing.Point(713, 118);
            this.btnChangePassword.Margin = new System.Windows.Forms.Padding(4);
            this.btnChangePassword.Name = "btnChangePassword";
            this.btnChangePassword.Size = new System.Drawing.Size(87, 28);
            this.btnChangePassword.TabIndex = 1;
            this.btnChangePassword.Text = "OK";
            this.btnChangePassword.UseVisualStyleBackColor = false;
            this.btnChangePassword.Click += new System.EventHandler(this.btnChangePassword_Click);
            // 
            // txtRDPPort
            // 
            this.txtRDPPort.Location = new System.Drawing.Point(593, 167);
            this.txtRDPPort.Margin = new System.Windows.Forms.Padding(4);
            this.txtRDPPort.Name = "txtRDPPort";
            this.txtRDPPort.Size = new System.Drawing.Size(115, 22);
            this.txtRDPPort.TabIndex = 0;
            this.txtRDPPort.Text = "3388";
            // 
            // chkAutoLogin
            // 
            this.chkAutoLogin.AutoSize = true;
            this.chkAutoLogin.BackColor = System.Drawing.Color.Transparent;
            this.chkAutoLogin.Checked = true;
            this.chkAutoLogin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoLogin.ForeColor = System.Drawing.Color.Cyan;
            this.chkAutoLogin.Location = new System.Drawing.Point(421, 238);
            this.chkAutoLogin.Margin = new System.Windows.Forms.Padding(4);
            this.chkAutoLogin.Name = "chkAutoLogin";
            this.chkAutoLogin.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkAutoLogin.Size = new System.Drawing.Size(150, 20);
            this.chkAutoLogin.TabIndex = 15;
            this.chkAutoLogin.Text = "Tự động đăng nhập";
            this.chkAutoLogin.UseVisualStyleBackColor = false;
            this.chkAutoLogin.CheckedChanged += new System.EventHandler(this.chkStartUp_CheckedChanged);
            // 
            // lblCurrentRDPPort
            // 
            this.lblCurrentRDPPort.AutoSize = true;
            this.lblCurrentRDPPort.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentRDPPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentRDPPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblCurrentRDPPort.Location = new System.Drawing.Point(532, 171);
            this.lblCurrentRDPPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentRDPPort.Name = "lblCurrentRDPPort";
            this.lblCurrentRDPPort.Size = new System.Drawing.Size(49, 17);
            this.lblCurrentRDPPort.TabIndex = 2;
            this.lblCurrentRDPPort.Text = "3389 ";
            // 
            // label100
            // 
            this.label100.AutoSize = true;
            this.label100.BackColor = System.Drawing.Color.Transparent;
            this.label100.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label100.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label100.Location = new System.Drawing.Point(491, 171);
            this.label100.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label100.Name = "label100";
            this.label100.Size = new System.Drawing.Size(43, 17);
            this.label100.TabIndex = 2;
            this.label100.Text = "Now:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BackColor = System.Drawing.Color.Transparent;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Red;
            this.label13.Location = new System.Drawing.Point(417, 171);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(75, 17);
            this.label13.TabIndex = 2;
            this.label13.Text = "RDP Port";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.BackColor = System.Drawing.Color.Transparent;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Red;
            this.label14.Location = new System.Drawing.Point(417, 80);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(155, 17);
            this.label14.TabIndex = 2;
            this.label14.Text = "Thay đổi User VPS  ";
            // 
            // txtNewPassword
            // 
            this.txtNewPassword.Location = new System.Drawing.Point(593, 121);
            this.txtNewPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.Size = new System.Drawing.Size(115, 22);
            this.txtNewPassword.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(417, 124);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(142, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "Thay đổi mật khẩu";
            // 
            // btnConfigWindows
            // 
            this.btnConfigWindows.BackColor = System.Drawing.Color.Red;
            this.btnConfigWindows.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfigWindows.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnConfigWindows.Location = new System.Drawing.Point(85, 665);
            this.btnConfigWindows.Margin = new System.Windows.Forms.Padding(0);
            this.btnConfigWindows.Name = "btnConfigWindows";
            this.btnConfigWindows.Size = new System.Drawing.Size(204, 46);
            this.btnConfigWindows.TabIndex = 15;
            this.btnConfigWindows.Text = "Bắt đầu tối ưu VPS";
            this.btnConfigWindows.UseVisualStyleBackColor = false;
            this.btnConfigWindows.Click += new System.EventHandler(this.btnConfigWindows_Click);
            // 
            // chkCheckAll
            // 
            this.chkCheckAll.AutoSize = true;
            this.chkCheckAll.BackColor = System.Drawing.Color.Red;
            this.chkCheckAll.ForeColor = System.Drawing.Color.LavenderBlush;
            this.chkCheckAll.Location = new System.Drawing.Point(104, 639);
            this.chkCheckAll.Margin = new System.Windows.Forms.Padding(4);
            this.chkCheckAll.Name = "chkCheckAll";
            this.chkCheckAll.Size = new System.Drawing.Size(148, 20);
            this.chkCheckAll.TabIndex = 13;
            this.chkCheckAll.Text = "Chọn/Bỏ chọn tất cả";
            this.chkCheckAll.UseVisualStyleBackColor = false;
            this.chkCheckAll.CheckedChanged += new System.EventHandler(this.btnCheckAll_CheckedChanged);
            // 
            // chkDeleteTempFolder
            // 
            this.chkDeleteTempFolder.AutoSize = true;
            this.chkDeleteTempFolder.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkDeleteTempFolder.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDeleteTempFolder.ForeColor = System.Drawing.Color.Red;
            this.chkDeleteTempFolder.Location = new System.Drawing.Point(33, 588);
            this.chkDeleteTempFolder.Margin = new System.Windows.Forms.Padding(4);
            this.chkDeleteTempFolder.Name = "chkDeleteTempFolder";
            this.chkDeleteTempFolder.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkDeleteTempFolder.Size = new System.Drawing.Size(379, 28);
            this.chkDeleteTempFolder.TabIndex = 9;
            this.chkDeleteTempFolder.Text = "Delete %Temp% folder                ";
            this.chkDeleteTempFolder.UseVisualStyleBackColor = false;
            // 
            // chkPerformanceRDP
            // 
            this.chkPerformanceRDP.AutoSize = true;
            this.chkPerformanceRDP.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkPerformanceRDP.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkPerformanceRDP.ForeColor = System.Drawing.Color.Red;
            this.chkPerformanceRDP.Location = new System.Drawing.Point(33, 534);
            this.chkPerformanceRDP.Margin = new System.Windows.Forms.Padding(4);
            this.chkPerformanceRDP.Name = "chkPerformanceRDP";
            this.chkPerformanceRDP.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkPerformanceRDP.Size = new System.Drawing.Size(383, 28);
            this.chkPerformanceRDP.TabIndex = 7;
            this.chkPerformanceRDP.Text = "Better RDP turning                      ";
            this.chkPerformanceRDP.UseVisualStyleBackColor = false;
            // 
            // chkDisableDriverSig
            // 
            this.chkDisableDriverSig.AutoSize = true;
            this.chkDisableDriverSig.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkDisableDriverSig.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDisableDriverSig.ForeColor = System.Drawing.Color.Red;
            this.chkDisableDriverSig.Location = new System.Drawing.Point(33, 421);
            this.chkDisableDriverSig.Margin = new System.Windows.Forms.Padding(4);
            this.chkDisableDriverSig.Name = "chkDisableDriverSig";
            this.chkDisableDriverSig.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkDisableDriverSig.Size = new System.Drawing.Size(379, 28);
            this.chkDisableDriverSig.TabIndex = 5;
            this.chkDisableDriverSig.Text = "Disable driver signature              ";
            this.chkDisableDriverSig.UseVisualStyleBackColor = false;
            // 
            // chkDisableRecovery
            // 
            this.chkDisableRecovery.AutoSize = true;
            this.chkDisableRecovery.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkDisableRecovery.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDisableRecovery.ForeColor = System.Drawing.Color.Red;
            this.chkDisableRecovery.Location = new System.Drawing.Point(32, 361);
            this.chkDisableRecovery.Margin = new System.Windows.Forms.Padding(4);
            this.chkDisableRecovery.Name = "chkDisableRecovery";
            this.chkDisableRecovery.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkDisableRecovery.Size = new System.Drawing.Size(373, 28);
            this.chkDisableRecovery.TabIndex = 3;
            this.chkDisableRecovery.Text = "Disable recovery at logon           ";
            this.chkDisableRecovery.UseVisualStyleBackColor = false;
            // 
            // chkDisableHiberfil
            // 
            this.chkDisableHiberfil.AutoSize = true;
            this.chkDisableHiberfil.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkDisableHiberfil.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDisableHiberfil.ForeColor = System.Drawing.Color.Red;
            this.chkDisableHiberfil.Location = new System.Drawing.Point(32, 246);
            this.chkDisableHiberfil.Margin = new System.Windows.Forms.Padding(4);
            this.chkDisableHiberfil.Name = "chkDisableHiberfil";
            this.chkDisableHiberfil.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkDisableHiberfil.Size = new System.Drawing.Size(371, 28);
            this.chkDisableHiberfil.TabIndex = 1;
            this.chkDisableHiberfil.Text = "Disable hiberfil.sys (save disk)   ";
            this.chkDisableHiberfil.UseVisualStyleBackColor = false;
            // 
            // chkDisableSleep
            // 
            this.chkDisableSleep.AutoSize = true;
            this.chkDisableSleep.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkDisableSleep.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDisableSleep.ForeColor = System.Drawing.Color.Red;
            this.chkDisableSleep.Location = new System.Drawing.Point(211, 302);
            this.chkDisableSleep.Margin = new System.Windows.Forms.Padding(4);
            this.chkDisableSleep.Name = "chkDisableSleep";
            this.chkDisableSleep.Size = new System.Drawing.Size(168, 28);
            this.chkDisableSleep.TabIndex = 12;
            this.chkDisableSleep.Text = "Disable sleep";
            this.chkDisableSleep.UseVisualStyleBackColor = false;
            // 
            // chkSmallTaskbar
            // 
            this.chkSmallTaskbar.AutoSize = true;
            this.chkSmallTaskbar.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkSmallTaskbar.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkSmallTaskbar.ForeColor = System.Drawing.Color.Red;
            this.chkSmallTaskbar.Location = new System.Drawing.Point(32, 476);
            this.chkSmallTaskbar.Margin = new System.Windows.Forms.Padding(4);
            this.chkSmallTaskbar.Name = "chkSmallTaskbar";
            this.chkSmallTaskbar.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkSmallTaskbar.Size = new System.Drawing.Size(401, 28);
            this.chkSmallTaskbar.TabIndex = 8;
            this.chkSmallTaskbar.Text = "Small taskbar                                ";
            this.chkSmallTaskbar.UseVisualStyleBackColor = false;
            // 
            // chkShowTrayIcon
            // 
            this.chkShowTrayIcon.AutoSize = true;
            this.chkShowTrayIcon.BackColor = System.Drawing.Color.Transparent;
            this.chkShowTrayIcon.Font = new System.Drawing.Font("Microsoft YaHei", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkShowTrayIcon.ForeColor = System.Drawing.Color.Red;
            this.chkShowTrayIcon.Location = new System.Drawing.Point(2065, 944);
            this.chkShowTrayIcon.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowTrayIcon.Name = "chkShowTrayIcon";
            this.chkShowTrayIcon.Size = new System.Drawing.Size(204, 30);
            this.chkShowTrayIcon.TabIndex = 6;
            this.chkShowTrayIcon.Text = "Show all tray icon";
            this.chkShowTrayIcon.UseVisualStyleBackColor = false;
            // 
            // chkDisableUpdate
            // 
            this.chkDisableUpdate.AutoSize = true;
            this.chkDisableUpdate.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkDisableUpdate.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDisableUpdate.ForeColor = System.Drawing.Color.Red;
            this.chkDisableUpdate.Location = new System.Drawing.Point(191, 194);
            this.chkDisableUpdate.Margin = new System.Windows.Forms.Padding(4);
            this.chkDisableUpdate.Name = "chkDisableUpdate";
            this.chkDisableUpdate.Size = new System.Drawing.Size(190, 28);
            this.chkDisableUpdate.TabIndex = 4;
            this.chkDisableUpdate.Text = "Disable update ";
            this.chkDisableUpdate.UseVisualStyleBackColor = false;
            // 
            // chkTurnoffESC
            // 
            this.chkTurnoffESC.AutoSize = true;
            this.chkTurnoffESC.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkTurnoffESC.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkTurnoffESC.ForeColor = System.Drawing.Color.Red;
            this.chkTurnoffESC.Location = new System.Drawing.Point(32, 302);
            this.chkTurnoffESC.Margin = new System.Windows.Forms.Padding(4);
            this.chkTurnoffESC.Name = "chkTurnoffESC";
            this.chkTurnoffESC.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkTurnoffESC.Size = new System.Drawing.Size(220, 28);
            this.chkTurnoffESC.TabIndex = 2;
            this.chkTurnoffESC.Text = "Turn off IE ESC    ";
            this.chkTurnoffESC.UseVisualStyleBackColor = false;
            // 
            // chkDisableUAC
            // 
            this.chkDisableUAC.AutoSize = true;
            this.chkDisableUAC.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkDisableUAC.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkDisableUAC.FlatAppearance.BorderSize = 10;
            this.chkDisableUAC.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Red;
            this.chkDisableUAC.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.chkDisableUAC.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDisableUAC.ForeColor = System.Drawing.Color.Red;
            this.chkDisableUAC.Location = new System.Drawing.Point(35, 194);
            this.chkDisableUAC.Margin = new System.Windows.Forms.Padding(4);
            this.chkDisableUAC.Name = "chkDisableUAC";
            this.chkDisableUAC.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkDisableUAC.Size = new System.Drawing.Size(210, 28);
            this.chkDisableUAC.TabIndex = 0;
            this.chkDisableUAC.Text = "Disable UAC       ";
            this.chkDisableUAC.UseVisualStyleBackColor = false;
            // 
            // btnInstall
            // 
            this.btnInstall.BackColor = System.Drawing.Color.Red;
            this.btnInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInstall.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnInstall.Location = new System.Drawing.Point(505, 670);
            this.btnInstall.Margin = new System.Windows.Forms.Padding(4);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(201, 43);
            this.btnInstall.TabIndex = 14;
            this.btnInstall.Text = "Bắt đầu cài đặt";
            this.btnInstall.UseVisualStyleBackColor = false;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // txtFirefoxVer
            // 
            this.txtFirefoxVer.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtFirefoxVer.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFirefoxVer.ForeColor = System.Drawing.Color.ForestGreen;
            this.txtFirefoxVer.Location = new System.Drawing.Point(667, 598);
            this.txtFirefoxVer.Margin = new System.Windows.Forms.Padding(4);
            this.txtFirefoxVer.Multiline = true;
            this.txtFirefoxVer.Name = "txtFirefoxVer";
            this.txtFirefoxVer.Size = new System.Drawing.Size(100, 31);
            this.txtFirefoxVer.TabIndex = 3;
            this.txtFirefoxVer.Text = "Latest";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label6.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.ForestGreen;
            this.label6.Location = new System.Drawing.Point(617, 604);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 24);
            this.label6.TabIndex = 2;
            this.label6.Text = "Ver.";
            // 
            // chkTor
            // 
            this.chkTor.AutoSize = true;
            this.chkTor.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkTor.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkTor.ForeColor = System.Drawing.Color.ForestGreen;
            this.chkTor.Location = new System.Drawing.Point(620, 449);
            this.chkTor.Margin = new System.Windows.Forms.Padding(4);
            this.chkTor.Name = "chkTor";
            this.chkTor.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkTor.Size = new System.Drawing.Size(159, 28);
            this.chkTor.TabIndex = 7;
            this.chkTor.Text = "Tor browser";
            this.chkTor.UseVisualStyleBackColor = false;
            // 
            // chkBrave
            // 
            this.chkBrave.AutoSize = true;
            this.chkBrave.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkBrave.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkBrave.ForeColor = System.Drawing.Color.ForestGreen;
            this.chkBrave.Location = new System.Drawing.Point(413, 526);
            this.chkBrave.Margin = new System.Windows.Forms.Padding(4);
            this.chkBrave.Name = "chkBrave";
            this.chkBrave.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkBrave.Size = new System.Drawing.Size(183, 28);
            this.chkBrave.TabIndex = 7;
            this.chkBrave.Text = "Brave browser";
            this.chkBrave.UseVisualStyleBackColor = false;
            // 
            // chkFirefox
            // 
            this.chkFirefox.AutoSize = true;
            this.chkFirefox.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkFirefox.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkFirefox.ForeColor = System.Drawing.Color.ForestGreen;
            this.chkFirefox.Location = new System.Drawing.Point(620, 368);
            this.chkFirefox.Margin = new System.Windows.Forms.Padding(4);
            this.chkFirefox.Name = "chkFirefox";
            this.chkFirefox.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkFirefox.Size = new System.Drawing.Size(171, 28);
            this.chkFirefox.TabIndex = 2;
            this.chkFirefox.Text = "Firefox         ";
            this.chkFirefox.UseVisualStyleBackColor = false;
            // 
            // chkOpera
            // 
            this.chkOpera.AutoSize = true;
            this.chkOpera.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkOpera.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkOpera.ForeColor = System.Drawing.Color.ForestGreen;
            this.chkOpera.Location = new System.Drawing.Point(619, 526);
            this.chkOpera.Margin = new System.Windows.Forms.Padding(4);
            this.chkOpera.Name = "chkOpera";
            this.chkOpera.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkOpera.Size = new System.Drawing.Size(166, 28);
            this.chkOpera.TabIndex = 7;
            this.chkOpera.Text = "Opera          ";
            this.chkOpera.UseVisualStyleBackColor = false;
            // 
            // chkChrome
            // 
            this.chkChrome.AutoSize = true;
            this.chkChrome.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkChrome.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkChrome.ForeColor = System.Drawing.Color.ForestGreen;
            this.chkChrome.Location = new System.Drawing.Point(413, 368);
            this.chkChrome.Margin = new System.Windows.Forms.Padding(4);
            this.chkChrome.Name = "chkChrome";
            this.chkChrome.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkChrome.Size = new System.Drawing.Size(190, 28);
            this.chkChrome.TabIndex = 0;
            this.chkChrome.Text = "Chrome           ";
            this.chkChrome.UseVisualStyleBackColor = false;
            // 
            // chkCoccoc
            // 
            this.chkCoccoc.AutoSize = true;
            this.chkCoccoc.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkCoccoc.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkCoccoc.ForeColor = System.Drawing.Color.ForestGreen;
            this.chkCoccoc.Location = new System.Drawing.Point(415, 449);
            this.chkCoccoc.Margin = new System.Windows.Forms.Padding(4);
            this.chkCoccoc.Name = "chkCoccoc";
            this.chkCoccoc.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkCoccoc.Size = new System.Drawing.Size(185, 28);
            this.chkCoccoc.TabIndex = 5;
            this.chkCoccoc.Text = "Cốc Cốc          ";
            this.chkCoccoc.UseVisualStyleBackColor = false;
            // 
            // chkUnikey
            // 
            this.chkUnikey.AutoSize = true;
            this.chkUnikey.BackColor = System.Drawing.Color.Transparent;
            this.chkUnikey.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkUnikey.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.chkUnikey.Location = new System.Drawing.Point(415, 402);
            this.chkUnikey.Margin = new System.Windows.Forms.Padding(4);
            this.chkUnikey.Name = "chkUnikey";
            this.chkUnikey.Size = new System.Drawing.Size(79, 21);
            this.chkUnikey.TabIndex = 1;
            this.chkUnikey.Text = "Unikey";
            this.chkUnikey.UseVisualStyleBackColor = false;
            // 
            // chkNET48
            // 
            this.chkNET48.AutoSize = true;
            this.chkNET48.BackColor = System.Drawing.Color.Transparent;
            this.chkNET48.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkNET48.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.chkNET48.Location = new System.Drawing.Point(621, 482);
            this.chkNET48.Margin = new System.Windows.Forms.Padding(4);
            this.chkNET48.Name = "chkNET48";
            this.chkNET48.Size = new System.Drawing.Size(94, 21);
            this.chkNET48.TabIndex = 10;
            this.chkNET48.Text = ".NET 4.8";
            this.chkNET48.UseVisualStyleBackColor = false;
            // 
            // chkNotepad
            // 
            this.chkNotepad.AutoSize = true;
            this.chkNotepad.BackColor = System.Drawing.Color.Transparent;
            this.chkNotepad.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkNotepad.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.chkNotepad.Location = new System.Drawing.Point(619, 402);
            this.chkNotepad.Margin = new System.Windows.Forms.Padding(4);
            this.chkNotepad.Name = "chkNotepad";
            this.chkNotepad.Size = new System.Drawing.Size(109, 21);
            this.chkNotepad.TabIndex = 6;
            this.chkNotepad.Text = "Notepad++";
            this.chkNotepad.UseVisualStyleBackColor = false;
            // 
            // chkCcleaner
            // 
            this.chkCcleaner.AutoSize = true;
            this.chkCcleaner.BackColor = System.Drawing.Color.Transparent;
            this.chkCcleaner.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkCcleaner.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.chkCcleaner.Location = new System.Drawing.Point(413, 484);
            this.chkCcleaner.Margin = new System.Windows.Forms.Padding(4);
            this.chkCcleaner.Name = "chkCcleaner";
            this.chkCcleaner.Size = new System.Drawing.Size(94, 21);
            this.chkCcleaner.TabIndex = 8;
            this.chkCcleaner.Text = "Ccleaner";
            this.chkCcleaner.UseVisualStyleBackColor = false;
            // 
            // chkWinRAR
            // 
            this.chkWinRAR.AutoSize = true;
            this.chkWinRAR.BackColor = System.Drawing.Color.Transparent;
            this.chkWinRAR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkWinRAR.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.chkWinRAR.Location = new System.Drawing.Point(620, 560);
            this.chkWinRAR.Margin = new System.Windows.Forms.Padding(4);
            this.chkWinRAR.Name = "chkWinRAR";
            this.chkWinRAR.Size = new System.Drawing.Size(89, 21);
            this.chkWinRAR.TabIndex = 9;
            this.chkWinRAR.Text = "WinRAR";
            this.chkWinRAR.UseVisualStyleBackColor = false;
            // 
            // chk7zip
            // 
            this.chk7zip.AutoSize = true;
            this.chk7zip.BackColor = System.Drawing.Color.Transparent;
            this.chk7zip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk7zip.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.chk7zip.Location = new System.Drawing.Point(415, 560);
            this.chk7zip.Margin = new System.Windows.Forms.Padding(4);
            this.chk7zip.Name = "chk7zip";
            this.chk7zip.Size = new System.Drawing.Size(60, 21);
            this.chk7zip.TabIndex = 9;
            this.chk7zip.Text = "7zip";
            this.chk7zip.UseVisualStyleBackColor = false;
            // 
            // chkForceChangePass
            // 
            this.chkForceChangePass.AutoSize = true;
            this.chkForceChangePass.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkForceChangePass.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkForceChangePass.ForeColor = System.Drawing.Color.RoyalBlue;
            this.chkForceChangePass.Location = new System.Drawing.Point(868, 471);
            this.chkForceChangePass.Margin = new System.Windows.Forms.Padding(4);
            this.chkForceChangePass.Name = "chkForceChangePass";
            this.chkForceChangePass.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkForceChangePass.Size = new System.Drawing.Size(287, 23);
            this.chkForceChangePass.TabIndex = 15;
            this.chkForceChangePass.Text = "Đổi mật khẩu thời gian tiếp theo";
            this.chkForceChangePass.UseVisualStyleBackColor = false;
            this.chkForceChangePass.CheckedChanged += new System.EventHandler(this.chkForceChangePass_CheckedChanged);
            // 
            // chkStartUp
            // 
            this.chkStartUp.AutoSize = true;
            this.chkStartUp.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chkStartUp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkStartUp.ForeColor = System.Drawing.Color.RoyalBlue;
            this.chkStartUp.Location = new System.Drawing.Point(868, 423);
            this.chkStartUp.Margin = new System.Windows.Forms.Padding(4);
            this.chkStartUp.Name = "chkStartUp";
            this.chkStartUp.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chkStartUp.Size = new System.Drawing.Size(237, 23);
            this.chkStartUp.TabIndex = 15;
            this.chkStartUp.Text = "Khởi động cùng hệ thống";
            this.chkStartUp.UseVisualStyleBackColor = false;
            this.chkStartUp.CheckedChanged += new System.EventHandler(this.chkStartUp_CheckedChanged);
            // 
            // ttAutologin
            // 
            this.ttAutologin.AutomaticDelay = 50;
            // 
            // ckhMicroE
            // 
            this.ckhMicroE.AutoSize = true;
            this.ckhMicroE.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ckhMicroE.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ckhMicroE.ForeColor = System.Drawing.Color.ForestGreen;
            this.ckhMicroE.Location = new System.Drawing.Point(413, 602);
            this.ckhMicroE.Margin = new System.Windows.Forms.Padding(4);
            this.ckhMicroE.Name = "ckhMicroE";
            this.ckhMicroE.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.ckhMicroE.Size = new System.Drawing.Size(192, 28);
            this.ckhMicroE.TabIndex = 16;
            this.ckhMicroE.Text = "MicroEmulator ";
            this.ckhMicroE.UseVisualStyleBackColor = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Lime;
            this.label7.Location = new System.Drawing.Point(835, 75);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 24);
            this.label7.TabIndex = 17;
            this.label7.Text = "IPV4: ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Lime;
            this.label8.Location = new System.Drawing.Point(835, 105);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(105, 24);
            this.label8.TabIndex = 18;
            this.label8.Text = "GateWay: ";
            // 
            // lbIPV4
            // 
            this.lbIPV4.AutoSize = true;
            this.lbIPV4.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lbIPV4.Font = new System.Drawing.Font("Arial Black", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbIPV4.ForeColor = System.Drawing.Color.RoyalBlue;
            this.lbIPV4.Location = new System.Drawing.Point(959, 73);
            this.lbIPV4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbIPV4.Name = "lbIPV4";
            this.lbIPV4.Size = new System.Drawing.Size(117, 27);
            this.lbIPV4.TabIndex = 19;
            this.lbIPV4.Text = "127.0.0.1";
            // 
            // lbGateW
            // 
            this.lbGateW.AutoSize = true;
            this.lbGateW.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lbGateW.Font = new System.Drawing.Font("Arial Black", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbGateW.ForeColor = System.Drawing.Color.RoyalBlue;
            this.lbGateW.Location = new System.Drawing.Point(959, 102);
            this.lbGateW.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbGateW.Name = "lbGateW";
            this.lbGateW.Size = new System.Drawing.Size(117, 27);
            this.lbGateW.TabIndex = 20;
            this.lbGateW.Text = "127.0.0.1";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Red;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Lavender;
            this.button1.Location = new System.Drawing.Point(920, 516);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(192, 46);
            this.button1.TabIndex = 21;
            this.button1.Text = "Đổi hình nền";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(-31, -57);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(133, 62);
            this.pictureBox1.TabIndex = 22;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(1439, 404);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(57, 44);
            this.pictureBox2.TabIndex = 23;
            this.pictureBox2.TabStop = false;
            // 
            // form_LowEndVietFastVPSConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.BackgroundImage = global::LowEndViet_com_VPS_Tool_Properties_Resources.brackgroud;
            this.ClientSize = new System.Drawing.Size(1271, 732);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lbGateW);
            this.Controls.Add(this.lbIPV4);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.ckhMicroE);
            this.Controls.Add(this.btnChangeAdminAcc);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.btnChangeRDPPort);
            this.Controls.Add(this.txtFirefoxVer);
            this.Controls.Add(this.txtAdminAcc);
            this.Controls.Add(this.btnChangePassword);
            this.Controls.Add(this.txtRDPPort);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.chkAutoLogin);
            this.Controls.Add(this.lblCurrentRDPPort);
            this.Controls.Add(this.chkTor);
            this.Controls.Add(this.label100);
            this.Controls.Add(this.chkUnikey);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.chkBrave);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtNewPassword);
            this.Controls.Add(this.chkFirefox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chkOpera);
            this.Controls.Add(this.chkChrome);
            this.Controls.Add(this.chkCoccoc);
            this.Controls.Add(this.chkNET48);
            this.Controls.Add(this.chkNotepad);
            this.Controls.Add(this.btnConfigWindows);
            this.Controls.Add(this.chkCcleaner);
            this.Controls.Add(this.chkForceChangePass);
            this.Controls.Add(this.chkWinRAR);
            this.Controls.Add(this.chkStartUp);
            this.Controls.Add(this.chk7zip);
            this.Controls.Add(this.rdDHCP);
            this.Controls.Add(this.chkCheckAll);
            this.Controls.Add(this.chkDeleteTempFolder);
            this.Controls.Add(this.rdStatic);
            this.Controls.Add(this.chkPerformanceRDP);
            this.Controls.Add(this.chkDisableDriverSig);
            this.Controls.Add(this.bntConfigNetwork);
            this.Controls.Add(this.chkDisableRecovery);
            this.Controls.Add(this.chkDisableHiberfil);
            this.Controls.Add(this.cmbDNS);
            this.Controls.Add(this.chkDisableSleep);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.chkSmallTaskbar);
            this.Controls.Add(this.chkShowTrayIcon);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkDisableUpdate);
            this.Controls.Add(this.chkTurnoffESC);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkDisableUAC);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.txtCustomDNS);
            this.Controls.Add(this.txtNetmask);
            this.Controls.Add(this.txtGateway);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "form_LowEndVietFastVPSConfig";
            this.Text = "Thành VPS - VieCloud - 0949.184.456 - ";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.form_LowEndVietFastVPSConfig_FormClosed);
            this.Load += new System.EventHandler(this.form_LowEndVietFastVPSConfig_Load);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_LowEndVietFastVPSConfig_MouseClick);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

	}
}
