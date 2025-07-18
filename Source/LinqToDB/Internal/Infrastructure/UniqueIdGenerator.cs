﻿using System.Diagnostics;

namespace LinqToDB.Internal.Infrastructure
{
	[DebuggerDisplay("Generator({_current})")]
	public class UniqueIdGenerator<T> : IUniqueIdGenerator<T>
	{
		int _current;

		public int GetNext()
		{
			return _current++;
		}
	}
}
