# Windows Flag Decoder
A tool created to assist with the decoding of Windows flags using bitwise AND (&amp;) operation.

# IMPORTANT
Do note that this tool is not perfect.

Due to the destructible nature of the bitwise OR (|) operation used to combine Windows flags there is no way to 100% guarantee a flag was used to create another.

However, the results from this tool should significantly narrow down potential candidates such that any relevant ones can be selected.

# Flag Decoding
The tool will take a flag code from the user and attempt to decode it.

It does this by taking the flag to decode and bitwise AND'ing (&) it with a list of Windows flags.

This list is expected to be a file in a specific format, the FlagFile format further described below.

The application allows for a pre-existing FlagFile to be used or for another file to be skimmed for flag names and then saved as a FlagFile.

The flags that may potentially be a part of the inputted flag are then presented to the user.

## FlagFile Format
The format used to interpret Windows flags is refered to as the FlagFile format.

It places a Windows flag name on one line and the corresponding code on the next.

It does this on repeat for all the flags.

The codes may be 0x prepended or not. The 0x is removed by the program and is optional but allowed.

The codes are expected to all be in hexadecimal.

The FlagFile is in plain text and free editing of the file is allowed, even encouraged to save time in case small amounts of flags are to be considered.

## Skimming other files
The application allows for other files to be skimmed in case a FlagFile does not yet exist for use.

This works by taking the file and allowing the user to enter a RegEx string to filter out all flag names.

These flag names are then added to a temporary .cpp file in such a way that it prints the results in the FlagFile format.
- The format is std::cout << "flagName\n" << std::hex << flagName << std::endl;
- This is so it corresponds with the expected FlagFile format once the output the C++ executable file make is read and interpreted.
	
This .cpp file is then compiled (using Visual Studio compilation with help of vcvars64.bat and cl.exe) and executed.

The results printed (which are the flag names and codes in FlagFile format) by this process are then retrieved by the Windows Flag Decoder process.

The user can then choose to do this for more files, or save the current file for later use.
