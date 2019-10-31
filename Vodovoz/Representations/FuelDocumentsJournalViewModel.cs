using System;
using QS.DomainModel.Entity;
using QS.RepresentationModel.GtkUI;
using Vodovoz.Core.Journal;
using Vodovoz.Domain.Fuel;
using Gamma.Utilities;
using QS.Utilities.Text;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using QS.DomainModel.UoW;
using Gamma.ColumnConfig;
using NHibernate.Transform;
using Vodovoz.Dialogs.Fuel;
using Vodovoz.Infrastructure.Services;
using Vodovoz.EntityRepositories.Subdivisions;
using NHibernate.Criterion;
using Vodovoz.EntityRepositories.Fuel;
using QS.Services;
using QS.Project.Domain;
using Vodovoz.Domain.Cash;
using System.Linq;

namespace Vodovoz.Representations
{
	public class FuelDocumentsJournalViewModel : MultipleEntityModelBase<FuelDocumentVMNode>
	{
		private readonly IEmployeeService employeeService;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly ICommonServices services;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly IFuelRepository fuelRepository;
		private readonly IRepresentationEntityPicker representationEntityPicker;

		public FuelDocumentsJournalViewModel(
			IEmployeeService employeeService,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices services, 
			ISubdivisionRepository subdivisionRepository,
			IFuelRepository fuelRepository, 
			IRepresentationEntityPicker representationEntityPicker)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.services = services ?? throw new ArgumentNullException(nameof(services));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			this.representationEntityPicker = representationEntityPicker ?? throw new ArgumentNullException(nameof(representationEntityPicker));

			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			RepresentationFilter = null;
			JournalFilter = null;

			RegisterIncomeInvoice();
			RegisterTransferDocument();
			RegisterWriteoffDocument();
			
			UpdateOnChanges(
				typeof(FuelIncomeInvoice),
				typeof(FuelIncomeInvoiceItem),
				typeof(FuelTransferDocument),
				typeof(FuelWriteoffDocument),
				typeof(FuelWriteoffDocumentItem)
			);

			AfterSourceFillFunction = (list) => {
				return list.OrderByDescending(x => x.CreationDate).ToList();
			};

			TreeViewConfig = FluentColumnsConfig<FuelDocumentVMNode>.Create()
				.AddColumn("№").AddTextRenderer(node => node.DocumentId.ToString())
				.AddColumn("Тип").AddTextRenderer(node => node.DisplayName)
				.AddColumn("Дата").AddTextRenderer(node => node.CreationDate.ToShortDateString())
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.AddColumn("Сотрудник").AddTextRenderer(node => node.Employee)
				.AddColumn("Статус").AddTextRenderer(node => node.Status)
				.AddColumn("Литры").AddTextRenderer(node => node.Liters.ToString("0"))
				.AddColumn("Статья расх.").AddTextRenderer(node => node.ExpenseCategory)

				.AddColumn("Отправлено из").AddTextRenderer(node => node.SubdivisionFrom)
				.AddColumn("Время отпр.").AddTextRenderer(node => node.SendTime.HasValue ? node.SendTime.Value.ToShortDateString() : "")

				.AddColumn("Отправлено в").AddTextRenderer(node => node.SubdivisionTo)
				.AddColumn("Время принятия").AddTextRenderer(node => node.ReceiveTime.HasValue ? node.ReceiveTime.Value.ToShortDateString() : "")

				.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
				.Finish();
		}

		public override string GetSummaryInfo()
		{
			var balance = fuelRepository.GetAllFuelsBalance(UoW);

			string result = "";

			foreach(var item in balance) {
				result += $"{item.Key.Name}: {item.Value.ToString("0")} л., ";
			}
			result.Trim(' ', ',');
			return result;
		}

		private void RegisterIncomeInvoice()
		{
			FuelDocumentVMNode resultAlias = null;
			var fuelIncomeInvoiceConfig = RegisterEntity<FuelIncomeInvoice>();
			//функция получения данных
			fuelIncomeInvoiceConfig.AddDataFunction(() => {
				IList<FuelDocumentVMNode> fuelIncomeInvoiceResultList = new List<FuelDocumentVMNode>();

				FuelIncomeInvoice fuelIncomeInvoiceAlias = null;
				FuelIncomeInvoiceItem fuelIncomeInvoiceItemAlias = null;
				Employee authorAlias = null;
				Subdivision subdivisionToAlias = null;
				var fuelIncomeInvoiceQuery = UoW.Session.QueryOver<FuelIncomeInvoice>(() => fuelIncomeInvoiceAlias);

				fuelIncomeInvoiceResultList = fuelIncomeInvoiceQuery
					.Left.JoinQueryOver(() => fuelIncomeInvoiceAlias.Author, () => authorAlias)
					.Left.JoinQueryOver(() => fuelIncomeInvoiceAlias.Subdivision, () => subdivisionToAlias)
					.Left.JoinQueryOver(() => fuelIncomeInvoiceAlias.FuelIncomeInvoiceItems, () => fuelIncomeInvoiceItemAlias)
					.SelectList(list => list
						.SelectGroup(() => fuelIncomeInvoiceAlias.Id).WithAlias(() => resultAlias.DocumentId)
						.Select(() => fuelIncomeInvoiceAlias.СreationTime).WithAlias(() => resultAlias.CreationDate)
						.Select(() => fuelIncomeInvoiceAlias.Comment).WithAlias(() => resultAlias.Comment)
						.Select(Projections.Sum(Projections.Property(() => fuelIncomeInvoiceItemAlias.Liters))).WithAlias(() => resultAlias.Liters)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
						.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
						.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

						.Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo)
					)

					.TransformUsing(Transformers.AliasToBean<FuelDocumentVMNode<FuelIncomeInvoice>>())
					.List<FuelDocumentVMNode>();

				return fuelIncomeInvoiceResultList;
			});

