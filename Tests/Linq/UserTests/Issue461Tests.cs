﻿using System.Linq;

using LinqToDB;
using LinqToDB.Internal.Common;

using NUnit.Framework;

namespace Tests.UserTests
{
	[TestFixture]
	public class Issue461Tests : TestBase
	{
		public class ValueHolder
		{
			//public ValueHolder(int id)
			//{
			//	Id = id;
			//}

			//public ValueHolder()
			//	: this(0)
			//{ }

			public int Id;

			public int ParentId;

			public override bool Equals(object? obj)
			{
				if (!(obj is ValueHolder vh))
					return false;

				if (ReferenceEquals(this, vh))
					return true;

				return Id == vh.Id && ParentId == vh.ParentId;
			}

			public override int GetHashCode()
			{
				return (Id * 407) ^ (ParentId * -407);
			}
		}

		public class ValueValueHolder
		{
			public ValueHolder? Child;

			public override bool Equals(object? obj)
			{
				if (!(obj is ValueValueHolder vvh))
					return false;

				if (ReferenceEquals(this, vvh))
					return true;

				if (Child != null)
					return Child.Equals(vvh.Child);

				return vvh.Child == null;
			}

			public override int GetHashCode()
			{
				return Child != null ? Child.GetHashCode() : 0;
			}
		}

		[Test]
		[ThrowsForProvider(typeof(LinqToDBException), TestProvName.AllSybase, ErrorMessage = ErrorHelper.Sybase.Error_JoinToDerivedTableWithTakeInvalid)]
		public void SelectToAnonimousTest1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var result = (from sep in db.Parent
							  select new
							  {
								  Child =
								  (from l in db.Child
								   select new
								   {
									   Id = l.ParentID + 1
								   }).FirstOrDefault()
							  }).ToList();

				var expected = from sep in Parent
							   select new
							   {
								   Child =
								   (from l in Child
									select new
									{
										Id = l.ParentID + 1
									}).FirstOrDefault()
							   };

				AreEqual(expected, result);
			}
		}

		[Test]
		[ThrowsForProvider(typeof(LinqToDBException), TestProvName.AllSybase, ErrorMessage = ErrorHelper.Sybase.Error_JoinToDerivedTableWithTakeInvalid)]
		public void SelectToAnonymousTest2([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var result = (from sep in db.Parent
							  select new
							  {
								  Child =
								  (from l in db.Child
								   select new
								   {
									   Id = l.ParentID + 1,
									   ParentId = l.ParentID
								   }).FirstOrDefault()
							  }).ToList();

				var expected = from sep in Parent
							   select new
							   {
								   Child =
								   (from l in Child
									select new
									{
										Id = l.ParentID + 1,
										ParentId = l.ParentID
									}).FirstOrDefault()
							   };

				AreEqual(expected, result);
			}
		}

		[Test]
		[ThrowsForProvider(typeof(LinqToDBException), TestProvName.AllSybase, ErrorMessage = ErrorHelper.Sybase.Error_JoinToDerivedTableWithTakeInvalid)]
		public void SelectToTypeTest1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var result = (from sep in db.Parent
							  select new ValueValueHolder
							  {
								  Child =
								  (from l in db.Child
								   select new ValueHolder
								   {
									   Id = l.ParentID + 1
								   }).FirstOrDefault()
							  }).ToList();

				var expected = from sep in Parent
							   select new ValueValueHolder
							   {
								   Child =
								   (from l in Child
									select new ValueHolder
									{
										Id = l.ParentID + 1
									}).FirstOrDefault()
							   };

				AreEqual(expected, result);
			}
		}

		[Test]
		[ThrowsForProvider(typeof(LinqToDBException), TestProvName.AllSybase, ErrorMessage = ErrorHelper.Sybase.Error_JoinToDerivedTableWithTakeInvalid)]
		public void SelectToTypeTest2([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var result = (from sep in db.Parent
							  select new ValueValueHolder
							  {
								  Child =
								  (from l in db.Child
								   select new ValueHolder
								   {
									   Id = l.ParentID + 1,
									   ParentId = l.ParentID
								   }).FirstOrDefault()
							  }).ToList();

				var expected = from sep in Parent
							   select new ValueValueHolder
							   {
								   Child =
								   (from l in Child
									select new ValueHolder
									{
										Id = l.ParentID + 1,
										ParentId = l.ParentID
									}).FirstOrDefault()
							   };

				AreEqual(expected, result);
			}
		}

		// Sybase do not supports limiting subqueries
		[Test]
		public void SelectPlainTest1([DataSources(TestProvName.AllSybase, TestProvName.AllInformix)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var expected =    Parent.Select(p =>    Child.Select(c => c.ParentID + 1).FirstOrDefault());
				var result   = db.Parent.Select(p => db.Child.Select(c => c.ParentID + 1).FirstOrDefault());

				AreEqual(expected, result);
			}
		}

		// Sybase do not supports limiting subqueries
		[Test]
		public void SelectPlainTest2([DataSources(TestProvName.AllSybase, TestProvName.AllInformix)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var expected =    Parent.Select(p => new { Id = p.ParentID, V =    Child.Select(c => c.ParentID + 1).FirstOrDefault() }).ToList().Select(_ => _.V);
				var result   = db.Parent.Select(p => new { Id = p.ParentID, V = db.Child.Select(c => c.ParentID + 1).FirstOrDefault() }).ToList().Select(_ => _.V);

				AreEqual(expected, result);
			}
		}

		[Test]
		public void SimpleSelectToType([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				AreEqual(
					   Parent.Select(_ => new ValueValueHolder { Child = new ValueHolder { Id = _.ParentID + 1 } }),
					db.Parent.Select(_ => new ValueValueHolder { Child = new ValueHolder { Id = _.ParentID + 1 } })
				);
			}
		}

		[Test]
		public void SimpleSelectToAnonimous([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				AreEqual(
					   Parent.Select(_ => new { Child = new { Id = _.ParentID + 1, ParentId = _.ParentID } }),
					db.Parent.Select(_ => new { Child = new { Id = _.ParentID + 1, ParentId = _.ParentID } })
				);
			}
		}
	}
}
