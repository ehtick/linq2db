﻿using System.Linq;

using LinqToDB;

using NUnit.Framework;

namespace Tests.Exceptions
{
	[TestFixture]
	public class InheritanceTests : TestBase
	{
		[Test]
		public void Test1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var q = from p in db.ParentInheritance2 select p;
				Assert.Throws<LinqToDBException>(() => q.ToList());
			}
		}
	}
}
