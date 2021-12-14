using System;
using System.Threading;
using NLog;

namespace Fias.Service.Loaders
{
	public abstract class FiasDataLoader
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

		protected FiasDataLoader(IFiasService fiasService)
		{
			Fias = fiasService ?? throw new ArgumentNullException(nameof(fiasService));
		}

		protected IFiasService Fias { get; }

		protected abstract void CancelLoading();
	}
}
