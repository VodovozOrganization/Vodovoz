using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class EdoProblemException : Exception
	{
		public EdoProblemException(Exception ex) 
			: base("Возникло исключение при выполнении ЭДО процесса", ex)
		{
		}

		public EdoProblemException(Exception ex, IEnumerable<EdoTaskItem> taskItems)
			: base("Возникло исключение при выполнении ЭДО процесса", ex)
		{
			ProblemItems = taskItems ?? throw new ArgumentNullException(nameof(taskItems));
		}

		public EdoProblemException(Exception ex, IEnumerable<EdoProblemCustomItem> customItems)
			: base("Возникло исключение при выполнении ЭДО процесса", ex)
		{
			CustomItems = customItems ?? throw new ArgumentNullException(nameof(customItems));
		}

		public IEnumerable<EdoTaskItem> ProblemItems { get; } = new List<EdoTaskItem>();

		public IEnumerable<EdoProblemCustomItem> CustomItems { get; set; } = new List<EdoProblemCustomItem>();
	}

	
}
