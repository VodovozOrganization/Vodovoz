using Gamma.Utilities;
using QS.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderProblemViewModel : ViewModelBase
	{
		public EdoInOrderProblemViewModel(EdoInOrderProblemNode problemNode)
		{
			ProblemNode = problemNode ?? throw new ArgumentNullException(nameof(problemNode));
			CreationTime = ProblemNode.Time.ToString("dd.MM.yyyy HH:mm");
			State = ProblemNode.State.GetEnumTitle();
			Message = ProblemNode.Message;
			Description = ProblemNode.Description;
			Recomendation = ProblemNode.Recommendation;
			ProblemItems = ProblemNode.ProblemItems.ToList();
		}

		public EdoInOrderProblemNode ProblemNode { get; }

		public string CreationTime { get; }
		public string State { get; }
		public string Message { get; }
		public string Description { get; }
		public string Recomendation { get; }
		public IList<string> ProblemItems { get; }
	}
}
