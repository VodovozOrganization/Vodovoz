using System;
using System.Threading;
using NLog;

namespace Fias.Service.Loaders
{
	public abstract class FiasDataLoader
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

		protected FiasDataLoader(IFiasApiClient fiasApiClient)
		{
			Fias = fiasApiClient ?? throw new ArgumentNullException(nameof(fiasApiClient));
		}

		protected IFiasApiClient Fias { get; }

		protected abstract void CancelLoading();
	}
}
