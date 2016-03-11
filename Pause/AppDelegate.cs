using System.Collections.Generic;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace Pause
{
	[Register("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		private NSStatusItem statusItem;
		private NSMenu menu = new NSMenu();
		private List<Process> processes = new List<Process>();

		public override void DidFinishLaunching(NSNotification notification)
		{
			this.statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
			this.statusItem.Menu = this.menu;
			this.statusItem.Button.Image = NSImage.ImageNamed(NSImageName.RefreshTemplate);

			this.Populate(this);
		}

		[Action("populate:")]
		public void Populate(NSObject sender)
		{
			if (this.menu.Count > 0)
			{
				this.menu.RemoveAllItems();
			}

			this.menu.AddItem("Refresh", new Selector("populate:"), "r");

			this.menu.AddItem(NSMenuItem.SeparatorItem);

			if (this.processes.Count > 0)
			{
				this.processes.Clear();
			}

			foreach (var app in NSWorkspace.SharedWorkspace.RunningApplications)
			{
				if (app.ActivationPolicy != NSApplicationActivationPolicy.Regular)
				{
					continue;
				}

				this.processes.Add(new Process(app));
			}

			this.processes.Sort();

			var key = 0;

			foreach (var process in this.processes)
			{
				if (key < 10)
				{
					process.KeyEquivalent = (key++).ToString();
				}

				this.menu.AddItem(process);
			}

			this.menu.AddItem(NSMenuItem.SeparatorItem);

			this.menu.AddItem("Quit Pause", new Selector("terminate:"), "q");
		}

		[Action("terminate:")]
		public void Terminate(NSObject sender)
		{
			NSApplication.SharedApplication.Terminate(this);
		}
	}
}