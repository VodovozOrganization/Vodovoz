using Gamma.Utilities;
using QS.ViewModels;
using System;
using Vodovoz.Core.Data.Repositories;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderTaxcomDocflowViewModel : ViewModelBase
	{
		public EdoInOrderTaxcomDocflowViewModel(EdoInOrderTaxcomDocflowNode docflowNode)
		{
			DocflowNode = docflowNode ?? throw new ArgumentNullException(nameof(docflowNode));

			SendTime = docflowNode.TaxcomDocflowSendTime?.ToString("dd.MM.yyyy HH:mm");
			TaxcomDocflowId = docflowNode.TaxcomDocflowId?.ToString();
			Status = docflowNode.TaxcomStatus?.GetEnumTitle();
			LastUpdateTime = docflowNode.LastTaxcomStatusUpdateTime?.ToString("dd.MM.yyyy HH:mm");
			TrueMarkTraceabilityStatus = docflowNode.TaxcomTraceabilityStatus?.GetEnumTitle();
			ErrorMessage = docflowNode.TaxcomErrorMessage;
		}

		public EdoInOrderTaxcomDocflowNode DocflowNode { get; }

		public string SendTime { get; set; }
		public string TaxcomDocflowId { get; set; }
		public string Status { get; set; }
		public string LastUpdateTime{ get; set; }
		public string TrueMarkTraceabilityStatus { get; set; }
		public string ErrorMessage { get; set; }
	}
}
