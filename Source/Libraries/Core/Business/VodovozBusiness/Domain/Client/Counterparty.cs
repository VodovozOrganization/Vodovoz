using Microsoft.Extensions.DependencyInjection;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Services;
using QS.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Retail;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Counterparty;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.Domain.Client
{
	public class Counterparty : CounterpartyEntity, IValidatableObject, INamed, IArchivable
	{
		//Используется для валидации, не получается истолльзовать бизнес объект так как наследуемся от AccountOwnerBase
		private const int _specialContractNameLimit = 800;
		private const int _cargoReceiverLimitSymbols = 500;

		private int? _defaultExpenseCategoryId;

		private EdoOperator _edoOperator;
		private IObservableList<CounterpartyEdoOperator> _counterpartyEdoOperators = new ObservableList<CounterpartyEdoOperator>();
		private IObservableList<CounterpartyEdoAccount> _counterpartyEdoAccounts = new ObservableList<CounterpartyEdoAccount>();
		private IList<CounterpartyContract> _counterpartyContracts;
		private IList<DeliveryPoint> _deliveryPoints = new List<DeliveryPoint>();
		private GenericObservableList<DeliveryPoint> _observableDeliveryPoints;
		private IList<Tag> _tags = new List<Tag>();
		private GenericObservableList<Tag> _observableTags;
		private IList<Contact> _contact = new List<Contact>();
		private Employee _closeDeliveryPerson;
		private IList<Proxy> _proxies;
		private Counterparty _mainCounterparty;
		private Counterparty _previousCounterparty;
		private IList<Phone> _phones = new List<Phone>();
		private GenericObservableList<Phone> _observablePhones;
		private string _ogrnip;
		private IList<Email> _emails = new List<Email>();
		private Employee _accountant;
		private Employee _salesManager;
		private Employee _bottlesManager;
		private Contact _mainContact;
		private Contact _financialContact;
		private ClientCameFrom _cameFrom;
		private Order _firstOrder;
		private LogisticsRequirements _logisticsRequirements;
		private Account _ourOrganizationAccountForBills;
		private IList<SalesChannel> _salesChannels = new List<SalesChannel>();
		private GenericObservableList<SalesChannel> _observableSalesChannels;
		private IList<SupplierPriceItem> _suplierPriceItems = new List<SupplierPriceItem>();
		private GenericObservableList<SupplierPriceItem> _observableSuplierPriceItems;
		private IList<NomenclatureFixedPrice> _nomenclatureFixedPrices = new List<NomenclatureFixedPrice>();
		private GenericObservableList<NomenclatureFixedPrice> _observableNomenclatureFixedPrices;
		private Organization _worksThroughOrganization;
		private IList<ISupplierPriceNode> _priceNodes = new List<ISupplierPriceNode>();
		private GenericObservableList<ISupplierPriceNode> _observablePriceNodes;
		private CounterpartySubtype _counterpartySubtype;
		private Counterparty _refferer;

		#region Свойства

		[Display(Name = "Договоры")]
		public virtual IList<CounterpartyContract> CounterpartyContracts
		{
			get => _counterpartyContracts;
			set => SetField(ref _counterpartyContracts, value);
		}

		[Display(Name = "Точки доставки")]
		public virtual IList<DeliveryPoint> DeliveryPoints
		{
			get => _deliveryPoints;
			set => SetField(ref _deliveryPoints, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPoint> ObservableDeliveryPoints
		{
			get
			{
				if(_observableDeliveryPoints == null)
				{
					_observableDeliveryPoints = new GenericObservableList<DeliveryPoint>(DeliveryPoints);
				}

				return _observableDeliveryPoints;
			}
		}

		[Display(Name = "Теги")]
		public virtual IList<Tag> Tags
		{
			get => _tags;
			set => SetField(ref _tags, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Tag> ObservableTags
		{
			get
			{
				if(_observableTags == null)
				{
					_observableTags = new GenericObservableList<Tag>(Tags);
				}

				return _observableTags;
			}
		}

		[Display(Name = "Контактные лица")]
		public virtual IList<Contact> Contacts
		{
			get => _contact;
			set => SetField(ref _contact, value);
		}

		#region CloseDelivery

		[Display(Name = "Сотрудник закрывший поставки")]
		public virtual Employee CloseDeliveryPerson
		{
			get => _closeDeliveryPerson;
			protected set => SetField(ref _closeDeliveryPerson, value);
		}

		#endregion CloseDelivery

		[Display(Name = "Доверенности")]
		public virtual IList<Proxy> Proxies
		{
			get => _proxies;
			set => SetField(ref _proxies, value);
		}

		[Display(Name = "Расход по-умолчанию")]
		[HistoryIdentifier(TargetType = typeof(FinancialExpenseCategory))]
		public virtual int? DefaultExpenseCategoryId
		{
			get => _defaultExpenseCategoryId;
			set => SetField(ref _defaultExpenseCategoryId, value);
		}

		[Display(Name = "Головная организация")]
		public virtual Counterparty MainCounterparty
		{
			get => _mainCounterparty;
			set => SetField(ref _mainCounterparty, value);
		}

		[Display(Name = "Предыдущий контрагент")]
		public virtual Counterparty PreviousCounterparty
		{
			get => _previousCounterparty;
			set => SetField(ref _previousCounterparty, value);
		}

		[Display(Name = "Телефоны")]
		public new virtual IList<Phone> Phones
		{
			get => _phones;
			set => SetField(ref _phones, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Phone> ObservablePhones
		{
			get
			{
				if(_observablePhones == null)
				{
					_observablePhones = new GenericObservableList<Phone>(Phones);
				}

				return _observablePhones;
			}
		}

		[Display(Name = "E-mail адреса")]
		public new virtual IList<Email> Emails
		{
			get => _emails;
			set => SetField(ref _emails, value);
		}

		[Display(Name = "ЭДО операторы контрагента")]
		public virtual IObservableList<CounterpartyEdoOperator> CounterpartyEdoOperators
		{
			get => _counterpartyEdoOperators;
			set => SetField(ref _counterpartyEdoOperators, value);
		}

		[Display(Name = "ЭДО аккаунты контрагента")]
		public new virtual IObservableList<CounterpartyEdoAccount> CounterpartyEdoAccounts
		{
			get => _counterpartyEdoAccounts;
			set => SetField(ref _counterpartyEdoAccounts, value);
		}

		[Display(Name = "Бухгалтер")]
		public virtual Employee Accountant
		{
			get => _accountant;
			set => SetField(ref _accountant, value);
		}

		[Display(Name = "Менеджер по продажам")]
		public virtual Employee SalesManager
		{
			get => _salesManager;
			set => SetField(ref _salesManager, value);
		}

		[Display(Name = "Менеджер по бутылям")]
		public virtual Employee BottlesManager
		{
			get => _bottlesManager;
			set => SetField(ref _bottlesManager, value);
		}

		[Display(Name = "Главное контактное лицо")]
		public virtual Contact MainContact
		{
			get => _mainContact;
			set => SetField(ref _mainContact, value);
		}

		[Display(Name = "Контакт по финансовым вопросам")]
		public virtual Contact FinancialContact
		{
			get => _financialContact;
			set => SetField(ref _financialContact, value);
		}

		[Display(Name = "Откуда клиент")]
		public virtual ClientCameFrom CameFrom
		{
			get => _cameFrom;
			set => SetField(ref _cameFrom, value);
		}

		[Display(Name = "Первый заказ")]
		public virtual Order FirstOrder
		{
			get => _firstOrder;
			set => SetField(ref _firstOrder, value);
		}

		[Display(Name = "Требования к логистике")]
		public virtual LogisticsRequirements LogisticsRequirements
		{
			get => _logisticsRequirements;
			set => SetField(ref _logisticsRequirements, value);
		}

		#region ОсобаяПечать

		[Display(Name = "Расчетный счет для счета на олпату")]
		public virtual Account OurOrganizationAccountForBills
		{
			get => _ourOrganizationAccountForBills;
			set => SetField(ref _ourOrganizationAccountForBills, value);
		}

		#endregion ОсобаяПечать

		#region ЭДО и Честный знак

		[Display(Name = "Оператор ЭДО")]
		public virtual EdoOperator EdoOperator
		{
			get => _edoOperator;
			set => SetField(ref _edoOperator, value);
		}

		#endregion

		[Display(Name = "Подтип контрагента")]
		public virtual CounterpartySubtype CounterpartySubtype
		{
			get => _counterpartySubtype;
			set => SetField(ref _counterpartySubtype, value);
		}

		[PropertyChangedAlso(nameof(ObservableSalesChannels))]
		[Display(Name = "Каналы сбыта")]
		public virtual IList<SalesChannel> SalesChannels
		{
			get => _salesChannels;
			set => SetField(ref _salesChannels, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SalesChannel> ObservableSalesChannels
		{
			get
			{
				if(_observableSalesChannels == null)
				{
					_observableSalesChannels = new GenericObservableList<SalesChannel>(SalesChannels);
				}

				return _observableSalesChannels;
			}
		}

		[PropertyChangedAlso(nameof(ObservablePriceNodes))]
		[Display(Name = "Цены на ТМЦ")]
		public virtual IList<SupplierPriceItem> SuplierPriceItems
		{
			get => _suplierPriceItems;
			set => SetField(ref _suplierPriceItems, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SupplierPriceItem> ObservableSuplierPriceItems
		{
			get
			{
				if(_observableSuplierPriceItems == null)
				{
					_observableSuplierPriceItems = new GenericObservableList<SupplierPriceItem>(SuplierPriceItems);
				}

				return _observableSuplierPriceItems;
			}
		}

		[Display(Name = "Фиксированные цены")]
		public virtual new IList<NomenclatureFixedPrice> NomenclatureFixedPrices
		{
			get => _nomenclatureFixedPrices;
			set => SetField(ref _nomenclatureFixedPrices, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclatureFixedPrice> ObservableNomenclatureFixedPrices
		{
			get => _observableNomenclatureFixedPrices ?? (_observableNomenclatureFixedPrices =
				new GenericObservableList<NomenclatureFixedPrice>(NomenclatureFixedPrices));
		}

		[Display(Name = "Работает через организацию")]
		public virtual new Organization WorksThroughOrganization
		{
			get => _worksThroughOrganization;
			set => SetField(ref _worksThroughOrganization, value);
		}

		[Display(Name = "Клиент, который привёл друга")]
		public virtual Counterparty Referrer
		{
			get => _refferer;
			set => SetField(ref _refferer, value);
		}

		#endregion Свойства

		#region Calculated Properties

		public virtual string RawJurAddress
		{
			get => JurAddress;
			set
			{
				StringBuilder sb = new StringBuilder(value);
				sb.Replace("\n", "");
				JurAddress = sb.ToString();
				OnPropertyChanged(nameof(RawJurAddress));
			}
		}

		public virtual bool IsNotEmpty
		{
			get
			{
				bool result = false;
				CheckSpecialField(ref result, SpecialContractName);
				CheckSpecialField(ref result, PayerSpecialKPP);
				CheckSpecialField(ref result, CargoReceiver);
				CheckSpecialField(ref result, SpecialCustomer);
				CheckSpecialField(ref result, GovContract);
				CheckSpecialField(ref result, SpecialDeliveryAddress);
				return result;
			}
		}

		public virtual IList<ISupplierPriceNode> PriceNodes
		{
			get => _priceNodes;
			set => SetField(ref _priceNodes, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ISupplierPriceNode> ObservablePriceNodes
		{
			get
			{
				if(_observablePriceNodes == null)
				{
					_observablePriceNodes = new GenericObservableList<ISupplierPriceNode>(PriceNodes);
				}

				return _observablePriceNodes;
			}
		}

		#endregion

		#region CloseDelivery

		public virtual void AddCloseDeliveryComment(string newComment, Employee currentEmployee)
		{
			CloseDeliveryComment = currentEmployee.ShortName + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ": " + newComment;
		}

		protected virtual bool ManualCloseDelivery(Employee currentEmployee, bool canOpenCloseDeliveries)
		{
			if(!canOpenCloseDeliveries)
			{
				return false;
			}

			CloseDelivery(currentEmployee);

			return true;
		}

		public virtual void CloseDelivery(Employee currentEmployee)
		{
			IsDeliveriesClosed = true;
			CloseDeliveryDate = DateTime.Now;
			CloseDeliveryPerson = currentEmployee;
		}

		protected virtual bool OpenDelivery(bool canOpenCloseDeliveries)
		{
			if(!canOpenCloseDeliveries)
			{
				return false;
			}

			IsDeliveriesClosed = false;
			CloseDeliveryDate = null;
			CloseDeliveryPerson = null;
			CloseDeliveryComment = null;
			CloseDeliveryDebtType = null;

			return true;
		}

		public virtual bool ToggleDeliveryOption(Employee currentEmployee, bool canOpenCloseDeliveries)
		{
			return IsDeliveriesClosed ? OpenDelivery(canOpenCloseDeliveries) : ManualCloseDelivery(currentEmployee, canOpenCloseDeliveries);
		}

		public virtual string GetCloseDeliveryInfo()
		{
			return CloseDeliveryPerson?.ShortName + " " + CloseDeliveryDate?.ToString("dd/MM/yyyy HH:mm");
		}

		#endregion CloseDelivery

		#region цены поставщика

		public virtual void SupplierPriceListRefresh(string[] searchValues = null)
		{
			bool ShortOrFullNameContainsSearchValues(Nomenclature nom)
			{
				if(searchValues == null)
				{
					return true;
				}

				var shortOrFullName = nom.ShortOrFullName.ToLower();

				foreach(var val in searchValues)
				{
					if(!shortOrFullName.Contains(val.ToLower()))
					{
						return false;
					}
				}

				return true;
			}

			int cnt = 0;
			ObservablePriceNodes.Clear();
			var pItems = SuplierPriceItems.Select(i => i.NomenclatureToBuy)
										  .Distinct()
										  .Where(i => ShortOrFullNameContainsSearchValues(i)
												|| searchValues.Contains(i.Id.ToString()));

			foreach(var nom in pItems)
			{
				var sNom = new SellingNomenclature
				{
					NomenclatureToBuy = nom,
					Parent = null,
					PosNr = (++cnt).ToString()
				};

				var children = SuplierPriceItems.Cast<ISupplierPriceNode>().Where(i => i.NomenclatureToBuy == nom).ToList();

				foreach(var i in children)
				{
					i.Parent = sNom;
				}

				sNom.Children = children;
				ObservablePriceNodes.Add(sNom);
			}
		}

		public virtual void AddSupplierPriceItems(Nomenclature nomenclatureFromSupplier)
		{
			foreach(SupplierPaymentType type in Enum.GetValues(typeof(SupplierPaymentType)))
			{
				ObservableSuplierPriceItems.Add(
					new SupplierPriceItem
					{
						Supplier = this,
						NomenclatureToBuy = nomenclatureFromSupplier,
						PaymentType = type
					}
				);
			}
		}

		public virtual void RemoveNomenclatureWithPrices(int nomenclatureId)
		{
			var removableItems = new List<SupplierPriceItem>(
				ObservableSuplierPriceItems.Where(i => i.NomenclatureToBuy.Id == nomenclatureId).ToList());

			foreach(var item in removableItems)
			{
				ObservableSuplierPriceItems.Remove(item);
			}
		}

		#endregion цены поставщика

		public new virtual CounterpartyEdoAccount DefaultEdoAccount(int organizationId)
		{
			return CounterpartyEdoAccounts
				.FirstOrDefault(x => x.OrganizationId == organizationId && x.IsDefault);
		}
		
		public virtual CounterpartyEdoAccount EdoAccount(int organizationId, string account)
		{
			return CounterpartyEdoAccounts
				.SingleOrDefault(x => x.OrganizationId == organizationId
					&& string.Equals(x.PersonalAccountIdInEdo, account, StringComparison.CurrentCultureIgnoreCase));
		}

		public override bool LegalAndHasAnyDefaultAccountAgreedForEdo =>
			PersonType == PersonType.legal
			&& CounterpartyEdoAccounts.Any(
				x => x.IsDefault && x.ConsentForEdoStatus == ConsentForEdoStatus.Agree);
		
		private void CheckSpecialField(ref bool result, string fieldValue)
		{
			if(!string.IsNullOrWhiteSpace(fieldValue))
			{
				result = true;
			}
		}

		public virtual string GetSpecialContractString()
		{
			if(!string.IsNullOrWhiteSpace(SpecialContractName)
				&& string.IsNullOrWhiteSpace(SpecialContractNumber)
				&& !SpecialContractDate.HasValue)
			{
				return SpecialContractName;
			}

			var contractNumber = !string.IsNullOrWhiteSpace(SpecialContractNumber)
				? $"№ {SpecialContractNumber}"
				: string.Empty;

			var contractDate = SpecialContractDate.HasValue
				? $"от {SpecialContractDate.Value.ToShortDateString()}"
				: string.Empty;

			return $"{SpecialContractName} {contractNumber} {contractDate}";
		}

		public Counterparty()
		{
			Name = string.Empty;
			FullName = string.Empty;
			Comment = string.Empty;
			INN = string.Empty;
			OGRN = string.Empty;
			JurAddress = string.Empty;
			PhoneFrom1c = string.Empty;
		}

		#region IValidatableObject implementation

		public virtual bool CheckForINNDuplicate(ICounterpartyRepository counterpartyRepository, IUnitOfWork uow)
		{
			IList<Counterparty> counterarties = counterpartyRepository.GetCounterpartiesByINN(uow, INN);

			if(counterarties == null)
			{
				return false;
			}

			if(counterarties.Any(x => x.Id != Id))
			{
				return true;
			}

			return false;
		}

		public virtual IList<Counterparty> CheckForPhoneNumberDuplicate(ICounterpartyRepository counterpartyRepository, IUnitOfWork uow, string phoneNumber)
		{
			IList<Counterparty> counterarties = counterpartyRepository.GetNotArchivedCounterpartiesByPhoneNumber(uow, phoneNumber);

			return counterarties.Where(x => x?.Id != Id).ToList();
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{			
			var counterpartySettings = validationContext.GetRequiredService<ICounterpartySettings>();
			var counterpartyRepository = validationContext.GetRequiredService<ICounterpartyRepository>();
			var bottlesRepository = validationContext.GetRequiredService<IBottlesRepository>();
			var depositRepository = validationContext.GetRequiredService<IDepositRepository>();
			var orderRepository = validationContext.GetRequiredService<IOrderRepository>();
			var commonServices = validationContext.GetRequiredService<ICommonServices>();
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();

			if(CargoReceiverSource == CargoReceiverSource.Special && string.IsNullOrWhiteSpace(CargoReceiver))
			{
				yield return new ValidationResult("Если выбран особый грузополучатель, необходимо ввести данные о нем");
			}

			if(CargoReceiver != null && CargoReceiver.Length > _cargoReceiverLimitSymbols)
			{
				yield return new ValidationResult(
					$"Длина строки \"Грузополучатель\" не должна превышать {_cargoReceiverLimitSymbols} символов");
			}

			using(var uow = uowFactory.CreateWithoutRoot())
			{
				if(CheckForINNDuplicate(counterpartyRepository, uow))
				{
					yield return new ValidationResult(
						"Контрагент с данным ИНН уже существует.",
						new[] { nameof(INN) });
				}

				if(UseSpecialDocFields && PayerSpecialKPP != null && PayerSpecialKPP.Length != 9)
				{
					yield return new ValidationResult("Длина КПП для документов должна равнятся 9-ти.",
						new[] { nameof(KPP) });
				}

				if(PersonType == PersonType.legal)
				{
					if(TypeOfOwnership == null || TypeOfOwnership.Length == 0)
					{
						yield return new ValidationResult("Не заполнена Форма собственности.",
							new[] { nameof(TypeOfOwnership) });
					}

					if(KPP?.Length != 9 && KPP?.Length != 0 && TypeOfOwnership != "ИП")
					{
						yield return new ValidationResult("Длина КПП должна равняться 9-ти.",
							new[] { nameof(KPP) });
					}

					if(INN.Length != CompanyConstants.NotPrivateBusinessmanInnLength && INN.Length != 0 && TypeOfOwnership != "ИП")
					{
						yield return new ValidationResult("Длина ИНН должна равняться 10-ти.",
							new[] { nameof(INN) });
					}

					if(INN.Length != CompanyConstants.PrivateBusinessmanInnLength && INN.Length != 0 && TypeOfOwnership == "ИП")
					{
						yield return new ValidationResult($"Длина ИНН для ИП должна равняться {CompanyConstants.PrivateBusinessmanInnLength}-ти.",
							new[] { nameof(INN) });
					}

					if(string.IsNullOrWhiteSpace(KPP) && TypeOfOwnership != "ИП")
					{
						yield return new ValidationResult("Для организации необходимо заполнить КПП.",
							new[] { nameof(KPP) });
					}

					if(string.IsNullOrWhiteSpace(INN))
					{
						yield return new ValidationResult("Для организации необходимо заполнить ИНН.",
							new[] { nameof(INN) });
					}

					if(KPP != null && !Regex.IsMatch(KPP, "^[0-9]*$") && TypeOfOwnership != "ИП")
					{
						yield return new ValidationResult("КПП может содержать только цифры.",
							new[] { nameof(KPP) });
					}

					if(!Regex.IsMatch(INN, "^[0-9]*$"))
					{
						yield return new ValidationResult("ИНН может содержать только цифры.",
							new[] { nameof(INN) });
					}

					if(!string.IsNullOrWhiteSpace(OGRN))
					{
						if(!Regex.IsMatch(OGRN, "^[0-9]*$"))
						{
							yield return new ValidationResult("ОГРН может содержать только цифры.",
								new[] { nameof(OGRN) });
						}

						if(TypeOfOwnership == "ИП" && OGRN.Length != CompanyConstants.PrivateBusinessmanOgrnLength)
						{
							yield return new ValidationResult(
								$"У ИП ОГРНИП состоит из {CompanyConstants.PrivateBusinessmanOgrnLength} символов",
								new[] { nameof(KPP) });
						}
						
						if(TypeOfOwnership != "ИП" && OGRN.Length != CompanyConstants.NotPrivateBusinessmanOgrnLength)
						{
							yield return new ValidationResult(
								$"ОГРН должен содержать {CompanyConstants.NotPrivateBusinessmanOgrnLength} символов",
								new[] { nameof(KPP) });
						}
					}
				}

				if(IsDeliveriesClosed && string.IsNullOrWhiteSpace(CloseDeliveryComment))
				{
					yield return new ValidationResult("Необходимо заполнить комментарий по закрытию поставок",
						new[] { nameof(CloseDeliveryComment) });
				}

				if(IsArchive)
				{
					var unclosedContracts = CounterpartyContracts.Where(c => !c.IsArchive)
						.Select(c => c.Id.ToString()).ToList();

					if(unclosedContracts.Count > 0)
					{
						yield return new ValidationResult(
							string.Format("Вы не можете сдать контрагента в архив с открытыми договорами: {0}",
								string.Join(", ", unclosedContracts)),
							new[] { nameof(CounterpartyContracts) });
					}

					var debt = orderRepository.GetCounterpartyDebt(uow, Id);

					if(debt != 0)
					{
						yield return new ValidationResult(
							$"Вы не можете сдать контрагента в архив так как у него имеется долг: {CurrencyWorks.GetShortCurrencyString(debt)}");
					}

					var activeOrders = orderRepository.GetCurrentOrders(uow, this);

					if(activeOrders.Count > 0)
					{
						yield return new ValidationResult(
							string.Format("Вы не можете сдать контрагента в архив с незакрытыми заказами: {0}",
								string.Join(", ", activeOrders.Select(o => o.Id.ToString()))),
							new[] { nameof(CounterpartyContracts) });
					}

					var deposit = depositRepository.GetDepositsAtCounterparty(uow, this, null);

					if(deposit != 0)
					{
						yield return new ValidationResult(
							$"Вы не можете сдать контрагента в архив так как у него есть невозвращенные залоги: {CurrencyWorks.GetShortCurrencyString(deposit)}");
					}

					var bottles = bottlesRepository.GetBottlesDebtAtCounterparty(uow, this);

					if(bottles != 0)
					{
						yield return new ValidationResult(
							$"Вы не можете сдать контрагента в архив так как он не вернул {bottles} бутылей");
					}
				}

				if(CameFrom == null
					&& (Id == 0 || commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CounterpartyPermissions.CanEditClientRefer)))
				{
					yield return new ValidationResult("Необходимо заполнить поле \"Откуда клиент\"");
				}

				if(CounterpartyType == CounterpartyType.Dealer && string.IsNullOrEmpty(OGRN))
				{
					yield return new ValidationResult("Для дилеров необходимо заполнить поле \"ОГРН\"");
				}

				if(Id == 0 && PersonType == PersonType.legal && TaxType == TaxType.None)
				{
					yield return new ValidationResult("Для новых клиентов необходимо заполнить поле \"Налогообложение\"");
				}

				var everyAddedMinCountValueCount = NomenclatureFixedPrices
					.GroupBy(p => new { p.Nomenclature, p.MinCount })
					.Select(p => new { NomenclatureName = p.Key.Nomenclature?.Name, MinCountValue = p.Key.MinCount, Count = p.Count() });

				foreach(var p in everyAddedMinCountValueCount)
				{
					if(p.Count > 1)
					{
						yield return new ValidationResult(
							$"\"{p.NomenclatureName}\": фиксированная цена для количества \"{p.MinCountValue}\" указана {p.Count} раз(а)",
							new[] { nameof(NomenclatureFixedPrices) });
					}
				}

				foreach(var fixedPrice in NomenclatureFixedPrices)
				{
					var fixedPriceValidationResults = fixedPrice.Validate(validationContext);
					foreach(var fixedPriceValidationResult in fixedPriceValidationResults)
					{
						yield return fixedPriceValidationResult;
					}
				}

				if(Id == 0 && UseSpecialDocFields)
				{
					if(!string.IsNullOrWhiteSpace(SpecialContractName)
						&& (string.IsNullOrWhiteSpace(SpecialContractNumber) || !SpecialContractDate.HasValue))
					{
						yield return new ValidationResult("Помимо специального названия договора надо заполнить его номер и дату");
					}

					if(!string.IsNullOrWhiteSpace(SpecialContractNumber)
						&& (string.IsNullOrWhiteSpace(SpecialContractName) || !SpecialContractDate.HasValue))
					{
						yield return new ValidationResult("Помимо специального номера договора надо заполнить его название и дату");
					}

					if(SpecialContractDate.HasValue
						&& (string.IsNullOrWhiteSpace(SpecialContractNumber) || string.IsNullOrWhiteSpace(SpecialContractName)))
					{
						yield return new ValidationResult("Помимо специальной даты договора надо заполнить его название и номер");
					}
				}

				if(UseSpecialDocFields)
				{
					if(!string.IsNullOrWhiteSpace(SpecialContractName) && SpecialContractName.Length > _specialContractNameLimit)
					{
						yield return new ValidationResult(
							$"Длина наименования особого договора превышена на {SpecialContractName.Length - _specialContractNameLimit}");
					}
				}

				if(TechnicalProcessingDelay > 0 && AttachedFileInformations.Count == 0)
				{
					yield return new ValidationResult("Для установки дней отсрочки тех обработки необходимо загрузить документ");
				}

				var phonesValidationStringBuilder = new StringBuilder();
				var phoneNumberDuplicatesIsChecked = new List<string>();

				var phonesDuplicates =
					counterpartyRepository.GetNotArchivedCounterpartiesAndDeliveryPointsDescriptionsByPhoneNumber(uow, Phones.Where(p => !p.IsArchive).ToList(), Id);

				foreach(var phone in phonesDuplicates)
				{
					phonesValidationStringBuilder.AppendLine($"Телефон {phone.Key} уже указан у контрагентов:");

					foreach(var message in phone.Value)
					{
						phonesValidationStringBuilder.AppendLine($"\t{message}");
					}
				}

				foreach(var phone in Phones)
				{
					if(phone.RoboAtsCounterpartyName == null)
					{
						phonesValidationStringBuilder.AppendLine($"Для телефона {phone.Number} не указано имя контрагента.");
					}

					if(phone.RoboAtsCounterpartyPatronymic == null)
					{
						phonesValidationStringBuilder.AppendLine($"Для телефона {phone.Number} не указано отчество контрагента.");
					}

					if(!phone.IsValidPhoneNumber)
					{
						phonesValidationStringBuilder.AppendLine($"Номер {phone.Number} имеет неправильный формат.");
					}

					#region Проверка дубликатов номера телефона

					if(!phoneNumberDuplicatesIsChecked.Contains(phone.Number))
					{
						if(Phones.Where(p => p.Number == phone.Number).Count() > 1)
						{
							phonesValidationStringBuilder.AppendLine(
								$"Телефон {phone.Number} в карточке контрагента указан несколько раз.");
						}

						phoneNumberDuplicatesIsChecked.Add(phone.Number);
					}

					#endregion
				}

				var phonesValidationMessage = phonesValidationStringBuilder.ToString();

				if(!string.IsNullOrEmpty(phonesValidationMessage))
				{
					yield return new ValidationResult(phonesValidationMessage);
				}

				if((ReasonForLeaving == ReasonForLeaving.Resale || ReasonForLeaving == ReasonForLeaving.Tender)
				   && string.IsNullOrWhiteSpace(INN))
				{
					yield return new ValidationResult("Для перепродажи должен быть заполнен ИНН");
				}

				if(IsNotSendDocumentsByEdo && IsPaperlessWorkflow)
				{
					yield return new ValidationResult(
						"При выборе \"Не отправлять документы по EDO\" должен быть отключен \"Отказ от печатных документов\"");
				}

				if(IsNotSendEquipmentTransferByEdo && IsPaperlessWorkflow)
				{
					yield return new ValidationResult(
						"При выборе \"Не отправлять акты приёма-передачи по EDO\" должен быть отключен \"Отказ от печатных документов\"");
				}

				foreach(var email in Emails)
				{
					if(!email.IsValidEmail)
					{
						yield return new ValidationResult($"Адрес электронной почты {email.Address} имеет неправильный формат.");
					}
				}
			}

			if(CameFrom != null && Referrer == null && CameFrom.Id == counterpartySettings.ReferFriendPromotionCameFromId)
			{
				yield return new ValidationResult("Не выбран клиент, который привёл друга");
			}

			if(Referrer?.Id == Id)
			{
				yield return new ValidationResult("Клиент не мог привести сам себя");
			}

			#region Counterparty Edo account duplicates

			var counterpartyEdoAccountDuplicates = CounterpartyEdoAccounts?
				.Where(x => !string.IsNullOrWhiteSpace(x.PersonalAccountIdInEdo))
				.GroupBy(a => new { a.OrganizationId, a.PersonalAccountIdInEdo })
				.Where(g => g.Count() > 1)
				.Select(g => g.First())
				.ToArray();

			if(counterpartyEdoAccountDuplicates != null && counterpartyEdoAccountDuplicates.Any())
			{
				yield return new ValidationResult(
					$"Найдены дубликаты аккаунтов ЭДО в рамках одной организации: " +
					$"{string.Join(", ", counterpartyEdoAccountDuplicates.Select(x => x.PersonalAccountIdInEdo))}");
			}

			#endregion Counterparty Edo account duplicates
		}

		#endregion
	}
}
