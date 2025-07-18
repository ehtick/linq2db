﻿#if NETFRAMEWORK && COMPAT
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(LinqToDB.Configuration.LinqToDBSection))]
#elif NETFRAMEWORK || COMPAT
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;

using CSS = System.Configuration.ConnectionStringSettings;

namespace LinqToDB.Configuration
{
	/// <summary>
	/// Implementation of custom configuration section.
	/// </summary>
	public class LinqToDBSection : ConfigurationSection, ILinqToDBSettings
	{
		static readonly ConfigurationPropertyCollection _properties               = new ConfigurationPropertyCollection();
		static readonly ConfigurationProperty           _propDataProviders        = new ConfigurationProperty("dataProviders",        typeof(DataProviderElementCollection), new DataProviderElementCollection(), ConfigurationPropertyOptions.None);
		static readonly ConfigurationProperty           _propDefaultConfiguration = new ConfigurationProperty("defaultConfiguration", typeof(string),                        null,                                ConfigurationPropertyOptions.None);
		static readonly ConfigurationProperty           _propDefaultDataProvider  = new ConfigurationProperty("defaultDataProvider",  typeof(string),                        null,                                ConfigurationPropertyOptions.None);

		static LinqToDBSection()
		{
			_properties.Add(_propDataProviders);
			_properties.Add(_propDefaultConfiguration);
			_properties.Add(_propDefaultDataProvider);
		}

		private static LinqToDBSection? _instance;
		/// <summary>
		/// linq2db configuration section.
		/// </summary>
		public  static LinqToDBSection?  Instance
		{
			get
			{
				if (_instance == null)
				{
					try
					{
						_instance = (LinqToDBSection?)ConfigurationManager.GetSection("linq2db") ?? new();
					}
					catch (SecurityException)
					{
						return null;
					}
				}

				return _instance;
			}
		}

		protected override ConfigurationPropertyCollection Properties => _properties;

		/// <summary>
		/// Gets list of data providers configuration elements.
		/// </summary>
		public DataProviderElementCollection DataProviders => (DataProviderElementCollection)base[_propDataProviders];

		/// <summary>
		/// Gets default connection configuration name.
		/// </summary>
		public string DefaultConfiguration => (string)base[_propDefaultConfiguration];
		/// <summary>
		/// Gets default data provider configuration name.
		/// </summary>
		public string DefaultDataProvider  => (string)base[_propDefaultDataProvider];

		IEnumerable<IConnectionStringSettings> ILinqToDBSettings.ConnectionStrings
		{
			get
			{
				foreach (CSS css in ConfigurationManager.ConnectionStrings)
					yield return new ConnectionStringEx(css);
			}
		}

		IEnumerable<IDataProviderSettings> ILinqToDBSettings.DataProviders => DataProviders.OfType<DataProviderElement>();

		sealed class ConnectionStringEx : IConnectionStringSettings
		{
			private readonly CSS _css;

			public ConnectionStringEx(CSS css)
			{
				_css = css;
			}

			public string ConnectionString => _css.ConnectionString;
			public string Name             => _css.Name;
			public string ProviderName     => _css.ProviderName;
			public bool   IsGlobal         => IsMachineConfig(_css);
		}

		internal static bool IsMachineConfig(CSS css)
		{
			string? source;
			bool   isPresent;

			try
			{
				source    = css.ElementInformation.Source;
				isPresent = css.ElementInformation.IsPresent;
			}
			catch
			{
				source    = "";
				isPresent = true;
			}

			return
				 isPresent == false &&
				(source    == null  ||
				source.EndsWith("machine.config", StringComparison.OrdinalIgnoreCase));
		}
	}
}
#endif
