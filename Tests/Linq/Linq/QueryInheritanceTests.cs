﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Internal.SqlProvider;
using LinqToDB.Internal.SqlQuery;
using LinqToDB.Mapping;

using NUnit.Framework;

using Tests.Model;

namespace Tests.Linq
{
	[TestFixture]
	public class QueryInheritanceTests : TestBase
	{
		static IEnumerable<T> QueryTable<T>(IDataContext dataContext)
		{
			var query = new SqlSelectStatement();
			var table = SqlTable.Create<T>(dataContext);
			var tableSource = new SqlTableSource(table, "t");
			query.SelectQuery.From.Tables.Add(tableSource);

			var connection = (DataConnection) dataContext;

			var sqlBuilder   = connection.DataProvider.CreateSqlBuilder(connection.MappingSchema, connection.Options);
			var sqlOptimizer = connection.DataProvider.GetSqlOptimizer(dataContext.Options);
			var factory      = sqlOptimizer.CreateSqlExpressionFactory(connection.MappingSchema, connection.Options);
			var sb           = new StringBuilder();

			sqlBuilder.BuildSql(0, query, sb,
				new OptimizationContext(
					evaluationContext : new EvaluationContext(),
					dataOptions : dataContext.Options,
					sqlProviderFlags : dataContext.SqlProviderFlags,
					mappingSchema : dataContext.MappingSchema,
					optimizerVisitor : sqlOptimizer.CreateOptimizerVisitor(false),
					convertVisitor : sqlOptimizer.CreateConvertVisitor(false), 
					factory : factory,
					isParameterOrderDepended : false,
					isAlreadyOptimizedAndConverted : false,
					parametersNormalizerFactory : connection.DataProvider.GetQueryParameterNormalizer
				),
				aliases : new AliasesContext(), nullabilityContext : null);

			return connection.Query<T>(sb.ToString());
		}

