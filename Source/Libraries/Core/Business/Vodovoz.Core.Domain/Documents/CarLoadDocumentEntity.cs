using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "талоны погрузки автомобилей",
		Nominative = "талон погрузки автомобиля")]
	[EntityPermission]
	[HistoryTrace]
	public class CarLoadDocumentEntity : PropertyChangedBase, IDomainObject
	{
		private CarLoadDocumentLoadOperationState _loadOperationState;

		public virtual int Id { get; set; }

		[Display(Name = "Статус талона погрузки")]
		public virtual CarLoadDocumentLoadOperationState LoadOperationState
		{
			get => _loadOperationState;
			set => SetField(ref _loadOperationState, value);
		}
	}
}
