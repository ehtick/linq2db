﻿using System;
using System.Linq.Expressions;

using LinqToDB.Internal.Expressions;
using LinqToDB.Internal.SqlQuery;

namespace LinqToDB.Internal.Linq.Builder
{
	[BuildsMethodCall("InnerJoin", "LeftJoin", "RightJoin", "FullJoin")]
	[BuildsMethodCall("Join", CanBuildName = nameof(CanBuildJoin))]
	sealed class AllJoinsBuilder : MethodCallBuilder
	{
		public static bool CanBuildJoin(MethodCallExpression call)
			=> call.IsQueryable() && call.Arguments.Count == 3;

		public static bool CanBuildMethod(MethodCallExpression call)
			=> call.IsQueryable() && call.Arguments.Count == 2;

		protected override BuildSequenceResult BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			var argument = methodCall.Arguments[0];
			if (buildInfo.Parent != null)
			{
				argument = SequenceHelper.MoveToScopedContext(argument, buildInfo.Parent);
			}

			var buildResult = builder.TryBuildSequence(new BuildInfo(buildInfo, argument));
			if (buildResult.BuildContext == null)
				return buildResult;

			var sequence = buildResult.BuildContext;

			JoinType joinType;
			var conditionIndex = 1;

			switch (methodCall.Method.Name)
			{
				case "InnerJoin" : joinType = JoinType.Inner; break;
				case "LeftJoin"  : joinType = JoinType.Left;  break;
				case "RightJoin" : joinType = JoinType.Right; break;
				case "FullJoin"  : joinType = JoinType.Full;  break;
				default:
					conditionIndex = 2;

					joinType = (SqlJoinType) builder.EvaluateExpression(methodCall.Arguments[1])! switch
					{
						SqlJoinType.Inner => JoinType.Inner,
						SqlJoinType.Left  => JoinType.Left,
						SqlJoinType.Right => JoinType.Right,
						SqlJoinType.Full  => JoinType.Full,
						_                 => throw new InvalidOperationException($"Unexpected join type: {(SqlJoinType)builder.EvaluateExpression(methodCall.Arguments[1])!}")
					};
					break;
			}

			buildInfo.JoinType = joinType;

			sequence = new SubQueryContext(sequence);
			var result = sequence;

			if (methodCall.Arguments[conditionIndex] != null)
			{
				var condition = (LambdaExpression)methodCall.Arguments[conditionIndex].Unwrap();

				result = builder.BuildWhere(result, condition: condition, enforceHaving: false, out var error);

				if (result == null)
					return BuildSequenceResult.Error(error ?? methodCall);

				/*if (joinType == JoinType.Full)
				{
					result.SelectQuery.Where.SearchCondition =
						QueryHelper.CorrectComparisonForJoin(result.SelectQuery.Where.SearchCondition);
				}*/

				result.SetAlias(condition.Parameters[0].Name);
			}

			if (joinType is JoinType.Left or JoinType.Full)
			{
				result = new DefaultIfEmptyBuilder.DefaultIfEmptyContext(buildInfo.Parent,
					sequence: result,
					nullabilitySequence: result,
					defaultValue: null,
					isNullValidationDisabled: false);
			}

			return BuildSequenceResult.FromContext(result);
		}
	}
}
