﻿using System;
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.DataProvider.SapHana;
using LinqToDB.Mapping;

using NUnit.Framework;

using Tests.Model;

namespace Tests.DataProvider
{
	[TestFixture]
	public class SapHanaTests : DataProviderTestBase
	{
		[Table("BulkInsertLowerCaseColumns")]
		public class BulkInsertLowerCaseColumns
		{
			[PrimaryKey] public int       ID;
			[Column]     public decimal   MoneyValue;
			[Column]     public DateTime? DateTimeValue;
			[Column]     public bool?     BoolValue;
			[Column]     public Guid?     GuidValue;
			[Column]     public short?    SmallIntValue;
			[Column]     public int?      IntValue;
			[Column]     public long?     BigIntValue;
		}

		[Table("BulkInsertUpperCaseColumns")]
		public class BulkInsertUpperCaseColumns
		{
			[PrimaryKey]              public int ID;
			[Column("MONEYVALUE")]    public decimal MoneyValue;
			[Column("DATETIMEVALUE")] public DateTime? DateTimeValue;
			[Column("BOOLVALUE")]     public bool? BoolValue;
			[Column("GUIDVALUE")]     public Guid? GuidValue;
			[Column("SMALLINTVALUE")] public short? SmallIntValue;
			[Column("INTVALUE")]      public int? IntValue;
			[Column("BIGINTVALUE")]   public long? BigIntValue;
		}

		const string CurrentProvider = TestProvName.AllSapHana;

		protected override string  GetNullSql  (DataConnection dc) => "SELECT \"{0}\" FROM {1} WHERE \"ID\" = 1";
		protected override string  GetValueSql (DataConnection dc) => "SELECT \"{0}\" FROM {1} WHERE \"ID\" = 2";
		protected override string? PassNullSql(DataConnection dc, out int paramCount)
		{
			paramCount = 1;
			return dc.DataProvider.Name == ProviderName.SapHanaOdbc
				? "SELECT \"ID\" FROM {1} WHERE \"{0}\" IS NULL AND ? IS NULL"
				: "SELECT \"ID\" FROM {1} WHERE \"{0}\" IS NULL AND :p IS NULL";
		}
		protected override string  PassValueSql(DataConnection dc) =>
			dc.DataProvider.Name == ProviderName.SapHanaOdbc
				? "SELECT \"ID\" FROM {1} WHERE \"{0}\" = ?"
				: "SELECT \"ID\" FROM {1} WHERE \"{0}\" = :p";

