using QS.Project.Filter;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Presentation.ViewModels.Organisations.Journals
{
	/// <summary>
	/// Вью модель фильтра журнала расчетных счетов банковских выписок
	/// </summary>
	public class BusinessAccountsFilterViewModel : FilterViewModelBase<BusinessAccountsFilterViewModel>
	{
		private bool _showArchived;
		private string _number;
		private string _name;
		private string _bank;
		private Funds _funds;
		private BusinessActivity _businessActivity;
		private AccountFillType _accountFillType;

		/// <summary>
		/// Показывать архивные
		/// </summary>
		public bool ShowArchived
		{
			get => _showArchived;
			set => UpdateFilterField(ref _showArchived, value);
		}
		
		/// <summary>
		/// Название
		/// </summary>
		public string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		/// <summary>
		/// Номер р/сч
		/// </summary>
		public string Number
		{
			get => _number;
			set => SetField(ref _number, value);
		}
		
		/// <summary>
		/// Банк
		/// </summary>
		public string Bank
		{
			get => _bank;
			set => SetField(ref _bank, value);
		}
		
		/// <summary>
		/// Направление деятельности
		/// </summary>
		public virtual BusinessActivity BusinessActivity
		{
			get => _businessActivity;
			set => SetField(ref _businessActivity, value);
		}
		
		/// <summary>
		/// Форма денежных средств
		/// </summary>
		public virtual Funds Funds
		{
			get => _funds;
			set => SetField(ref _funds, value);
		}
		
		/// <summary>
		/// Тип заполнения данных
		/// </summary>
		public virtual AccountFillType AccountFillType
		{
			get => _accountFillType;
			set => SetField(ref _accountFillType, value);
		}
	}
}
