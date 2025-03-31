using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception
{
	public class EdoProblemException : System.Exception
	{
		public EdoProblemException(System.Exception ex) 
			: base("Возникло исключение при выполнении ЭДО процесса", ex)
		{
		}

		public EdoProblemException(System.Exception ex, IEnumerable<EdoTaskItem> taskItems)
			: base("Возникло исключение при выполнении ЭДО процесса", ex)
		{
			ProblemItems = taskItems ?? throw new ArgumentNullException(nameof(taskItems));
		}

		public EdoProblemException(System.Exception ex, IEnumerable<EdoProblemCustomItem> customItems)
			: base("Возникло исключение при выполнении ЭДО процесса", ex)
		{
			CustomItems = customItems ?? throw new ArgumentNullException(nameof(customItems));
		}

		public IEnumerable<EdoTaskItem> ProblemItems { get; }

		public IEnumerable<EdoProblemCustomItem> CustomItems { get; set; }
	}

	
}
