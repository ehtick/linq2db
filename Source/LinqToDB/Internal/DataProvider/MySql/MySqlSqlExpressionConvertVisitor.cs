﻿using System.Collections.Generic;

using LinqToDB.Internal.Extensions;
using LinqToDB.Internal.SqlProvider;
using LinqToDB.Internal.SqlQuery;
using LinqToDB.SqlQuery;

namespace LinqToDB.Internal.DataProvider.MySql
{
	public class MySqlSqlExpressionConvertVisitor : SqlExpressionConvertVisitor
	{
		public MySqlSqlExpressionConvertVisitor(bool allowModify) : base(allowModify)
		{
		}

		protected override ISqlExpression ConvertConversion(SqlCastExpression cast)
		{
			cast = FloorBeforeConvert(cast);

			var castType = cast.SystemType.ToUnderlying();

			if ((castType == typeof(double) || castType == typeof(float)) && cast.Expression.SystemType == typeof(decimal))
				return cast.Expression;

			return base.ConvertConversion(cast);
		}

		public override IQueryElement ConvertSqlBinaryExpression(SqlBinaryExpression element)
		{
			switch (element)
			{
				case SqlBinaryExpression(var type, var ex1, "+", var ex2) when type.SystemType == typeof(string) :
				{
					return ConvertFunc(new (type, "Concat", ex1, ex2));

					static SqlFunction ConvertFunc(SqlFunction func)
					{
						for (var i = 0; i < func.Parameters.Length; i++)
						{
							switch (func.Parameters[i])
							{
								case SqlBinaryExpression(var t, var e1, "+", var e2) when t.SystemType == typeof(string) :
								{
									var ps = new List<ISqlExpression>(func.Parameters);

									ps.RemoveAt(i);
									ps.Insert(i,     e1);
									ps.Insert(i + 1, e2);

									return ConvertFunc(new (t, func.Name, ps.ToArray()));
								}

								case SqlFunction { Name: "Concat", Type: var t } f when t.SystemType == typeof(string):
								{
									var ps = new List<ISqlExpression>(func.Parameters);

									ps.RemoveAt(i);
									ps.InsertRange(i, f.Parameters);

									return ConvertFunc(new (t, func.Name, ps.ToArray()));
								}
							}
						}

						return func;
					}
				}
			}

			return base.ConvertSqlBinaryExpression(element);
		}

		public override ISqlPredicate ConvertSearchStringPredicate(SqlPredicate.SearchString predicate)
		{
			var caseSensitive = predicate.CaseSensitive.EvaluateBoolExpression(EvaluationContext);

			if (caseSensitive == null || caseSensitive == false)
			{
				var searchExpr = predicate.Expr2;
				var dataExpr   = predicate.Expr1;

#pragma warning disable CA1508 // https://github.com/dotnet/roslyn-analyzers/issues/6868
				if (caseSensitive == false)
#pragma warning restore CA1508
				{
					searchExpr = PseudoFunctions.MakeToLower(searchExpr, MappingSchema);
					dataExpr   = PseudoFunctions.MakeToLower(dataExpr, MappingSchema);
				}

				ISqlPredicate? newPredicate = null;
				switch (predicate.Kind)
				{
					case SqlPredicate.SearchString.SearchKind.Contains:
					{
						newPredicate = new SqlPredicate.ExprExpr(
							new SqlFunction(MappingSchema.GetDbDataType(typeof(int)), "LOCATE", searchExpr, dataExpr), SqlPredicate.Operator.Greater,
							new SqlValue(0), null);
						break;
					}
				}

				if (newPredicate != null)
				{
					newPredicate = newPredicate.MakeNot(predicate.IsNot);

					return newPredicate;
				}

#pragma warning disable CA1508 // https://github.com/dotnet/roslyn-analyzers/issues/6868
				if (caseSensitive == false)
#pragma warning restore CA1508
				{
					predicate = new SqlPredicate.SearchString(
						dataExpr,
						predicate.IsNot,
						searchExpr,
						predicate.Kind,
						new SqlValue(false));
				}
			}
			else
			{
				predicate = new SqlPredicate.SearchString(
					new SqlExpression(MappingSchema.GetDbDataType(typeof(string)), $"{{0}} COLLATE utf8_bin", Precedence.Primary, predicate.Expr1),
					predicate.IsNot,
					predicate.Expr2,
					predicate.Kind,
					new SqlValue(false));
			}

			return ConvertSearchStringPredicateViaLike(predicate);
		}

		public override ISqlExpression ConvertSqlFunction(SqlFunction func)
		{
			switch (func)
			{
				case { Name: PseudoFunctions.LENGTH }:
					return func.WithName("CHAR_LENGTH");

				default:
					return base.ConvertSqlFunction(func);
			}
		}

		protected override ISqlExpression WrapColumnExpression(ISqlExpression expr)
		{
			if (expr is SqlValue
				{
					Value: decimal or uint or ulong or long or double
				} value)
			{
				expr = new SqlCastExpression(expr, value.ValueType, null, isMandatory: true);
			}
			else if (expr is SqlParameter param)
			{
				var paramType = param.Type.SystemType.UnwrapNullableType();
				if (paramType == typeof(uint)
					|| paramType == typeof(ulong)
					|| paramType == typeof(long)
					|| paramType == typeof(double)
					|| paramType == typeof(decimal))
					expr = new SqlCastExpression(expr, param.Type, null, isMandatory: true);
			}

			return base.WrapColumnExpression(expr);
		}
	}
}
