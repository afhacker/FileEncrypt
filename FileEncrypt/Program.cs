if (args[0].Equals("encrypt", StringComparison.OrdinalIgnoreCase) && args.Length == 2)
{
    var aes = Aes.Create();

    var transform = aes.CreateEncryptor();

    var fileInfo = new FileInfo(args[1]);

    var outDir = Path.GetDirectoryName(fileInfo.FullName);

    if (string.IsNullOrWhiteSpace(outDir))
    {
        Console.WriteLine("Invalid output directory");

        return;
    }

    var outFile = Path.Combine(outDir, $"{fileInfo.Name}.encrypted");

    using var outFs = new FileStream(outFile, FileMode.Create);

    using var outStreamEncrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write);

    int count = 0;
    int offset = 0;

    // blockSizeBytes can be any arbitrary size.
    int blockSizeBytes = aes.BlockSize / 8;
    byte[] data = new byte[blockSizeBytes];
    int bytesRead = 0;

    using var file = new FileStream(fileInfo.FullName, FileMode.Open);

    do
    {
        count = file.Read(data, 0, blockSizeBytes);
        offset += count;
        outStreamEncrypted.Write(data, 0, count);
        bytesRead += blockSizeBytes;
    } while (count > 0);

    outStreamEncrypted.FlushFinalBlock();

    var stringBuilder = new StringBuilder();

    stringBuilder.AppendLine($"Key: {Convert.ToBase64String(aes.Key)}");
    stringBuilder.AppendLine($"IV: {Convert.ToBase64String(aes.IV)}");

    var keyFile = Path.Combine(outDir, $"{fileInfo.Name}.encrypted.key");

    File.WriteAllText(keyFile, stringBuilder.ToString());
}
else if (args[0].Equals("decrypt", StringComparison.OrdinalIgnoreCase) && args.Length == 4)
{
    var aes = Aes.Create();

    var fileInfo = new FileInfo(args[1]);

    var outDir = Path.GetDirectoryName(fileInfo.FullName);

    if (string.IsNullOrWhiteSpace(outDir))
    {
        Console.WriteLine("Invalid output directory");

        return;
    }

    var outFile = Path.Combine(outDir, fileInfo.Name.Replace(".encrypted", string.Empty));

    var key = Convert.FromBase64String(args[2]);
    var iv = Convert.FromBase64String(args[3]);

    using var file = new FileStream(fileInfo.FullName, FileMode.Open);

    var transform = aes.CreateDecryptor(key, iv);
    
    using var outFs = new FileStream(outFile, FileMode.Create);

    int count = 0;
    int offset = 0;

    int blockSizeBytes = aes.BlockSize / 8;
    byte[] data = new byte[blockSizeBytes];

    using var outStreamDecrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write);

    do
    {
        count = file.Read(data, 0, blockSizeBytes);
        offset += count;
        outStreamDecrypted.Write(data, 0, count);
    } while (count > 0);

    outStreamDecrypted.FlushFinalBlock();
}
else
{
    Console.WriteLine("Invalid parameters");
}
