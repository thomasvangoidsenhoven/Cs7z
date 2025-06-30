// See https://aka.ms/new-console-template for more information

using Cs7z.Core;
using Cs7z.Service.OmniPlatform;

Console.WriteLine("Hello, World!");
const string testArchivePath = "bin.zip";

var sourceLocation = new OmniPlatformSevenZipExecutableSource();
var archive = new SevenZipArchive(sourceLocation);
var listResult = await archive.ListArchive(testArchivePath);
await archive.ExtractToDirectoryAsync(testArchivePath, "tmp");


Console.WriteLine("done");


