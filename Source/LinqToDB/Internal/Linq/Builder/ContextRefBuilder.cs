﻿using LinqToDB.Internal.Expressions;

namespace LinqToDB.Internal.Linq.Builder
{
	[BuildsAny]
	sealed class ContextRefBuilder : ISequenceBuilder
	{
		public static bool CanBuild(BuildInfo buildInfo)
			=> buildInfo.Expression is ContextRefExpression contextRef;

		public BuildSequenceResult BuildSequence(ExpressionBuilder builder, BuildInfo buildInfo)
		{
			var contextRef = (ContextRefExpression)buildInfo.Expression;

			var context = contextRef.BuildContext;

			if (!buildInfo.CreateSubQuery)
				return BuildSequenceResult.FromContext(context);

			var elementContext = context.GetContext(buildInfo.Expression, buildInfo);

			if (elementContext != null)
				return BuildSequenceResult.FromContext(elementContext);

			return BuildSequenceResult.NotSupported();
		}
		
		public bool IsSequence(ExpressionBuilder builder, BuildInfo buildInfo)
		{
			return true;
		}
	}
}
