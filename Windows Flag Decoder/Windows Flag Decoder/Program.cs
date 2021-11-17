using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowsFlagDecoder
{
	public class Program
	{
		/// <summary>
		/// The flags, where the key is the name of the flag, and the value is the code.
		/// </summary>
		private static Dictionary<string, int> flags = new Dictionary<string, int>();

		public static void Main(string[] args)
		{
			Console.Write("Please enter the flag to decode (in hexadecimal format): ");
			string flagToDecodeStr = Console.ReadLine();
			int flagToDecode = 0;
			if(flagToDecodeStr == null)
			{
				Console.WriteLine("Please enter a value!");
				return;
			}
			else
			{
				flagToDecode = HexStrToInt(flagToDecodeStr);
				if(flagToDecode < 0)
				{
					return;
				}
			}

			Console.WriteLine("1. Select FlagFile.");
			Console.WriteLine("2. Create FlagFile.");
			Console.Write("Please enter the number of your choice: ");
			string choiceNumberStr = Console.ReadLine();
			int choiceNumber = 0;
			if(choiceNumberStr == null)
			{
				Console.WriteLine("Please enter a value!");
				return;
			}
			else
			{
				try
				{
					choiceNumber = int.Parse(choiceNumberStr);
				}
				catch(Exception exc)
				{
					Console.WriteLine(exc.Message);
					return;
				}
			}

			if(choiceNumber == 1)
			{
				Console.Write("Please enter the name and path of the FlagFile: ");
				string flagFilePath = Console.ReadLine();
				try
				{
					ReadFlagFile(flagFilePath);
				} 
				catch(Exception exc)
				{
					Console.WriteLine(exc.Message);
					return;
				}
			}
			else if(choiceNumber == 2)
			{
				Console.Write("Please enter the name of the file where the names of the desired flags are located: ");
				string flagFilePath = Console.ReadLine();
				try
				{
					ReadNewFile(flagFilePath);
				}
				catch(Exception exc)
				{
					Console.WriteLine(exc.Message);
					return;
				}
			}
			else
			{
				Console.WriteLine("Please choose a valid number!");
			}

			return;
		}

		/// <summary>
		/// Removes the 0x from the given hexStr, if present. Then parses the remainder as a hexadecimal int and returns it. Prints an Exception to the Console if thrown, and returns -1.
		/// </summary>
		/// <param name="hexStr">The string to convert to a hexadecimal integer.</param>
		/// <returns></returns>
		private static int HexStrToInt(string hexStr)
		{
			if(hexStr != null)
			{
				if(hexStr.StartsWith("0x"))
				{
					hexStr = hexStr.Remove(0, 2);
				}

				try
				{
					return int.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
				}
				catch(Exception exc)
				{
					Console.WriteLine(exc.Message);
				}
			}

			return -1;
		}

		/// <summary>
		/// Reads the file at the given flagFilePath and puts the flag names/codes in the flags Dictionary. Throws an Exception on error.
		/// </summary>
		/// <param name="flagFilePath">The path to the flag file, including the name of the file.</param>
		/// <exception cref="Exception"></exception>
		private static void ReadFlagFile(string flagFilePath)
		{
			if (File.Exists(flagFilePath))
			{
				using (StreamReader sr = new StreamReader(flagFilePath))
				{
					string line = null;
					while ((line = sr.ReadLine()) != null)
					{
						if (line != null)
						{
							string flagName = line;

							line = sr.ReadLine();
							if (line != null)
							{
								int flagCode = HexStrToInt(line);
								if (flagCode < 0)
								{
									throw new Exception("Invalid flag code <" + flagCode + "> found!");
								}

								flags.Add(flagName, flagCode);
							}
							else
							{
								throw new Exception("Flag without code <" + flagName + "> found!");
							}
						}
					}
				}
			}
			else
			{
				throw new Exception("The flag file with path and name <" + flagFilePath + "> does not exist!");
			}
		}

		/// <summary>
		/// Reads, filters, and saves the file at the given newFileName path as a FlagFile. Throws an Exception on Error.
		/// </summary>
		/// <param name="newFileName">The path to the file to read, and the name of the file.</param>
		/// <exception cref="Exception"></exception>
		private static void ReadNewFile(string newFileName)
		{
			if(File.Exists(newFileName))
			{
				Console.WriteLine("Please enter the RegEx string used to find all the flags in the document:");
				string regExStr = Console.ReadLine();
				if(regExStr == null)
				{
					throw new Exception("Please enter a RegEx string!");
				}
			}
			else
			{
				throw new Exception("The file <" + newFileName + "> does not exist!");
			}
		}
	}
}