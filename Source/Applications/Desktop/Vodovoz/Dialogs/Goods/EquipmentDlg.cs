using System;
using System.IO;
using NHibernate.Criterion;
using NLog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QS.Validation;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Equipments;
using QS.Project.Services;
using Autofac;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz
{
	public partial class EquipmentDlg : QS.Dialog.Gtk.EntityDialogBase<Equipment>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IEquipmentRepository _equipmentRepository = ScopeProvider.Scope.Resolve<IEquipmentRepository>();

		//FIXME Возможно нужно удалить конструктор, так как создание нового оборудования отсюда должно быть закрыто.
		public EquipmentDlg ()
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Equipment>();
			ConfigureDlg ();
		}

		public EquipmentDlg (int id)
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Equipment> (id);
			ConfigureDlg ();
			FillLocation();
		}

		public EquipmentDlg (Equipment sub): this(sub.Id) {}

		private void ConfigureDlg ()
		{
			notebook1.ShowTabs = false;
			radiobuttonInfo.Active = true;

			checkOnDuty.Binding.AddBinding (Entity, e => e.OnDuty, w => w.Active).InitializeFromSource ();
			dataLastService.Binding.AddBinding (Entity, e => e.LastServiceDate, w => w.Date).InitializeFromSource ();
			entrySerialNumber.Binding.AddBinding (Entity, e => e.Serial, w => w.Text).InitializeFromSource ();
			textComment.Binding.AddBinding (Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource ();
			referenceNomenclature.SubjectType = typeof(Nomenclature);
			referenceNomenclature.Binding.AddBinding(Entity, e => e.Nomenclature, w => w.Subject).InitializeFromSource();
			referenceNomenclature.ItemsCriteria = UoW.Session.CreateCriteria<Nomenclature>()
				.Add(Restrictions.Eq("Category", NomenclatureCategory.equipment));
			ydatepickerWarrantyEnd.Binding.AddBinding (UoWGeneric.Root, 
				equipment => equipment.WarrantyEndDate, 
				widget => widget.DateOrNull
			);
		}

		public override bool Save ()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info ("Сохраняем оборудование...");
			UoWGeneric.Save();
			return true;
		}

		protected void OnRadiobuttonInfoToggled (object sender, EventArgs e)
		{
			if (radiobuttonInfo.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnRadiobuttonStickerToggled (object sender, EventArgs e)
		{
			if (radiobuttonSticker.Active) {
				if(UoWGeneric.HasChanges)
				{
					if(CommonDialogs.SaveBeforePrint (typeof(Equipment), "наклейки"))
					{
						UoWGeneric.Save ();
					}
					else if(UoWGeneric.IsNew)
					{
						radiobuttonInfo.Active = true;
						return;
					}
				}
				notebook1.CurrentPage = 1;
				PreparedReport ();
			}
		}

		void PreparedReport()
		{
			string param = "equipment_id=" + UoWGeneric.Root.Id +
				"&dup=0";
			string reportPath = System.IO.Path.Combine (Directory.GetCurrentDirectory (), "Reports", "Equipment" + ".rdl");
			reportviewerSticker.LoadReport (new Uri (reportPath), param, QSMain.ConnectionString);
		}

		void FillLocation()
		{
			var location = _equipmentRepository.GetLocation(UoW, Entity);
			labelWhere.LabelProp = location.Title;
		}
	}
}
