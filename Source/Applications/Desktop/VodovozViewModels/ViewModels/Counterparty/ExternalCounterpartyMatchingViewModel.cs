using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Utilities.Numeric;
using QS.ViewModels;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Settings.Roboats;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class ExternalCounterpartyMatchingViewModel : EntityTabViewModelBase<ExternalCounterpartyMatching>
	{
		private readonly ILifetimeScope _scope;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly IExternalCounterpartyFactory _externalCounterpartyFactory;
		private object _selectedMatch;
		private object _selectedDiscrepancy;
		private DelegateCommand _openOrderJournalCommand;
		private DelegateCommand _assignCounterpartyCommand;
		private DelegateCommand _reAssignCounterpartyCommand;
		private DelegateCommand _legalCounterpartyCommand;
		private bool _needCreateNotification;
		
		public ExternalCounterpartyMatchingViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ITdiCompatibilityNavigation navigation,
			ILifetimeScope scope,
			IRoboatsRepository roboatsRepository,
			IEmailRepository emailRepository,
			IRoboatsSettings roboatsSettings,
			IExternalCounterpartyFactory externalCounterpartyFactory)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_externalCounterpartyFactory =
				externalCounterpartyFactory ?? throw new ArgumentNullException(nameof(externalCounterpartyFactory));

			ConfigureEntityChangingRelations();
			UpdateMatches();
			UpdateDiscrepancies();
		}

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.AssignedExternalCounterparty, () => HasAssignedCounterparty);
			SetPropertyChangeRelation(e => e.AssignedExternalCounterparty, () => CounterpartyName);
		}

		public GenericObservableList<CounterpartyMatchingNode> ContactMatches = new GenericObservableList<CounterpartyMatchingNode>();
		public GenericObservableList<ExistingExternalCounterpartyNode> Discrepancies =
			new GenericObservableList<ExistingExternalCounterpartyNode>();
		public ITdiCompatibilityNavigation Navigation { get; }

		public bool HasAssignedCounterparty => Entity.AssignedExternalCounterparty != null;
		public string EntityDate => Entity.Created.HasValue ? Entity.Created.Value.ToString("g") : string.Empty;
		public string PhoneNumber => Entity.PhoneNumber;
		public string DigitsPhoneNumber => new PhoneFormatter(PhoneFormat.DigitsTen).FormatString(Entity.PhoneNumber);
		public string CounterpartyFrom => Entity.CounterpartyFrom.GetEnumTitle();
		public string ExternalCounterpartyId => Entity.ExternalCounterpartyGuid.ToString();
		public bool HasDiscrepancies => Discrepancies.Any();
		
		public string CounterpartyName
		{
			get
			{
				var counterparty = Entity.AssignedExternalCounterparty?.Phone?.Counterparty;
				return counterparty != null
					? $"Id {counterparty.Id} {counterparty.Name}"
					: "-";
			}
		}

		public object SelectedMatch
		{
			get => _selectedMatch;
			set
			{
				if(SetField(ref _selectedMatch, value))
				{
					OnPropertyChanged(nameof(HasSelectedMatch));
					OnPropertyChanged(nameof(HasSelectedNotAssignedCounterpartyMatchingNode));
				}
			}
		}
		
		public object SelectedDiscrepancy
		{
			get => _selectedDiscrepancy;
			set => SetField(ref _selectedDiscrepancy, value);
		}

		public bool HasSelectedMatch => SelectedMatch != null;
		public bool HasSelectedNotAssignedCounterpartyMatchingNode =>
			SelectedMatch is CounterpartyMatchingNode counterpartyNode && counterpartyNode.ExternalCounterpartyId == null;

		public DelegateCommand AssignCounterpartyCommand =>
			_assignCounterpartyCommand ?? (_assignCounterpartyCommand = new DelegateCommand(
				() =>
				{
					if(!(SelectedMatch is CounterpartyMatchingNode counterpartyNode))
					{
						return;
					}

					if(!ValidateAssign(counterpartyNode))
					{
						return;
					}

					ExternalCounterparty externalCounterparty = null;
					
					if(Discrepancies.Count == 1)
					{
						var discrepancy = Discrepancies.First();

						if(discrepancy.ExternalCounterpartyGuid == Entity.ExternalCounterpartyGuid)
						{
							externalCounterparty = UoW.GetById<ExternalCounterparty>(discrepancy.ExternalCounterpartyId);
							externalCounterparty.Phone = GetPhone(counterpartyNode);
						}
						else
						{
							ShowWarningMessage("Ошибка регистрации пользователя\n" +
								"Обратитесь в РПО");
						}
					}
					else
					{
						externalCounterparty = _externalCounterpartyFactory.CreateNewExternalCounterparty(Entity.CounterpartyFrom);
						externalCounterparty.Phone = GetPhone(counterpartyNode);
						externalCounterparty.ExternalCounterpartyId = Entity.ExternalCounterpartyGuid;
						externalCounterparty.Email = _emailRepository.GetEmailForExternalCounterparty(UoW, counterpartyNode.EntityId);
					}
					
					Entity.AssignCounterparty(externalCounterparty);
					_needCreateNotification = true;
					UpdateMatches();
					UpdateDiscrepancies();
				}));

		public DelegateCommand ReAssignCounterpartyCommand =>
			_reAssignCounterpartyCommand ?? (_reAssignCounterpartyCommand = new DelegateCommand(
				() =>
				{
					if(!(SelectedDiscrepancy is ExistingExternalCounterpartyNode selectedDiscrepancyNode))
					{
						return;
					}

					ReAssignCounterparty(selectedDiscrepancyNode);
					_needCreateNotification = true;
					UpdateMatches();
					UpdateDiscrepancies();
				}));

		public DelegateCommand OpenOrderJournalCommand => _openOrderJournalCommand ?? (_openOrderJournalCommand = new DelegateCommand(
			() =>
			{
				var filter = _scope.Resolve<OrderJournalFilterViewModel>();

				switch(SelectedMatch)
				{
					case CounterpartyMatchingNode counterpartyNode:
						filter.RestrictCounterparty = UoW.GetById<Domain.Client.Counterparty>(counterpartyNode.EntityId);
						break;
					case DeliveryPointMatchingNode dpNode:
					{
						var counterparty = UoW.GetById<Domain.Client.Counterparty>(dpNode.CounterpartyMatchingNode.EntityId);
						filter.SetAndRefilterAtOnce(
							f => f.DeliveryPoint = UoW.GetById<DeliveryPoint>(dpNode.EntityId),
							f => f.RestrictCounterparty = counterparty);
						break;
					}
				}
				
				NavigationManager.OpenViewModel<OrderJournalViewModel, OrderJournalFilterViewModel>(this, filter, OpenPageOptions.AsSlave);
			}
			));

		public DelegateCommand LegalCounterpartyCommand => _legalCounterpartyCommand ?? (_legalCounterpartyCommand = new DelegateCommand(
			() =>
			{
				Entity.SetLegalCounterpartyStatus();
				SaveAndClose();
			}
			));

		public void UpdateMatches()
		{
			ContactMatches.Clear();

			ExternalCounterpartyMatchingNode resultAlias = null;
			Phone phoneAlias = null;
			Domain.Client.Counterparty counterpartyAlias = null;
			Domain.Client.Counterparty deliveryPointCounterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			ExternalCounterparty externalCounterpartyAlias = null;

			var counterpartyLastOrderDate = QueryOver.Of<Order>()
				.Where(o => o.Client.Id == counterpartyAlias.Id)
				.Select(o => o.DeliveryDate)
				.OrderBy(o => o.DeliveryDate).Desc
				.Take(1);
			
			var deliveryPointLastOrderDate = QueryOver.Of<Order>()
				.Where(o => o.DeliveryPoint.Id == deliveryPointAlias.Id)
				.Select(o => o.DeliveryDate)
				.OrderBy(o => o.DeliveryDate).Desc
				.Take(1);
			
			var deliveryPointCounterpartyLastOrderDate = QueryOver.Of<Order>()
				.Where(o => o.Client.Id == deliveryPointCounterpartyAlias.Id)
				.Select(o => o.DeliveryDate)
				.OrderBy(o => o.DeliveryDate).Desc
				.Take(1);

			var lastOrderDate = Projections.Conditional(
				Restrictions.IsNotNull(Projections.Property(() => counterpartyAlias.Id)),
				Projections.SubQuery(counterpartyLastOrderDate),
				Projections.SubQuery(deliveryPointLastOrderDate));
			
			var result = UoW.Session.QueryOver(() => phoneAlias)
				.Left.JoinAlias(p => p.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(p => p.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Counterparty, () => deliveryPointCounterpartyAlias)
				.JoinEntityAlias(
					() => externalCounterpartyAlias,
					() => externalCounterpartyAlias.Phone.Id == phoneAlias.Id
						&& externalCounterpartyAlias.CounterpartyFrom == Entity.CounterpartyFrom
						&& !externalCounterpartyAlias.IsArchive,
					JoinType.LeftOuterJoin)
				.Where(Restrictions.Eq(Projections.Property(() => phoneAlias.DigitsNumber), DigitsPhoneNumber))
				.And(() => counterpartyAlias.Id != null || deliveryPointAlias.Id != null)
				.And(p => !p.IsArchive)
				.SelectList(list => list
					.Select(Projections.Conditional(
						Restrictions.IsNotNull(Projections.Property(() => counterpartyAlias.Id)),
						Projections.Property(() => counterpartyAlias.Id),
						Projections.Property(() => deliveryPointAlias.Id))).WithAlias(() => resultAlias.EntityId)
					.Select(() => externalCounterpartyAlias.Id).WithAlias(() => resultAlias.ExternalCounterpartyId)
					.Select(() => deliveryPointCounterpartyAlias.Id).WithAlias(() => resultAlias.DeliveryPointCounterpartyId)
					.Select(() => deliveryPointCounterpartyAlias.Name).WithAlias(() => resultAlias.DeliveryPointCounterpartyName)
					.SelectGroup(p => p.Id).WithAlias(() => resultAlias.PhoneId)
					.Select(Projections.Conditional(
						Restrictions.IsNotNull(Projections.Property(() => counterpartyAlias.Id)),
						Projections.Constant(nameof(Counterparty)),
						Projections.Constant(nameof(DeliveryPoint)))).WithAlias(() => resultAlias.EntityType)
					.Select(Projections.Conditional(
						Restrictions.IsNotNull(Projections.Property(() => counterpartyAlias.Id)),
						Projections.Property(() => counterpartyAlias.PersonType),
						Projections.Property(() => deliveryPointCounterpartyAlias.PersonType))).WithAlias(() => resultAlias.PersonType)
					.Select(Projections.Conditional(
						Restrictions.IsNotNull(Projections.Property(() => counterpartyAlias.Id)),
						Projections.Property(() => counterpartyAlias.Name),
						Projections.Property(() => deliveryPointAlias.CompiledAddress))).WithAlias(() => resultAlias.Title)
					.Select(lastOrderDate).WithAlias(() => resultAlias.LastOrderDate)
					.Select(Projections.SubQuery(deliveryPointCounterpartyLastOrderDate))
						.WithAlias(() => resultAlias.DeliveryPointCounterpartyLastOrderDate)
				)
				.TransformUsing(Transformers.AliasToBean<ExternalCounterpartyMatchingNode>())
				.OrderByAlias(() => resultAlias.EntityType).Asc
				.List<ExternalCounterpartyMatchingNode>();

			FillMatches(result);
		}

		protected override void AfterSave()
		{
			if(_needCreateNotification)
			{
				CreateNotification(Entity.AssignedExternalCounterparty);
			}
		}
		
		private bool ValidateAssign(CounterpartyMatchingNode counterpartyNode)
		{
			if(counterpartyNode.PersonType == PersonType.legal)
			{
				ShowWarningMessage("Невозможно присвоить юридическое лицо.\n" +
					"Выберете физическое лицо или создайте нового контрагента");
				return false;
			}

			if(Discrepancies.Count > 1)
			{
				ShowWarningMessage("Слишком много расхождений для одного пользователя\n" +
					"Обратитесь в РПО");
				return false;
			}

			return true;
		}

		private void ArchiveOtherExternalCounterparties()
		{
			foreach(var discrepancy in Discrepancies)
			{
				var externalCounterparty = UoW.GetById<ExternalCounterparty>(discrepancy.ExternalCounterpartyId);
				externalCounterparty.IsArchive = true;
				UoW.Save(externalCounterparty);
			}
		}
		
		private void ReAssignCounterparty(ExistingExternalCounterpartyNode selectedDiscrepancyNode)
		{
			if(Entity.ExternalCounterpartyGuid == selectedDiscrepancyNode.ExternalCounterpartyGuid
				&& DigitsPhoneNumber != selectedDiscrepancyNode.PhoneNumber)
			{
				ReAssignCounterpartyWithChangePhone(selectedDiscrepancyNode);
			}
			else if(DigitsPhoneNumber == selectedDiscrepancyNode.PhoneNumber
				&& Entity.ExternalCounterpartyGuid != selectedDiscrepancyNode.ExternalCounterpartyGuid)
			{
				ReAssignCounterpartyWithChangeExternalId(selectedDiscrepancyNode);
			}
		}

		private void ReAssignCounterpartyWithChangePhone(ExistingExternalCounterpartyNode selectedDiscrepancyNode)
		{
			var externalCounterparty = UoW.GetById<ExternalCounterparty>(selectedDiscrepancyNode.ExternalCounterpartyId);
			externalCounterparty.Phone = GetPhoneForReAssign(selectedDiscrepancyNode.EntityId, Entity.PhoneNumber.Remove(0, 2));

			Entity.AssignCounterparty(externalCounterparty);
			_needCreateNotification = true;
		}

		private Phone GetPhoneForReAssign(int counterpartyId, string phoneNumber)
		{
			var counterparty = UoW.GetById<Domain.Client.Counterparty>(counterpartyId);
			var phone = counterparty.Phones.FirstOrDefault(x => x.DigitsNumber == phoneNumber)
				?? CreateAndFillContactPhone(counterparty);

			return phone;
		}

		private void ReAssignCounterpartyWithChangeExternalId(ExistingExternalCounterpartyNode selectedDiscrepancyNode)
		{
			var externalCounterparty = UoW.GetById<ExternalCounterparty>(selectedDiscrepancyNode.ExternalCounterpartyId);
			externalCounterparty.ExternalCounterpartyId = Entity.ExternalCounterpartyGuid;
			Entity.AssignCounterparty(externalCounterparty);
		}

		private void FillMatches(IList<ExternalCounterpartyMatchingNode> result)
		{
			var nodes = new Dictionary<int, CounterpartyMatchingNode>();

			foreach(var item in result)
			{
				CounterpartyMatchingNode counterpartyNode;

				var hasOtherExternalCounterparty =
					item.ExternalCounterpartyId.HasValue
					&& Entity.AssignedExternalCounterparty != null 
					&& item.ExternalCounterpartyId.Value != Entity.AssignedExternalCounterparty.Id;
				
				if(item.EntityType == nameof(Counterparty))
				{
					counterpartyNode = CounterpartyMatchingNode.Create(
						item.EntityId, item.PersonType, item.LastOrderDate, item.Title, true, item.PhoneId,
						item.ExternalCounterpartyId, hasOtherExternalCounterparty);
					
					if(nodes.ContainsKey(counterpartyNode.EntityId))
					{
						continue;
					}

					nodes.Add(counterpartyNode.EntityId, counterpartyNode);
					ContactMatches.Add(counterpartyNode);
				}
				else
				{
					nodes.TryGetValue(item.DeliveryPointCounterpartyId.Value, out counterpartyNode);

					if(counterpartyNode is null)
					{
						counterpartyNode = CounterpartyMatchingNode.Create(item.DeliveryPointCounterpartyId.Value, item.PersonType,
							item.DeliveryPointCounterpartyLastOrderDate, item.DeliveryPointCounterpartyName, false);
						nodes.Add(counterpartyNode.EntityId, counterpartyNode);
						ContactMatches.Add(counterpartyNode);
					}

					var dp = DeliveryPointMatchingNode.Create(
						item.EntityId, item.PersonType, item.LastOrderDate, counterpartyNode, item.Title);
					counterpartyNode.DeliveryPoints.Add(dp);
				}
			}

			UpdateMatchFromAssignedCounterparty(nodes);
		}

		private void UpdateMatchFromAssignedCounterparty(IDictionary<int, CounterpartyMatchingNode> nodes)
		{
			if(Entity.AssignedExternalCounterparty == null)
			{
				return;
			}

			UpdateMatch(nodes);
		}

		private void UpdateMatch(IDictionary<int, CounterpartyMatchingNode> nodes)
		{
			var counterparty = Entity.AssignedExternalCounterparty.Phone.Counterparty;
			nodes.TryGetValue(counterparty.Id, out var counterpartyNode);

			if(counterpartyNode != null)
			{
				counterpartyNode.ExternalCounterpartyId = Entity.AssignedExternalCounterparty.Id;
			}
			else
			{
				var lastOrderDate = UoW.Session.QueryOver<Order>()
					.Where(o => o.Client.Id == counterparty.Id)
					.Select(o => o.DeliveryDate)
					.OrderBy(o => o.DeliveryDate).Desc
					.Take(1)
					.SingleOrDefault<DateTime?>();
				
				counterpartyNode = CounterpartyMatchingNode.Create(
					counterparty.Id, counterparty.PersonType, lastOrderDate, counterparty.Name, true,
					Entity.AssignedExternalCounterparty.Phone.Id);
				counterpartyNode.ExternalCounterpartyId = Entity.AssignedExternalCounterparty.Id;
				
				ContactMatches.Add(counterpartyNode);
			}
		}

		private void UpdateDiscrepancies()
		{
			Discrepancies.Clear();
			
			if(Entity.AssignedExternalCounterparty != null)
			{
				return;
			}
			
			ExistingExternalCounterpartyNode resultAlias = null;
			ExternalCounterparty externalCounterpartyAlias = null;
			Domain.Client.Counterparty counterpartyAlias = null;
			Phone phoneAlias = null;
			
			var result = UoW.Session.QueryOver(() => externalCounterpartyAlias)
				.JoinAlias(ec => ec.Phone, () => phoneAlias)
				.JoinAlias(() => phoneAlias.Counterparty, () => counterpartyAlias)
				.Where(Restrictions.Or(
					Restrictions.Where(
						() => phoneAlias.DigitsNumber == DigitsPhoneNumber
							&& externalCounterpartyAlias.ExternalCounterpartyId != Entity.ExternalCounterpartyGuid),
					Restrictions.Where(
						() => externalCounterpartyAlias.ExternalCounterpartyId == Entity.ExternalCounterpartyGuid
							&& phoneAlias.DigitsNumber != DigitsPhoneNumber)
					))
				.And(() => externalCounterpartyAlias.CounterpartyFrom == Entity.CounterpartyFrom)
				.And(() => !externalCounterpartyAlias.IsArchive)
				.SelectList(list => list
					.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.EntityId)
					.Select(() => phoneAlias.Id).WithAlias(() => resultAlias.PhoneId)
					.Select(() => phoneAlias.DigitsNumber).WithAlias(() => resultAlias.PhoneNumber)
					.Select(() => counterpartyAlias.PersonType).WithAlias(() => resultAlias.PersonType)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.Select(ec => ec.ExternalCounterpartyId).WithAlias(() => resultAlias.ExternalCounterpartyGuid)
					.Select(ec => ec.Id).WithAlias(() => resultAlias.ExternalCounterpartyId)
				)
				.TransformUsing(Transformers.AliasToBean<ExistingExternalCounterpartyNode>())
				.List<ExistingExternalCounterpartyNode>();

			FillDiscrepancies(result);
		}

		private void FillDiscrepancies(IList<ExistingExternalCounterpartyNode> result)
		{
			foreach(var item in result)
			{
				Discrepancies.Add(item);
			}
		}

		private Phone GetPhone(ICounterpartyWithPhoneNode counterpartyWithPhoneNode)
		{
			Phone phone;
			if(!counterpartyWithPhoneNode.PhoneId.HasValue)
			{
				var counterparty = UoW.GetById<Domain.Client.Counterparty>(counterpartyWithPhoneNode.EntityId);
				phone = CreateAndFillContactPhone(counterparty);
			}
			else
			{
				phone = UoW.GetById<Phone>(counterpartyWithPhoneNode.PhoneId.Value);
			}

			return phone;
		}

		private Phone CreateAndFillContactPhone(Domain.Client.Counterparty counterparty)
		{
			var phone = new Phone
			{
				Counterparty = counterparty,
				Number = new PhoneFormatter(PhoneFormat.DigitsTen).FormatString(Entity.PhoneNumber)
			};
			FillCounterpartyContact(phone, counterparty.FirstName, counterparty.Patronymic);
			UoW.Save(phone);

			return phone;
		}

		private void FillCounterpartyContact(Phone phone, string counterpartyName, string counterpartyPatronymic)
		{
			var roboatsName = _roboatsRepository.GetCounterpartyName(UoW, counterpartyName);

			if(roboatsName is null)
			{
				FillCounterpartyContactByDefault(phone);
				return;
			}

			var roboatsPatronymic = _roboatsRepository.GetCounterpartyPatronymic(UoW, counterpartyPatronymic);

			if(roboatsPatronymic is null)
			{
				FillCounterpartyContactByDefault(phone);
				return;
			}
			
			phone.RoboAtsCounterpartyName = roboatsName;
			phone.RoboAtsCounterpartyPatronymic = roboatsPatronymic;
		}
		
		private void FillCounterpartyContactByDefault(Phone phone)
		{
			phone.RoboAtsCounterpartyName =
				UoW.GetById<RoboAtsCounterpartyName>(_roboatsSettings.DefaultCounterpartyNameId);
			phone.RoboAtsCounterpartyPatronymic =
				UoW.GetById<RoboAtsCounterpartyPatronymic>(_roboatsSettings.DefaultCounterpartyPatronymicId);
		}
		
		private void CreateNotification(ExternalCounterparty externalCounterparty)
		{
			var notification = new ExternalCounterpartyAssignNotification
			{
				ExternalCounterparty = externalCounterparty
			};
			UoW.Save(notification);
			UoW.Commit();
		}
	}
}
