using System;
using System.Collections.Generic;
using System.Linq;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Service;

namespace Vodovoz
{
	public partial class CarUnloadDocumentDlg : OrmGtkDialogBase<CarUnloadDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		IList<Equipment> alreadyUnloadedEquipment;

		public CarUnloadDocumentDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CarUnloadDocument> ();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			if (CurrentUserSettings.Settings.DefaultWarehouse != null)
				Entity.Warehouse = UoWGeneric.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);

			ConfigureDlg ();
		}

		public CarUnloadDocumentDlg (int routeListId, int? warehouseId) : this()
		{
			if(warehouseId.HasValue)
				Entity.Warehouse = UoW.GetById<Warehouse>(warehouseId.Value);
			Entity.RouteList = UoW.GetById<RouteList>(routeListId);
			UpdateRouteListInfo();
		}

		public CarUnloadDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CarUnloadDocument> (id);
			ConfigureDlg ();
		}

		public CarUnloadDocumentDlg (CarUnloadDocument sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			yentryrefWarehouse.SubjectType = typeof(Warehouse);

			bottlereceptionview1.UoW = UoW;
			returnsreceptionview1.UoW = UoW;

			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new RouteListsFilter(UoW);
			filter.RestrictStatus = RouteListStatus.EnRoute;
			yentryrefRouteList.RepresentationModel = new ViewModel.RouteListsVM(filter);
			yentryrefRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();


			returnsreceptionview1.Warehouse = Entity.Warehouse;

			UpdateWidgetsVisible();
			if(!UoW.IsNew)
				LoadReception();
		}

		public override bool Save ()
		{
			UpdateReceivedItemsOnEntity();

			var valid = new QSValidation.QSValidator<CarUnloadDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info ("Сохраняем разгрузочный талон...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		void UpdateRouteListInfo()
		{
			if(Entity.RouteList == null)
			{
				ytextviewRouteListInfo.Buffer.Text = String.Empty;
				return;
			}

			ytextviewRouteListInfo.Buffer.Text =
				String.Format ("Маршрутный лист №{0} от {1:d}\nВодитель: {2}\nМашина: {3}({4})\nЭкспедитор: {5}",
					Entity.RouteList.Id,
					Entity.RouteList.Date,
					Entity.RouteList.Driver.FullName,
					Entity.RouteList.Car.Model,
					Entity.RouteList.Car.RegistrationNumber,
					Entity.RouteList.Forwarder != null ? Entity.RouteList.Forwarder.FullName : "(Отсутствует)" 
				);
		}

		void UpdateAlreadyUnloaded()
		{
			alreadyUnloadedEquipment = Repository.EquipmentRepository.GetEquipmentUnloadedTo(UoW, Entity.RouteList);
			returnsreceptionview1.AlreadyUnloadedEquipment = alreadyUnloadedEquipment;
		}

		void SetupForNewRouteList()
		{
			UpdateRouteListInfo();
			if (Entity.RouteList != null)
			{
				UpdateAlreadyUnloaded();
			}
			equipmentreceptionview1.RouteList = Entity.RouteList;
			returnsreceptionview1.RouteList = Entity.RouteList;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(CarUnloadDocument), "талона"))
				Save ();

			var reportInfo = new QSReport.ReportInfo
				{
					Title = Entity.Title,
					Identifier = "Store.CarUnloadDoc",
					Parameters = new System.Collections.Generic.Dictionary<string, object>
					{
						{ "id",  Entity.Id }
					}
				};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo),
				this);
		}

		private void UpdateWidgetsVisible()
		{
			bottlereceptionview1.Visible = Entity.Warehouse != null && Entity.Warehouse.CanReceiveBottles;
			equipmentreceptionview1.Visible = Entity.Warehouse != null && Entity.Warehouse.CanReceiveEquipment;
		}

		void LoadReception()
		{
			foreach(var item in Entity.Items)
			{
				var bottle = bottlereceptionview1.Items.FirstOrDefault(x => x.NomenclatureId == item.MovementOperation.Nomenclature.Id);
				if(bottle != null)
				{
					bottle.Amount = (int)item.MovementOperation.Amount;
					continue;
				}

				var returned = item.MovementOperation.Equipment != null
					? returnsreceptionview1.Items.FirstOrDefault(x => x.EquipmentId == item.MovementOperation.Equipment.Id)
					: returnsreceptionview1.Items.FirstOrDefault(x => x.NomenclatureId == item.MovementOperation.Nomenclature.Id);
				if(returned != null)
				{
					returned.Amount = (int)item.MovementOperation.Amount;
					continue;
				}

				if (item.MovementOperation.Equipment != null)
				{
					var equipment = equipmentreceptionview1.Items.FirstOrDefault(x => x.EquipmentId == item.MovementOperation.Equipment.Id);
					if (equipment != null)
					{
						equipment.Amount = (int)item.MovementOperation.Amount;
						continue;
					}
					else
					{
						equipmentreceptionview1.Items.Add(new ReceptionEquipmentItemNode
							{
								Amount = (int)item.MovementOperation.Amount,
								EquipmentId = item.MovementOperation.Equipment.Id,
								Returned = true,
								ServiceClaim = item.ServiceClaim,
								Name = item.MovementOperation.Nomenclature.Name
							});
					}
				}
				else
					throw new InvalidProgramException(String.Format("В документе присутствует строка ID {0}, которую не удалось отнести не к одному виджету.", item.Id));
			}

			foreach(var item in bottlereceptionview1.Items)
			{
				var returned = Entity.Items.FirstOrDefault(x => x.MovementOperation.Nomenclature.Id == item.NomenclatureId);
				item.Amount = returned != null ? (int)returned.MovementOperation.Amount : 0;
			}

			foreach(var item in equipmentreceptionview1.Items)
			{
				var returned = Entity.Items.FirstOrDefault(x => x.MovementOperation.Equipment.Id == item.EquipmentId);
				item.Amount = returned != null ? (int)returned.MovementOperation.Amount : 0;
			}

		}

		void UpdateReceivedItemsOnEntity()
		{
			//Собираем список всего на возврат из разных виджетов.
			var tempItemList = new List<InternalItem>();

			foreach (var node in bottlereceptionview1.Items) 
			{
				if (node.Amount == 0)
					continue;

				var item = new InternalItem {
						ReciveType = ReciveTypes.Bottle,
						NomenclatureId = node.NomenclatureId,
						Amount = node.Amount
					};
					tempItemList.Add(item);
			}

			foreach (var node in returnsreceptionview1.Items) 
			{
				if (node.Amount == 0)
					continue;

				var	item = new InternalItem {
					ReciveType = ReciveTypes.Returnes,
						NomenclatureId = node.NomenclatureId,
						EquipmentId = node.EquipmentId,
						Amount = node.Amount
					};
					tempItemList.Add(item);
			}

			foreach (var node in equipmentreceptionview1.Items) 
			{
				if (node.Amount == 0)
					continue;

				var	item = new InternalItem {
					ReciveType = ReciveTypes.Equipment,
						NomenclatureId = node.NomenclatureId,
						EquipmentId = node.EquipmentId,
						Amount = node.Amount,
						ServiceClaim = node.ServiceClaim
					};
					tempItemList.Add(item);

				node.ServiceClaim.UoW = UoW;
				if (node.IsNew)
				{
					node.NewEquipment.AssignedToClient = node.ServiceClaim.Counterparty;
					UoW.Save(node.NewEquipment);
					node.ServiceClaim.FillNewEquipment(node.NewEquipment);
				}
				//FIXME предположительно нужно возвращать статус заявки если поступление удаляется.
				if(node.ServiceClaim.Status == ServiceClaimStatus.PickUp)
				{
					node.ServiceClaim.AddHistoryRecord(ServiceClaimStatus.DeliveredToWarehouse,
						String.Format("Поступил на склад '{0}', по талону разгрузки №{1} для МЛ №{2}", 
							Entity.Warehouse.Name,
							Entity.Id,
							Entity.RouteList.Id
						)
					);
				}
				UoW.Save(node.ServiceClaim);
			}

			//Обновляем Entity
			var nomenclatures = UoW.GetById<Nomenclature>(tempItemList.Select(x => x.NomenclatureId).ToArray());
			var equipments = UoW.GetById<Equipment>(tempItemList.Select(x => x.EquipmentId).ToArray());
			foreach (var tempItem in tempItemList) {
				var item = tempItem.EquipmentId > 0
					? Entity.Items.FirstOrDefault(x => x.MovementOperation.Equipment?.Id == tempItem.EquipmentId)
					: Entity.Items.FirstOrDefault(x => x.MovementOperation.Nomenclature.Id == tempItem.NomenclatureId);
				if (item == null) {
					var nom = nomenclatures.First(x => x.Id == tempItem.NomenclatureId);
					var equ = equipments.FirstOrDefault(x => x.Id == tempItem.EquipmentId);
					Entity.AddItem(
						tempItem.ReciveType,
						nom,
						equ,
						tempItem.Amount,
						tempItem.ServiceClaim
					);
				}
				else
				{
					if(item.MovementOperation.Amount != tempItem.Amount)
						item.MovementOperation.Amount = tempItem.Amount;
					if (item.ServiceClaim != tempItem.ServiceClaim)
						item.ServiceClaim = tempItem.ServiceClaim;
				}
			}

			foreach(var item in Entity.Items.ToList())
			{
				var exist = item.MovementOperation.Equipment != null
					? tempItemList.Any(x => x.EquipmentId == item.MovementOperation.Equipment.Id)
					: tempItemList.Any(x => x.NomenclatureId == item.MovementOperation.Nomenclature.Id);
				
				if(!exist)
				{
					UoW.Delete(item.MovementOperation);
					Entity.ObservableItems.Remove(item);
				}
			}
		}

		protected void OnYentryrefWarehouseChanged(object sender, EventArgs e)
		{
			UpdateWidgetsVisible();
			returnsreceptionview1.Warehouse = Entity.Warehouse;
		}

		protected void OnYentryrefRouteListChanged(object sender, EventArgs e)
		{
			SetupForNewRouteList();
		}

		class InternalItem{
			
			public ReciveTypes ReciveType;
			public ServiceClaim ServiceClaim;
			public int NomenclatureId;
			public int EquipmentId;

			public decimal Amount;
		}
	}
}

