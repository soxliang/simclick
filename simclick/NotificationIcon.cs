using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace simclick
{
	public sealed class NotificationIcon
	{
		private NotifyIcon notifyIcon;

		private ContextMenu notificationMenu;

		private GridForm gridform = new GridForm();

		public NotificationIcon()
		{
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu(InitializeMenu());
			notifyIcon.DoubleClick += IconDoubleClick;
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(NotificationIcon));
			notifyIcon.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			notifyIcon.ContextMenu = notificationMenu;
		}

		private MenuItem[] InitializeMenu()
		{
			return new MenuItem[2]
			{
				new MenuItem("About", menuAboutClick),
				new MenuItem("Exit", menuExitClick)
			};
		}

		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			bool createdNew;
			using (new Mutex(true, "simclick", out createdNew))
			{
				if (createdNew)
				{
					NotificationIcon notificationIcon = new NotificationIcon();
					notificationIcon.notifyIcon.Visible = true;
					Application.Run();
					notificationIcon.notifyIcon.Dispose();
				}
			}
		}

		private void menuConfigClick(object sender, EventArgs e)
		{
		}

		private void menuAboutClick(object sender, EventArgs e)
		{
		}

		private void menuExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void IconDoubleClick(object sender, EventArgs e)
		{
		}
	}
}
