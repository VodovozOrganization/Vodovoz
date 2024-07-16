using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Dialogs.Roboats;

namespace Vodovoz.ViewModels.Dialogs.Logistic
{
	public class DeliveryScheduleViewModel : EntityTabViewModelBase<DeliverySchedule>
	{
		private readonly ICommonServices _commonServices;
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;
		private readonly IRoboatsViewModelFactory _roboatsViewModelFactory;
		private readonly bool _canEdit;
		private readonly bool _canCreate;

		private bool _isDefaultName;

		public DeliveryScheduleViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDeliveryScheduleRepository deliveryScheduleRepository,
			IRoboatsViewModelFactory roboatsViewModelFactory
		) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			Entity.PropertyChanged += Entity_PropertyChanged;
			RoboatsEntityViewModel = _roboatsViewModelFactory.CreateViewModel(Entity);

			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DeliverySchedule));
			_canEdit = permissionResult.CanUpdate;
			_canCreate = permissionResult.CanCreate;
		}

		public bool CanEdit => _canEdit || (_canCreate && UoW.IsNew);

		private RoboatsEntityViewModel _roboatsEntityViewModel;

		public RoboatsEntityViewModel RoboatsEntityViewModel
		{
			get => _roboatsEntityViewModel;
			set => SetField(ref _roboatsEntityViewModel, value);
		}

		public override bool Save(bool close)
		{
			var all = _deliveryScheduleRepository.All(UoWGeneric);

			var notArchivedList = all
				.Where(ds => ds.IsArchive == false
					&& ds.From == Entity.From
					&& ds.To == Entity.To
					&& ds.Id != Entity.Id)
				.ToList();

			if(notArchivedList.Any() && UoWGeneric.Root.IsArchive == false)
			{
				//при архивировании интервала эти проверки не нужны
				//есть вероятность, что среди активных интервалов есть дубликаты, так что берем первый
				var active = notArchivedList.First();
				var message = "Уже существует интервал с таким же периодом.\n" +
					"Создание нового интервала невозможно.\n" +
					"Существующий интервал:\n" +
					$"Код: {active.Id}\n" +
					$"Название: {active.Name}\n" +
					$"Период: {active.DeliveryTime}\n";

				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
				return false; // нашли активный
			}

			var archivedList = all
				.Where(ds => ds.IsArchive
					&& ds.From == Entity.From
					&& ds.To == Entity.To)
				.ToList();

			if(UoW.IsNew && archivedList.Any() && UoWGeneric.Root.IsArchive == false)
			{
				//при архивировании интервала эти проверки не нужны
				//т.к. интервалы нельзя удалять, архивными могут быть несколько, так что берем первый
				var archived = archivedList.First();
				var message = "Уже существует архивный интервал с таким же периодом.\n" +
					"Создание нового интервала невозможно.\n" +
					"Разархивировать существующий интервал?";
				if(_commonServices.InteractiveService.Question(message))
				{
					//отменяем изменения текущей сущности интервала и обновляем найденный архивный
					UoWGeneric.Delete(UoWGeneric.Root);
					archived.IsArchive = false;
					UoWGeneric.Save(archived);
					UoWGeneric.Commit();
					var infoMessage = "Разархивирован интервал:\n" +
						$"Код: {archived.Id}\n" +
						$"Название: {archived.Name}\n" +
						$"Период: {archived.DeliveryTime}\n";
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, infoMessage);
				}
				return false; // нашли/разархивировали старый
			}

			return base.Save(close);
		}

		protected override bool BeforeSave()
		{
			return RoboatsEntityViewModel.Save();
		}

		public void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if(TabParent == null)
			{
				OnTabClosed();
			}
			else
			{
				base.Close(askSave, source);
			}
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(Entity.From):
				case nameof(Entity.To):
					GenerateDefaultName();
					break;
				default:
					break;
			}
		}

		private void GenerateDefaultName()
		{
			_isDefaultName = string.IsNullOrWhiteSpace(Entity.Name);
			if(!UoW.IsNew || !_isDefaultName)
			{
				return;
			}
			_isDefaultName = true;
			Entity.Name = $"{VeryShortTime(Entity.From)}-{VeryShortTime(Entity.To)}";
		}

		private string VeryShortTime(TimeSpan time)
		{
			return (time.Minutes == 0) ? $"{time.Hours}" : $"{time.Hours}:{time.Minutes}";
		}
	}
}
