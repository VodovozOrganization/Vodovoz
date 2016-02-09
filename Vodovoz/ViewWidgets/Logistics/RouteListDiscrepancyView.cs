using System;
using Vodovoz.Domain;
using Gamma.GtkWidgets;
using System.Collections.Generic;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListDiscrepancyView : Gtk.Bin
	{
		public RouteListDiscrepancyView()
		{
			this.Build();
			Configure();
		}

		List<Discrepancy> items;
		public List<Discrepancy> Items{ 
			get{
				return items;
			}
			set{
				items = value;
				ytreeview2.ItemsDataSource = items;
			}
		}


		protected void Configure()
		{
			ytreeview2.ColumnsConfig = ColumnsConfigFactory.Create<Discrepancy>()
				.AddColumn("Название")
				.AddTextRenderer(node => node.Name)					
				.AddColumn("Указано")
				.AddNumericRenderer(node => node.FromClient)
				.AddColumn("Выгружено")
				.AddNumericRenderer(node => node.ToWarehouse)
				.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
				.Finish();
		}		
	}

	public class Discrepancy{
		public string Name{get;set;}
		public int NomenclatureId{get;set;}
		public int Id{get;set;}
		public decimal FromClient{ get; set; }
		public decimal ToWarehouse{ get; set;}
		public bool Trackable{ get; set; }
		public NomenclatureCategory NomenclatureCategory{ get; set; }
		public string Serial{ get { 
				if (Trackable) {
					return Id > 0 ? Id.ToString () : "(не определен)";
				} else
					return String.Empty;
			}
		}
	}

}

