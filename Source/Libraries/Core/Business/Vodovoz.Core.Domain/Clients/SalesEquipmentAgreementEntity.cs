using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения продажи оборудования",
		Nominative = "доп. соглашение продажи оборудования")]
	[HistoryTrace]
	[EntityPermission]
	public class SalesEquipmentAgreementEntity : AdditionalAgreementEntity
	{
		private IObservableList<SalesEquipmentEntity> _salesEqipments = new ObservableList<SalesEquipmentEntity>();

		/// <summary>
		/// Оборудование на продажу
		/// </summary>
		[Display(Name = "Оборудование на продажу")]
		public virtual IObservableList<SalesEquipmentEntity> SalesEqipments
		{
			get => _salesEqipments;
			set => SetField(ref _salesEqipments, value);
		}
	}
}
