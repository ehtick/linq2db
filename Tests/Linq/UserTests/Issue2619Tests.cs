﻿using System.Linq;

using LinqToDB;

using NUnit.Framework;

using Shouldly;

namespace Tests.UserTests
{
	[TestFixture]
	public class Issue2619Tests : TestBase
	{
		[Test]
		public void OrderByUnion ([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var persons = db.Person
						.OrderBy (c => c.LastName);

				var union = persons
						.Union (persons);

				var sql = union.ToSqlQuery().Sql;

				sql.ShouldNotContain("ORDER");

				var act = () => union.ToArray();
				act.ShouldNotThrow();
			}
		}

		[Test]
		public void OrderByUnionModifier([DataSources(TestProvName.AllSybase)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var persons = db.Person
					.OrderBy (c => c.LastName).Take(1);

				var union = persons
					.Union (persons);

				var sql = union.ToSqlQuery().Sql;

				sql.ShouldContain("ORDER", Exactly.Twice());

				var act = () => union.ToArray();
				act.ShouldNotThrow();
			}
		}

		[Test]
		public void OrderByConcat([DataSources(ProviderName.SqlCe, TestProvName.AllSqlServer, TestProvName.AllSybase)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var persons = db.Person
					.OrderBy (c => c.LastName);

				var concat = persons
					.Concat(persons);

				var sql = concat.ToSqlQuery().Sql;

				sql.ShouldContain("ORDER", Exactly.Twice());

				var act = () => concat.ToArray();
				act.ShouldNotThrow();
			}
		}

		[Test]
		public void OrderByConcatModifier([DataSources(TestProvName.AllSybase)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var persons = db.Person
					.OrderBy (c => c.LastName).Take(1);

				var concat = persons
					.Concat(persons);

				var sql = concat.ToSqlQuery().Sql;

				sql.ShouldContain("ORDER", Exactly.Twice());

				var act = () => concat.ToArray();
				act.ShouldNotThrow();
			}
		}

		[Test]
		public void OrderByExcept([DataSources(TestProvName.AllSybase, TestProvName.AllSqlServer, TestProvName.AllAccess)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var persons = db.Person
					.OrderBy (c => c.LastName);

				var concat = persons
					.Except(persons);

				var sql = concat.ToSqlQuery().Sql;

				if (!sql.Contains("EXISTS"))
					sql.ShouldNotContain("ORDER");

				var act = () => concat.ToArray();
				act.ShouldNotThrow();
			}
		}

		[Test]
		public void OrderByExceptModifier([DataSources(TestProvName.AllSybase)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var persons = db.Person
					.OrderBy (c => c.LastName)
					.Take(1);

				var except = persons
					.Except(persons);

				var sql = except.ToSqlQuery().Sql;

				sql.ShouldContain("ORDER");

				var act = () => except.ToArray();
				act.ShouldNotThrow();
			}
		}

	}
}
