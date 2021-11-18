using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace WindowsFlagDecoder
{
	public class Program
	{
		/// <summary>
		/// The flags to check against the flagToDecode given by the user.
		/// </summary>
		private static Dictionary<string, int> flags = new Dictionary<string, int>();
		/// <summary>
		/// Temporary storage for flags loaded in from another file.
		/// </summary>
		private static Dictionary<string, int> newFlags = new Dictionary<string, int>();
		/// <summary>
		/// A number of known paths to the vcvars64.bat file on Windows systems.
		/// </summary>
		private static string[] vcvars64Paths = new string[]
		{
			"\"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvars64.bat\"",
			"\"C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\VC\\Auxiliary\\Build\\vcvars64.bat\"",
			"\"C:\\Program Files\\Microsoft Visual Studio\\2022\\Professional\\VC\\Auxiliary\\Build\\vcvars64.bat\"",
			"\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\VC\\Auxiliary\\Build\\vcvars64.bat\"",
			"\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\VC\\Auxiliary\\Build\\vcvars64.bat\"",
			"\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\VC\\Auxiliary\\Build\\vcvars64.bat\"",
			"\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\VC\\Auxiliary\\Build\\vcvars64.bat\"",
			"\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\VC\\Auxiliary\\Build\\vcvars64.bat\"",
			"\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Professional\\VC\\Auxiliary\\Build\\vcvars64.bat\""
		};

		public static void Main(string[] args)
		{
			int flagToDecode = 0;
			while (true)
			{
				Console.Write("Please enter the flag to decode (in hexadecimal format): ");
				string flagToDecodeStr = Console.ReadLine();
				if (flagToDecodeStr == null || flagToDecodeStr == "")
				{
					Console.WriteLine("Please enter a value!");
				}
				else
				{
					try
					{
						flagToDecode = HexStrToInt(flagToDecodeStr);
						break;
					}
					catch (Exception)
					{
						Console.WriteLine("Invalid flag code <" + flagToDecodeStr + "> used!");
					}
				}
			}

			while (true)
			{
				Console.WriteLine();
				Console.WriteLine("1. Add FlagFile.");
				Console.WriteLine("2. Create New Flagfile.");
				Console.WriteLine("3. Find Matching Flags.");
				Console.Write("Please enter the number of your choice: ");
				string choiceNumberStr = Console.ReadLine();
				int choiceNumber = 0;
				if (choiceNumberStr == null)
				{
					Console.WriteLine("Please enter a value!");
					continue;
				}
				else
				{
					try
					{
						choiceNumber = int.Parse(choiceNumberStr);
					}
					catch (Exception exc)
					{
						Console.WriteLine(exc.Message);
						continue;
					}
				}

				if (choiceNumber == 1)
				{
					Console.WriteLine();
					Console.WriteLine("Please enter the name and path of the FlagFile: ");
					string flagFilePath = Console.ReadLine();
					try
					{
						ReadFlagFile(flagFilePath);
					}
					catch (Exception exc)
					{
						Console.WriteLine(exc.Message);
					}
				}
				else if (choiceNumber == 2)
				{
					Console.WriteLine();
					Console.WriteLine("Please enter the name of the file where the desired flag names are located: ");
					string flagFilePath = Console.ReadLine();
					if (flagFilePath == null || flagFilePath == "")
					{
						Console.WriteLine("Please enter a value!");
						continue;
					}

					try
					{
						newFlags.Clear();
						ReadNewFile(flagFilePath);

						SaveFlags();

						foreach (string key in newFlags.Keys)
						{
							if (!flags.ContainsKey(key))
							{
								int value = newFlags[key];
								flags.Add(key, value);
							}
						}
					}
					catch (Exception exc)
					{
						Console.WriteLine(exc.Message);
						continue;
					}
				}
				else if(choiceNumber == 3)
				{
					if(flags.Count == 0) 
					{
						Console.WriteLine("No flags have been added yet! Please add some first!");
						continue;
					}
					else
					{
						break;
					}
				}
				else
				{
					Console.WriteLine("Please choose a valid number!");
					continue;
				}
			}

			// Now that we've got the flags, do the execution
			FindMatchingFlags(flagToDecode);

			return;
		}

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
							if (!flags.ContainsKey(flagName))
							{
								if (line != null)
								{
									try
									{
										int flagCode = HexStrToInt(line);
										flags.Add(flagName, flagCode);
									}
									catch (Exception)
									{
										Console.WriteLine("Invalid flag code <" + line + "> found!");
									}
								}
								else
								{
									Console.WriteLine("Flag without code <" + flagName + "> found!");
								}
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

		private static void ReadNewFile(string newFileName)
		{
			if (File.Exists(newFileName))
			{
				string wholeFileStr = File.ReadAllText(newFileName);
				if (wholeFileStr.Length <= 0)
				{
					throw new Exception("The file at <" + newFileName + "> was empty!");
				}

				// Filter the files results into a list with use of RegEx
				List<string> finalMatches = RegExFilter(wholeFileStr);

				// Cleanup files that may still exist from a previous compilation effort
				string workingDirectory = Directory.GetCurrentDirectory();
				Cleanup(workingDirectory);

				// A list of line numbers to exclude from the CPP file
				List<int> excludeLines = new List<int>();
				while (true)
				{
					// Generate and save the CPP file here.
					CreateCppFile(finalMatches, workingDirectory, excludeLines);

					// Attempt to compile the CPP file
					AttemptCompile(workingDirectory);

					// Check if the compilation was a success, and if not, allow for adjustments to be made and recompilation
					string executableName = workingDirectory + "\\temp.exe";
					if (!File.Exists(executableName) && excludeLines.Count == 0)
					{
						Console.WriteLine();
						Console.WriteLine("Could not find <" + executableName + ">!");
						Console.WriteLine("This could indicate there were compiler errors. See the compiler output above for more info.");
						Console.Write("Would you like to attempt to remove the problematic lines? (yY/nN): ");
						string response = Console.ReadLine();
						if (response.ToLower() == "y")
						{
							Console.WriteLine();
							while (true)
							{
								Console.Write("Please enter the number to exclude, or nothing and press enter to stop: ");
								string excludeStr = Console.ReadLine();
								if (excludeStr == null || excludeStr == "")
								{
									break;
								}

								try
								{
									int exclude = int.Parse(excludeStr);
									excludeLines.Add(exclude);
								}
								catch (Exception)
								{
									Console.WriteLine("Please enter a valid number!");
								}
							}
						}
						else if (response.ToLower() == "n")
						{
							throw new Exception("Could not find <" + executableName + ">!");
						}
						else
						{
							Console.WriteLine("Please pick a valid option! (yY/nN)");
						}
					}
					else if(!File.Exists(executableName) && excludeLines.Count > 0)
					{
						throw new Exception("Could not find <" + executableName + ">!");
					}
					else
					{
						break;
					}
				}

				// Execute CPP file here
				ProcessStartInfo psi2 = new ProcessStartInfo();
				psi2.FileName = workingDirectory + "\\temp.exe";
				psi2.UseShellExecute = false;
				psi2.RedirectStandardOutput = true;
				using (Process process = Process.Start(psi2))
				{
					using (StreamReader sr = process.StandardOutput)
					{
						string line = null;
						while ((line = sr.ReadLine()) != null)
						{
							string flagName = line;
							string flagCodeStr = sr.ReadLine();
							if (!newFlags.ContainsKey(flagName))
							{
								try
								{
									int flagCode = HexStrToInt(flagCodeStr);
									newFlags.Add(flagName, flagCode);
								}
								catch (Exception exc)
								{
									throw new Exception("Invalid flag code <" + flagCodeStr + "> found!");
								}
							}
						}
					}

					process.WaitForExit();
				}

				// Cleanup the files used for compilation and retrieving the flags
				Cleanup(workingDirectory);
			}
			else
			{
				throw new Exception("The file <" + newFileName + "> does not exist!");
			}
		}

		/// <summary>
		/// Convert the given hexStr to an integer, interpreted as a hexadecimal number. Also removes any prepended 0x, should there be one.
		/// </summary>
		/// <param name="hexStr">The string to convert.</param>
		/// <returns></returns>
		private static int HexStrToInt(string hexStr)
		{
			if (hexStr != null)
			{
				if (hexStr.StartsWith("0x"))
				{
					hexStr = hexStr.Remove(0, 2);
				}

				try
				{
					return int.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
				}
				catch (Exception exc)
				{
					throw exc;
				}
			}

			return 0;
		}

		/// <summary>
		/// Attempt to compile the C++ file temp.cpp found in the given workingDirectory into temp.exe. Will print compiler output to the console.
		/// </summary>
		/// <param name="workingDirectory">The path where temp.cpp is found.</param>
		/// <exception cref="Exception"></exception>
		private static void AttemptCompile(string workingDirectory)
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = "C:\\Windows\\System32\\cmd.exe";
			if (!File.Exists(psi.FileName))
			{
				throw new Exception("CMD could not be found at <" + psi.FileName + ">?!");
			}
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardInput = true;

			string pathToBatch = null;
			foreach (string path in vcvars64Paths)
			{
				if (File.Exists(path.Remove(0, 1).Remove(path.Length - 2, 1)))
				{
					pathToBatch = path;
					break;
				}
			}

			if (pathToBatch == null)
			{
				Console.WriteLine("Could not find the vcvars64.bat. Please enter the path to your Visual Studio installation.");
				Console.WriteLine("Example path: C:\\Program Files\\Microsoft Visual Studio\\2022\\Community");
				string vsPath = Console.ReadLine();
				if (!vsPath.StartsWith("\""))
				{
					vsPath = "\"" + vsPath;
				}
				vsPath += "\\VC\\Auxiliary\\Build\\vcvars64.bat\"";
				if (!File.Exists(vsPath.Remove(0, 1).Remove(vsPath.Length - 2, 1)))
				{
					throw new Exception("Could not find vcvars64.bat at <" + vsPath + ">!");
				}

				pathToBatch = vsPath;
			}

			using (Process process = Process.Start(psi))
			{
				using (StreamWriter sw = process.StandardInput)
				{
					sw.WriteLine(pathToBatch);
					sw.WriteLine("cl /EHsc " + "\"" + workingDirectory + "\\temp.cpp\"");
				}

				using (StreamReader sr = process.StandardOutput)
				{
					Console.WriteLine();
					Console.WriteLine("Compiler output:");
					string line = null;
					while ((line = sr.ReadLine()) != null)
					{
						Console.WriteLine(line);
					}
				}
			}
		}

		/// <summary>
		/// Generate the temp.cpp files based on the given flagNames and save it in the given workingDirectory. Exclude lines with numbers in the given excludeLines list.
		/// </summary>
		/// <param name="flagNames">A list of the flag names.</param>
		/// <param name="workingDirectory">The directory where to save the temp.cpp file.</param>
		/// <param name="excludeLines">Lines to exclude from the temp.cpp file.</param>
		private static void CreateCppFile(List<string> flagNames, string workingDirectory, List<int> excludeLines = null)
		{
			string cppFileStr = "#include <iostream>\n";
			cppFileStr += "#include <Windows.h>\n";
			cppFileStr += "int main()\n";
			cppFileStr += "{\n";
			int lineCount = 4;
			foreach (string flagName in flagNames)
			{
				lineCount += 1;
				if (excludeLines != null && excludeLines.Contains(lineCount))
				{
					continue;
				}
				cppFileStr += "std::cout << \"" + flagName + "\\n\" << std::hex << " + flagName + " << std::endl;\n";
			}
			cppFileStr += "}\n";

			using (StreamWriter sw = new StreamWriter(workingDirectory + "\\temp.cpp"))
			{
				sw.Write(cppFileStr);
			}
		}

		/// <summary>
		/// Go throught the flags Dictionary and attempt to check if any of the flags could've been used in the creation of the given flagToDecode.
		/// </summary>
		/// <param name="flagToDecode">The flag code to check.</param>
		private static void FindMatchingFlags(int flagToDecode)
		{
			string fullMatch = null;
			List<string> results = new List<string>();
			foreach (string key in flags.Keys)
			{
				int result = flagToDecode & flags[key];
				if (flagToDecode == flags[key])
				{
					fullMatch = key + " matched completely! (0x" + flags[key].ToString("X") + ")";
				}
				else if (result == flags[key])
				{
					results.Add(key + " was a match! (0x" + flags[key].ToString("X") + ")");
				}
			}

			Console.WriteLine();
			if (fullMatch != null || results.Count > 0)
			{
				Console.WriteLine("RESULTS:");
				if (fullMatch != null)
				{
					Console.WriteLine(fullMatch);
					Console.WriteLine();
				}

				foreach (string result in results)
				{
					Console.WriteLine(result);
				}
			}
			else
			{
				Console.WriteLine("No matches were found.");
				Console.WriteLine("The flags you're looking for are not here.");
			}
		}

		/// <summary>
		/// Attempt to save the flags loaded in so far.
		/// </summary>
		private static void SaveFlags()
		{
			while (true)
			{
				Console.WriteLine();
				Console.WriteLine("Would you like to save the flags to a file? (yY/nN)");
				string response = Console.ReadLine();
				if (response.ToLower() == "y")
				{
					Console.WriteLine();
					Console.WriteLine("If the file already exists the new flags will be appended to it.");
					Console.WriteLine("Please enter the path and name of the file to save to: ");
					string newFlagFilePath = Console.ReadLine();
					if (newFlagFilePath != null && newFlagFilePath != "")
					{
						using (StreamWriter sw = new StreamWriter(newFlagFilePath, true))
						{
							foreach (string key in newFlags.Keys)
							{
								int flagCode = newFlags[key];
								sw.WriteLine(key);
								sw.WriteLine("0x" + flagCode.ToString("X"));
							}
						}
						break;
					}
					else
					{
						Console.WriteLine("Please choose a valid path/name for the file!");
					}
				}
				else if (response.ToLower() == "n")
				{
					break;
				}
				else
				{
					Console.WriteLine("Please enter a valid choice!");
				}
			}
		}

		/// <summary>
		/// Filter entries from the given dataToFilter using RegEx and return them as a list of strings.
		/// </summary>
		/// <param name="dataToFilter">The data to filter.</param>
		/// <returns></returns>
		private static List<string> RegExFilter(string dataToFilter)
		{
			List<string> finalMatches = new List<string>();
			while (true)
			{
				Console.WriteLine();
				Console.WriteLine("Press enter without entering a value to stop filtering.");
				Console.WriteLine("Please enter the RegEx string used to find all the flags in the document:");
				string regExStr = Console.ReadLine();
				if (regExStr == null)
				{
					Console.WriteLine("Please enter a RegEx string!");
					continue;
				}
				else if (regExStr == "")
				{
					break;
				}

				Regex regex = new Regex(regExStr);
				MatchCollection matches = regex.Matches(dataToFilter);
				dataToFilter = "";
				finalMatches.Clear();
				foreach (Match match in matches)
				{
					dataToFilter += match.Value + "\n";
					finalMatches.Add(match.Value);
				}

				Console.WriteLine();
				Console.WriteLine(finalMatches.Count + " entries found!");
				Console.WriteLine("Here is a sample: ");
				for(int i = 0; i < 5; i++)
				{
					if(i < finalMatches.Count)
					{
						Console.WriteLine(i + " - " + finalMatches[i]);
					}
					else
					{
						break;
					}
				}
			}

			return finalMatches;
		}

		/// <summary>
		/// Remove temp.cpp, temp.exe, and temp.obj if the files exist.
		/// </summary>
		/// <param name="workingDirectory">The directory these files exist in.</param>
		private static void Cleanup(string workingDirectory)
		{
			if (File.Exists(workingDirectory + "\\temp.cpp"))
			{
				File.Delete(workingDirectory + "\\temp.cpp");
			}
			if(File.Exists(workingDirectory + "\\temp.exe"))
			{
				File.Delete(workingDirectory + "\\temp.exe");
			}
			if(File.Exists(workingDirectory + "\\temp.obj"))
			{
				File.Delete(workingDirectory + "\\temp.obj");
			}
		}
	}
}