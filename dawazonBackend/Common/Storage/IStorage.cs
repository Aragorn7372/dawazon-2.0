using CSharpFunctionalExtensions;
using dawazonBackend.Products.Errors;

namespace dawazonBackend.Common.Storage;

public interface IStorage
{
    Task<Result<string, ProductError>> SaveFileAsync(IFormFile file, string folder);

    Task<Result<bool, ProductError>> DeleteFileAsync(string filename);

    bool FileExists(string filename);

    string GetFullPath(string filename);
    
    string GetRelativePath(string filename, string folder = "productos");
}