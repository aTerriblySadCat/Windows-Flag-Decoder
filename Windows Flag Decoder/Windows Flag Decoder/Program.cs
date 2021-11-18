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
		/// The flags, where the key is the name of the flag, and the value is the code.
		/// </summary>
		private static Dictionary<string, int> flags = new Dictionary<string, int>();
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
				try
				{
					flagToDecode = HexStrToInt(flagToDecodeStr);
				}
				catch(Exception exc)
				{
					Console.WriteLine("Invalid flag code <" + flagToDecodeStr + "> used!");
					return;
				}
			}

			Console.WriteLine();
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
				while (true)
				{
					Console.WriteLine();
					Console.WriteLine("Press enter without entering a value if you're done with adding flags.");
					Console.Write("Please enter the name of the file where the desired flag names are located: ");
					string flagFilePath = Console.ReadLine();
					if(flagFilePath == null || flagFilePath == "")
					{
						break;
					}

					try
					{
						ReadNewFile(flagFilePath);
					}
					catch (Exception exc)
					{
						Console.WriteLine(exc.Message);
						return;
					}

					while (true) {
						Console.WriteLine();
						Console.WriteLine("Would you like to save the flags to a file? (yY/nN)");
						string response = Console.ReadLine();
						if (response.ToLower() == "y")
						{
							Console.WriteLine();
							Console.WriteLine("If the file already exists the new flags will be appended to it.");
							Console.Write("Please enter the path and name of the file to save to: ");
							string newFlagFilePath = Console.ReadLine();
							if (newFlagFilePath != null && newFlagFilePath != "")
							{
								using (StreamWriter sw = new StreamWriter(newFlagFilePath, true))
								{
									foreach (string key in flags.Keys)
									{
										int flagCode = flags[key];
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
			}
			else
			{
				Console.WriteLine("Please choose a valid number!");
				return;
			}

			// Now that we've got the flags, do the execution
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

			return;
		}

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
					throw exc;
				}
			}

			return 0;
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
							if (!flags.ContainsKey(flagName))
							{
								if (line != null)
								{
									try
									{
										int flagCode = HexStrToInt(line);
										flags.Add(flagName, flagCode);
									}
									catch (Exception exc)
									{
										throw new Exception("Invalid flag code <" + line + "> found!\n" + exc.Message);
									}
								}
								else
								{
									throw new Exception("Flag without code <" + flagName + "> found!");
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

		/// <summary>
		/// Reads, filters, and saves the file at the given newFileName path as a FlagFile. Throws an Exception on Error.
		/// </summary>
		/// <param name="newFileName">The path to the file to read, and the name of the file.</param>
		/// <exception cref="Exception"></exception>
		private static void ReadNewFile(string newFileName)
		{
			if(File.Exists(newFileName))
			{
				string wholeFileStr = File.ReadAllText(newFileName);
				if(wholeFileStr.Length <= 0) 
				{
					throw new Exception("The file at <" + newFileName + "> was empty!");
				}

				List<string> finalMatches = new List<string>();
				while (true)
				{
					Console.WriteLine();
					Console.WriteLine("Press enter without entering a value to stop filtering.");
					Console.WriteLine("Please enter the RegEx string used to find all the flags in the document:");
					string regExStr = Console.ReadLine();
					if (regExStr == null)
					{
						throw new Exception("Please enter a RegEx string!");
					}
					else if(regExStr == "")
					{
						break;
					}

					Regex regex = new Regex(regExStr);
					MatchCollection matches = regex.Matches(wholeFileStr);
					wholeFileStr = "";
					finalMatches.Clear();
					foreach (Match match in matches)
					{
						wholeFileStr += match.Value + "\n";
						finalMatches.Add(match.Value);
					}
				}

				// Create the CPP file here
				string workingDirectory = Directory.GetCurrentDirectory();
				CreateCppFile(finalMatches, workingDirectory);

				// Compile cppFile
				if(File.Exists(workingDirectory + "\\temp.exe"))
				{
					File.Delete(workingDirectory + "\\temp.exe");
					if(File.Exists(workingDirectory + "\\temp.obj"))
					{
						File.Delete(workingDirectory + "\\temp.obj");
					}
				}
				AttemptCompile(workingDirectory);

				// Execute cppFile
				ProcessStartInfo psi2 = new ProcessStartInfo();
				psi2.FileName = workingDirectory + "\\temp.exe";
				while (true)
				{
					if (!File.Exists(psi2.FileName))
					{
						Console.WriteLine();
						Console.WriteLine("Could not find <" + psi2.FileName + ">!");
						Console.WriteLine("This could indicate there were compiler errors. Would you like to remove the problematic flags? (yY/nN)");
						string response = Console.ReadLine();
						if (response.ToLower() == "y")
						{
							List<int> excludeLines = new List<int>();
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

							CreateCppFile(finalMatches, workingDirectory, excludeLines);
							AttemptCompile(workingDirectory);
						}
						else if(response.ToLower() == "n")
						{
							throw new Exception("Could not find <" + psi2.FileName + ">!");
						}
						else
						{
							Console.WriteLine("Please pick a valid option! (yY/nN)");
						}
					}
					else
					{
						break;
					}
				}

				if (File.Exists(workingDirectory + "\\temp.cpp"))
				{
					File.Delete(workingDirectory + "\\temp.cpp");
				}

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
							if (!flags.ContainsKey(flagName))
							{
								try
								{
									int flagCode = HexStrToInt(flagCodeStr);
									flags.Add(flagName, flagCode);
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

				if (File.Exists(workingDirectory + "\\temp.exe"))
				{
					File.Delete(workingDirectory + "\\temp.exe");
				}
				if (File.Exists(workingDirectory + "\\temp.obj"))
				{
					File.Delete(workingDirectory + "\\temp.obj");
				}
			}
			else
			{
				throw new Exception("The file <" + newFileName + "> does not exist!");
			}
		}

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

		private static void CreateCppFile(List<string> finalMatches, string workingDirectory, List<int> excludeLines = null)
		{
			string cppFileStr = "#include <iostream>\n";
			cppFileStr += "#include <Windows.h>\n";

			Console.WriteLine();
			Console.WriteLine("Press enter without a value to stop adding headers.");
			Console.WriteLine("Some files may require additional header (particularly non-Windows files) in order to be dynamically retrieved. Add them here.");
			while(true)
			{
				string headerToAdd = Console.ReadLine();
				if(headerToAdd == null || headerToAdd == "")
				{
					break;
				}
				cppFileStr += "#include <" + headerToAdd + ">\n";
			}

			cppFileStr += "int main()\n";
			cppFileStr += "{\n";
			int lineCount = 4;
			foreach (string flagName in finalMatches)
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
	}
}