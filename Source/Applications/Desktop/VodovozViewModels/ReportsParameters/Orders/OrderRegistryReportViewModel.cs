﻿using QS.Commands;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class OrderRegistryReportViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private DateTime? _startDate;
		private GenericObservableList<GeoGroup> _geoGroups;

		public OrderRegistryReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Реестр заказов";
			Identifier = "Orders.OrderRegistryReport";

			UoW = _uowFactory.CreateWithoutRoot();

			foreach(var gg in UoW.Session.QueryOver<GeoGroup>().List())
			{
				_geoGroups.Add(gg);
			}

			_startDate = DateTime.Now;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual GenericObservableList<GeoGroup> GeoGroups
		{
			get => _geoGroups;
			set => SetField(ref _geoGroups, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "date", StartDate },
						{ "geographic_groups", GetResultIds(GeoGroups.Select(g => g.Id)) }
					};

				return parameters;
			}
		}

		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null )
			{
				yield return new ValidationResult("Необходимо выбрать дату.", new[] { nameof(StartDate) });
			}
		}
	}
}
