using System;
using System.Linq;
using Cs7z.Archiving.Core.Parsing;
using Xunit;

namespace Cs7z.Archiving.Core.Tests.Parsing;

public class SevenZipOutputParserTests
{
    [Fact]
    public void ParseListOutput_WithEmptyString_ReturnsEmptyResult()
    {
        var result = SevenZipOutputParser.ParseListOutput(string.Empty);
        
        Assert.Empty(result.Files);
        Assert.Equal(0, result.TotalFiles);
        Assert.Equal(0, result.TotalDirectories);
        Assert.Equal(0, result.TotalSize);
        Assert.Equal(0, result.TotalCompressedSize);
    }
    
    [Fact]
    public void ParseListOutput_WithValidFileEntry_ParsesCorrectly()
    {
        const string output = @"7-Zip 19.00 (x64) : Copyright (c) 1999-2018 Igor Pavlov : 2019-02-21

Listing archive: test.zip

--
Path = test.zip
Type = zip

   Date      Time    Attr         Size   Compressed  Name
------------------- ----- ------------ ------------  ------------------------
2023-12-25 14:30:45 ....A         1024          512  folder/file.txt
------------------- ----- ------------ ------------  ------------------------
                                  1024          512  1 files";

        var result = SevenZipOutputParser.ParseListOutput(output);
        
        Assert.Single(result.Files);
        Assert.Equal(1, result.TotalFiles);
        Assert.Equal(0, result.TotalDirectories);
        Assert.Equal(1024, result.TotalSize);
        Assert.Equal(512, result.TotalCompressedSize);
        
        var file = result.Files.First();
        Assert.Equal("folder/file.txt", file.Path);
        Assert.Equal(1024, file.Size);
        Assert.Equal(512, file.CompressedSize);
        Assert.False(file.IsDirectory);
        Assert.Equal("....A", file.Attributes);
        Assert.Equal(new DateTime(2023, 12, 25, 14, 30, 45), file.Modified);
    }
    
    [Fact]
    public void ParseListOutput_WithDirectoryEntry_ParsesCorrectly()
    {
        const string output = @"   Date      Time    Attr         Size   Compressed  Name
------------------- ----- ------------ ------------  ------------------------
2023-12-25 14:30:45 D....            0            0  folder
------------------- ----- ------------ ------------  ------------------------";

        var result = SevenZipOutputParser.ParseListOutput(output);
        
        Assert.Single(result.Files);
        Assert.Equal(0, result.TotalFiles);
        Assert.Equal(1, result.TotalDirectories);
        
        var directory = result.Files.First();
        Assert.Equal("folder", directory.Path);
        Assert.Equal(0, directory.Size);
        Assert.True(directory.IsDirectory);
        Assert.Equal("D....", directory.Attributes);
    }
    
    [Fact]
    public void ParseListOutput_WithMultipleEntries_ParsesAllCorrectly()
    {
        const string output = @"   Date      Time    Attr         Size   Compressed  Name
------------------- ----- ------------ ------------  ------------------------
2023-12-25 14:30:45 D....            0            0  folder
2023-12-25 14:30:45 ....A         1024          512  folder/file1.txt
2023-12-25 14:30:46 .RH..         2048         1024  hidden-file.dat
------------------- ----- ------------ ------------  ------------------------
                                  3072         1536  2 files, 1 folders";

        var result = SevenZipOutputParser.ParseListOutput(output);
        
        Assert.Equal(3, result.Files.Count);
        Assert.Equal(2, result.TotalFiles);
        Assert.Equal(1, result.TotalDirectories);
        Assert.Equal(3072, result.TotalSize);
        Assert.Equal(1536, result.TotalCompressedSize);
    }
    
    [Fact]
    public void ParseListOutput_WithMissingCompressedSize_UsesOriginalSize()
    {
        const string output = @"   Date      Time    Attr         Size   Compressed  Name
------------------- ----- ------------ ------------  ------------------------
2023-12-25 14:30:45 ....A          500              small-file.txt
------------------- ----- ------------ ------------  ------------------------";

        var result = SevenZipOutputParser.ParseListOutput(output);
        
        Assert.Single(result.Files);
        var file = result.Files.First();
        Assert.Equal(500, file.Size);
        Assert.Equal(500, file.CompressedSize); // Should default to original size
    }
    
    [Fact]
    public void ParseListOutput_WithInvalidLines_IgnoresInvalidData()
    {
        const string output = @"   Date      Time    Attr         Size   Compressed  Name
------------------- ----- ------------ ------------  ------------------------
2023-12-25 14:30:45 ....A         1024          512  valid-file.txt
Invalid line that should be ignored
Another invalid line
2023-12-25 14:30:46 ....A         2048         1024  another-valid.txt
------------------- ----- ------------ ------------  ------------------------";

        var result = SevenZipOutputParser.ParseListOutput(output);
        
        Assert.Equal(2, result.Files.Count);
        Assert.Equal(2, result.TotalFiles);
        Assert.Equal(3072, result.TotalSize);
        Assert.Equal(1536, result.TotalCompressedSize);
    }
    
    [Fact]
    public void ParseListOutput_WithSpecialCharactersInPath_ParsesCorrectly()
    {
        const string output = @"   Date      Time    Attr         Size   Compressed  Name
------------------- ----- ------------ ------------  ------------------------
2023-12-25 14:30:45 ....A         1024          512  file with spaces.txt
2023-12-25 14:30:46 ....A         2048         1024  special@chars#file.txt
2023-12-25 14:30:47 ....A          512          256  path/with/multiple/folders/file.txt
------------------- ----- ------------ ------------  ------------------------";

        var result = SevenZipOutputParser.ParseListOutput(output);
        
        Assert.Equal(3, result.Files.Count);
        Assert.Equal("file with spaces.txt", result.Files[0].Path);
        Assert.Equal("special@chars#file.txt", result.Files[1].Path);
        Assert.Equal("path/with/multiple/folders/file.txt", result.Files[2].Path);
    }
    
    [Fact]
    public void ParseListOutput_WithDifferentAttributes_ParsesCorrectly()
    {
        const string output = @"   Date      Time    Attr         Size   Compressed  Name
------------------- ----- ------------ ------------  ------------------------
2023-12-25 14:30:45 RH.A.         1024          512  readonly-hidden.txt
2023-12-25 14:30:46 .S...         2048         1024  system-file.txt
2023-12-25 14:30:47 ..A..          512          256  archive-file.txt
------------------- ----- ------------ ------------  ------------------------";

        var result = SevenZipOutputParser.ParseListOutput(output);
        
        Assert.Equal(3, result.Files.Count);
        Assert.Equal("RH.A.", result.Files[0].Attributes);
        Assert.Equal(".S...", result.Files[1].Attributes);
        Assert.Equal("..A..", result.Files[2].Attributes);
    }
}