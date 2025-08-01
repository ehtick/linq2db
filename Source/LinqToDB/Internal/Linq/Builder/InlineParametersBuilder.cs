﻿using System.Linq.Expressions;

using LinqToDB.Internal.Expressions;
using LinqToDB.Internal.Reflection;

namespace LinqToDB.Internal.Linq.Builder
{
	[BuildsMethodCall(nameof(LinqExtensions.InlineParameters))]
	sealed class InlineParametersBuilder : MethodCallBuilder
	{
		public static bool CanBuildMethod(MethodCallExpression call)
			=> call.IsSameGenericMethod(Methods.LinqToDB.InlineParameters);

		protected override BuildSequenceResult BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			builder.PushTranslationModifier(builder.GetTranslationModifier().WithInlineParameters(true), false);

			var sequence = builder.TryBuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));

			builder.PopTranslationModifier();

			return sequence;
		}
	}
}
