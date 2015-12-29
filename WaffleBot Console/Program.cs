using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.Sql;

namespace WaffleBot_Console
{
	class Program
	{
		private IrcClient irc = new IrcClient();
		private const string server = "irc.twitch.tv";
		private const int port = 6667;

		// backups of the values below
		// private string channel = "#barbaricmustard";
		// private string key = "oauth:r6lnbamwmscxhovr9e4new8hp5zc7o";
		// private string dbname = "wafflebot.sqlite";
		// private string botuname = "ghostynugget";

		// values to be edited by config file
		// the values written in here are placeholders.
		// channel should always start with #
		private string channel = "";
		private string key = "";
		private string botuname = "";
		private string dbname = "";

		private const string configpath = @"botconfig.txt";

		// its main mane
		static void Main(string[] args)
		{
			Program program = new Program();
		}

		public Program()
		{
			try
			{
				
				if (!File.Exists(configpath))
				{
					File.WriteAllText(configpath, "#your username, e.g. barbaricmustard or #barbaricmustard\r\nchannelname=\r\n#your bot oauth token (sign into www.twitchapps.com/tmi/ as the bot)\r\nbottoken=\r\n#your bot's username, e.g. mustardbot\r\nbotusername=\r\n#the name of your sqlite db (only change if you are porting from modbot)\r\ndbname=wafflebot.sqlite");
					Console.WriteLine("botconfig.txt did not exist, created a new one.\nEdit the file then try again.");
					Console.ReadKey();
					Environment.Exit(0);
				}

				/*
				botconfig.txt should look like this:

				# blah blah blah
				channelname=[a-zA-Z0-9_]+
				# blah blah blah
				bottoken=oauth:[A-Za-z0-9]+
				# blah blah blah
				botusername=[a-zA-Z0-9_]+
				# blah blah blah
				dbname=[a-zA-Z0-9 _/-]+(\.sqlite|\.SQLITE)?

				*/

				StreamReader file = new StreamReader(configpath);
				string line = "";
				List<string> config = new List<string>();

				// read the lines from the config
				while ((line = file.ReadLine()) != null)
				{
					// if it isn't a comment, then parse the lines that are important
					if (!line.StartsWith("#"))
					{
						if (new Regex(@"^channelname=[a-zA-Z0-9_]+").IsMatch(line))
						{
							string value = new Regex("^channelname=").Replace(line, "");

							Console.WriteLine("found channelname: " + value);

							config.Add("channelname");

							// store channel name
							channel = (value.StartsWith("#") ? "" : "#") + value;
						}
						if (new Regex(@"^bottoken=oauth:[a-zA-Z0-9]+").IsMatch(line))
						{
							string value = new Regex("^bottoken=").Replace(line, "");

							Console.WriteLine("found bottoken: " + value);

							config.Add("bottoken");

							// store oauth token
							key = value;
						}
						if (new Regex(@"^botusername=[a-zA-Z0-9_]+").IsMatch(line))
						{
							string value = new Regex("^botusername=").Replace(line, "");

							Console.WriteLine("found botusername: " + value);

							config.Add("botname");

							// store bot user name
							botuname = value;
						}
						// just for future reference, the @ means that stuff inside cannot be escaped, aka \ = \ not \\ = \
						if (new Regex(@"^dbname=[a-zA-Z0-9 _/-]+(\.sqlite|\.SQLITE)?").IsMatch(line))
						{
							string value = new Regex("^dbname=").Replace(line, "");

							if (!new Regex(@"\.(sqlite|SQLITE)$").IsMatch(value))
							{
								value += ".sqlite";
							}

							Console.WriteLine("found dbname: " + value);

							config.Add("dbname");

							// store the database name
							dbname = value;
						}
					}
				}

				Console.WriteLine("");

				file.Close();
				
				if (config.Count != 4)
				{
					File.WriteAllText(configpath, "#your username, e.g. barbaricmustard or #barbaricmustard\r\nchannelname=\r\n#your bot oauth token (sign into www.twitchapps.com/tmi/ as the bot)\r\nbottoken=\r\n#your bot's username, e.g. mustardbot\r\nbotusername=\r\n#the name of your sqlite db (only change if you are porting from modbot)\r\ndbname=wafflebot.sqlite");
					Console.WriteLine("botconfig.txt was formed incorrectly, created a new one.\nWhat was found in the old one:");

					string stuff = "";
					foreach (string thing in config)
					{
						stuff += thing + ", ";
					}
					stuff = new Regex(@", $").Replace(stuff, "");
					Console.WriteLine(stuff);

					Console.ReadKey();
					Environment.Exit(0);
				}
				else
				{
					/* all of this is done when the stuff is read from the file now
					Console.WriteLine("Found channel setting: " + channel);
					// make sure to mask most of the oauth just in case
					Console.WriteLine("Found key setting: " + new Regex("@[a-zA-Z0-9]{18}$").Replace(key, "") + "******************");
					Console.WriteLine("Found bot username setting: " + botuname);
					Console.WriteLine("Found db name: " + dbname);
					*/
				}

			}
			catch (IOException e)
			{
				Console.WriteLine("There was an issue:\n" + e.Message);
				Console.ReadKey();
			}

			irc.OnConnected += new EventHandler(OnConnected);

			irc.OnChannelMessage += new IrcEventHandler(OnChannelMessage);

			irc.ActiveChannelSyncing = true;

			try
			{
				irc.Connect(server, port);
				
			}
			catch (Exception e)
			{
				Console.Write("Failed to connect:\n" + e.Message);
				Console.ReadKey();
			}
		}