		[Test]
		public void Test1([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(ParentInheritance, QueryTable<ParentInheritanceBase>(db));
		}

		[Test]
		public void Test2([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(ParentInheritance, QueryTable<ParentInheritanceBase>(db).Select(p => p));
		}

		[Test]
		public void Test3([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    ParentInheritance where p is ParentInheritance1 select p,
					from p in QueryTable<ParentInheritanceBase>(db) where p is ParentInheritance1 select p);
		}

		[Test]
		public void Test4([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    ParentInheritance where !(p is ParentInheritanceNull) select p,
					from p in QueryTable<ParentInheritanceBase>(db) where !(p is ParentInheritanceNull) select p);
		}

		[Test]
		public void Test5([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    ParentInheritance where p is ParentInheritanceValue select p,
					from p in QueryTable<ParentInheritanceBase>(db) where p is ParentInheritanceValue select p);
		}

		[Test]
		public void Test6([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var q = from p in QueryTable<ParentInheritance12>(db) where p is ParentInheritance12 select p;
				q.ToList();
			}
		}

		[Test]
		public void Test7([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    ParentInheritance where p is ParentInheritanceBase select p,
					from p in QueryTable<ParentInheritanceBase>(db) where p is ParentInheritanceBase select p);
		}

		[Test]
		public void Test8([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					ParentInheritance.OfType<ParentInheritance1>(),
					QueryTable<ParentInheritanceBase>(db).OfType<ParentInheritance1>());
		}

		[Test]
		public void Test9([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					ParentInheritance
						.Where(p => p.ParentID == 1 || p.ParentID == 2 || p.ParentID == 4)
						.OfType<ParentInheritanceNull>(),
					QueryTable<ParentInheritanceBase>(db)
						.Where(p => p.ParentID == 1 || p.ParentID == 2 || p.ParentID == 4)
						.OfType<ParentInheritanceNull>());
		}

		[Test]
		public void Test10([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					ParentInheritance.OfType<ParentInheritanceValue>(),
					QueryTable<ParentInheritanceBase>(db).OfType<ParentInheritanceValue>());
		}

		[Test]
		public void Test11([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var q = from p in QueryTable<ParentInheritance13>(db) where p is ParentInheritance13 select p;
				q.ToList();
			}
		}

		[Test]
		public void Test12([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    ParentInheritance1 where p.ParentID == 1 select p,
					from p in QueryTable<ParentInheritance1>(db) where p.ParentID == 1 select p);
		}

		[Test]
		public void TypeCastAsTest1([NorthwindDataContext] string context)
		{
			using (var db = new NorthwindDB(context))
			{
				var dd = GetNorthwindAsList(context);
				AreEqual(
					dd.DiscontinuedProduct.ToList()
						.Select(p => p as Northwind.Product)
						.Select(p => p == null ? "NULL" : p.ProductName),
					QueryTable<Northwind.DiscontinuedProduct>(db).Where(p => p.Discontinued)
						.Select(p => p as Northwind.Product)
						.Select(p => p == null ? "NULL" : p.ProductName));
			}
		}

		[Test]
		public void TypeCastAsTest11([NorthwindDataContext] string context)
		{
			using (var db = new NorthwindDB(context))
			{
				var dd = GetNorthwindAsList(context);
				AreEqual(
					dd.DiscontinuedProduct.ToList()
						.Select(p => new { p = p as Northwind.Product })
						.Select(p => p.p == null ? "NULL" : p.p.ProductName),
					QueryTable<Northwind.DiscontinuedProduct>(db).Where(p => p.Discontinued)
						.Select(p => new { p = p as Northwind.Product })
						.Select(p => p.p == null ? "NULL" : p.p.ProductName));
			}
		}

		[Test]
		public void TypeCastAsTest2([NorthwindDataContext] string context)
		{
			using (var db = new NorthwindDB(context))
			{
				var dd = GetNorthwindAsList(context);
				AreEqual(
					dd.Product.ToList()
						.Select(p => p as Northwind.DiscontinuedProduct)
						.Select(p => p == null ? "NULL" : p.ProductName),
					QueryTable<Northwind.Product>(db)
						.Select(p => p as Northwind.DiscontinuedProduct)
						.Select(p => p == null ? "NULL" : p.ProductName));
			}
		}

		[Test]
		public void FirstOrDefault([NorthwindDataContext] string context)
		{
			using (var db = new NorthwindDB(context))
			{
				var dd = GetNorthwindAsList(context);
				Assert.That(
					QueryTable<Northwind.DiscontinuedProduct>(db).Where(p => p.Discontinued).FirstOrDefault()!.ProductID, Is.EqualTo(dd.DiscontinuedProduct.FirstOrDefault()!.ProductID));
			}
		}

		[Test]
		public void Cast1([DataSources(false)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					ParentInheritance.OfType<ParentInheritance1>().Cast<ParentInheritanceBase>(),
					QueryTable<ParentInheritanceBase>(db).OfType<ParentInheritance1>().Cast<ParentInheritanceBase>());
		}

		sealed class ParentEx : Parent
		{
			[NotColumn]
			public bool Field1;

			public static void Test(QueryInheritanceTests inheritance, string context)
			{
				using (var db = inheritance.GetDataContext(context))
					inheritance.AreEqual(
						Enumerable.Select<Parent,ParentEx>(inheritance.Parent, p => new ParentEx { Field1 = true, ParentID = p.ParentID, Value1 = p.Value1 }).Cast<Parent>(),
						QueryTable<Parent>(db).Select(p => new ParentEx { Field1 = true, ParentID = p.ParentID, Value1 = p.Value1 }).Cast<Parent>());
			}
		}

		[Test]
		public void Cast2([DataSources(false)] string context)
		{
			ParentEx.Test(this, context);
		}

		[Table("Person", IsColumnAttributeRequired = false)]
		sealed class PersonEx : Person
		{
		}

		[Test]
		public void SimpleTest()
		{
			using (var db = new DataConnection())
				Assert.That(QueryTable<PersonEx>(db).Where(_ => _.FirstName == "John").Select(_ => _.ID).Single(), Is.EqualTo(1));
		}

		[Test]
		public void TypeCastIsChildConditional2([NorthwindDataContext] string context)
		{
			using (var db = new NorthwindDB(context))
			{
				var result   = QueryTable<Northwind.Product>(db).Select(x => x is Northwind.DiscontinuedProduct).ToArray();
				var expected = db.Product.ToList()              .Select(x => x is Northwind.DiscontinuedProduct);

				var list = result.ToList();

				Assert.That(list, Is.Not.Empty);
				Assert.That(list, Has.Count.EqualTo(expected.Count()));
				Assert.That(list.Except(expected).Count(), Is.Zero);
			}
		}

		public enum TypeCodeEnum
		{
			Base,
			A,
			A1,
			A2,
		}

		[Table(Name="LinqDataTypes")]
		public abstract class InheritanceBase
		{
			[Column] public Guid GuidValue { get; set; }

			[Column("ID")]
			public virtual TypeCodeEnum TypeCode => TypeCodeEnum.Base;
		}

		[InheritanceMapping(Code = TypeCodeEnum.A1, Type = typeof(InheritanceA1), IsDefault = false)]
		[InheritanceMapping(Code = TypeCodeEnum.A2, Type = typeof(InheritanceA2), IsDefault = true)]
		public abstract class InheritanceA : InheritanceBase
		{
			[Association(CanBeNull = true, ThisKey = "GuidValue", OtherKey = "GuidValue")]
			public List<InheritanceB> Bs { get; set; } = null!;

			[Column("ID", IsDiscriminator = true)]
			public override TypeCodeEnum TypeCode => TypeCodeEnum.A;
		}

		sealed class InheritanceA1 : InheritanceA
		{
			[Column("ID", IsDiscriminator = true)]
			public override TypeCodeEnum TypeCode => TypeCodeEnum.A1;
		}

		sealed class InheritanceA2 : InheritanceA
		{
			[Column("ID", IsDiscriminator = true)]
			public override TypeCodeEnum TypeCode => TypeCodeEnum.A2;
		}

		public class InheritanceB : InheritanceBase
		{
		}
	}
}
