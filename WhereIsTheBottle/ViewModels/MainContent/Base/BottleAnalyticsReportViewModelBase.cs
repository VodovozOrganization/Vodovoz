using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using QS.Models;
using QS.ViewModels;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public abstract class BottleAnalyticsReportViewModelBase<TBottleAnalyticsReportModel> : ViewModelBase
		where TBottleAnalyticsReportModel : UoWFactoryModelBase
	{
		private DateTime? _dateFormed;
		private DateTime? _endDate;
		private DateTime? _startDate;
		private TBottleAnalyticsReportModel _model;
		private bool _isDataLoaded;
		private bool _isDataLoading;
		private IDictionary<PropertyInfo, PropertyInfo> _bindProperties;

		public BottleAnalyticsReportViewModelBase(TBottleAnalyticsReportModel model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
		}

		protected TBottleAnalyticsReportModel Model
		{
			get => _model;
			set
			{
				var oldModel = _model;
				if(SetField(ref _model, value) && oldModel != null)
				{
					oldModel.PropertyChanged -= OnModelPropertyChanged;
				}
			}
		}

		public abstract string HeaderString { get; }

		public virtual string FormedString => DateFormed.HasValue
			? $"Сформировано: {DateFormed?.ToString("G", CultureInfo.CurrentCulture)}"
			: "";
		public virtual DateTime? DateFormed
		{
			get => _dateFormed;
			set
			{
				if(SetField(ref _dateFormed, value))
				{
					OnPropertyChanged(nameof(FormedString));
				}
			}
		}
		public virtual bool IsDataLoading
		{
			get => _isDataLoading;
			set => SetField(ref _isDataLoading, value);
		}
		public virtual bool IsDataLoaded
		{
			get => _isDataLoaded;
			set => SetField(ref _isDataLoaded, value);
		}
		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value, () => StartDate);
		}
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value, () => EndDate);
		}

		protected void SubscribeToModelPropertyUpdates()
		{
			if(Model == null)
			{
				throw new ArgumentNullException(nameof(Model));
			}
			_bindProperties = new Dictionary<PropertyInfo, PropertyInfo>();
			var modelProperties = Model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanRead && x.GetGetMethod(false) != null);
			var viewModelProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanWrite && x.GetSetMethod(false) != null)
				.ToList();

			foreach(var modelProperty in modelProperties)
			{
				var vmProperty = viewModelProperties.FirstOrDefault(x => x.Name == modelProperty.Name);
				if(vmProperty != null)
				{
					_bindProperties.Add(vmProperty, modelProperty);
				}
			}
			if(_bindProperties.Any())
			{
				Model.PropertyChanged += OnModelPropertyChanged;
			}
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var (vmProperty, modelProperty) = _bindProperties.FirstOrDefault(x => x.Key.Name == args.PropertyName);
			if(vmProperty != null && modelProperty != null)
			{
				OnPropertyChanged(vmProperty.Name);
			}
		}
	}
}
