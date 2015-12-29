using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Text.RegularExpressions;

// using http://www.fluxbytes.com/csharp/how-to-create-and-connect-to-an-sqlite-database-in-c/

namespace WaffleBot_Console
{
	class SQLiteUsers
	{
		private string _file;
		private string _streamer;

		private SQLiteConnection sqlc;

		/*

			a modbot style sql will have a table named after the streamer, which will then be setup like this:

			| rowid | user | currency | subscriber | btag | userlevel | display_name | time_watched |
			-----------------------------------------------------------------------------------------
			|   1   | mike |    45    |     0      |  ??  |   [0-5]   |     MikE     |      225     |
		
			COMMANDS TO USE WITH IT:
				get a user's currency value:
					SELECT currency FROM streamername WHERE user="ghostyx123"
				get a user's time watched:
					SELECT time_watched FROM streamername WHERE user="ghostyx123"
				get a user's currency, time watched, and display name (not in that order):
					SELECT display_name, time_watched, currency FROM streamername WHERE user="ghostyx123"
					=> | GhOstYx123 [for example] | 45 | 9

			*/

		public SQLiteUsers(string file, string streamername)
		{
			Regex filePattern = new Regex(@".+\.sqlite$");

			_file = (filePattern.IsMatch(file.ToLower())) ? (file) : (new Regex(@"(\..+)?$").Replace(file.ToLower(), ".sqlite"));
			_streamer = streamername;
			sqlc = new SQLiteConnection(@"Data Source=" + file + ";");
		}

		public string timeForUser(string username)
		{
			string user = username.ToLower();
			string query = "SELECT time_watched FROM " + _streamer.ToLower() + " WHERE user=\"" + user + "\"";



			return "placeholder string";
		}

		public string currencyForUser(string username)
		{
			return "placeholder string";
		}
	}
}
