using Gamma.Utilities;
using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;

namespace Vodovoz.ViewModels.Edo
{

	public class EdoInOrderDocflowsStageViewModel : WidgetViewModelBase
	{
		private readonly IEnumerable<EdoInOrderTaxcomDocflowNode> _docflows;
		private IList<EdoInOrderTaxcomDocflowViewModel> _taxcomDocflows;
		private EdoInOrderTaxcomDocflowViewModel _selectedTaxcomDocflow;

		public EdoInOrderDocflowsStageViewModel(
			IEnumerable<EdoInOrderTaxcomDocflowNode> docflows
			)
		{
			_docflows = docflows ?? throw new System.ArgumentNullException(nameof(docflows));
			var docflowNode = _docflows.FirstOrDefault();
			if(docflowNode == null)
			{
				return;
			}

			CreationTime = docflowNode.DocflowCreationTime.ToString("dd.MM.yyyy HH:mm");
			Status = docflowNode.DocflowStatus.GetEnumTitle();

			TaxcomDocflows = _docflows
				.Where(x => x.TaxcomDocflowSendTime != null)
				.Select(x => new EdoInOrderTaxcomDocflowViewModel(x)).ToList();
		}

		public string CreationTime { get; }

		public string Status { get; }


		public virtual IList<EdoInOrderTaxcomDocflowViewModel> TaxcomDocflows
		{
			get => _taxcomDocflows;
			set => SetField(ref _taxcomDocflows, value);
		}

		public virtual EdoInOrderTaxcomDocflowViewModel SelectedTaxcomDocflow
		{
			get => _selectedTaxcomDocflow;
			set => SetField(ref _selectedTaxcomDocflow, value);
		}
	}
}
