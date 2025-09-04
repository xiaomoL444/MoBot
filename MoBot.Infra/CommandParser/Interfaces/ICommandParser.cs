using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.CommandParser.Interfaces
{
	public interface ICommandParser
	{
		void Parse(string command);
	}
}
