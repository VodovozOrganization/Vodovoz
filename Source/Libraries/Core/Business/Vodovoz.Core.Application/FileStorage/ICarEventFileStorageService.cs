using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Core.Application.FileStorage
{
	public interface ICarEventFileStorageService : IFileStorageService
	{
		new Task<Result> CreateFileAsync(string fileName, Stream inputStream, CancellationToken cancellationToken);
	}
}
