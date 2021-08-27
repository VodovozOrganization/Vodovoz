using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalNodes;
using Vodovoz.Services;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class DistrictsSetJournalActionsViewModel : EntitiesJournalActionsViewModel, IDisposable
	{
		private readonly ICommonServices _commonServices;
		private readonly IEmployeeService _employeeService;
		private readonly IUnitOfWork _uow;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
		private readonly bool _canCreate;
		private string _onlinesText;
		private DelegateCommand _copyDistrictSetCommand;
		private DelegateCommand _updateOnlinesCommand;
		
		public DistrictsSetJournalActionsViewModel(
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IUnitOfWorkFactory unitOfWorkFactory,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider
			) : base(commonServices?.InteractiveService)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_deliveryRulesParametersProvider = 
				deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));

			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			
			_uow = unitOfWorkFactory.CreateWithoutRoot();

			_canCreate = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet)).CanCreate;
			CanChangeOnlineDeliveriesToday = 
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_online_deliveries_today");
			
			SetIsStoppedOnlineDeliveriesTodayAndOnlinesText();
		}
		
		private bool IsStoppedOnlineDeliveriesToday { get; set; }

		public override IList<object> SelectedItems 
		{
			get => selectedItems;
			set 
			{
				if (SetField(ref selectedItems, value)) 
				{
					foreach(var action in JournalActions)
					{
						action.OnPropertyChanged(nameof(action.Sensitive));
					}
					OnPropertyChanged(nameof(CanCopyDistrictSet));
				}
			}
		}

		public bool CanCopyDistrictSet => _canCreate && SelectedItems.OfType<DistrictsSetJournalNode>().FirstOrDefault() != null;
		
		public bool CanChangeOnlineDeliveriesToday { get; }

		public string OnlinesText
		{
			get => _onlinesText;
			private set => SetField(ref _onlinesText, value);
		}
		
		public DelegateCommand CopyDistrictSetCommand => _copyDistrictSetCommand ?? (_copyDistrictSetCommand = new DelegateCommand(
				() =>
				{
					var selectedNode = SelectedItems.OfType<DistrictsSetJournalNode>().FirstOrDefault();

					if(selectedNode == null)
					{
						return;
					}

					var districtsSetToCopy = _uow.GetById<DistrictsSet>(selectedNode.Id);
					var alreadyCopiedDistrict = _uow.Session.QueryOver<District>()
						.WhereRestrictionOn(x => x.CopyOf.Id)
						.IsIn(districtsSetToCopy.Districts.Select(x => x.Id).ToArray())
						.Take(1)
						.SingleOrDefault();
					
					if(alreadyCopiedDistrict != null) 
					{
						InteractiveService.ShowMessage(ImportanceLevel.Warning,
							$"Выбранная версия районов уже была скопирована\n" +
							$"Копия: (Код: {alreadyCopiedDistrict.DistrictsSet.Id}) {alreadyCopiedDistrict.DistrictsSet.Name}");
						return;
					}
					
					if(InteractiveService.Question($"Скопировать версию районов \"{selectedNode.Name}\"")) 
					{
						var copy = (DistrictsSet)districtsSetToCopy.Clone();
						copy.Name += " - копия";
						copy.Author = _employeeService.GetEmployeeForUser(_uow, _commonServices.UserService.CurrentUserId);
						copy.Status = DistrictsSetStatus.Draft;
						copy.DateCreated = DateTime.Now;
						
						_uow.Save(copy);
						_uow.Commit();
						InteractiveService.ShowMessage(ImportanceLevel.Info, "Копирование завершено");
					}
				},
				() => CanCopyDistrictSet
			)
		);
		
		public DelegateCommand UpdateOnlinesCommand => _updateOnlinesCommand ?? (_updateOnlinesCommand = new DelegateCommand(
				() =>
				{
					_deliveryRulesParametersProvider.UpdateOnlineDeliveriesTodayParameter($"{!IsStoppedOnlineDeliveriesToday}");
					SetIsStoppedOnlineDeliveriesTodayAndOnlinesText();
				}
			)
		);
		
		private void SetIsStoppedOnlineDeliveriesTodayAndOnlinesText()
		{
			IsStoppedOnlineDeliveriesToday = _deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday;
			OnlinesText = IsStoppedOnlineDeliveriesToday ? "Запустить онлайны" : "Остановить онлайны";
		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}