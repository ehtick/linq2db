﻿using System;
using System.Linq;
using System.Linq.Expressions;

using LinqToDB;
using LinqToDB.Data;

using NUnit.Framework;

namespace Tests.UserTests
{
	[TestFixture]
	public class GenericsFilterTests
	{
		[Test]
		public void WhenPredicateFactoryIsGeneric([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			var predicate = ById<Firm>(0);
			Assert.DoesNotThrow(() => CheckPredicate(predicate, context));
		}

		[Test]
		public void WhenPredicateFactoryIsNotGeneric([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			var predicate = ById(0);
			Assert.DoesNotThrow(() => CheckPredicate(predicate, context));
		}

		Expression<Func<T, bool>> ById<T>(int foobar)
			where T : class, IIdentifiable
		{
			return identifiable => identifiable.Id == foobar;
		}

		Expression<Func<Firm, bool>> ById(int foobar)
		{
			return identifiable => identifiable.Id == foobar;
		}

		void CheckPredicate(Expression<Func<Firm, bool>> predicate, string context)
		{
			using (var db = new DataConnection(new DataOptions().UseConnectionString(context, context == ProviderName.SQLiteMS ? "Data Source=:memory:;" : "Data Source=:memory:;Version=3;New=True;")))
			{
				db.CreateTable<TypeA>();
				db.CreateTable<TypeB>();

				var query = db.GetTable<TypeA>()
					.Select(a => new Firm { Id = a.Id, Value = db.GetTable<TypeB>().Select(b => b.Id).FirstOrDefault() });

				query.Where(predicate).GetEnumerator();
			}
		}

		interface IIdentifiable
		{
			int Id { get; set; }
		}

		sealed class Firm : IIdentifiable
		{
			public int Id { get; set; }
			public int Value { get; set; }
		}

		sealed class TypeA
		{
			public int Id { get; set; }
		}

		sealed class TypeB
		{
			public int Id { get; set; }
		}
	}
}
