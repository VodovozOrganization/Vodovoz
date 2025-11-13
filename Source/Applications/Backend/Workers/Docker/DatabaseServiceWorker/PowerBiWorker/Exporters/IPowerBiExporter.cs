using System.Threading;
using System.Threading.Tasks;

namespace DatabaseServiceWorker.PowerBiWorker.Exporters
{
	public interface IPowerBiExporter
	{
		public Task Export(CancellationToken cancellationToken);
	}
}
