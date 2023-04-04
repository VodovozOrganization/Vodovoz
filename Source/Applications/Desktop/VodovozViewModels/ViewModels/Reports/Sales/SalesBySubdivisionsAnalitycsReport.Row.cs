using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{
		public class DisplayRow
		{
			public virtual string Title { get; set; }

			public virtual IList<string> DynamicRows { get; set; }
		}

		public class Row : DisplayRow
		{
			public override string Title { get; set; }

			public IList<(decimal Amount, decimal Price)> SalesBySubdivision { get; set; }

			public IList<decimal> ResiduesByWarehouse { get; set; }

			private List<string> _dynamicRows;

			public override IList<string> DynamicRows
			{
				get
				{
					if (_dynamicRows == null)
					{
						_dynamicRows = new List<string>();
						
						foreach (var sale in SalesBySubdivision)
						{
							_dynamicRows.Add(sale.Amount.ToString());
							_dynamicRows.Add(sale.Price.ToString());
						}

						foreach (var residue in ResiduesByWarehouse)
						{
							_dynamicRows.Add(residue.ToString());
						}
					}
					
					return _dynamicRows;
				}
			}
		}

		public class SubTotalRow : Row
		{
			public IList<Row> NomenclatureRows { get; set; }
		}

		public class TotalRow : Row
		{
			public override string Title => "Итого";

			public IList<SubTotalRow> SubTotalRows { get; set; }
		}

		public class SubHeaderRow : DisplayRow
		{
			public override string Title { get; set; }

			public override IList<string> DynamicRows { get; set; }
		}

		public class HeaderRow : DisplayRow
		{
			public override string Title { get; set; }

			public IEnumerable<string> SubdivisionsTitles { get; set; }

			public IEnumerable<string> WarehousesTitles { get; set; }

			private List<string> _dynamicRows;

			public override IList<string> DynamicRows
			{
				get
				{
					if(_dynamicRows == null)
					{
						_dynamicRows = new List<string>(SubdivisionsTitles.Union(WarehousesTitles));
					}

					return _dynamicRows;
				}
			}
		}
	}
}
