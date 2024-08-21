using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace LowEndViet.com_VPS_Tool.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("LowEndViet.com_VPS_Tool.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap brackgroud
	{
		get
		{
			object @object = ResourceManager.GetObject("brackgroud", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap brackgroud__1_
	{
		get
		{
			object @object = ResourceManager.GetObject("brackgroud (1)", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap download
	{
		get
		{
			object @object = ResourceManager.GetObject("download", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap mau_background_dep_820x513
	{
		get
		{
			object @object = ResourceManager.GetObject("mau-background-dep-820x513", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal Resources()
	{
	}
}
