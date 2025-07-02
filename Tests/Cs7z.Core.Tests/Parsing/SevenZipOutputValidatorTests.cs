using System;
using System.IO;
using Cs7z.Core.Parsing;
using Xunit;

namespace Cs7z.Core.Tests.Parsing;

public class SevenZipOutputValidatorTests
{
    [Fact]
    public void ValidateOutput_WithValidOutput_DoesNotThrow()
    {
        const string output = "Valid output from 7-Zip";
        const string error = "";
        
        var exception = Record.Exception(() => SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Null(exception);
    }
    
    [Fact]
    public void ValidateOutput_WithEmptyOutput_ThrowsInvalidOperationException()
    {
        const string output = "";
        const string error = "";
        
        var exception = Assert.Throws<InvalidOperationException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("produced no output", exception.Message);
    }
    
    [Fact]
    public void ValidateOutput_WithWhitespaceOutput_ThrowsInvalidOperationException()
    {
        const string output = "   \n\t  ";
        const string error = "";
        
        var exception = Assert.Throws<InvalidOperationException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("produced no output", exception.Message);
    }
    
    [Fact]
    public void ValidateOutput_WithErrorInStderr_ThrowsInvalidOperationException()
    {
        const string output = "Some output";
        const string error = "ERROR: Something went wrong";
        
        var exception = Assert.Throws<InvalidOperationException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("7-Zip reported errors", exception.Message);
        Assert.Contains("Something went wrong", exception.Message);
    }
    
    [Fact]
    public void ValidateOutput_WithFatalInStderr_ThrowsInvalidOperationException()
    {
        const string output = "Some output";
        const string error = "FATAL: Critical error occurred";
        
        var exception = Assert.Throws<InvalidOperationException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("7-Zip reported errors", exception.Message);
        Assert.Contains("Critical error occurred", exception.Message);
    }
    
    [Fact]
    public void ValidateOutput_WithCannotOpenError_ThrowsFileNotFoundException()
    {
        const string output = "Error: Can not open file as archive";
        const string error = "";
        
        var exception = Assert.Throws<FileNotFoundException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("could not open the archive file", exception.Message);
    }
    
    [Theory]
    [InlineData("Error: Can not open the file")]
    [InlineData("ERROR: CAN NOT OPEN ARCHIVE")]
    [InlineData("error: archive can not open")]
    public void ValidateOutput_WithVariousCannotOpenErrors_ThrowsFileNotFoundException(string output)
    {
        const string error = "";
        
        var exception = Assert.Throws<FileNotFoundException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("could not open the archive file", exception.Message);
    }
    
    [Fact]
    public void ValidateOutput_WithWrongPasswordError_ThrowsUnauthorizedAccessException()
    {
        const string output = "Error: Wrong password";
        const string error = "";
        
        var exception = Assert.Throws<UnauthorizedAccessException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("password", exception.Message);
    }
    
    [Theory]
    [InlineData("Error: Unsupported method")]
    [InlineData("Error: Unsupported archive format")]
    [InlineData("ERROR: UNSUPPORTED ARCHIVE")]
    public void ValidateOutput_WithUnsupportedErrors_ThrowsNotSupportedException(string output)
    {
        const string error = "";
        
        var exception = Assert.Throws<NotSupportedException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("not supported", exception.Message);
    }
    
    [Theory]
    [InlineData("Error: CRC failed")]
    [InlineData("Error: Data error in archive")]
    [InlineData("ERROR: DATA ERROR")]
    public void ValidateOutput_WithDataErrors_ThrowsInvalidDataException(string output)
    {
        const string error = "";
        
        var exception = Assert.Throws<InvalidDataException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("corrupted", exception.Message);
    }
    
    [Theory]
    [InlineData("Error: Disk full")]
    [InlineData("Error: Not enough space on disk")]
    [InlineData("ERROR: NOT ENOUGH SPACE")]
    public void ValidateOutput_WithDiskSpaceErrors_ThrowsIOException(string output)
    {
        const string error = "";
        
        var exception = Assert.Throws<IOException>(() => 
            SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Contains("Insufficient disk space", exception.Message);
    }
    
    [Fact]
    public void ValidateOutput_WithNonErrorKeywords_DoesNotThrow()
    {
        const string output = @"Processing archive: test.zip
        
Extracting  error_log.txt
Extracting  passwords.txt
Extracting  unsupported_formats.doc
Everything is Ok

Files: 3
Size: 12345";
        const string error = "";
        
        var exception = Record.Exception(() => SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Null(exception);
    }
    
    [Fact]
    public void ValidateOutput_WithWarningsInStderr_DoesNotThrow()
    {
        const string output = "Valid output from 7-Zip";
        const string error = "WARNING: Minor issue detected";
        
        var exception = Record.Exception(() => SevenZipOutputValidator.ValidateOutput(output, error));
        
        Assert.Null(exception);
    }
}