using System.Globalization;
using System.Security.Cryptography;

public static class FilePatcher
{
    private static bool MatchFileHash(string filePath, string expectedSha256HexUpper)
    {
        using (SHA256 sha = SHA256.Create())
        using (FileStream fs = File.OpenRead(filePath))
        {

            byte[] calculatedHash = sha.ComputeHash(fs);
            string calculatedHashStr = Convert.ToHexString(calculatedHash).ToUpperInvariant();

            Console.WriteLine("\nExpected Hash: " + expectedSha256HexUpper);
            Console.WriteLine("Computed Hash: " + calculatedHashStr);

            if (calculatedHashStr != expectedSha256HexUpper)
                return false;
        }

        return true;
    }

    private static (bool success, bool wasModified) PatchBytes(string filePath, long fileOffset, byte[] expectedOriginalBytes, byte[] targetPatchBytes)
    {
        using (FileStream fs = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            if (fileOffset < 0 || fileOffset + expectedOriginalBytes.Length > fs.Length)
            {
                Console.WriteLine("There was an issue while reading the file and verification failed! (The offsets didnt match...how? why?");
                return (false, false);
            }

            fs.Seek(fileOffset, SeekOrigin.Begin);

            byte[] currentBytes = new byte[expectedOriginalBytes.Length];
            int readBytes = fs.Read(currentBytes, 0, currentBytes.Length);

            if (readBytes != currentBytes.Length)
            {
                Console.WriteLine("There was an issue while reading the file! Internal file reader error!");
                return (false, false);
            }

            // Verify original bytes
            if (expectedOriginalBytes != null)
            {
                bool preVerificationSuccess = true;
                for (int i = 0; i < expectedOriginalBytes.Length; i++)
                {
                    if (currentBytes[i] != expectedOriginalBytes[i])
                        preVerificationSuccess = false;
                }

                if (!preVerificationSuccess)
                {
                    Console.WriteLine($"Preverification failed! Expected bytes at file offset 0x{fileOffset:X2}: " + string.Join(',', expectedOriginalBytes.Select((byteVal) => byteVal.ToString("X2"))) + "...Actual bytes found: " + string.Join(',', currentBytes.Select(byteVal => byteVal.ToString("X2"))) + "...Probably the game has been updated/changed!");
                    return (false, false);
                }
            }
            else Console.WriteLine("There was an internal error!");

            Console.WriteLine("\nPre-verification succeeded.");
        }
        
        Console.WriteLine("Creating backup file of the original 'GameAssembly.dll'.");

        string bak = filePath + ".DONT_DELETE.bak";

        bool backupAlreadyExists = File.Exists(bak);
            
        if (!backupAlreadyExists)
        {
            File.Copy(filePath, bak);
            Console.WriteLine("Created a backup of the original GameAssembly.dll. You can use this backup to restore the original (by removing ''.DONT_DELETE.bak'' part from the GameAssembly.dll.DONT_DELETE.bak file. Make sure file extensions are visible in your file explorer).");
        }
        else Console.WriteLine("Backup of the original GameAssembly.dll already exists. You can use this backup to restore the original (by removing ''.DONT_DELETE.bak'' part from the GameAssembly.dll.DONT_DELETE.bak file. Make sure file extensions are visible in your file explorer).");
            
        Console.WriteLine("\n\nNow Patching!");

        using (FileStream fs2 = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            try
            {
                fs2.Seek(fileOffset, SeekOrigin.Begin);
                fs2.Write(targetPatchBytes, 0, targetPatchBytes.Length);
                fs2.Flush(true); // Ensure write to disk!
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an issue while patching!\n\n\nError: " + e.Message);
                return (false, true);
            }
        }

        Console.WriteLine($"Wrote {targetPatchBytes.Length} bytes at file offset 0x{fileOffset:X2}. Original bytes were: {string.Join(',', expectedOriginalBytes.Select((byteVal) => byteVal.ToString("X2")))}, new bytes written: {string.Join(',', targetPatchBytes.Select(byteVal => byteVal.ToString("X2")))}");

        Console.WriteLine("Doing post verification.");
        // Post patch verification
        using (var fs3 = File.OpenRead(filePath))
        {
            fs3.Seek(fileOffset, SeekOrigin.Begin);
            byte[] patchedBytes = new byte[targetPatchBytes.Length];

            fs3.ReadExactly(patchedBytes);

            for (int i = 0; i < patchedBytes.Length; i++)
            {
                if (patchedBytes[i] != targetPatchBytes[i])
                {
                    Console.WriteLine("Post-write verification failed! The patch was unsuccessful!");
                    return (false, true);
                }
            }
        }

        Console.WriteLine($"Post verification successful.");
        return (true, true);
    }

    private static byte[] GetFloatBytes(float f)
    {
        var b = BitConverter.GetBytes(f);
        if (!BitConverter.IsLittleEndian) Array.Reverse(b);
        return b;
    }

    public static void Main()
    {
        string baseDir = AppContext.BaseDirectory;

        List<string> filesInDir = [.. Directory.EnumerateFiles(baseDir)];

        bool foundExe = false;
        bool foundDll = false;

        string dllPath = "";

        foreach (string path in filesInDir)
        {
            if (path.EndsWith("LittleRocketLab.exe"))
                foundExe = true;

            if (path.EndsWith("GameAssembly.dll"))
            {
                foundDll = true;
                dllPath = path;
            }
        }

        if (!foundExe || !foundDll)
        {
            Console.WriteLine("\nPlease place this LRLRelativity.exe (and other LRL files) in the game's folder (where LittleRocketLab.exe and GameAssembly.dll files are present).");
            goto EndReadLine;
        }

        Console.WriteLine("Testing hash of GameAssembly.dll\n");
        bool hashMatched = MatchFileHash(dllPath, "765787245278B503B04E2B754930E72038E125B153B0AF31083235E4116D22C0");

        if (!hashMatched)
        {
            Console.WriteLine("\nHash mismatch! The file provided doesnt match the expected GameAssembly.dll! The file might be different or the game version might be different.");
            goto EndReadLine;
        }
        else Console.WriteLine("\nHashes matched.");

        Console.WriteLine("\nEnter new 'SecondsInHour' value (The amount of seconds in 1 in-game hour. The original value is 55. To double it try 110): ");

        string? line = null;

        try
        {
            line = Console.ReadLine();
        }
        catch (Exception e)
        {
            Console.WriteLine($"There was an error while reading input: \n{line}\n\n\nError:" + e.Message);
            goto EndReadLine;
        }

        bool invalid = false;
        float newValue = 55.0f;
        (bool success, bool modified) patchResult = (false, false);

        if (string.IsNullOrWhiteSpace(line) || !float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out newValue))
            invalid = true;

        if (invalid)
        {
            Console.WriteLine("\nPlease enter a valid float (decimal) number to set the new value of 'SecondsInHour' (for example: 100)! You entered: " + line);
            goto EndReadLine;
        }
        else
        {
            byte[] newValueBytes = GetFloatBytes(newValue);
            patchResult = PatchBytes(dllPath, 0x02AD8280, [0x00, 0x00, 0x5C, 0x42], newValueBytes);
        }

        if (patchResult.success)
            Console.WriteLine("\n\nThe patch was completed successfully!");
        else if (patchResult.modified)
            Console.WriteLine("\n\nThe patch was unsuccessful! It is recommended to delete the game folder and redownload (for safety).");
        else
            Console.WriteLine("\n\nThe patch was unsuccessful! The game files were not modified so it should still work normally!");
        
        EndReadLine:
        Console.ReadLine();
    }
}