		void OnConnected(object sender, EventArgs e)
		{
			irc.Login(botuname, botuname, 0, botuname, key);
			irc.RfcJoin(channel);

			Console.WriteLine("Connected to " + channel + " successfully!");
			me("has joined " + channel + ".");
			
			irc.Listen();
		}

		// this is where we handle messages that come in (parse for commands)
		void OnChannelMessage(object sender, IrcEventArgs e)
		{
			// message and nickname
			string msg = e.Data.Message;
			string nick = e.Data.Nick;

			// [DISABLED] log chat to console
			//Console.WriteLine("<" + e.Data.Nick + "> " + msg);

			// parse commands
			if (msg.StartsWith("!"))
			{

				// kill the bot if the nick is not the channel owner. during testing, i set it to me so that i can use it in mustard's chat
				if (new Regex("^!killbot$").IsMatch(msg) && nick.ToLower() == "ghostyx123")
				{
					me("is leaving " + channel + " by order of master " + /*channel.Replace("#", "")*/ "ghosty" + ".");
					System.Threading.Thread.Sleep(500);
					Environment.Exit(0);
				}
				if (new Regex("^!killbot$").IsMatch(msg) && nick.ToLower() != "ghostyx123")
				{
					say("Sorry, " + nick + ", but only master " + channel.Replace("#", "") + " can make me do that.");
				}
				if (new Regex(@"^!stab .+").IsMatch(msg))
				{
					string target = msg.Replace("!stab ", "");
					new Regex(@"^ *").Replace(target, "");
					me("stabs " + target + " on behalf of " + nick + " Kappa");
				}
				if (new Regex(@"^!hug .+").IsMatch(msg))
				{
					string target = msg.Replace("!hug ", "");
					new Regex(@"^ *").Replace(target, "");
					me("hugs " + target + " on behalf of " + nick + " Kappa");
				}
				if (new Regex(@"^!kiss .+").IsMatch(msg))
				{
					string target = msg.Replace("!hug ", "");
					new Regex(@"^ *").Replace(target, "");
					me("kisses " + target + " on behalf of " + nick + " (AWKWARD)");
				}
				if (new Regex(@"^!swiggity").IsMatch(msg))
				{
					say("LOOK AT THAT BOOTY! PogChamp");
				}
			}

		}

		void say(string text)
		{
			irc.SendMessage(SendType.Message, channel, text);
		}

		void me(string text)
		{
			irc.SendMessage(SendType.Message, channel, "/me " + text);
		}

		/// <summary>
		/// NOT WORKING - whispers to people
		/// </summary>
		/// <param name="uname">person to whisper to</param>
		/// <param name="text">text to whisper to them</param>
		void whisper(string uname, string text)
		{
			irc.SendMessage(SendType.Message, channel, "/w " + uname + " " + text);
		}
	}
}
