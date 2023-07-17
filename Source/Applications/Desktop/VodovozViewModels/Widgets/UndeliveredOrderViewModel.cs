using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Widgets
{
	public class UndeliveredOrderViewModel : EntityWidgetViewModelBase<UndeliveredOrder>
	{
		private UndeliveryObject _undeliveryObject;
		private IList<UndeliveryKind> _undeliveryKindSource;
		private IList<UndeliveryKind> _undeliveryKinds;
		private IList<UndeliveryObject> _undeliveryObjectSource;
		private UndeliveryKind _undeliveryKind;
		private readonly UndeliveryDetalizationJournalFilterViewModel _undeliveryDetalizationJournalFilterViewModel;

		public UndeliveredOrderViewModel(UndeliveredOrder entity, ICommonServices commonServices,
			IUndeliveryDetalizationJournalFactory undeliveryDetalizationJournalFactory, IUnitOfWork uow)
			: base(entity, commonServices)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));

			_undeliveryKinds = _undeliveryKindSource = UoW.GetAll<UndeliveryKind>().Where(k => !k.IsArchive).ToList();

			CanReadDetalization = CommonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(UndeliveryDetalization)).CanRead;

			_undeliveryDetalizationJournalFilterViewModel = new UndeliveryDetalizationJournalFilterViewModel();
			UndeliveryDetalizationSelectorFactory = (undeliveryDetalizationJournalFactory ?? throw new ArgumentException(nameof(undeliveryDetalizationJournalFactory)))
				.CreateUndeliveryDetalizationAutocompleteSelectorFactory(_undeliveryDetalizationJournalFilterViewModel);

			RefreshParentObjects();
		}

		public IList<UndeliveryObject> UndeliveryObjectSource =>
			_undeliveryObjectSource ?? (_undeliveryObjectSource = UoW.GetAll<UndeliveryObject>().Where(x => !x.IsArchive).ToList());

		public UndeliveryObject UndeliveryObject
		{
			get => _undeliveryObject;
			set
			{
				if(SetField(ref _undeliveryObject, value))
				{
					UndeliveryKindSource = value == null ? _undeliveryKinds : _undeliveryKinds.Where(x => x.UndeliveryObject == value).ToList();
					_undeliveryDetalizationJournalFilterViewModel.UndeliveryObject = value;
				}
			}
		}

		public IList<UndeliveryKind> UndeliveryKindSource
		{
			get
			{
				if(Entity.UndeliveryDetalization?.UndeliveryKind != null && Entity.UndeliveryDetalization.UndeliveryKind.IsArchive)
				{
					_undeliveryKindSource.Add(UoW.GetById<UndeliveryKind>(Entity.UndeliveryDetalization.UndeliveryKind.Id));
				}

				return _undeliveryKindSource;
			}
			set => SetField(ref _undeliveryKindSource, value);
		}

		public UndeliveryKind UndeliveryKind
		{
			get => _undeliveryKind;
			set
			{
				if(SetField(ref _undeliveryKind, value))
				{
					_undeliveryDetalizationJournalFilterViewModel.UndeliveryKind = value;
					OnPropertyChanged(nameof(CanChangeDetalization));
				}
			}
		}

		public bool CanEdit => PermissionResult.CanUpdate;

		[PropertyChangedAlso(nameof(CanChangeDetalization))]
		public bool CanReadDetalization { get; }
		public bool CanChangeDetalization => CanReadDetalization && _undeliveryDetalizationJournalFilterViewModel.UndeliveryKind != null;

		public IEntityAutocompleteSelectorFactory UndeliveryDetalizationSelectorFactory { get; set; }

		public void RefreshParentObjects()
		{
			UndeliveryObject = Entity.UndeliveryDetalization?.UndeliveryKind?.UndeliveryObject;
			UndeliveryKind = Entity.UndeliveryDetalization?.UndeliveryKind;
		}


		//--------------------

	}
}
