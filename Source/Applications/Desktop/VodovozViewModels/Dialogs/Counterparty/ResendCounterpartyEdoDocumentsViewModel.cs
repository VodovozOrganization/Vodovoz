using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.ViewModels.Dialogs.Counterparty
{
	public class ResendCounterpartyEdoDocumentsViewModel : EntityTabViewModelBase<Domain.Client.Counterparty>
	{
		private readonly ICommonServices _commonServices;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private GenericObservableList<EdoContainerSelectableNode> _edoContainerNodes = new GenericObservableList<EdoContainerSelectableNode>();

		public ResendCounterpartyEdoDocumentsViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			ICounterpartyRepository counterpartyRepository) :  base(uowBuilder, uowFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));

			Title = $"Отправить все неотправленные УПД контрагента {Entity.Name}";
		}

		#region Свойства

		public GenericObservableList<EdoContainerSelectableNode> EdoContainerNodes
		{
			get => _edoContainerNodes;
			set => SetField(ref _edoContainerNodes, value);
		}

		#endregion

		public override void Dispose()
		{
			UoW?.Dispose();
			base.Dispose();
		}

		public class EdoContainerSelectableNode
		{
			public bool IsSelected { get; set; }
			public EdoContainer EdoContainer { get; set; }
		}
	}
}
