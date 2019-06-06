using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using windowsService1.Logging;

namespace windowsService1
{
	class Program
	{
			static ILog Log = LogProvider.GetCurrentClassLogger();
		static void Main(string[] args)
		{
			var rc = HostFactory.Run(x =>                                   //1
			{
				x.UseNLog();
				x.Service<ConsoleRunner>();
				x.RunAsNetworkService();                                       //6

				x.SetDescription("Sample WindowsService Host");                   //7
				x.SetDisplayName("TestStuff");                                  //8
				x.SetServiceName("TestStuff");                                  //9
			});                                                             //10

			var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  //11
			Environment.ExitCode = exitCode;
			Log.Info(() => $"exit code is {exitCode}");
		}
	}

	class ConsoleRunner : ServiceControl
	{
		ILog Log = LogProvider.GetCurrentClassLogger();

		public bool Start(HostControl hostControl)
		{
			var IsOutputToStdout = true;

			var commandName = "consoleApp1.exe";

			var arguments = "-i \"hardcoded argument\"";
			var stdin = "some string that is should be passed as parameter to the console application";

			var TimeoutInMilliseconds = 3000;
			try
			{
				using (var stream = new MemoryStream())
					Run(commandName, true, IsOutputToStdout, arguments, stdin, TimeoutInMilliseconds, stream);
				return true;
			}
			catch (Exception e)
			{
				Log.FatalException($"failed to start {commandName}", e);
				return false;
			}
		
		}

		public int Run(string commandName, bool IsInputFromStdin, bool IsOutputToStdout, string arguments, string stdin, int TimeoutInMilliseconds, Stream stream)
		{
			if (!Path.IsPathRooted(commandName))
			{
				var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				commandName = Path.Combine(path, commandName);
			}

			using (var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					UseShellExecute = false,
					RedirectStandardOutput = IsOutputToStdout,
					RedirectStandardInput = IsInputFromStdin,
					WindowStyle = ProcessWindowStyle.Hidden,
					FileName = commandName,
					Arguments = arguments + (IsInputFromStdin ? " -r " : stdin),
				}
			})
			{
				Log.Debug($"Executing {process.StartInfo.FileName} {process.StartInfo.Arguments}");
				process.Start();

				if (IsInputFromStdin)
				{
					Log.Debug($"wrinting to stdin '{stdin}'");
					using (var standardInput = process.StandardInput)
					{
						standardInput.AutoFlush = true;
						standardInput.Write(stdin);
					}
				}
				if (IsOutputToStdout)
				{
					using (var standardOutput = process.StandardOutput)
					{
						standardOutput.BaseStream.CopyTo(stream);
					}

				}
				var exited = process.WaitForExit(TimeoutInMilliseconds);

				Log.Debug($"has got {process.StartInfo.FileName} output, stream:{stream.Length}Bytes");
				return process.ExitCode;
			}
		}

		public bool Stop(HostControl hostControl)
		{
			return true;
		}
	}
}
