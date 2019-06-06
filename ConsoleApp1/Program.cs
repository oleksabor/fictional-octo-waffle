using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
	class Program
	{
		static void Main(string[] args)
		{
			var config = args.Parse();

			var clargs = config.ReadArgsFromStdin || Console.IsInputRedirected
				? ReadStdin() : config.CLArgs;

			if (!string.IsNullOrEmpty(clargs))
			{
				Console.WriteLine($"info: {config.Info}");
				Console.WriteLine($"has got argument: {clargs}");
			}
			else
				Console.WriteLine("no arguments was set");
		}

		static string ReadStdin()
		{
			//if (Console.IsInputRedirected)
			{
				using (var stdin = Console.OpenStandardInput())
				{
					var sb = new StringBuilder();
					using (var strStream = new StringWriter(sb))
					{
						using (var input = new StreamReader(stdin))
							strStream.Write(input.ReadToEnd());
						return sb.ToString();
					}
				}
			}
			//throw new ApplicationException( "no stream redirected");
		}
	}
}
