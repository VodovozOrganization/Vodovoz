using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.JournalFilters;
using Vodovoz.TempAdapters;

namespace Vodovoz.Representations
{
	/// <summary>
	/// Модель отображения в списке количества по каждому оборудованию не посерийного учета.
	/// </summary>
	public class EquipmentsNonSerialForRentVM : RepresentationModelWithoutEntityBase<NomenclatureForRentVMNode>
	{
		public NomenclatureEquipTypeFilter Filter {
			get {
				return RepresentationFilter as NomenclatureEquipTypeFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		public EquipmentKind EquipmentKind { get; set; }

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			IList<NomenclatureForRentVMNode> items;
			if(Filter != null) {
				items = EquipmentRepositoryForViews.GetAllNonSerialEquipmentForRent(UoW, Filter.NomenEquipmentKind);
			} else if(EquipmentKind != null) {
				items = EquipmentRepositoryForViews.GetAllNonSerialEquipmentForRent(UoW, EquipmentKind);
			} else {
				items = EquipmentRepositoryForViews.GetAllNonSerialEquipmentForRent(UoW);
			}

			List<NomenclatureForRentVMNode> forRent = new List<NomenclatureForRentVMNode>();
			forRent.AddRange(items);
			forRent.Sort((x, y) => string.Compare(x.Nomenclature.Name, y.Nomenclature.Name, StringComparison.CurrentCulture));
			SetItemsSource(forRent);
		}

		static Gdk.Color colorBlack = new Gdk.Color (0, 0, 0);
		static Gdk.Color colorRed = new Gdk.Color (0xff, 0, 0);

		IColumnsConfig columnsConfig = FluentColumnsConfig <NomenclatureForRentVMNode>
			.Create()
			.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Оборудование").AddTextRenderer (node => node.Nomenclature.Name)
		    .AddColumn("Вид оборудования").AddTextRenderer (node => node.Type != null ? node.Type.Name : "")
			.AddColumn("Кол-во").AddTextRenderer (node => node.InStockText)
			.AddColumn("Зарезервировано").AddTextRenderer (node => node.ReservedText)
			.AddColumn("Доступно").AddTextRenderer (node => node.AvailableText)
			.AddSetter((cell, node) => cell.ForegroundGdk = node.Available > 0 ? colorBlack : colorRed)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		public EquipmentsNonSerialForRentVM() 
			: this(UnitOfWorkFactory.CreateWithoutRoot ()) 
		{ }

		public EquipmentsNonSerialForRentVM(NomenclatureEquipTypeFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public EquipmentsNonSerialForRentVM(IUnitOfWork uow, EquipmentKind equipType) : this(uow)
		{
			Filter = null;
			EquipmentKind = equipType;
		}

		public EquipmentsNonSerialForRentVM(IUnitOfWork uow)
		{
			this.UoW = uow;
		}

		#region implemented abstract members of RepresentationModelWithoutEntityBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion
	}
}
