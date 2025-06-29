using ImageSorter.ResultWriting.Abstractions.Model;

namespace ImageSorter.ResultWriting.Abstractions;

public interface IResultWriter
{
    Task<ICollection<FileWriteResultDto>> Write(ICollection<FileWriteDto> writeDtos, CancellationToken cancellationToken);
}