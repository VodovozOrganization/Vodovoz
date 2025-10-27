using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Контрагенты, платежи от которых не подпадают под распределение
	/// </summary>
	[EntityPermission]
	[HistoryTrace]
	public class NotAllocatedCounterparty : PropertyChangedBase, IDomainObject
	{
		private string _inn;
		private bool _isArchive;
		private ProfitCategory _profitCategory;

		public virtual int Id { get; set; }

		/// <summary>
		/// Инн контрагента
		/// </summary>
		[Display(Name = "Инн контрагента")]
		public string Inn
		{
			get => _inn;
			set => SetField(ref _inn, value);
		}
		
		/// <summary>
		/// Инн контрагента
		/// </summary>
		[Display(Name = "Архивный")]
		public bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
		
		/// <summary>
		/// Категория прихода, выставляемая в функционале загрузки платежей по умолчанию
		/// </summary>
		[Display(Name = "Категория прихода")]
		public ProfitCategory ProfitCategory
		{
			get => _profitCategory;
			set => SetField(ref _profitCategory, value);
		}
	}
}
