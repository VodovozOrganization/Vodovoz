using Autofac;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.FilterViewModels.Suppliers
{
	public class RequestsToSuppliersFilterViewModel : FilterViewModelBase<RequestsToSuppliersFilterViewModel>
	{
		private ILifetimeScope _lifetimeScope;
		private DateTime? _restrictStartDate;
		private DateTime? _restrictEndDate;
		private Nomenclature _restrictNomenclature;
		private RequestStatus? _restrictStatus = RequestStatus.InProcess;
		private DialogViewModelBase _journal;

		public RequestsToSuppliersFilterViewModel(
			ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public virtual DateTime? RestrictStartDate
		{
			get => _restrictStartDate;
			set
			{
				if(UpdateFilterField(ref _restrictStartDate, value))
				{
					CanChangeStartDate = false;
				}
			}
		}

		public bool CanChangeStartDate { get; private set; } = true;

		public virtual DateTime? RestrictEndDate
		{
			get => _restrictEndDate;
			set
			{
				if(UpdateFilterField(ref _restrictEndDate, value))
				{
					CanChangeEndDate = false;
				}
			}
		}

		public bool CanChangeEndDate { get; private set; } = true;

		public virtual Nomenclature RestrictNomenclature
		{
			get => _restrictNomenclature;
			set
			{
				if(UpdateFilterField(ref _restrictNomenclature, value))
				{
					CanChangeNomenclature = false;
				}
			}
		}

		public bool CanChangeNomenclature { get; private set; } = true;

		public virtual RequestStatus? RestrictStatus
		{
			get => _restrictStatus;
			set
			{
				if(UpdateFilterField(ref _restrictStatus, value))
				{
					CanChangeStatus = false;
				}
			}
		}
		public bool CanChangeStatus { get; private set; } = true;

		public IEntityEntryViewModel NomenclatureViewModel { get; private set; }

		public DialogViewModelBase Journal
		{
			get => _journal;
			set
			{
				if(_journal is null)
				{
					_journal = value;

					NomenclatureViewModel = new CommonEEVMBuilderFactory<RequestsToSuppliersFilterViewModel>(_journal, this, UoW, _journal.NavigationManager, _lifetimeScope)
						.ForProperty(x => x.RestrictNomenclature)
						.UseViewModelDialog<NomenclatureViewModel>()
						.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
						{

						})
						.Finish();
				}
			}
		}

		public override void Dispose()
		{
			_journal = null;
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
