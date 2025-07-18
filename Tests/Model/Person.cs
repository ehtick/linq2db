﻿using System;

using LinqToDB;
using LinqToDB.Mapping;

namespace Tests.Model
{
	public class Person : IPerson
	{
		public Person()
		{
		}

		public Person(int id)
		{
			ID = id;
		}

		public Person(int id, string firstName)
		{
			ID        = id;
			FirstName = firstName;
		}

		// Firebird: it duplicates identity generation trigger job
		//[SequenceName(ProviderName.Firebird, "PersonID")]
		[Column("PersonID", Configuration = ProviderName.ClickHouse)]
		[Column("PersonID", IsIdentity = true), PrimaryKey]
		                                               public int     ID;
		[NotNull]                                      public string  FirstName { get; set; } = null!;
		[NotNull]                                      public string  LastName = null!;
		[Nullable]                                     public string? MiddleName;
		[Column(DataType = DataType.Char, Length = 1)] public Gender  Gender;

		[NotColumn]
		int IPerson.ID
		{
			get => ID;
			set => ID = value;
		}

		[NotColumn]
		string IPerson.FirstName
		{
			get { return FirstName; }
			set { FirstName = value; }
		}

		[NotColumn]
		string IPerson.LastName
		{
			get { return LastName; }
			set { LastName = value; }
		}
		[NotColumn]
		string? IPerson.MiddleName
		{
			get { return MiddleName; }
			set { MiddleName = value; }
		}
		[NotColumn]
		Gender IPerson.Gender
		{
			get { return Gender; }
			set { Gender = value; }
		}

		[NotColumn] public string Name { get { return FirstName + " " + LastName; }}

		[Association(ThisKey = "ID", OtherKey = "PersonID", CanBeNull=true)]
		public Patient? Patient;

		public override bool Equals(object? obj)
		{
			return Equals(obj as Person);
		}

		public bool Equals(Person? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return
				other.ID == ID &&
				Equals(other.LastName,   LastName) &&
				Equals(other.MiddleName, MiddleName) &&
				other.Gender == Gender &&
				Equals(other.FirstName,  FirstName);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(
				ID,
				LastName,
				MiddleName,
				Gender,
				FirstName
			);
		}
	}
}