		[Test]
		public void TestParameters([IncludeDataSources(CurrentProvider)] string context)
		{
			var param1Name = context.Contains("Odbc") ? "?" : ":p";
			var param2Name = context.Contains("Odbc") ? "?" : ":p2";
			using (var conn = GetDataConnection(context))
			{
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<string>($"SELECT {param1Name} from dummy", new { p = 1 }), Is.EqualTo("1"));
					Assert.That(conn.Execute<string>($"SELECT {param1Name} from dummy", new { p = "1" }), Is.EqualTo("1"));
					Assert.That(conn.Execute<int>($"SELECT {param1Name} from dummy", new { p = new DataParameter { Value = 1 } }), Is.EqualTo(1));
					Assert.That(conn.Execute<string>($"SELECT {param1Name} from dummy", new { p = new DataParameter { Value = "1" } }), Is.EqualTo("1"));
					Assert.That(conn.Execute<int>($"SELECT {param1Name} + {param2Name} from dummy", new { p = 2, p2 = 3 }), Is.EqualTo(5));
					Assert.That(conn.Execute<int>($"SELECT {param2Name} + {param1Name} from dummy", new { p2 = 2, p = 3 }), Is.EqualTo(5));
				}
			}
		}

		[Test]
		public void TestDataTypes([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var conn = GetDataConnection(context))
			{
				using (Assert.EnterMultipleScope())
				{
					Assert.That(TestType<long?>(conn, "bigintDataType", DataType.Int64), Is.EqualTo(123456789123456789));
					Assert.That(TestType<short?>(conn, "smallintDataType", DataType.Int16), Is.EqualTo(12345));
					Assert.That(TestType<decimal?>(conn, "decimalDataType", DataType.Decimal), Is.EqualTo(1234.567m));
					Assert.That(TestType<decimal?>(conn, "smalldecimalDataType", DataType.Decimal), Is.EqualTo(123.456m));
					Assert.That(TestType<int?>(conn, "intDataType", DataType.Int32), Is.EqualTo(123456789));
					Assert.That(TestType<byte?>(conn, "tinyintDataType", DataType.Byte), Is.EqualTo(123));
					Assert.That(TestType<double?>(conn, "floatDataType", DataType.Double), Is.EqualTo(1234.567));

					//Assert.That(TestType<float?>(conn, "realDataType", DataType.Single), Is.EqualTo(1234.567f));

					Assert.That(TestType<DateTime?>(conn, "dateDataType", DataType.Date), Is.EqualTo(new DateTime(2012, 12, 12)));
					Assert.That(TestType<TimeSpan?>(conn, "timeDataType", DataType.Time), Is.EqualTo(new TimeSpan(12, 12, 12)));
					Assert.That(TestType<DateTime?>(conn, "seconddateDataType", DataType.DateTime), Is.EqualTo(new DateTime(2012, 12, 12, 12, 12, 12)));
					Assert.That(TestType<DateTime?>(conn, "timestampDataType", DataType.Timestamp), Is.EqualTo(new DateTime(2012, 12, 12, 12, 12, 12, 123)));

					Assert.That(TestType<char?>(conn, "charDataType", DataType.Char), Is.EqualTo('1'));
					Assert.That(TestType<string>(conn, "charDataType", DataType.Char), Is.EqualTo("1"));
					Assert.That(TestType<string>(conn, "charDataType", DataType.NChar), Is.EqualTo("1"));
					Assert.That(TestType<string>(conn, "varcharDataType", DataType.VarChar), Is.EqualTo("bcd"));
					Assert.That(TestType<string>(conn, "varcharDataType", DataType.NVarChar), Is.EqualTo("bcd"));

					Assert.That(TestType<string>(conn, "shorttextDataType", DataType.Text), Is.EqualTo("def"));
					Assert.That(TestType<char?>(conn, "ncharDataType", DataType.NChar), Is.EqualTo('ą'));
					Assert.That(TestType<string>(conn, "nvarcharDataType", DataType.NVarChar), Is.EqualTo("ąčęėįš"));
					Assert.That(TestType<string>(conn, "alphanumDataType", DataType.Text), Is.EqualTo("qwert123QWE"));

					Assert.That(TestType<byte[]>(conn, "binaryDataType", DataType.Binary), Is.EqualTo(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104 }));
					Assert.That(TestType<byte[]>(conn, "binaryDataType", DataType.VarBinary), Is.EqualTo(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104 }));
					Assert.That(TestType<byte[]>(conn, "varbinaryDataType", DataType.Binary), Is.EqualTo(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104 }));
					Assert.That(TestType<byte[]>(conn, "varbinaryDataType", DataType.VarBinary), Is.EqualTo(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104 }));
					Assert.That(TestType<Binary>(conn, "varbinaryDataType", DataType.VarBinary).ToArray(), Is.EqualTo(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104 }));
				}

				//field types, than cannot be included in where clause

				//Assert.That(TestType<string>(conn, "textDataType", DataType.Text), Is.EqualTo("abcdefgh"));

				//Assert.That(TestType<byte[]>(conn, "blobDataType", DataType.Binary), Is.EqualTo(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104 }));
				//Assert.That(TestType<byte[]>(conn, "blobDataType", DataType.VarBinary), Is.EqualTo(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104 }));
				//Assert.That(TestType<byte[]>(conn, "blobDataType", DataType.Blob), Is.EqualTo(new byte[] { 97, 98, 99, 100, 101, 102, 103, 104 }));

				//Assert.That(TestType<string>(conn, "clobDataType", DataType.Text), Is.EqualTo("qwertyuiop"));
				//Assert.That(TestType<string>(conn, "clobDataType", DataType.NText), Is.EqualTo("qwertyuiop"));
				//Assert.That(TestType<string>(conn, "clobDataType", DataType.VarChar), Is.EqualTo("qwertyuiop"));
				//Assert.That(TestType<string>(conn, "clobDataType", DataType.NVarChar), Is.EqualTo("qwertyuiop"));

				//Assert.That(TestType<string>(conn, "nclobDataType", DataType.Text), Is.EqualTo("ąčęėįšqwerty123456"));
				//Assert.That(TestType<string>(conn, "nclobDataType", DataType.NText), Is.EqualTo("ąčęėįšqwerty123456"));
				//Assert.That(TestType<string>(conn, "nclobDataType", DataType.VarChar), Is.EqualTo("ąčęėįšqwerty123456"));
				//Assert.That(TestType<string>(conn, "nclobDataType", DataType.NVarChar), Is.EqualTo("ąčęėįšqwerty123456"));
			}
		}

		[Test]
		public void TestDate([IncludeDataSources(CurrentProvider)] string context)
		{
			var paramName = context.Contains("Odbc") ? "?" : ":p";
			using (var conn = GetDataConnection(context))
			{
				var dateTime = new DateTime(2012, 12, 12);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<DateTime>("SELECT Cast('2012-12-12' as date) from dummy"), Is.EqualTo(dateTime));
					Assert.That(conn.Execute<DateTime?>("SELECT Cast('2012-12-12' as date) from dummy"), Is.EqualTo(dateTime));
					Assert.That(conn.Execute<DateTime>($"SELECT {paramName} from dummy", DataParameter.Date("p", dateTime)), Is.EqualTo(dateTime));
					Assert.That(conn.Execute<DateTime?>($"SELECT {paramName} from dummy", new DataParameter("p", dateTime, DataType.Date)), Is.EqualTo(dateTime));
				}
			}
		}

		[Test]
		public void TestDateTime([IncludeDataSources(CurrentProvider)] string context)
		{
			var paramName = context.Contains("Odbc") ? "?" : ":p";
			using (var conn = GetDataConnection(context))
			{
				var dateTime = new DateTime(2012, 12, 12, 12, 12, 12);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<DateTime>("SELECT Cast('2012-12-12 12:12:12' as datetime) from dummy"), Is.EqualTo(dateTime));
					Assert.That(conn.Execute<DateTime?>("SELECT Cast('2012-12-12 12:12:12' as datetime) from dummy"), Is.EqualTo(dateTime));

					Assert.That(conn.Execute<DateTime>($"SELECT {paramName} from dummy", DataParameter.DateTime("p", dateTime)), Is.EqualTo(dateTime));
					Assert.That(conn.Execute<DateTime?>($"SELECT {paramName} from dummy", new DataParameter("p", dateTime)), Is.EqualTo(dateTime));
					Assert.That(conn.Execute<DateTime?>($"SELECT {paramName} from dummy", new DataParameter("p", dateTime, DataType.DateTime)), Is.EqualTo(dateTime));
				}
			}
		}

		[Test]
		public void TestChar([IncludeDataSources(CurrentProvider)] string context)
		{
			var paramName = context.Contains("Odbc") ? "?" : ":p";
			using (var conn = GetDataConnection(context))
			{
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<char>("SELECT Cast('1' as char) from dummy"), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>("SELECT Cast('1' as char) from dummy"), Is.EqualTo('1'));
					Assert.That(conn.Execute<char>("SELECT Cast('1' as char(1)) from dummy"), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>("SELECT Cast('1' as char(1)) from dummy"), Is.EqualTo('1'));

					Assert.That(conn.Execute<char>($"SELECT {paramName} from dummy", DataParameter.Char("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>($"SELECT {paramName} from dummy", DataParameter.Char("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char>($"SELECT Cast({paramName} as char) from dummy", DataParameter.Char("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>($"SELECT Cast({paramName} as char) from dummy", DataParameter.Char("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char>($"SELECT Cast({paramName} as char(1)) from dummy", DataParameter.Char("@p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>($"SELECT Cast({paramName} as char(1)) from dummy", DataParameter.Char("@p", '1')), Is.EqualTo('1'));

					Assert.That(conn.Execute<char>($"SELECT {paramName} from dummy", DataParameter.VarChar("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>($"SELECT {paramName} from dummy", DataParameter.VarChar("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char>($"SELECT {paramName} from dummy", DataParameter.NChar("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>($"SELECT {paramName} from dummy", DataParameter.NChar("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char>($"SELECT {paramName} from dummy", DataParameter.NVarChar("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>($"SELECT {paramName} from dummy", DataParameter.NVarChar("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char>($"SELECT {paramName} from dummy", DataParameter.Create("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>($"SELECT {paramName} from dummy", DataParameter.Create("p", '1')), Is.EqualTo('1'));

					Assert.That(conn.Execute<char>($"SELECT {paramName} from dummy", new DataParameter { Name = "p", Value = '1' }), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>($"SELECT {paramName} from dummy", new DataParameter { Name = "p", Value = '1' }), Is.EqualTo('1'));
				}
			}
		}

		[Test]
		public void TestString([IncludeDataSources(CurrentProvider)] string context)
		{
			var paramName = context.Contains("Odbc") ? "?" : ":p";
			using (var conn = GetDataConnection(context))
			{
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<string>("SELECT Cast('12345' as char(20)) from dummy"), Is.EqualTo("12345"));
					Assert.That(conn.Execute<string>("SELECT Cast(NULL    as char(20)) from dummy"), Is.Null);

					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", DataParameter.Char("p", "123")), Is.EqualTo("123"));
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", DataParameter.VarChar("p", "123")), Is.EqualTo("123"));
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", DataParameter.Text("p", "123")), Is.EqualTo("123"));
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", DataParameter.NChar("p", "123")), Is.EqualTo("123"));
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", DataParameter.NVarChar("p", "123")), Is.EqualTo("123"));
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", DataParameter.NText("p", "123")), Is.EqualTo("123"));
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", DataParameter.Create("p", "123")), Is.EqualTo("123"));

					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", new DataParameter { Name = "p", Value = "1" }), Is.EqualTo("1"));
				}
			}
		}

		[Test]
		public void TestBinaryFromDb([IncludeDataSources(CurrentProvider)] string context)
		{
			var arr = new byte[] {97, 98, 99, 100, 101, 102, 103, 104};
			using (var conn = GetDataConnection(context))
			{
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<byte[]>("SELECT \"binaryDataType\" from \"AllTypes\" WHERE ID=2"), Is.EqualTo(arr));
					Assert.That(conn.Execute<byte[]>("SELECT \"varbinaryDataType\" from \"AllTypes\" WHERE ID=2"), Is.EqualTo(arr));
				}
			}
		}

		[Test]
		[ActiveIssue("Binary literal regression", Configuration = CurrentProvider)]
		public void TestBinaryParameterSelect([IncludeDataSources(CurrentProvider)] string context)
		{
			var arr1 = new byte[] { 46, 127, 0, 5 };

			using (var conn = GetDataConnection(context))
			{
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<byte[]>("SELECT :p from dummy", DataParameter.Binary("p", arr1)), Is.EqualTo(arr1));
					Assert.That(conn.Execute<byte[]>("SELECT :p from dummy", DataParameter.VarBinary("p", arr1)), Is.EqualTo(arr1));
					Assert.That(conn.Execute<byte[]>("SELECT :p from dummy", DataParameter.Create("p", arr1)), Is.EqualTo(arr1));
					Assert.That(conn.Execute<byte[]>("SELECT :p from dummy", DataParameter.VarBinary("p", null)), Is.Null);
					Assert.That(conn.Execute<byte[]>("SELECT :p from dummy", DataParameter.VarBinary("p", Array.Empty<byte>())), Is.EqualTo(Array.Empty<byte>()));
					Assert.That(conn.Execute<byte[]>("SELECT :p from dummy", DataParameter.Image("p", Array.Empty<byte>())), Is.EqualTo(Array.Empty<byte>()));
					Assert.That(conn.Execute<byte[]>("SELECT :p from dummy", new DataParameter { Name = "p", Value = arr1 }), Is.EqualTo(arr1));
					Assert.That(conn.Execute<byte[]>("SELECT :p from dummy", DataParameter.Create("p", new Binary(arr1))), Is.EqualTo(arr1));
					Assert.That(conn.Execute<byte[]>("SELECT :p from dummy", new DataParameter("p", new Binary(arr1))), Is.EqualTo(arr1));
				}
			}
		}

		[Test]
		public void TestXml([IncludeDataSources(CurrentProvider)] string context)
		{
			var paramName = context.Contains("Odbc") ? "?" : ":p";
			using (var conn = GetDataConnection(context))
			{
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<string>("SELECT '<xml/>' from dummy"), Is.EqualTo("<xml/>"));
					Assert.That(conn.Execute<XDocument>("SELECT '<xml/>' from dummy").ToString(), Is.EqualTo("<xml />"));
					Assert.That(conn.Execute<XmlDocument>("SELECT '<xml/>' from dummy").InnerXml, Is.EqualTo("<xml />"));
				}

				var xdoc = XDocument.Parse("<xml/>");
				var xml = Convert<string, XmlDocument>.Lambda("<xml/>");
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", DataParameter.Xml("p", "<xml/>")), Is.EqualTo("<xml/>"));
					Assert.That(conn.Execute<XDocument>($"SELECT {paramName} from dummy", DataParameter.Xml("p", xdoc)).ToString(), Is.EqualTo("<xml />"));
					Assert.That(conn.Execute<XmlDocument>($"SELECT {paramName} from dummy", DataParameter.Xml("p", xml)).InnerXml, Is.EqualTo("<xml />"));
					Assert.That(conn.Execute<XDocument>($"SELECT {paramName} from dummy", new DataParameter("p", xdoc)).ToString(), Is.EqualTo("<xml />"));
					Assert.That(conn.Execute<XDocument>($"SELECT {paramName} from dummy", new DataParameter("p", xml)).ToString(), Is.EqualTo("<xml />"));
				}
			}
		}

		enum TestEnum
		{
			[MapValue("A")] AA,
			[MapValue("B")] BB,
		}

		[Test]
		public void TestEnum1([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var conn = GetDataConnection(context))
			{
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<TestEnum>("SELECT 'A' from dummy"), Is.EqualTo(TestEnum.AA));
					Assert.That(conn.Execute<TestEnum?>("SELECT 'A' from dummy"), Is.EqualTo(TestEnum.AA));
					Assert.That(conn.Execute<TestEnum>("SELECT 'B' from dummy"), Is.EqualTo(TestEnum.BB));
					Assert.That(conn.Execute<TestEnum?>("SELECT 'B' from dummy"), Is.EqualTo(TestEnum.BB));
				}
			}
		}

		[Test]
		public void TestEnum2([IncludeDataSources(CurrentProvider)] string context)
		{
			var paramName = context.Contains("Odbc") ? "?" : ":p";
			using (var conn = GetDataConnection(context))
			{
				using (Assert.EnterMultipleScope())
				{
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", new { p = TestEnum.AA }), Is.EqualTo("A"));
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", new { p = (TestEnum?)TestEnum.BB }), Is.EqualTo("B"));

					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", new { p = ConvertTo<string>.From((TestEnum?)TestEnum.AA) }), Is.EqualTo("A"));
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", new { p = ConvertTo<string>.From(TestEnum.AA) }), Is.EqualTo("A"));
					Assert.That(conn.Execute<string>($"SELECT {paramName} from dummy", new { p = conn.MappingSchema.GetConverter<TestEnum?, string>()!(TestEnum.AA) }), Is.EqualTo("A"));
				}
			}
		}

		[Table("AllTypes")]
		public partial class AllType
		{
			[PrimaryKey, Identity]
			public int ID { get; set; } // int(11)
			[Column, Nullable]
			public long? bigintDataType { get; set; } // bigint(20)
			[Column, Nullable]
			public short? smallintDataType { get; set; } // smallint(6)
			[Column, Nullable]
			public decimal? decimalDataType { get; set; } // decimal(19,4)
			[Column, Nullable]
			public decimal? smalldecimalDataType { get; set; } // decimal(10,4)
			[Column, Nullable]
			public int? intDataType { get; set; } // int(11)
			[Column, Nullable]
			public byte? tinyintDataType { get; set; } // tinyint(4)
			[Column, Nullable]
			public double? floatDataType { get; set; } // float
			[Column, Nullable]
			public float? realDataType { get; set; } // real

			[Column, Nullable]
			public DateTime? dateDataType { get; set; } // date
			[Column, Nullable]
			public TimeSpan? timeDataType { get; set; } // time
			[Column, Nullable]
			public DateTime? seconddateDataType { get; set; } // datetime
			[Column, Nullable]
			public DateTime? timestampDataType { get; set; } // timestamp

			[Column, Nullable]
			public char? charDataType { get; set; } // char(1)
			[Column, Nullable]
			public string? char20DataType { get; set; } // varchar(20)
			[Column, Nullable]
			public string? varcharDataType { get; set; } // varchar(20)
			[Column, Nullable]
			public string? textDataType { get; set; } // text
			[Column, Nullable]
			public string? shorttextDataType { get; set; } // text
			[Column, Nullable]
			public char? ncharDataType { get; set; } // char(1)
			[Column, Nullable]
			public string? nchar20DataType { get; set; } // varchar(20)
			[Column, Nullable]
			public string? nvarcharDataType { get; set; } // varchar(20)
			[Column, Nullable]
			public string? alphanumDataType { get; set; } // varchar(20)

			[Column, Nullable]
			public byte[]? binaryDataType { get; set; } // binary(3)
			[Column, Nullable]
			public byte[]? varbinaryDataType { get; set; } // varbinary(5)

			[Column, Nullable]
			public byte[]? blobDataType { get; set; } // blob
			[Column, Nullable]
			public string? clobDataType { get; set; } // clob
			[Column, Nullable]
			public string? nclobDataType { get; set; } // nclob
		}

		void BulkCopyTest(string context, BulkCopyType bulkCopyType)
		{
			using (var conn = GetDataConnection(context))
			{
				conn.BeginTransaction();

				conn.BulkCopy(new BulkCopyOptions { MaxBatchSize = 50, BulkCopyType = bulkCopyType },
					Enumerable.Range(0, 100).Select(n =>
						new AllType
						{
							ID                   = 2000 + n,
							bigintDataType       = 3000 + n,
							smallintDataType     = (short)(4000 + n),
							decimalDataType      = 900000 + n,
							smalldecimalDataType = 90000 + n,
							intDataType          = 7000 + n,
							tinyintDataType      = (byte)(5000 + n),
							floatDataType        = 7700 + n,
							realDataType         = 7600 + n,

							dateDataType       = TestData.DateTime,
							timeDataType       = TestData.TimeOfDay,
							seconddateDataType = TestData.DateTime,
							timestampDataType  = TestData.DateTime,

							charDataType      = 'A',
							varcharDataType   = "AA",
							textDataType      = "text",
							shorttextDataType = "shorttext",
							ncharDataType     = '\u00fc',
							nvarcharDataType  = "A\u00fcfsdf\u00fc",
							alphanumDataType  = "abcQWE654",
							binaryDataType    = new byte[] { 1 },
							varbinaryDataType = new byte[] { 1, 2, 3 },
							blobDataType      = new byte[] { 1, 2, 3, 4, 5, 6 },
							clobDataType      = "clobclobclob",
							nclobDataType     = "nclob\u00fcnclob\u00fcnclob\u00fc"
						}));
			}
		}

		async Task BulkCopyTestAsync(string context, BulkCopyType bulkCopyType)
		{
			using (var conn = GetDataConnection(context))
			{
				conn.BeginTransaction();

				await conn.BulkCopyAsync(new BulkCopyOptions { MaxBatchSize = 50, BulkCopyType = bulkCopyType },
					Enumerable.Range(0, 100).Select(n =>
						new AllType
						{
							ID                   = 2000 + n,
							bigintDataType       = 3000 + n,
							smallintDataType     = (short)(4000 + n),
							decimalDataType      = 900000 + n,
							smalldecimalDataType = 90000 + n,
							intDataType          = 7000 + n,
							tinyintDataType      = (byte)(5000 + n),
							floatDataType        = 7700 + n,
							realDataType         = 7600 + n,

							dateDataType       = TestData.DateTime,
							timeDataType       = TestData.TimeOfDay,
							seconddateDataType = TestData.DateTime,
							timestampDataType  = TestData.DateTime,

							charDataType      = 'A',
							varcharDataType   = "AA",
							textDataType      = "text",
							shorttextDataType = "shorttext",
							ncharDataType     = '\u00fc',
							nvarcharDataType  = "A\u00fcfsdf\u00fc",
							alphanumDataType  = "abcQWE654",
							binaryDataType    = new byte[] { 1 },
							varbinaryDataType = new byte[] { 1, 2, 3 },
							blobDataType      = new byte[] { 1, 2, 3, 4, 5, 6 },
							clobDataType      = "clobclobclob",
							nclobDataType     = "nclob\u00fcnclob\u00fcnclob\u00fc"
						}));
			}
		}

		[Test]
		public void BulkCopyMultipleRows([IncludeDataSources(CurrentProvider)] string context)
		{
			BulkCopyTest(context, BulkCopyType.MultipleRows);
		}

		[Test]
		public void BulkCopyProviderSpecific([IncludeDataSources(CurrentProvider)] string context)
		{
			BulkCopyTest(context, BulkCopyType.ProviderSpecific);
		}

		[Test]
		public async Task BulkCopyMultipleRowsAsync([IncludeDataSources(CurrentProvider)] string context)
		{
			await BulkCopyTestAsync(context, BulkCopyType.MultipleRows);
		}

		[Test]
		public async Task BulkCopyProviderSpecificAsync([IncludeDataSources(CurrentProvider)] string context)
		{
			await BulkCopyTestAsync(context, BulkCopyType.ProviderSpecific);
		}

		[Test]
		public void BulkCopyProviderSpecificUpperCaseColumns([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				try
				{
					var result = db.BulkCopy(
						new BulkCopyOptions { BulkCopyType = BulkCopyType.ProviderSpecific },
						Enumerable.Range(0, 10).Select(n =>
							new BulkInsertUpperCaseColumns
							{
								ID            = 4000 + n,
								MoneyValue    = 1000m + n,
								DateTimeValue = new DateTime(2001, 1, 11, 1, 11, 21, 100),
								BoolValue     = true,
								GuidValue     = TestData.SequentialGuid(n),
								SmallIntValue = (short)n
							}
						));
					Assert.That(result.RowsCopied, Is.EqualTo(10));
				}
				finally
				{
					var count = db.GetTable<BulkInsertUpperCaseColumns>().Delete(p => p.ID >= 4000);
					Assert.That(count, Is.EqualTo(10));
				}
			}
		}

		[Test]
		public async Task BulkCopyProviderSpecificUpperCaseColumnsAsync([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				try
				{
					var result = await db.BulkCopyAsync(
						new BulkCopyOptions { BulkCopyType = BulkCopyType.ProviderSpecific },
						Enumerable.Range(0, 10).Select(n =>
							new BulkInsertUpperCaseColumns
							{
								ID            = 4000 + n,
								MoneyValue    = 1000m + n,
								DateTimeValue = new DateTime(2001, 1, 11, 1, 11, 21, 100),
								BoolValue     = true,
								GuidValue     = TestData.SequentialGuid(n),
								SmallIntValue = (short)n
							}
						));
					Assert.That(result.RowsCopied, Is.EqualTo(10));
				}
				finally
				{
					var count = await db.GetTable<BulkInsertUpperCaseColumns>().DeleteAsync(p => p.ID >= 4000);
					Assert.That(count, Is.EqualTo(10));
				}
			}
		}

		[Test]
		public void BulkCopyProviderSpecificLowerCaseColumns([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				try
				{
					var result = db.BulkCopy(
						new BulkCopyOptions { BulkCopyType = BulkCopyType.ProviderSpecific },
						Enumerable.Range(0, 10).Select(n =>
							new BulkInsertLowerCaseColumns
							{
								ID            = 4000 + n,
								MoneyValue    = 1000m + n,
								DateTimeValue = new DateTime(2001, 1, 11, 1, 11, 21, 100),
								BoolValue     = true,
								GuidValue     = TestData.SequentialGuid(n),
								SmallIntValue = (short)n
							}
						));
					Assert.That(result.RowsCopied, Is.EqualTo(10));
				}
				finally
				{
					var count = db.GetTable<BulkInsertLowerCaseColumns>().Delete(p => p.ID >= 4000);
					Assert.That(count, Is.EqualTo(10));
				}
			}
		}

		[Test]
		public async Task BulkCopyProviderSpecificLowerCaseColumnsAsync([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				try
				{
					var result = await db.BulkCopyAsync(
						new BulkCopyOptions { BulkCopyType = BulkCopyType.ProviderSpecific },
						Enumerable.Range(0, 10).Select(n =>
							new BulkInsertLowerCaseColumns
							{
								ID = 4000 + n,
								MoneyValue = 1000m + n,
								DateTimeValue = new DateTime(2001, 1, 11, 1, 11, 21, 100),
								BoolValue = true,
								GuidValue = TestData.SequentialGuid(n),
								SmallIntValue = (short)n
							}
						));
					Assert.That(result.RowsCopied, Is.EqualTo(10));
				}
				finally
				{
					var count = await db.GetTable<BulkInsertLowerCaseColumns>().DeleteAsync(p => p.ID >= 4000);
					Assert.That(count, Is.EqualTo(10));
				}
			}
		}

		[Test]
		public void BulkCopyLinqTypes([IncludeDataSources(CurrentProvider)] string context)
		{
			foreach (var bulkCopyType in new[] { BulkCopyType.MultipleRows, BulkCopyType.ProviderSpecific })
			{
				using (var db = GetDataConnection(context))
				{
					try
					{
						db.BulkCopy(
							new BulkCopyOptions { BulkCopyType = bulkCopyType },
							Enumerable.Range(0, 10).Select(n =>
								new LinqDataTypes
								{
									ID            = 4000 + n,
									MoneyValue    = 1000m + n,
									DateTimeValue = new DateTime(2001, 1, 11, 1, 11, 21, 100),
									BoolValue     = true,
									GuidValue     = TestData.SequentialGuid(n),
									SmallIntValue = (short)n
								}
							));
					}
					finally
					{
						db.GetTable<LinqDataTypes>().Delete(p => p.ID >= 4000);
					}
				}
			}
		}

		[Test]
		public async Task BulkCopyLinqTypesAsync([IncludeDataSources(CurrentProvider)] string context)
		{
			foreach (var bulkCopyType in new[] { BulkCopyType.MultipleRows, BulkCopyType.ProviderSpecific })
			{
				using (var db = GetDataConnection(context))
				{
					try
					{
						await db.BulkCopyAsync(
							new BulkCopyOptions { BulkCopyType = bulkCopyType },
							Enumerable.Range(0, 10).Select(n =>
								new LinqDataTypes
								{
									ID            = 4000 + n,
									MoneyValue    = 1000m + n,
									DateTimeValue = new DateTime(2001, 1, 11, 1, 11, 21, 100),
									BoolValue     = true,
									GuidValue     = TestData.SequentialGuid(n),
									SmallIntValue = (short)n
								}
							));
					}
					finally
					{
						await db.GetTable<LinqDataTypes>().DeleteAsync(p => p.ID >= 4000);
					}
				}
			}
		}

		[ActiveIssue("Calculation view missing in database", Configuration = CurrentProvider)]
		[Test]
		public void CalculationViewLinqQuery([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var ctx = new CalcViewInputParameters(context))
			{
				var query = ctx.CaParamTest(10,10.01, "mandatory", null, null, "optional")
					.Where(x=>x.intmandatory > 0);
				var result = query.ToList();
				Assert.That(result, Is.Not.Empty);
			}
		}

		[Test]
		public void CalculationViewLinqQueryCaching([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var ctx = new CalcViewInputParameters(context))
			{
				var query1 = ctx.CaParamTest(10, 10.01, "mandatory1", null, null, "optional1");
				var query2 = ctx.CaParamTest(100, 100.001, "mandatory2", null, 10.1, "optional2");

				// test view missing
				//query1.ToArray();
				//query2.ToArray();

				Assert.That(query1.ToSqlQuery().Sql, Is.Not.EqualTo(query2.ToSqlQuery().Sql));
			}
		}

		public class CalcViewInputParameters : DataConnection
		{
			public CalcViewInputParameters(string configuration) : base(configuration)
			{

			}

			[CalculationViewInputParametersExpression]
			public LinqToDB.ITable<FIT_CA_PARAM_TEST> CaParamTest(
				[SqlQueryDependent] int  ipIntMandatory, [SqlQueryDependent] double  ipDoubleMandatory, [SqlQueryDependent] string ipStringMandatory,
				[SqlQueryDependent] int? ipIntOptional,  [SqlQueryDependent] double? ipDoubleOptional,  [SqlQueryDependent] string ipStringOptional)
			{
				return this.TableFromExpression(() => CaParamTest(
					ipIntMandatory, ipDoubleMandatory,
					ipStringMandatory, ipIntOptional,
					ipDoubleOptional, ipStringOptional));
			}
		}

		[Table(Schema = "_SYS_BIC", Name = "FIT/CA_PARAM_TEST")]
		public partial class FIT_CA_PARAM_TEST
		{
			[Column, NotNull]
			public int commonMiscConstantId { get; set; }
			[Column, NotNull]
			public int intmandatory { get; set; }
			[Column, NotNull]
			public double doublemandatory { get; set; }
			[Column, NotNull]
			public string stringmandatory { get; set; } = null!;
			[Column, Nullable]
			public int intoptional { get; set; }
			[Column, Nullable]
			public double doubleoptional { get; set; }
			[Column, Nullable]
			public string? stringoptional { get; set; }
		}

		[Test]
		public void SelectAllTypes([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var db = GetDataContext(context))
			{
				// This query fails for ODBC x64 provider with
				// "Arithmetic operation resulted in an overflow"
				db.GetTable<AllType>().Take(100).ToList();
			}
		}

		[Test]
		public void ByDefaultLoadCurrentSchemaOnly([IncludeDataSources(CurrentProvider)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				var currentSchema = TestUtils.GetSchemaName(db, context);
				var schema = db.DataProvider.GetSchemaProvider().GetSchema(db);

				foreach (var table in schema.Tables)
					Assert.That(table.SchemaName, Is.EqualTo(currentSchema));

				foreach (var procedure in schema.Procedures)
					Assert.That(procedure.SchemaName, Is.EqualTo(currentSchema));
			}
		}

		[Table(Name = "AllTypesGeo")]
		public partial class AllTypesGeo
		{
			[PrimaryKey, Identity        ] public int     ID                 { get; set; } // INTEGER
			[Column("dataType")          ] public string? DataType           { get; set; } // VARCHAR(20)
			[Column("stgeometryDataType")] public byte[]? StgeometryDataType { get; set; } // ST_GEOMETRY
		}

		[Test]
		public void TestGeometryTypesNative([IncludeDataSources(true, ProviderName.SapHanaNative)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var data = db.GetTable<AllTypesGeo>().ToArray();

				Assert.That(data, Has.Length.EqualTo(7));
			}
		}

		[Test]
		public void TestGeometryTypesODBC([IncludeDataSources(ProviderName.SapHanaOdbc)] string context)
		{
			// ODBC provider doesn't support spatial types
			// https://github.com/dotnet/runtime/issues/40707
			using (var db = GetDataContext(context))
			{
				Assert.Throws<ArgumentException>(() => db.GetTable<AllTypesGeo>().ToArray());
			}
		}

		[Test]
		public void TestModule([IncludeDataSources(false, TestProvName.AllSapHana)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				Assert.That(db.ExecuteProc<int>("TEST_PROCEDURE", new { i = 1 }), Is.EqualTo(4));

				// native provider cannot call library member As it replaces : separator with ? parameter token
				if (context.IsAnyOf(ProviderName.SapHanaOdbc))
				{
					using (Assert.EnterMultipleScope())
					{
						Assert.That(db.QueryProc<int>("TEST_PACKAGE1:TEST_PROCEDURE", new { i = 1 }).First(), Is.EqualTo(2));
						Assert.That(db.QueryProc<int>("TEST_PACKAGE2:TEST_PROCEDURE", new { i = 1 }).First(), Is.EqualTo(3));
					}
				}

				Assert.That(db.Person.Select(p => SapHanaModuleFunctions.TestFunction(1)).First(), Is.EqualTo(4));
				if (context.IsAnyOf(ProviderName.SapHanaOdbc))
				{
					using (Assert.EnterMultipleScope())
					{
						Assert.That(db.Person.Select(p => SapHanaModuleFunctions.TestFunctionP1(1)).First(), Is.EqualTo(2));
						Assert.That(db.Person.Select(p => SapHanaModuleFunctions.TestFunctionP2(1)).First(), Is.EqualTo(3));
					}
				}

				Assert.That(SapHanaModuleFunctions.TestTableFunction(db, 1).Select(r => r.O).First(), Is.EqualTo(4));
				if (context.IsAnyOf(ProviderName.SapHanaOdbc))
				{
					using (Assert.EnterMultipleScope())
					{
						Assert.That(SapHanaModuleFunctions.TestTableFunctionP1(db, 1).Select(r => r.O).First(), Is.EqualTo(2));
						Assert.That(SapHanaModuleFunctions.TestTableFunctionP2(db, 1).Select(r => r.O).First(), Is.EqualTo(3));
					}
				}
			}
		}

		static class SapHanaModuleFunctions
		{
			[Sql.Function("TEST_FUNCTION", ServerSideOnly = true)]
			public static int TestFunction(int param)
			{
				throw new InvalidOperationException("Scalar function cannot be called outside of query");
			}

			[Sql.Function("TEST_PACKAGE1:TEST_FUNCTION", ServerSideOnly = true)]
			public static int TestFunctionP1(int param)
			{
				throw new InvalidOperationException("Scalar function cannot be called outside of query");
			}

			[Sql.Function("TEST_PACKAGE2:TEST_FUNCTION", ServerSideOnly = true)]
			public static int TestFunctionP2(int param)
			{
				throw new InvalidOperationException("Scalar function cannot be called outside of query");
			}

			[Sql.TableFunction("TEST_TABLE_FUNCTION", argIndices: new[] { 1 })]
			public static LinqToDB.ITable<Record> TestTableFunction(IDataContext db, int param1)
			{
				return db.GetTable<Record>(null, (MethodInfo)MethodBase.GetCurrentMethod()!, db, param1);
			}

			[Sql.TableFunction("TEST_TABLE_FUNCTION", argIndices: new[] { 1 }, Package = "TEST_PACKAGE1")]
			public static LinqToDB.ITable<Record> TestTableFunctionP1(IDataContext db, int param1)
			{
				return db.GetTable<Record>(null, (MethodInfo)MethodBase.GetCurrentMethod()!, db, param1);
			}

			[Sql.TableFunction("TEST_TABLE_FUNCTION", argIndices: new[] { 1 }, Package = "TEST_PACKAGE2")]
			public static LinqToDB.ITable<Record> TestTableFunctionP2(IDataContext db, int param1)
			{
				return db.GetTable<Record>(null, (MethodInfo)MethodBase.GetCurrentMethod()!, db, param1);
			}

			public sealed class Record
			{
				public int O { get; set; }
			}
		}
	}
}
