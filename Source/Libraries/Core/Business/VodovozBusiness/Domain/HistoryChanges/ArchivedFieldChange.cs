using System;
using System.Reflection;
using NHibernate.Event;
using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.HistoryLog.Domain;
using QS.Project.DB;
using QS.Utilities.Extensions;

namespace Vodovoz.Domain.HistoryChanges
{
	public class ArchivedFieldChange : FieldChangeBase
	{
		#region Конфигурация

		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		#endregion

		private string _oldFormatedDiffText;
		string _newFormatedDiffText;
		private bool _isDiffMade;

		#region Свойства

		public virtual ArchivedChangedEntity Entity { get; set; }

		#endregion

		#region Расчетные

		public virtual string OldValueText => ValueDisplay(OldValue);
		public virtual string NewValueText => ValueDisplay(NewValue);

		public virtual string OldFormatedDiffText
		{
			get
			{
				if(!_isDiffMade)
				{
					MakeDiff();
				}

				return _oldFormatedDiffText;
			}
			protected set
			{
				_oldFormatedDiffText = value;
			}
		}

		public virtual string NewFormatedDiffText
		{
			get
			{
				if(!_isDiffMade)
				{
					MakeDiff();
				}

				return _newFormatedDiffText;
			}
			protected set
			{
				_newFormatedDiffText = value;
			}
		}

		public virtual string FieldTitle
		{
			get { return HistoryMain.ResolveFieldTitle(Entity.EntityClassName, Path); }
		}

		#endregion

		#region Внутренние методы

		private void MakeDiff()
		{
			if(DiffFormatter == null)
			{
				return;
			}

			DiffFormatter.SideBySideDiff(OldValueText, NewValueText, out _oldFormatedDiffText, out _newFormatedDiffText);
			_isDiffMade = true;
		}

		#endregion

		#region Методы сравнения для разных типов

		private static bool StringCompare(ref ArchivedFieldChange change, string valueOld, string valueNew)
		{
			if(string.IsNullOrWhiteSpace(valueNew) && string.IsNullOrWhiteSpace(valueOld))
			{
				return false;
			}

			if(string.Equals(valueOld, valueNew))
			{
				return false;
			}

			change = new ArchivedFieldChange
			{
				OldValue = valueOld,
				NewValue = valueNew
			};
			return true;
		}

		private static bool EntityCompare(ref ArchivedFieldChange change, object valueOld, object valueNew)
		{
			if(DomainHelper.EqualDomainObjects(valueOld, valueNew))
			{
				return false;
			}

			change = new ArchivedFieldChange();
			if(valueOld != null)
			{
				change.OldValue = GetObjectTitle(valueOld);
				change.OldId = DomainHelper.GetId(valueOld);
			}
			if(valueNew != null)
			{
				change.NewValue = GetObjectTitle(valueNew);
				change.NewId = DomainHelper.GetId(valueNew);
			}
			return true;
		}

		private static bool DateTimeCompare(ref ArchivedFieldChange change, PropertyInfo info, object valueOld, object valueNew)
		{
			var dateOld = valueOld as DateTime?;
			var dateNew = valueNew as DateTime?;

			if(dateOld != null && dateNew != null && DateTime.Equals(dateOld.Value, dateNew.Value))
			{
				return false;
			}

			var dateOnly = info.GetCustomAttributes(typeof(HistoryDateOnlyAttribute), true).Length > 0;

			change = new ArchivedFieldChange();
			if(dateOld != null)
			{
				change.OldValue = dateOnly ? dateOld.Value.ToShortDateString() : dateOld.Value.ToString();
			}
			if(dateNew != null)
			{
				change.NewValue = dateOnly ? dateNew.Value.ToShortDateString() : dateNew.Value.ToString();
			}

			return true;
		}

		private static bool DecimalCompare(ref ArchivedFieldChange change, PropertyInfo info, object valueOld, object valueNew)
		{
			var numberOld = valueOld as decimal?;
			var numberNew = valueNew as decimal?;

			if(numberOld != null && numberNew != null && decimal.Equals(numberOld.Value, numberNew.Value))
			{
				return false;
			}

			change = new ArchivedFieldChange();
			if(numberOld != null)
			{
				change.OldValue = numberOld.Value.ToString("G20");
			}
			if(numberNew != null)
			{
				change.NewValue = numberNew.Value.ToString("G20");
			}

			return true;
		}

