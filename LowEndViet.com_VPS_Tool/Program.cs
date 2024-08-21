using System.Windows.Forms;

namespace LowEndViet.com_VPS_Tool;

internal static class Program
{
	private static void Main(string[] args)
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Application.Run(new form_LowEndVietFastVPSConfig(args));
	}
}
