using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WMS.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    void DeleteFile(string relativePath);
}
