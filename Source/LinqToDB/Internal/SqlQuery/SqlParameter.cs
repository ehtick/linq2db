﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LinqToDB.Internal.SqlQuery
{
	public sealed class SqlParameter : SqlExpressionBase
	{
		public SqlParameter(DbDataType type, string? name, object? value)
		{
			IsQueryParameter = true;
			Name             = name;
			Type             = type;
			Value            = value;

#if DEBUG
			_paramNumber = ++_paramCounter;
#endif
		}

#if DEBUG
		readonly int _paramNumber;
		static   int _paramCounter;
#endif

		// meh, nullable...
		public   string?    Name             { get; set; }
		public   DbDataType Type             { get; set; }
		public   bool       IsQueryParameter { get; set; }
		public   int?       AccessorId       { get; internal set; }

		public object? Value     { get; }
		/// <summary>
		/// Enable parameter wrapping with type-cast. Ignored when <see cref="IsQueryParameter"/> is <c>false</c>.
		/// </summary>
		public bool    NeedsCast { get; set; }

		public object? CorrectParameterValue(object? rawValue)
		{
			var value = rawValue;

			var valueConverter = ValueConverter;
			return valueConverter == null ? value : valueConverter(value);
		}

		#region Value Converter

		internal List<int>? TakeValues;

		private Func<object?, object?>? _valueConverter;
		public  Func<object?, object?>?  ValueConverter
		{
			get
			{
				if (_valueConverter == null && TakeValues != null)
					foreach (var take in TakeValues.ToArray())
						SetTakeConverter(take);

				return _valueConverter;
			}

			set => _valueConverter = value;
		}

		internal void SetTakeConverter(int take)
		{
			TakeValues ??= new List<int>();

			TakeValues.Add(take);

			SetTakeConverterInternal(take);
		}

		void SetTakeConverterInternal(int take)
		{
			var conv = _valueConverter;

			if (conv == null)
				_valueConverter = v => v == null ? null : ((int) v + take);
			else
				_valueConverter = v => v == null ? null : ((int) conv(v)! + take);
		}

		#endregion

		#region SqlExpressionBase overrides

		public override int  Precedence => LinqToDB.SqlQuery.Precedence.Primary;
		public override Type SystemType => Type.SystemType;

		public override bool CanBeNullable(NullabilityContext nullability)
			=> QueryHelper.TypeCanBeNull(Type.SystemType);

		public override bool Equals(ISqlExpression other, Func<ISqlExpression, ISqlExpression, bool> comparer)
		{
			return ReferenceEquals(this, other) && comparer(this, other);
		}

		#endregion

		#region IQueryElement Members

		public override QueryElementType ElementType => QueryElementType.SqlParameter;

		public override QueryElementTextWriter ToString(QueryElementTextWriter writer)
		{
			writer.DebugAppendUniqueId(this);

			if (NeedsCast)
				writer.Append("$Cast$(");

			if (Name?.StartsWith("@") == false)
				writer.Append('@');

			writer
				.Append(Name ?? "parameter");

			if (AccessorId != null)
				writer.Append("(A:").Append(AccessorId.Value).Append(')');
#if DEBUG
			writer.Append('(').Append(_paramNumber).Append(')');
#endif
			if (Value != null)
				writer
					.Append('[')
					.Append(Value)
					.Append(']');

			if (NeedsCast)
				writer.Append(")");

			return writer;
		}

		public override int GetElementHashCode()
		{
			var hash = new HashCode();
			hash.Add(ElementType);
			hash.Add(Name);
			hash.Add(Type);
			hash.Add(IsQueryParameter);
			hash.Add(AccessorId);
			if (AccessorId == null)
				hash.Add(Value);
			return hash.ToHashCode();
		}

		#endregion
	}
}
