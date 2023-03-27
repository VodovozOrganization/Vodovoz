using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Utilities.Numeric;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class ExternalCounterpartyMatchingViewModel : EntityTabViewModelBase<ExternalCounterpartyMatching>
	{
		private readonly ILifetimeScope _scope;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly IRoboatsSettings _roboatsSettings;
		private object _selectedMatch;
		private object _selectedDiscrepancy;
		private DelegateCommand _openOrderJournalCommand;
		private DelegateCommand _assignCounterpartyCommand;
		private DelegateCommand _reAssignCounterpartyCommand;
		private bool _needCreateNotification;
		
		public ExternalCounterpartyMatchingViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ITdiCompatibilityNavigation navigation,
			ILifetimeScope scope,
			IRoboatsRepository roboatsRepository,
			IEmailRepository emailRepository,
			IRoboatsSettings roboatsSettings)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));

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
		public string EntityDate => Entity.Created.HasValue ? Entity.Created.Value.ToShortDateString() : string.Empty;
		public string PhoneNumber => Entity.PhoneNumber;
		public string CounterpartyFrom => Entity.CounterpartyFrom.GetEnumTitle();
		public string ExternalCounterpartyId => Entity.ExternalCounterpartyId.ToString();
		public bool HasDiscrepancies => Entity.ExistingExternalCounterpartyWithSameParams != null;
		
		public string CounterpartyName => Entity.AssignedExternalCounterparty?.Phone?.Counterparty != null
			? $"Id {Entity.AssignedExternalCounterparty?.Phone?.Counterparty.Id} {Entity.AssignedExternalCounterparty?.Phone?.Counterparty.Name}" : "-";

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
					
					var externalCounterparty = CreateNewExternalCounterparty();
					var phone = GetPhone(counterpartyNode);
					externalCounterparty.Phone = phone;
					externalCounterparty.ExternalCounterpartyId = Entity.ExternalCounterpartyId;
					externalCounterparty.Email = _emailRepository.GetEmailForExternalCounterparty(UoW, counterpartyNode.EntityId);
					//TODO проверить сохранение подчиненной сущности
					//UoW.Save(externalCounterparty);
					
					Entity.AssignCounterparty(externalCounterparty);
					_needCreateNotification = true;
					//CreateNotification(externalCounterparty);
					UpdateMatches();
					UpdateDiscrepancies();
				}));

		public DelegateCommand ReAssignCounterpartyCommand =>
			_reAssignCounterpartyCommand ?? (_reAssignCounterpartyCommand = new DelegateCommand(
				() =>
				{
					if(!(SelectedDiscrepancy is ExistingExternalCounterpartyNode counterpartyNode))
					{
						return;
					}

					ReAssignCounterparty(counterpartyNode);
					UpdateMatches();
					UpdateDiscrepancies();
				}));

		private void ReAssignCounterparty(ExistingExternalCounterpartyNode counterpartyNode)
		{
			if(Entity.ExistingExternalCounterpartyWithSameParams.ExternalCounterpartyId ==
				counterpartyNode.ExternalCounterpartyGuid)
			{
				ReAssignCounterpartyWithChangePhone(counterpartyNode);
			}
			else if(Entity.ExistingExternalCounterpartyWithSameParams.Phone.DigitsNumber ==
					new PhoneFormatter(PhoneFormat.DigitsTen).FormatString(Entity.PhoneNumber))
			{
				var externalCounterparty = UoW.GetById<ExternalCounterparty>(counterpartyNode.ExternalCounterpartyId);
				CreateNewGuidForExternalCounterparty(externalCounterparty);
			}
		}

		private void CreateNewGuidForExternalCounterparty(ExternalCounterparty externalCounterparty)
		{
			Guid newGuid;
			ExternalCounterparty externalCounterpartyWithSameGuid;
			
			do
			{
				newGuid = Guid.NewGuid();
				externalCounterpartyWithSameGuid =
					UoW.GetAll<ExternalCounterparty>()
						.SingleOrDefault(x => x.ExternalCounterpartyId == newGuid
							&& x.CounterpartyFrom == Entity.ExistingExternalCounterpartyWithSameParams.CounterpartyFrom);
			} while(externalCounterpartyWithSameGuid != null);

			externalCounterparty.ExternalCounterpartyId = newGuid;
		}

		private void ReAssignCounterpartyWithChangePhone(ExistingExternalCounterpartyNode counterpartyNode)
		{
			var externalCounterparty = UoW.GetById<ExternalCounterparty>(counterpartyNode.ExternalCounterpartyId);
			counterpartyNode.PhoneId = null;
			externalCounterparty.Phone = GetPhone(counterpartyNode);
			//UoW.Save(externalCounterparty);

			Entity.AssignCounterparty(externalCounterparty);
			_needCreateNotification = true;
			//CreateNotification(externalCounterparty);
		}

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
		
		public void UpdateMatches()
		{
			ContactMatches.Clear();

			ExternalCounterpartyMatchingNode resultAlias = null;
			Phone phoneAlias = null;
			Domain.Client.Counterparty counterpartyAlias = null;
			Domain.Client.Counterparty deliveryPointCounterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;

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
				.Where(Restrictions.Like(
					Projections.Property<Phone>(p => p.DigitsNumber),
					new PhoneFormatter(PhoneFormat.DigitsTen).FormatString(Entity.PhoneNumber),
					MatchMode.Anywhere))
				.And(() => counterpartyAlias.Id != null || deliveryPointAlias.Id != null)
				.And(p => !p.IsArchive)
				.SelectList(list => list
					.Select(Projections.Conditional(
						Restrictions.IsNotNull(Projections.Property(() => counterpartyAlias.Id)),
						Projections.Property(() => counterpartyAlias.Id),
						Projections.Property(() => deliveryPointAlias.Id))).WithAlias(() => resultAlias.EntityId)
					.Select(() => deliveryPointCounterpartyAlias.Id).WithAlias(() => resultAlias.DeliveryPointCounterpartyId)
					.Select(() => deliveryPointCounterpartyAlias.Name).WithAlias(() => resultAlias.DeliveryPointCounterpartyName)
					.Select(p => p.Id).WithAlias(() => resultAlias.PhoneId)
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

		private void FillMatches(IList<ExternalCounterpartyMatchingNode> result)
		{
			var nodes = new Dictionary<int, CounterpartyMatchingNode>();

			foreach(var item in result)
			{
				CounterpartyMatchingNode counterpartyNode;

				if(item.EntityType == nameof(Counterparty))
				{
					counterpartyNode = CounterpartyMatchingNode.Create(
						item.EntityId, item.PersonType, item.LastOrderDate, item.Title, true, item.PhoneId);
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
			UpdateMatchFromExistingExternalCounterpartyWithSameParams(nodes);
		}

		private void UpdateMatchFromAssignedCounterparty(IDictionary<int, CounterpartyMatchingNode> nodes)
		{
			if(Entity.AssignedExternalCounterparty == null)
			{
				return;
			}

			UpdateMatch(nodes, Entity.AssignedExternalCounterparty.Phone.Counterparty.Id, Entity.AssignedExternalCounterparty.Id);
		}

		private void UpdateMatchFromExistingExternalCounterpartyWithSameParams(IDictionary<int, CounterpartyMatchingNode> nodes)
		{
			if(Entity.ExistingExternalCounterpartyWithSameParams == null)
			{
				return;
			}

			UpdateMatch(
				nodes,
				Entity.ExistingExternalCounterpartyWithSameParams.Phone.Counterparty.Id,
				Entity.ExistingExternalCounterpartyWithSameParams.Id,
				true);
		}
		
		private void UpdateMatch(
			IDictionary<int, CounterpartyMatchingNode> nodes,
			int counterpartyId,
			int externalCounterpartyId,
			bool hasOtherExternalCounterparty = false)
		{
			nodes.TryGetValue(counterpartyId, out var counterpartyNode);

			if(counterpartyNode != null)
			{
				counterpartyNode.ExternalCounterpartyId = externalCounterpartyId;
				counterpartyNode.HasOtherExternalCounterparty = hasOtherExternalCounterparty;
			}
		}

		private void UpdateDiscrepancies()
		{
			Discrepancies.Clear();
			
			if(Entity.ExistingExternalCounterpartyWithSameParams is null)
			{
				return;
			}
			
			ExistingExternalCounterpartyNode resultAlias = null;
			Domain.Client.Counterparty counterpartyAlias = null;
			Phone phoneAlias = null;
			
			var result = UoW.Session.QueryOver<ExternalCounterparty>()
				.JoinAlias(ec => ec.Phone, () => phoneAlias)
				.JoinAlias(() => phoneAlias.Counterparty, () => counterpartyAlias)
				.Where(ec => ec.Id == Entity.ExistingExternalCounterpartyWithSameParams.Id)
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

		private ExternalCounterparty CreateNewExternalCounterparty()
		{
			ExternalCounterparty externalCounterparty;
			switch(Entity.CounterpartyFrom)
			{
				case Domain.Client.CounterpartyFrom.MobileApp:
					externalCounterparty = new MobileAppCounterparty();
					break;
				case Domain.Client.CounterpartyFrom.WebSite:
					externalCounterparty = new WebSiteCounterparty();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return externalCounterparty;
		}
		
		private Phone GetPhone(ICounterpartyWithPhoneNode counterpartyWithPhoneNode)
		{
			Phone phone;
			if(!counterpartyWithPhoneNode.PhoneId.HasValue)
			{
				var counterparty = UoW.GetById<Domain.Client.Counterparty>(counterpartyWithPhoneNode.EntityId);
				phone = new Phone
				{
					Counterparty = counterparty,
					Number = new PhoneFormatter(PhoneFormat.DigitsTen).FormatString(Entity.PhoneNumber)
				};
				FillCounterpartyContact(phone, counterparty.FirstName, counterparty.Patronymic);
				//UoW.Save(phone);
			}
			else
			{
				phone = UoW.GetById<Phone>(counterpartyWithPhoneNode.PhoneId.Value);
			}

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