			fuelIncomeInvoiceConfig.AddViewModelDocumentConfiguration<FuelIncomeInvoiceViewModel>(
				//функция идентификации документа 
				(FuelDocumentVMNode node) => node.EntityType == typeof(FuelIncomeInvoice),
				//заголовок действия для создания нового документа
				"Входящая накладная",
				//функция диалога создания документа
				() => new FuelIncomeInvoiceViewModel(EntityUoWBuilder.ForCreate(), unitOfWorkFactory, employeeService, representationEntityPicker, subdivisionRepository, fuelRepository, services),
				//функция диалога открытия документа
				(node) => new FuelIncomeInvoiceViewModel(EntityUoWBuilder.ForOpen(node.DocumentId), unitOfWorkFactory, employeeService, representationEntityPicker, subdivisionRepository, fuelRepository, services)
			);

			//завершение конфигурации
			fuelIncomeInvoiceConfig.FinishConfiguration();
		}

		private void RegisterTransferDocument()
		{
			FuelDocumentVMNode resultAlias = null;
			var fuelTransferConfig = RegisterEntity<FuelTransferDocument>();
			//функция получения данных
			fuelTransferConfig.AddDataFunction(() => {
				IList<FuelDocumentVMNode> fuelTransferResultList = new List<FuelDocumentVMNode>();

				FuelTransferDocument fuelTransferAlias = null;
				Employee authorAlias = null;
				Subdivision subdivisionFromAlias = null;
				Subdivision subdivisionToAlias = null;
				var fuelTransferQuery = UoW.Session.QueryOver<FuelTransferDocument>(() => fuelTransferAlias);

				fuelTransferResultList = fuelTransferQuery
					.Left.JoinQueryOver(() => fuelTransferAlias.Author, () => authorAlias)
					.Left.JoinQueryOver(() => fuelTransferAlias.CashSubdivisionFrom, () => subdivisionFromAlias)
					.Left.JoinQueryOver(() => fuelTransferAlias.CashSubdivisionTo, () => subdivisionToAlias)
					.SelectList(list => list
						.Select(() => fuelTransferAlias.Id).WithAlias(() => resultAlias.DocumentId)
						.Select(() => fuelTransferAlias.CreationTime).WithAlias(() => resultAlias.CreationDate)
						.Select(() => fuelTransferAlias.Status).WithAlias(() => resultAlias.TransferDocumentStatus)
						.Select(() => fuelTransferAlias.TransferedLiters).WithAlias(() => resultAlias.Liters)
						.Select(() => fuelTransferAlias.Comment).WithAlias(() => resultAlias.Comment)
						.Select(() => fuelTransferAlias.SendTime).WithAlias(() => resultAlias.SendTime)
						.Select(() => fuelTransferAlias.ReceiveTime).WithAlias(() => resultAlias.ReceiveTime)

						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
						.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
						.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

						.Select(() => subdivisionFromAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom)
						.Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo)
					)

					.TransformUsing(Transformers.AliasToBean<FuelDocumentVMNode<FuelTransferDocument>>())
					.List<FuelDocumentVMNode>();

				return fuelTransferResultList;
			});

			fuelTransferConfig.AddViewModelDocumentConfiguration<FuelTransferDocumentViewModel>(
				//функция идентификации документа 
				(FuelDocumentVMNode node) => node.EntityType == typeof(FuelTransferDocument),
				//заголовок действия для создания нового документа
				"Перемещение",
				//функция диалога создания документа
				() =>  new FuelTransferDocumentViewModel(EntityUoWBuilder.ForCreate(), unitOfWorkFactory, employeeService, subdivisionRepository, fuelRepository, services),
				//функция диалога открытия документа
				(node) => new FuelTransferDocumentViewModel(EntityUoWBuilder.ForOpen(node.DocumentId), unitOfWorkFactory, employeeService, subdivisionRepository, fuelRepository, services)
			);

			//завершение конфигурации
			fuelTransferConfig.FinishConfiguration();
		}

		private void RegisterWriteoffDocument()
		{
			FuelDocumentVMNode resultAlias = null;
			var fuelTransferConfig = RegisterEntity<FuelWriteoffDocument>();
			//функция получения данных
			fuelTransferConfig.AddDataFunction(() => {
				IList<FuelDocumentVMNode> fuelWriteoffResultList = new List<FuelDocumentVMNode>();

				FuelWriteoffDocument fuelWriteoffAlias = null;
				Employee cashierAlias = null;
				Employee employeeAlias = null;
				Subdivision subdivisionAlias = null;
				ExpenseCategory expenseCategoryAlias = null;
				FuelWriteoffDocumentItem fuelWriteoffItemAlias = null;
				var fuelWriteoffQuery = UoW.Session.QueryOver<FuelWriteoffDocument>(() => fuelWriteoffAlias);

				fuelWriteoffResultList = fuelWriteoffQuery
					.Left.JoinQueryOver(() => fuelWriteoffAlias.Cashier, () => cashierAlias)
					.Left.JoinQueryOver(() => fuelWriteoffAlias.Employee, () => employeeAlias)
					.Left.JoinQueryOver(() => fuelWriteoffAlias.CashSubdivision, () => subdivisionAlias)
					.Left.JoinQueryOver(() => fuelWriteoffAlias.ExpenseCategory, () => expenseCategoryAlias)
					.Left.JoinQueryOver(() => fuelWriteoffAlias.FuelWriteoffDocumentItems, () => fuelWriteoffItemAlias)
					.SelectList(list => list
						.SelectGroup(() => fuelWriteoffAlias.Id).WithAlias(() => resultAlias.DocumentId)
						.Select(() => fuelWriteoffAlias.Date).WithAlias(() => resultAlias.CreationDate)
						.Select(() => fuelWriteoffAlias.Reason).WithAlias(() => resultAlias.Comment)

						.Select(() => cashierAlias.Name).WithAlias(() => resultAlias.AuthorName)
						.Select(() => cashierAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
						.Select(() => cashierAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

						.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
						.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
						.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)

						.Select(() => expenseCategoryAlias.Name).WithAlias(() => resultAlias.ExpenseCategory)
						.Select(Projections.Sum(Projections.Property(() => fuelWriteoffItemAlias.Liters))).WithAlias(() => resultAlias.Liters)

						.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom)
					)

					.TransformUsing(Transformers.AliasToBean<FuelDocumentVMNode<FuelWriteoffDocument>>())
					.List<FuelDocumentVMNode>();

				return fuelWriteoffResultList;
			});

			fuelTransferConfig.AddViewModelDocumentConfiguration<FuelWriteoffDocumentViewModel>(
				//функция идентификации документа 
				(FuelDocumentVMNode node) => node.EntityType == typeof(FuelWriteoffDocument),
				//заголовок действия для создания нового документа
				"Акт выдачи топлива",
				//функция диалога создания документа
				() => new FuelWriteoffDocumentViewModel(EntityUoWBuilder.ForCreate(), unitOfWorkFactory, employeeService, fuelRepository, subdivisionRepository, services),
				//функция диалога открытия документа
				(node) => new FuelWriteoffDocumentViewModel(EntityUoWBuilder.ForOpen(node.DocumentId), unitOfWorkFactory, employeeService, fuelRepository, subdivisionRepository, services)
			);

			//завершение конфигурации
			fuelTransferConfig.FinishConfiguration();
		}
	}

	public class FuelDocumentVMNode<TEntity> : FuelDocumentVMNode
		where TEntity : class, IDomainObject
	{
		public FuelDocumentVMNode()
		{
			EntityType = typeof(TEntity);
		}
	}

	public class FuelDocumentVMNode : MultipleEntityVMNodeBase
	{
		#region MultipleDocumentJournalVMNodeBase implementation

		public override Type EntityType { get; set; }

		public override int DocumentId { get; set; }

		[UseForSearch]
		public override string DisplayName {
			get {
				if(EntityType == typeof(FuelIncomeInvoice)) {
					return "Входящая накладная";
				} else if(EntityType == typeof(FuelTransferDocument)) {
					return "Перемещение";
				} else if(EntityType == typeof(FuelWriteoffDocument)) {
					return "Акт выдачи";
				} else {
					return typeof(FuelTransferDocument).GetAttribute<AppellativeAttribute>(true)?.Nominative;
				}
			}
		}

		#endregion

		public DateTime CreationDate { get; set; }

		public FuelTransferDocumentStatuses TransferDocumentStatus { get; set; }
		public string Status {
			get {
				if(EntityType == typeof(FuelTransferDocument)) {
					return TransferDocumentStatus.GetEnumTitle();
				}
				return "";
			}
		}

		public decimal Liters { get; set; }
		public string SubdivisionFrom { get; set; }
		public string SubdivisionTo { get; set; }
		public DateTime? SendTime { get; set; }
		public DateTime? ReceiveTime { get; set; }

		public string AuthorSurname { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }
		[UseForSearch]
		public string Author => PersonHelper.PersonNameWithInitials(AuthorSurname, AuthorName, AuthorPatronymic);

		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }
		[UseForSearch]
		public string Employee => PersonHelper.PersonNameWithInitials(EmployeeSurname, EmployeeName, EmployeePatronymic);

		public string ExpenseCategory { get; set; }

		[UseForSearch]
		public string Comment { get; set; }
	}
}
