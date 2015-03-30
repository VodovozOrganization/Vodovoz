using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace DataAccess.NhibernateFixes
{
	/// <summary>
	/// Clone of the Nhibernate class: NHibernate.Type.TimeAsTimeSpanType
	/// This one actually works though...
	/// </summary>
	[Serializable]
	public class TimeAsTimeSpanTypeClone : PrimitiveType, IVersionType, IType, ICacheAssembler
	{
		//FIXME До исправления бага в NHibernate и восстановления работы с TimeAsTimeSpan 
		// http://stackoverflow.com/questions/11314426/error-persisting-a-timespan-property-to-a-time-type-mysql-column
		// https://nhibernate.jira.com/browse/NH-1851?jql=project%20%3D%20NH%20AND%20resolution%20%3D%20Unresolved%20AND%20text%20~%20%22TimeAsTimeSpan%22%20ORDER%20BY%20priority%20DESC%2C%20updated%20DESC
		private static readonly DateTime BaseDateValue = new DateTime(1753, 1, 1);

		public override string Name
		{
			get
			{
				return "TimeAsTimeSpan";
			}
		}

		public override System.Type ReturnedClass
		{
			get
			{
				return typeof(TimeSpan);
			}
		}

		public IComparer Comparator
		{
			get
			{
				return (IComparer)Comparer<TimeSpan>.Default;
			}
		}

		public override System.Type PrimitiveClass
		{
			get
			{
				return typeof(TimeSpan);
			}
		}

		public override object DefaultValue
		{
			get
			{
				return (object)TimeSpan.Zero;
			}
		}

		static TimeAsTimeSpanTypeClone()
		{
		}

		public TimeAsTimeSpanTypeClone()
			: base(SqlTypeFactory.Time)
		{
		}

		public override object Get(IDataReader rs, int index)
		{
			try
			{
				object obj = rs[index];
				if (obj is TimeSpan)
					return (object)(TimeSpan)obj;
				else
					return (object)((DateTime)obj).TimeOfDay;
			}
			catch (Exception ex)
			{
				throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", rs[index]), ex);
			}
		}

		public override object Get(IDataReader rs, string name)
		{
			try
			{
				object obj = rs[name];
				if (obj is TimeSpan)
					return (object)(TimeSpan)obj;
				else
					return (object)((DateTime)obj).TimeOfDay;
			}
			catch (Exception ex)
			{
				throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", rs[name]), ex);
			}
		}

		public override void Set(IDbCommand st, object value, int index)
		{
			DateTime dateTime = TimeAsTimeSpanTypeClone.BaseDateValue.AddTicks(((TimeSpan)value).Ticks);
			((IDataParameter)st.Parameters[index]).Value = (object)dateTime.TimeOfDay; // <<<<  fix here. Added ".TimeOfDay"
		}

		public override string ToString(object val)
		{
			return ((TimeSpan)val).Ticks.ToString();
		}

		public object Next(object current, ISessionImplementor session)
		{
			return this.Seed(session);
		}

		public virtual object Seed(ISessionImplementor session)
		{
			return (object)new TimeSpan(DateTime.Now.Ticks);
		}

		public object StringToObject(string xml)
		{
			return (object)TimeSpan.Parse(xml);
		}

		public override object FromStringValue(string xml)
		{
			return (object)TimeSpan.Parse(xml);
		}

		public override string ObjectToSQLString(object value, NHibernate.Dialect.Dialect dialect)
		{
			return (string)(object)'\'' + (object)((TimeSpan)value).Ticks.ToString() + (string)(object)'\'';
		}
	}
}