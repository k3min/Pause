using System;
using AppKit;
using Foundation;
using CoreGraphics;
using ObjCRuntime;

namespace Pause
{
	[Register("Process")]
	public sealed class Process : NSMenuItem, IComparable<Process>
	{
		private const string title = "{0} ({1}%)";

		private const string ps = "ps -p {0} -o state=,%cpu= | tr -s ' ' ','";
		private const string kill = "kill -{0} {1}";

		private static Selector action = new Selector("pause:");
		private static CGSize size = new CGSize(16, 16);

		private NSRunningApplication app;

		public float CPU
		{
			get;
			private set;
		}

		public int ID
		{
			get { return this.app.ProcessIdentifier; }
		}

		public override string Title
		{
			get
			{
				return string.Format(Process.title, this.app.LocalizedName, this.CPU);
			}
		}

		public bool Paused
		{
			get
			{
				return (base.State == NSCellStateValue.On);
			}

			set
			{
				base.State = value ? NSCellStateValue.On : NSCellStateValue.Off;
			}
		}

		public Process(NSRunningApplication app)
		{
			this.app = app;

			this.Action = Process.action;
			this.Target = this;

			this.Image = this.app.Icon;
			this.Image.Size = Process.size;

			var task = Process.Task(string.Format(Process.ps, this.ID)).Split(',');

			this.Paused = (task[0] == "T");
			this.CPU = float.Parse(task[1]);
		}

		[Action("pause:")]
		public void Pause(Process sender)
		{
			Process.Task(string.Format(Process.kill, (sender.Paused = !sender.Paused) ? "STOP" : "CONT", sender.ID));
		}

		public static string Task(string command)
		{
			var pipe = NSPipe.Create();
			var file = pipe.ReadHandle;

			var task = new NSTask
			{
				LaunchPath = "/bin/bash",
				Arguments = new [] { "-c", command },
				StandardOutput = pipe
			};

			task.Launch();

			var data = file.ReadDataToEndOfFile();

			file.CloseFile();

			return data.ToString(NSStringEncoding.UTF8);
		}

		public int CompareTo(Process other)
		{
			if (Math.Abs(this.CPU - other.CPU) < float.Epsilon)
			{
				return 0;
			}
			else if (this.CPU > other.CPU)
			{
				return -1;
			}

			return 1;
		}
	}
}