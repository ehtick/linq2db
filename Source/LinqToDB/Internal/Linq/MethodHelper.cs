﻿using System;
using System.Reflection;

namespace LinqToDB.Internal.Linq
{
	// TODO: replace remaining calls in API with
	// Methods.*.MakeGenericMethod calls
	public static class MethodHelper
	{
		public static MethodInfo GetMethodInfo(this Delegate del)
		{
			if ((object)del == null)
				throw new ArgumentNullException(nameof(del));
			return del.Method;
		}

		#region Helper methods to obtain MethodInfo in a safe way

#pragma warning disable IDE0060 // Remove unused parameter
		public static MethodInfo GetMethodInfo<T1,T2>(Func<T1,T2> f, T1 unused1)
		{
			return f.GetMethodInfo();
		}

		public static MethodInfo GetMethodInfo<T1,T2,T3>(Func<T1,T2,T3> f, T1 unused1, T2 unused2)
		{
			return f.GetMethodInfo();
		}

		public static MethodInfo GetMethodInfo<T1,T2,T3,T4>(Func<T1,T2,T3,T4> f, T1 unused1, T2 unused2, T3 unused3)
		{
			return f.GetMethodInfo();
		}

		public static MethodInfo GetMethodInfo<T1,T2,T3,T4,T5>(Func<T1,T2,T3,T4,T5> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4)
		{
			return f.GetMethodInfo();
		}

		public static MethodInfo GetMethodInfo<T1,T2,T3,T4,T5,T6>(Func<T1,T2,T3,T4,T5,T6> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5)
		{
			return f.GetMethodInfo();
		}

		public static MethodInfo GetMethodInfo<T1,T2,T3,T4,T5,T6, T7>(Func<T1,T2,T3,T4,T5,T6,T7> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5, T6 unused6)
		{
			return f.GetMethodInfo();
		}
#pragma warning restore IDE0060 // Remove unused parameter

		#endregion
	}
}
