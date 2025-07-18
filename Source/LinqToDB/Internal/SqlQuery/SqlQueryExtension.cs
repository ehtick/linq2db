﻿using System;
using System.Collections.Generic;

namespace LinqToDB.Internal.SqlQuery
{
	public sealed class SqlQueryExtension : QueryElement
	{
		public SqlQueryExtension()
		{
		}

		/// <summary>
		/// Gets optional configuration, to which extension should be applied.
		/// </summary>
		public string?                            Configuration      { get; init; }
		/// <summary>
		/// Gets extension apply scope/location.
		/// </summary>
		public Sql.QueryExtensionScope            Scope              { get; init; }
		/// <summary>
		/// Gets extension arguments.
		/// </summary>
		public required Dictionary<string,ISqlExpression>  Arguments { get; init; }
		/// <summary>
		/// Gets optional extension builder type. Must implement <see cref="ISqlQueryExtensionBuilder"/> or <see cref="ISqlTableExtensionBuilder"/> interface.
		/// </summary>
		public Type?                              BuilderType        { get; init; }

		public override QueryElementType ElementType => QueryElementType.SqlQueryExtension;

		public override QueryElementTextWriter ToString(QueryElementTextWriter writer)
		{
			return writer.Append("extension");
		}

		public override int GetElementHashCode()
		{
			var hash = new HashCode();
			hash.Add(ElementType);
			hash.Add(Configuration);
			hash.Add(Scope);
			hash.Add(BuilderType);

			foreach (var arg in Arguments)
			{
				hash.Add(arg.Key);
				hash.Add(arg.Value.GetElementHashCode());
			}

			return hash.ToHashCode();
		}
	}
}