		private static bool IntCompare<TNumber>(ref ArchivedFieldChange change, PropertyInfo info, object valueOld, object valueNew)
			where TNumber : struct
		{
			var numberOld = valueOld as TNumber?;
			var numberNew = valueNew as TNumber?;

			if(numberOld != null && numberNew != null && Equals(numberOld.Value, numberNew.Value))
			{
				return false;
			}

			change = new ArchivedFieldChange();
			if(numberOld != null)
			{
				change.OldValue = string.Format("{0:D}", numberOld);
			}
			if(numberNew != null)
			{
				change.NewValue = string.Format("{0:D}", numberNew);
			}

			return true;
		}

		private static bool BooleanCompare(ref ArchivedFieldChange change, PropertyInfo info, object valueOld, object valueNew)
		{
			var boolOld = valueOld as bool?;
			var boolNew = valueNew as bool?;

			if(boolOld != null && boolNew != null && bool.Equals(boolOld.Value, boolNew.Value))
			{
				return false;
			}

			change = new ArchivedFieldChange();
			if(boolOld != null)
			{
				change.OldValue = boolOld.Value.ToString();
			}
			if(boolNew != null)
			{
				change.NewValue = boolNew.Value.ToString();
			}

			return true;
		}

		protected static bool EnumCompare(ref ArchivedFieldChange change, PropertyInfo info, object valueOld, object valueNew)
		{
			if(valueOld != null && valueNew != null && Equals(valueOld, valueNew))
			{
				return false;
			}

			change = new ArchivedFieldChange
			{
				OldValue = valueOld?.ToString(),
				NewValue = valueNew?.ToString()
			};
			return true;
		}

		#endregion

		#region Статические методы

		public static ArchivedFieldChange CheckChange(int i, PostUpdateEvent ue)
		{
			return CreateChange(ue.State[i], ue.OldState[i], ue.Persister, i);
		}

		public static ArchivedFieldChange CheckChange(int i, PostInsertEvent ie)
		{
			return CreateChange(ie.State[i], null, ie.Persister, i);
		}

		private static ArchivedFieldChange CreateChange(object valueNew, object valueOld, NHibernate.Persister.Entity.IEntityPersister persister, int i)
		{
			if(valueOld == null && valueNew == null)
			{
				return null;
			}

			IType propType = persister.PropertyTypes[i];
			string propName = persister.PropertyNames[i];

			var propInfo = persister.MappedClass.GetPropertyInfo(propName);
			if(propInfo.GetCustomAttributes(typeof(IgnoreHistoryTraceAttribute), true).Length > 0)
			{
				return null;
			}

			ArchivedFieldChange change = null;

			#region Обработка в зависимости от типа данных

			if(propType is StringType && !StringCompare(ref change, (string)valueOld, (string)valueNew))
			{
				return null;
			}

			var link = propType as ManyToOneType;
			if(link != null)
			{
				if(!EntityCompare(ref change, valueOld, valueNew))
				{
					return null;
				}
			}

			if((propType is DateTimeType || propType is TimestampType) && !DateTimeCompare(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			if(propType is DecimalType && !DecimalCompare(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			if(propType is BooleanType && !BooleanCompare(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			if(propType is Int16Type && !IntCompare<Int16>(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			if(propType is Int32Type && !IntCompare<Int32>(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			if(propType is Int64Type && !IntCompare<Int64>(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			if(propType is UInt16Type && !IntCompare<UInt16>(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			if(propType is UInt32Type && !IntCompare<UInt32>(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			if(propType is UInt64Type && !IntCompare<UInt64>(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			if(propType is EnumStringType && !EnumCompare(ref change, propInfo, valueOld, valueNew))
			{
				return null;
			}

			#endregion

			if(change != null)
			{
				change.Path = propName;
				change.UpdateType();
				return change;
			}

			_logger.Warn("Трекер не умеет сравнивать изменения в полях типа {0}. Поле {1} пропущено.", propType, propName);
			return null;
		}

		#endregion

		#region Методы отображения разных типов

		protected string ValueDisplay(string value)
		{
			var claz = OrmConfig.FindMappingByShortClassName(Entity.EntityClassName);
			var property = GetPropertyOrNull(claz, Path);
			if(property != null)
			{
				if(property.Type is BooleanType)
				{
					return BooleanDisplay(value);
				}
				if(property.Type is EnumStringType)
				{
					return EnumDisplay(value, property);
				}
			}
			return value;
		}

		#endregion
	}
}
