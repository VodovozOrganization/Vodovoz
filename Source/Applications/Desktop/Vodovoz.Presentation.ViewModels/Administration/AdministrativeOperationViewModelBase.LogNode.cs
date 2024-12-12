using Microsoft.Extensions.Logging;
using System;

namespace Vodovoz.Presentation.ViewModels.Administration
{
	public abstract partial class AdministrativeOperationViewModelBase
	{
		public class LogNode
		{
			public DateTime DateTime { get; set; }
			public LogLevel LogLevel { get; set; }
			public string Message { get; set; }
		}
	}
}
