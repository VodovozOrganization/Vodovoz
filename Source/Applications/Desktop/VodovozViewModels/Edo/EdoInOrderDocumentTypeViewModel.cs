using QS.ViewModels;
using Gamma.Utilities;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderDocumentTypeViewModel : ViewModelBase
	{
		public int Quantity { get; set; }
		public string Title { get; }
		public EdoInOrderDocumentGroupType DocumentGroupType { get; }

		public EdoInOrderDocumentTypeViewModel(EdoInOrderDocumentGroupType documentGroupType)
		{
			DocumentGroupType = documentGroupType;
			Title = documentGroupType.GetEnumTitle();
		}

	}
}
