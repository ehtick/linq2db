﻿using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.CodeModel
{
	/// <summary>
	/// Method call expression.
	/// </summary>
	public sealed class CodeCallExpression : CodeCallBase, ICodeExpression
	{
		internal CodeCallExpression(
			bool                         extension,
			ICodeExpression              callee,
			CodeIdentifier               method,
			IEnumerable<CodeTypeToken>   genericArguments,
			IEnumerable<ICodeExpression> parameters,
			IType                        returnType)
			: base(extension, callee, method, genericArguments, parameters)
		{
			ReturnType = returnType;
		}

		public CodeCallExpression(
			bool                         extension,
			ICodeExpression              callee,
			CodeIdentifier               method,
			IEnumerable<IType>           genericArguments,
			IEnumerable<ICodeExpression> parameters,
			IType                        returnType)
			: this(extension, callee, method, genericArguments.Select(static t => new CodeTypeToken(t)), parameters, returnType)
		{
		}

		public IType ReturnType { get; }

		IType           ICodeExpression.Type        => ReturnType;
		CodeElementType ICodeElement   .ElementType => CodeElementType.CallExpression;
	}
}