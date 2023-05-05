﻿using System;
using QS.Project.Journal;
using Vodovoz.Domain.Goods;
namespace Vodovoz.Journals.JournalNodes
{
	public class NomenclatureStockJournalNode : JournalEntityNodeBase<Nomenclature>
	{
		public string NomenclatureName { get; set; }

		public decimal StockAmount { get; set; }

		public decimal MinNomenclatureAmount { get; set; }

		public decimal DiffCount => StockAmount - MinNomenclatureAmount;

		public bool NomenclatureIsArchive { get; set; }
		public bool HasInventoryAccounting { get; set; }

		public string UnitName { get; set; }

		public short UnitDigits { get; set; }

		public string AmountText => string.Format("{0:" + String.Format("F{0}", UnitDigits) + "} {1}", StockAmount, UnitName);

		public string MinCountText => string.Format("{0:" + String.Format("F{0}", UnitDigits) + "} {1}", MinNomenclatureAmount, UnitName);

		public string DiffCountText => string.Format("{0:" + String.Format("F{0}", UnitDigits) + "} {1}", DiffCount, UnitName);
	}
}
