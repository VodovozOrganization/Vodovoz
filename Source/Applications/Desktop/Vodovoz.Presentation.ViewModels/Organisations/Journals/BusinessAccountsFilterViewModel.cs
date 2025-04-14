using System;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Presentation.ViewModels.Organisations.Journals
{
	/// <summary>
	/// Вью модель фильтра журнала расчетных счетов банковских выписок
	/// </summary>
	public class BusinessAccountsFilterViewModel : FilterViewModelBase<BusinessAccountsFilterViewModel>
	{
		private readonly ViewModelEEVMBuilder<Funds> _fundsViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<BusinessActivity> _businessActivityViewModelEEVMBuilder;
		private bool _showArchived;
		private string _number;
		private string _name;
		private string _bank;
		private Funds _funds;
		private BusinessActivity _businessActivity;
		private AccountFillType? _accountFillType;
		private DialogViewModelBase _journalTab;

		public BusinessAccountsFilterViewModel(
			ViewModelEEVMBuilder<Funds> fundsViewModelEEVMBuilder,
			ViewModelEEVMBuilder<BusinessActivity> businessActivityViewModelEEVMBuilder
		)
		{
			_fundsViewModelEEVMBuilder = fundsViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(fundsViewModelEEVMBuilder));
			_businessActivityViewModelEEVMBuilder =
				businessActivityViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(businessActivityViewModelEEVMBuilder));
		}

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
			set => UpdateFilterField(ref _businessActivity, value);
		}

		/// <summary>
		/// Форма денежных средств
		/// </summary>
		public virtual Funds Funds
		{
			get => _funds;
			set => UpdateFilterField(ref _funds, value);
		}

		/// <summary>
		/// Тип заполнения данных
		/// </summary>
		public virtual AccountFillType? AccountFillType
		{
			get => _accountFillType;
			set => UpdateFilterField(ref _accountFillType, value);
		}

		public IEntityEntryViewModel FundsViewModel { get; private set; }
		public IEntityEntryViewModel BusinessActivityViewModel { get; private set; }

		public DialogViewModelBase JournalTab
		{
			get => _journalTab;
			set
			{
				_journalTab = value;
				if(value != null)
				{
					InitializeEntryViewModels();
				}
			}
		}

		private void InitializeEntryViewModels()
		{
			var fundsViewModel = _fundsViewModelEEVMBuilder
				.ForProperty(this, x => x.Funds)
				.SetUnitOfWork(UoW)
				.SetViewModel(_journalTab)
				.UseViewModelDialog<FundsViewModel>()
				.UseViewModelJournalAndAutocompleter<FundsJournalViewModel>()
				.Finish();

			fundsViewModel.CanViewEntity = false;
			FundsViewModel = fundsViewModel;

			var businessActivityViewModel = _businessActivityViewModelEEVMBuilder
				.ForProperty(this, x => x.BusinessActivity)
				.SetUnitOfWork(UoW)
				.SetViewModel(_journalTab)
				.UseViewModelDialog<BusinessActivityViewModel>()
				.UseViewModelJournalAndAutocompleter<BusinessActivitiesJournalViewModel>()
				.Finish();

			businessActivityViewModel.CanViewEntity = false;
			BusinessActivityViewModel = businessActivityViewModel;
		}

		public override void Dispose()
		{
			_journalTab = null;
			base.Dispose();
		}
	}
}
