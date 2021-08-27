using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class FinancialDistrictsSetJournalActionsViewModel : EntitiesJournalActionsViewModel, IDisposable
	{
		private readonly ICommonServices _commonServices;
		private readonly IEmployeeService _employeeService;
		private readonly IUnitOfWork _uow;
		private readonly bool _canCreate;
		private DelegateCommand _copyDistrictSetCommand;

		public FinancialDistrictsSetJournalActionsViewModel(
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IUnitOfWorkFactory unitOfWorkFactory) : base(commonServices?.InteractiveService)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));

			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_uow = unitOfWorkFactory.CreateWithoutRoot();

			_canCreate = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FinancialDistrictsSet)).CanCreate;
		}

		public override IList<object> SelectedItems
		{
			get => selectedItems;
			set
			{
				if(SetField(ref selectedItems, value))
				{
					foreach(var action in JournalActions)
					{
						action.OnPropertyChanged(nameof(action.Sensitive));
						action.OnPropertyChanged(nameof(action.Visible)); //TODO удостовериться, что действительно надо дергать visible
					}
					OnPropertyChanged(nameof(CanCopyFinancialDistrictSet));
				}
			}
		}

		public bool CanCopyFinancialDistrictSet =>
			_canCreate && SelectedItems.OfType<FinancialDistrictsSetsJournalNode>().FirstOrDefault() != null;

		public DelegateCommand CopyDistrictSetCommand => _copyDistrictSetCommand ?? (_copyDistrictSetCommand = new DelegateCommand(
			() =>
				{
					var selectedNode = SelectedItems.OfType<FinancialDistrictsSetsJournalNode>().FirstOrDefault();

					if(selectedNode == null)
					{
						return;
					}

					var districtsSetToCopy = _uow.GetById<FinancialDistrictsSet>(selectedNode.Id);
					var alreadyCopiedDistrict =
						_uow.Session.QueryOver<FinancialDistrict>()
							.WhereRestrictionOn(x => x.CopyOf.Id)
							.IsIn(districtsSetToCopy.FinancialDistricts.Select(x => x.Id).ToArray())
							.Take(1)
							.SingleOrDefault();

					if(alreadyCopiedDistrict != null)
					{
						InteractiveService.ShowMessage(ImportanceLevel.Warning,
							$"Выбранная версия районов уже была скопирована\n" +
							$"Копия: {alreadyCopiedDistrict.FinancialDistrictsSet.Id} {alreadyCopiedDistrict.FinancialDistrictsSet.Name}");
						return;
					}

					if(InteractiveService.Question($"Скопировать версию районов \"{selectedNode.Name}\""))
					{
						var copy = districtsSetToCopy.Clone() as FinancialDistrictsSet;
						copy.Name += " - копия";
						copy.Author = _employeeService.GetEmployeeForUser(_uow, _commonServices.UserService.CurrentUserId);
						copy.Status = DistrictsSetStatus.Draft;
						copy.DateCreated = DateTime.Now;

						_uow.Save(copy);
						_uow.Commit();
						InteractiveService.ShowMessage(ImportanceLevel.Info, "Копирование завершено");
					}
				},
			() => CanCopyFinancialDistrictSet
			)
		);

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}