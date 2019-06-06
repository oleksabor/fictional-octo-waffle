using Fclp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
	public class Config
	{
		public bool ReadArgsFromStdin { get; set; }
		public string CLArgs { get; set; }
		public string Info { get; set; }
	}

	public static class ArgsToConfig
	{
		public static Config Parse(this string[] args)
		{
			var parser = new FluentCommandLineParser<Config>();
			parser.Setup(_ => _.ReadArgsFromStdin).As('r', "read-args-from-stdin").SetDefault(false);
			parser.Setup(_ => _.CLArgs).As('a', "cliargs");
			parser.Setup(_ => _.Info).As('i', "info");

			var res = parser.Parse(args);

			if (res.HasErrors)
				throw new ArgumentException(string.Format("failed to parse command line\r\n{0}", res.ErrorText));
			return parser.Object;
		}
	}
}
