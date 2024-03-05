using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
			NominativePlural = "Причины отмены онлайн заказа",
			Nominative = "Причина отмены онлайн заказа",
			Prepositional = "Причине отмены онлайн заказа",
			PrepositionalPlural = "Причинах отмены онлайн заказа"
		)
	]
	[HistoryTrace]
	public class OnlineOrderCancellationReason : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private bool _isArchive;

		public virtual int Id { get; set; }

		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
	}
}
