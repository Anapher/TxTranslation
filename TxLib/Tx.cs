﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace TxLib
{
	public static class Tx
	{
		#region Constants

		/// <summary>
		/// Environment variable name that specifies the log file name. If the environment variable
		/// is set but empty, the log messages are sent to Trace. The environment variable is only
		/// evaluated when the process starts.
		/// </summary>
		private const string LogFileEnvironmentVariableName = "TX_LOG_FILE";

		/// <summary>
		/// Environment variable name that indicates whether the usage of text keys is tracked and
		/// unused text keys from the primary culture are logged before the process ends. To enable
		/// this option, set the environment variable value to "1". This will only work if a log
		/// file is specified (not Trace) and a primary culture has been set. If this option is
		/// enabled, placeholder names provided by the application but unused by the translated
		/// text will also be logged to the current log target. The environment variable is only
		/// evaluated when the process starts.
		/// </summary>
		private const string LogUnusedEnvironmentVariableName = "TX_LOG_UNUSED";

		private const int WriteLockTimeout = 10000;
		private const int ReadLockTimeout = 1000;
		private const int ReloadChangesDelay = 2000;

		/// <summary>
		/// Defines text keys that are used by various format methods of the Tx class.
		/// </summary>
		public static class SystemKeys
		{
			public const string Colon = "Tx:colon";
			public const string ParenthesisBegin = "Tx:parenthesis begin";
			public const string ParenthesisEnd = "Tx:parenthesis end";
			public const string QuoteBegin = "Tx:quote begin";
			public const string QuoteEnd = "Tx:quote end";
			public const string QuoteNestedBegin = "Tx:quote nested begin";
			public const string QuoteNestedEnd = "Tx:quote nested end";
			public const string NumberNegative = "Tx:number.negative";
			public const string NumberDecimalSeparator = "Tx:number.decimal separator";
			public const string NumberGroupSeparator = "Tx:number.group separator";
			public const string NumberGroupSeparatorThreshold = "Tx:number.group separator threshold";
			public const string NumberUnitSeparator = "Tx:number.unit separator";
			public const string NumberOrdinal = "Tx:number.ordinal";
			public const string NumberOrdinalFeminin = "Tx:number.ordinal f";
			public const string ByteUnit = "Tx:byte unit";
			public const string DateYear = "Tx:date.year";
			public const string DateYearMonth = "Tx:date.year month";
			public const string DateYearMonthAbbr = "Tx:date.year month abbr";
			public const string DateYearMonthLong = "Tx:date.year month long";
			public const string DateYearMonthDay = "Tx:date.year month day";
			public const string DateYearMonthDayAbbr = "Tx:date.year month day abbr";
			public const string DateYearMonthDayLong = "Tx:date.year month day long";
			public const string DateMonth = "Tx:date.month";
			public const string DateMonthAbbr = "Tx:date.month abbr";
			public const string DateMonthLong = "Tx:date.month long";
			public const string DateMonthDay = "Tx:date.month day";
			public const string DateMonthDayAbbr = "Tx:date.month day abbr";
			public const string DateMonthDayLong = "Tx:date.month day long";
			public const string DateDay = "Tx:date.day";
			public const string DateYearQuarter = "Tx:date.year quarter";
			public const string DateQuarter = "Tx:date.quarter";
			public const string DateDowWithDate = "Tx:date.dow with date";
			public const string TimeHourMinuteSecondMs = "Tx:time.hour minute second ms";
			public const string TimeHourMinuteSecond = "Tx:time.hour minute second";
			public const string TimeHourMinute = "Tx:time.hour minute";
			public const string TimeHour = "Tx:time.hour";
			public const string TimeAM = "Tx:time.am";
			public const string TimePM = "Tx:time.pm";
			public const string TimeRelativeSeparator = "Tx:time.relative separator";
			public const string TimeNow = "Tx:time.now";
			public const string TimeNever = "Tx:time.never";
			public const string TimeRelative = "Tx:time.relative";
			public const string TimeRelativeNeg = "Tx:time.relative neg";
			public const string TimeRelativeYears = "Tx:time.relative.years";
			public const string TimeRelativeMonths = "Tx:time.relative.months";
			public const string TimeRelativeDays = "Tx:time.relative.days";
			public const string TimeRelativeHours = "Tx:time.relative.hours";
			public const string TimeRelativeMinutes = "Tx:time.relative.minutes";
			public const string TimeRelativeSeconds = "Tx:time.relative.seconds";
			public const string TimeRelativeNegYears = "Tx:time.relative neg.years";
			public const string TimeRelativeNegMonths = "Tx:time.relative neg.months";
			public const string TimeRelativeNegDays = "Tx:time.relative neg.days";
			public const string TimeRelativeNegHours = "Tx:time.relative neg.hours";
			public const string TimeRelativeNegMinutes = "Tx:time.relative neg.minutes";
			public const string TimeRelativeNegSeconds = "Tx:time.relative neg.seconds";
			public const string TimeSpanRelative = "Tx:time.relative span";
			public const string TimeSpanRelativeNeg = "Tx:time.relative span neg";
			public const string TimeSpanRelativeYears = "Tx:time.relative span.years";
			public const string TimeSpanRelativeMonths = "Tx:time.relative span.months";
			public const string TimeSpanRelativeDays = "Tx:time.relative span.days";
			public const string TimeSpanRelativeHours = "Tx:time.relative span.hours";
			public const string TimeSpanRelativeMinutes = "Tx:time.relative span.minutes";
			public const string TimeSpanRelativeSeconds = "Tx:time.relative span.seconds";
			public const string TimeSpanRelativeNegYears = "Tx:time.relative span neg.years";
			public const string TimeSpanRelativeNegMonths = "Tx:time.relative span neg.months";
			public const string TimeSpanRelativeNegDays = "Tx:time.relative span neg.days";
			public const string TimeSpanRelativeNegHours = "Tx:time.relative span neg.hours";
			public const string TimeSpanRelativeNegMinutes = "Tx:time.relative span neg.minutes";
			public const string TimeSpanRelativeNegSeconds = "Tx:time.relative span neg.seconds";
			public const string TimeSpan = "Tx:time.span";
			public const string TimeSpanNeg = "Tx:time.span neg";
			public const string TimeSpanYears = "Tx:time.span.years";
			public const string TimeSpanMonths = "Tx:time.span.months";
			public const string TimeSpanDays = "Tx:time.span.days";
			public const string TimeSpanHours = "Tx:time.span.hours";
			public const string TimeSpanMinutes = "Tx:time.span.minutes";
			public const string TimeSpanSeconds = "Tx:time.span.seconds";
			public const string TimeSpanNegYears = "Tx:time.span neg.years";
			public const string TimeSpanNegMonths = "Tx:time.span neg.months";
			public const string TimeSpanNegDays = "Tx:time.span neg.days";
			public const string TimeSpanNegHours = "Tx:time.span neg.hours";
			public const string TimeSpanNegMinutes = "Tx:time.span neg.minutes";
			public const string TimeSpanNegSeconds = "Tx:time.span neg.seconds";
		}

		#endregion Constants

		#region Global data

		/// <summary>
		/// Reader/writer lock object that controls access to the global dictionary and the
		/// fileWatchers list.
		/// </summary>
		/// <remarks>
		/// A note on locking access to the dictionary: There is two places where a read lock must
		/// be acquired. One is the deepest level of helper functions. This is basically only
		/// GetText which tries several cultures to find a text. This lock at the deepest level of
		/// the call hierarchy ensures that every read access of the dictionary is secured. The
		/// other place is every method that has multiple calls to functions that will eventually
		/// access the dictionary. This is to ensure that every retrieved text belongs to the same
		/// generation of texts and that the dictionary was not changed for example between the
		/// opening and closing quotation mark. Other methods that only call GetText, ResolveData
		/// or the like a single time need not be locked.
		/// </remarks>
		private static ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();

		/// <summary>
		/// Contains all loaded languages associated with their culture name.
		/// The first level defines the culture name.
		/// The second level defines the text key.
		/// The third level defines the quantifier.
		/// The fourth level contains the actual text value.
		/// </summary>
		/// <remarks>
		/// Access to this object is synchronised through rwlock.
		/// </remarks>
		private static Dictionary<string, Dictionary<string, Dictionary<int, string>>> languages =
			new Dictionary<string, Dictionary<string, Dictionary<int, string>>>();

		/// <remarks>
		/// Access to this object is synchronised through rwlock.
		/// </remarks>
		private static bool useFileSystemWatcher;

		/// <remarks>
		/// Access to this object is synchronised through rwlock.
		/// </remarks>
		private static string primaryCulture;

		/// <summary>
		/// A list of all culture names accepted by the browser, sorted by descending preference.
		/// This must be updated by the calling ASP.NET application for every page request from the
		/// HTTP_ACCEPT_LANGUAGE header value through the SetWebCulture method.
		/// </summary>
		/// <remarks>
		/// No locking is required for this field because every thread has its own instance.
		/// </remarks>
		[ThreadStatic]
		private static string[] httpPreferredCultures;

		/// <summary>
		/// Contains a FileSystemWatcher instance monitoring each loaded file for changes.
		/// </summary>
		/// <remarks>
		/// Access to this object is synchronised through rwlock.
		/// </remarks>
		private static Dictionary<string, FileSystemWatcher> fileWatchers =
			new Dictionary<string, FileSystemWatcher>();

		private static object reloadTimerLock = new object();

		/// <remarks>
		/// Access to this object is synchronised through reloadTimerLock.
		/// </remarks>
		private static Timer reloadTimer;

		/// <summary>
		/// Contains the last write time of every loaded file to compare it for updates.
		/// </summary>
		/// <remarks>
		/// Access to this object is synchronised through rwlock.
		/// </remarks>
		private static Dictionary<string, DateTime> fileTimes =
			new Dictionary<string, DateTime>();
		
		private static object logLock = new object();

		/// <summary>
		/// File name to write log messages to. If it is empty (but not null), Trace is the log target.
		/// </summary>
		/// <remarks>
		/// Access to this object is synchronised through logLock.
		/// </remarks>
		private static string logFileName;

		/// <remarks>
		/// Access to this object is synchronised through logLock.
		/// </remarks>
		private static StreamWriter logWriter;

		/// <summary>
		/// Contains all text keys that have been requested. Used to determine unused text keys.
		/// </summary>
		/// <remarks>
		/// Access to this object is synchronised through logLock.
		/// </remarks>
		private static HashSet<string> usedKeys;

		#endregion Global data

		#region Events

		public static event EventHandler DictionaryChanged;
		private static void RaiseDictionaryChanged()
		{
			EventHandler handler = DictionaryChanged;
			if (handler != null)
			{
				// Sanity check to prevent later failures
				if (rwlock.IsWriteLockHeld)
					throw new InvalidOperationException("Internal error: DictionaryChanged event raised with writer lock still set.");

				handler(null, EventArgs.Empty);
			}
		}

		#endregion Events

		#region Static constructor

		/// <summary>
		/// Initialises the Tx class.
		/// </summary>
		static Tx()
		{
			LogFileName =
				Environment.GetEnvironmentVariable(LogFileEnvironmentVariableName) ??
				Environment.GetEnvironmentVariable(LogFileEnvironmentVariableName, EnvironmentVariableTarget.User);

			if (Environment.GetEnvironmentVariable(LogUnusedEnvironmentVariableName) == "1" ||
				Environment.GetEnvironmentVariable(LogUnusedEnvironmentVariableName, EnvironmentVariableTarget.User) == "1")
			{
				usedKeys = new HashSet<string>();
			}
		}

		#endregion Static constructor

		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether the loaded files should be monitored for
		/// changes using FileSystemWatcher instances and reloaded automatically. Only files loaded
		/// after setting this property can be monitored.
		/// </summary>
		public static bool UseFileSystemWatcher
		{
			get
			{
				using (new ReadLock(rwlock))
				{
					return useFileSystemWatcher;
				}
			}
			set
			{
				using (new UpgradeableReadLock(rwlock))
				{
					if (value != useFileSystemWatcher)
					{
						using (new WriteLock(rwlock))
						{
							useFileSystemWatcher = value;
							if (!useFileSystemWatcher)
							{
								// Dispose all created FileSystemWatcher instances for the previously loaded files
								foreach (FileSystemWatcher fsw in fileWatchers.Values)
								{
									fsw.Dispose();
								}
								fileWatchers.Clear();

								lock (reloadTimerLock)
								{
									if (reloadTimer != null)
									{
										// Cancel running timer (pending callbacks may still be invoked)
										reloadTimer.Dispose();
										reloadTimer = null;
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion Properties

		#region Load methods

		/// <summary>
		/// Loads all XML files in a directory into the global dictionary.
		/// </summary>
		/// <param name="path">Directory to load files from.</param>
		/// <param name="filePrefix">File name prefix to limit loading to.</param>
		/// <remarks>
		/// <para>
		/// This method searches the directory for files that end with the extension .txd (for Tx
		/// dictionary) and an optional culture name directly before the extension. If filePrefix
		/// is set, the matching file names additionally need to begin with this prefix and may not
		/// contain additional characters between the prefix and the culture and extension. For
		/// example, the files "demo.en-us.txd" and "demo.txd" match the prefix "demo" as well as
		/// no prefix at all, but no other prefix.
		/// </para>
		/// <para>
		/// Since the filePrefix value is directly used in a regular expression, it should not
		/// contain characters that are special to a regular expression pattern. Safe characters
		/// include letters, digits, underline (_) and hyphen (-). You may use regular expression
		/// syntax to specify wildcards, for example ".*".
		/// </para>
		/// </remarks>
		public static void LoadDirectory(string path, string filePrefix = null)
		{
			string regex = @"(\.(([a-z]{2})([-][a-z]{2})?))?\.(?:txd|xml)$";
			if (!string.IsNullOrEmpty(filePrefix))
			{
				regex = "^" + filePrefix + regex;
			}
			using (new WriteLock(rwlock))
			{
				foreach (string fileName in Directory.GetFiles(path))
				{
					Match m = Regex.Match(Path.GetFileName(fileName), regex, RegexOptions.IgnoreCase);
					if (m.Success)
					{
						LoadFromXmlFile(fileName);
					}
				}
			}
		}

		/// <summary>
		/// Loads all text definitions from an XML file into the global dictionary.
		/// </summary>
		/// <param name="fileName">Name of the XML file to load.</param>
		/// <remarks>
		/// <para>
		/// If the file name ends with ".[culture name].txd", then the culture name of the file
		/// name is used. Otherwise a combined file is assumed that contains a "culture" element
		/// with a "name" attribute for each defined culture.
		/// </para>
		/// <para>
		/// Culture names follow the format that the CultureInfo class supports and are actually
		/// validated that way. Examples are "de", "en", "de-de", "en-us" or "pt-br". Culture
		/// names are handled case-insensitively for this method.
		/// </para>
		/// </remarks>
		public static void LoadFromXmlFile(string fileName)
		{
			using (new WriteLock(rwlock))
			{
				LoadFromXmlFile(fileName, languages);
			}
		}

		/// <summary>
		/// Loads all text definitions from an XML file into a dictionary.
		/// </summary>
		/// <param name="fileName">Name of the XML file to load.</param>
		/// <param name="dict">Dictionary to load the texts into.</param>
		private static void LoadFromXmlFile(string fileName, Dictionary<string, Dictionary<string, Dictionary<int, string>>> dict)
		{
			if (!Path.IsPathRooted(fileName))
			{
				fileName = Path.GetFullPath(fileName);
			}

			using (new WriteLock(rwlock))
			{
				if (UseFileSystemWatcher)
				{
					// Monitor this file to automatically reload it when it is changed
					if (!fileWatchers.ContainsKey(fileName))
					{
						FileSystemWatcher fsw = new FileSystemWatcher(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
						fsw.InternalBufferSize = 4096;   // Minimum possible value
						fsw.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
						fsw.Changed += fsw_Changed;
						fsw.EnableRaisingEvents = true;
						fileWatchers[fileName] = fsw;
					}
				}

				// Remember last write time of the file to be able to compare it later
				fileTimes[fileName] = new FileInfo(fileName).LastWriteTimeUtc;
			}

			// First load the XML file into an XmlDocument for further processing
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(fileName);

			// Try to recognise the culture name from the file name
			Match m = Regex.Match(fileName, @"\.(([a-z]{2})([-][a-z]{2})?)\.(?:txd|xml)$", RegexOptions.IgnoreCase);
			if (m.Success)
			{
				CultureInfo ci = CultureInfo.GetCultureInfo(m.Groups[1].Value);
				LoadFromXml(ci.Name, xmlDoc.DocumentElement, dict);

				// Set the primary culture if a file claims to be it
				XmlAttribute primaryAttr = xmlDoc.DocumentElement.Attributes["primary"];
				if (primaryAttr != null && primaryAttr.Value == "true")
				{
					PrimaryCulture = ci.Name;
				}
				return;
			}

			// Try to find the culture name inside a combined XML document
			foreach (XmlElement xe in xmlDoc.DocumentElement.SelectNodes("culture[@name]"))
			{
				CultureInfo ci = CultureInfo.GetCultureInfo(xe.Attributes["name"].Value);
				LoadFromXml(ci.Name, xe, dict);

				// Set the primary culture if a culture in the file claims to be it
				XmlAttribute primaryAttr = xe.Attributes["primary"];
				if (primaryAttr != null && primaryAttr.Value == "true")
				{
					PrimaryCulture = ci.Name;
				}
			}
		}

		private static void fsw_Changed(object sender, FileSystemEventArgs e)
		{
			lock (reloadTimerLock)
			{
				if (reloadTimer != null)
				{
					// Cancel running timer (pending callbacks may still be invoked)
					reloadTimer.Dispose();
				}
				reloadTimer = new Timer(reloadTimer_Callback, null, ReloadChangesDelay, Timeout.Infinite);
			}
		}

		private static void reloadTimer_Callback(object state)
		{
			lock (reloadTimerLock)
			{
				if (reloadTimer == null) return;   // Timer has been cancelled, do nothing

				reloadTimer.Dispose();
				reloadTimer = null;
			}

			try
			{
				// Read all loaded files into a temporary dictionary
				Dictionary<string, Dictionary<string, Dictionary<int, string>>> newLanguages =
					new Dictionary<string, Dictionary<string, Dictionary<int, string>>>();
				foreach (string fileName in new List<string>(fileTimes.Keys))
				{
					LoadFromXmlFile(fileName, newLanguages);
				}

				// Replace the global dictionary with the new one to apply the new texts
				using (new WriteLock(rwlock))
				{
					languages = newLanguages;
				}

				Log("{0} files reloaded.", fileTimes.Count);
				RaiseDictionaryChanged();
			}
			catch (Exception ex)
			{
				// Catch and log any exceptions, Visual Studio will instantly terminate debugging
				// if something unexcepted happens in this thread.
				// TODO: Test behaviour with a global unhandled exception handler in place.
				System.Diagnostics.Trace.WriteLine("Unhandled " + ex.GetType().Name + " while reloading the dictionary files: " + ex.Message);
			}
		}

		/// <summary>
		/// Loads all text definitions from an embedded resource XML file into the global
		/// dictionary. Only the combined format with all cultures in one document is supported by
		/// this method.
		/// </summary>
		/// <param name="name">Name of the embedded resource file to load.</param>
		/// <remarks>
		/// The resource name is the project's default namespace and the file path relative to the
		/// project, combined with dots (.) and all path separators also replaced with dots.
		/// </remarks>
		public static void LoadFromEmbeddedResource(string name)
		{
			// First load the XML file into an XmlDocument for further processing
			Stream stream = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream(name);
			if (stream == null)
			{
				throw new ArgumentException("The embedded resource name was not found in the calling assembly.");
			}
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(stream);

			// Try to find the culture name inside a combined XML document
			using (new WriteLock(rwlock))
			{
				foreach (XmlElement xe in xmlDoc.DocumentElement.SelectNodes("culture[@name]"))
				{
					CultureInfo ci = CultureInfo.GetCultureInfo(xe.Attributes["name"].Value);
					LoadFromXml(ci.Name, xe, languages);

					// Set the primary culture if a culture in the file claims to be it
					XmlAttribute primaryAttr = xe.Attributes["primary"];
					if (primaryAttr != null && primaryAttr.Value == "true")
					{
						PrimaryCulture = ci.Name;
					}
				}
			}
			// Need to raise the event after releasing the write lock
			RaiseDictionaryChanged();
		}

		/// <summary>
		/// Loads all text definitions from an XML element into a dictionary.
		/// </summary>
		/// <param name="culture">Culture name to add the text definitions to. Must be the exact name of an existing culture in .NET.</param>
		/// <param name="xe">XML element to read the text definitions from.</param>
		/// <param name="dict">Dictionary to add texts to.</param>
		private static void LoadFromXml(string culture, XmlElement xe, Dictionary<string, Dictionary<string, Dictionary<int, string>>> dict)
		{
			// We only need the lock if we're writing directly to the global dictionary
			bool isGlobalDict;
			using (new ReadLock(rwlock))
			{
				isGlobalDict = dict == languages;
			}
			using (new WriteLock(isGlobalDict ? rwlock : null))
			{
				// Get a reference to the specified language dictionary
				Dictionary<string, Dictionary<int, string>> language;
				if (!dict.TryGetValue(culture, out language))
				{
					language = new Dictionary<string, Dictionary<int, string>>();
					dict.Add(culture, language);
				}

				// Read the XML document
				foreach (XmlNode textNode in xe.SelectNodes("text[@key]"))
				{
					string text = textNode.InnerText;
					string key = textNode.Attributes["key"].Value;

					if (key == "")
					{
						Log("Load XML: Key attribute is empty. Ignoring definition.");
						continue;
					}

					int count = -1;
					XmlAttribute countAttr = textNode.Attributes["count"];
					if (countAttr != null)
					{
						if (!int.TryParse(countAttr.Value, out count))
						{
							// Count value unparsable. Skip invalid entries
							Log("Load XML: Count attribute value of key {0} is not an integer. Ignoring definition.", key);
							continue;
						}
						if (count < 0 || count > ushort.MaxValue)
						{
							// Count value out of range. Skip invalid entries
							Log("Load XML: Count attribute value of key {0} is out of range. Ignoring definition.", key);
							continue;
						}
					}

					int modulo = 0;
					XmlAttribute moduloAttr = textNode.Attributes["mod"];
					if (moduloAttr != null)
					{
						if (!int.TryParse(moduloAttr.Value, out modulo))
						{
							// Modulo value unparsable. Skip invalid entries
							Log("Load XML: Modulo attribute of key {0} is not an integer. Ignoring definition.", key);
							continue;
						}
						if (modulo < 2 || modulo > 1000)
						{
							// Modulo value out of range. Skip invalid entries
							Log("Load XML: Modulo attribute of key {0} is out of range. Ignoring definition.", key);
							continue;
						}
					}

					Dictionary<int, string> textItem;
					if (language.TryGetValue(key, out textItem))
					{
						// Key has already been read, add the new text item.
						// Existing text items are overwritten.
						if (count != -1 && modulo != 0)
						{
							// Encode the modulo value into the quantifier.
							count = (modulo << 16) | count;
						}
						textItem[count] = text;
					}
					else
					{
						// New key.
						textItem = new Dictionary<int, string>();
						textItem.Add(count, text);
						language.Add(key, textItem);
					}
				}
			}

			if (isGlobalDict && !rwlock.IsWriteLockHeld)
			{
				// Raise the changed event if the texts have been loaded into the global
				// dictionary, but not if a write lock is held because then, nothing could be read
				// from the dictionary by others. In a situation where this method is called with
				// a write lock, the caller must raise the changed event after releasing the lock.
				RaiseDictionaryChanged();
			}
		}

		/// <summary>
		/// Checks the last write time of all loaded files and reloads all files if something has
		/// changed. This method should be called to reload the text files if necessary and only if
		/// UseFileSystemWatcher is false. In an ASP.NET environment, this could be a new page
		/// loading.
		/// </summary>
		public static void CheckReloadFiles()
		{
			using (new UpgradeableReadLock(rwlock))
			{
				bool changed = false;

				if (languages.Count == 0)
				{
					// Sometimes this seems to happen in an ASP.NET environment.
					changed = true;
				}
				else
				{
					try
					{
						foreach (KeyValuePair<string, DateTime> kvp in fileTimes)
						{
							if (new FileInfo(kvp.Key).LastWriteTimeUtc > kvp.Value)
							{
								changed = true;
								break;
							}
						}
					}
					catch
					{
						changed = true;
					}
				}

				if (changed)
				{
					// Read all known files into a temporary dictionary
					Dictionary<string, Dictionary<string, Dictionary<int, string>>> newLanguages =
						new Dictionary<string, Dictionary<string, Dictionary<int, string>>>();
					foreach (string fileName in new List<string>(fileTimes.Keys))
					{
						LoadFromXmlFile(fileName, newLanguages);
					}

					// Replace the global dictionary with the new one to apply the new texts
					using (new WriteLock(rwlock))
					{
						languages = newLanguages;
					}

					RaiseDictionaryChanged();
				}
			}
		}

		/// <summary>
		/// Adds a text to the dictionary.
		/// </summary>
		/// <param name="culture">New or existing culture name to add the text definition to. Must be the exact name of an existing culture in .NET.</param>
		/// <param name="key">Text key to add or update.</param>
		/// <param name="text">Text value to add.</param>
		public static void AddText(string culture, string key, string text)
		{
			AddText(culture, key, -1, 0, text);
		}

		/// <summary>
		/// Adds a text to the dictionary.
		/// </summary>
		/// <param name="culture">New or existing culture name to add the text definition to. Must be the exact name of an existing culture in .NET.</param>
		/// <param name="key">Text key to add or update.</param>
		/// <param name="count">Count value for the text.</param>
		/// <param name="text">Text value to add.</param>
		public static void AddText(string culture, string key, int count, string text)
		{
			AddText(culture, key, count, 0, text);
		}

		/// <summary>
		/// Adds a text to the dictionary.
		/// </summary>
		/// <param name="culture">New or existing culture name to add the text definition to. Must be the exact name of an existing culture in .NET.</param>
		/// <param name="key">Text key to add or update.</param>
		/// <param name="count">Count value for the text.</param>
		/// <param name="modulo">Modulo value for the text.</param>
		/// <param name="text">Text value to add.</param>
		public static void AddText(string culture, string key, int count, int modulo, string text)
		{
			if (count < -1 || count > ushort.MaxValue)
			{
				throw new ArgumentOutOfRangeException("count", "The count value must be in the range of 0 to ushort.MaxValue.");
			}
			if (modulo != 0 && (modulo < 2 || modulo > 1000))
			{
				throw new ArgumentOutOfRangeException("modulo", "The modulo value must be in the range of 2 to 1000.");
			}
			if (count == -1 && modulo > 0)
			{
				throw new ArgumentException("A modulo value cannot be used if no count value is set.");
			}

			using (new WriteLock(rwlock))
			{
				// Get a reference to the specified language dictionary
				Dictionary<string, Dictionary<int, string>> language;
				if (!languages.TryGetValue(culture, out language))
				{
					language = new Dictionary<string, Dictionary<int, string>>();
					languages.Add(culture, language);
				}

				Dictionary<int, string> textItem;
				if (language.TryGetValue(key, out textItem))
				{
					// Key is already defined, add the new text item.
					// Existing text items are overwritten.
					if (count != -1 && modulo != 0)
					{
						// Encode the modulo value into the quantifier.
						count = (modulo << 16) | count;
					}
					textItem[count] = text;
				}
				else
				{
					// New key.
					textItem = new Dictionary<int, string>();
					textItem.Add(count, text);
					language.Add(key, textItem);
				}
			}

			RaiseDictionaryChanged();
		}

		/// <summary>
		/// Clears the global dictionary and removes all currently loaded languages and texts.
		/// </summary>
		public static void Clear()
		{
			using (new WriteLock(rwlock))
			{
				// Dispose all created FileSystemWatcher instances for the previously loaded files
				foreach (FileSystemWatcher fsw in fileWatchers.Values)
				{
					fsw.Dispose();
				}
				fileWatchers.Clear();
				fileTimes.Clear();
				languages.Clear();
			}
			RaiseDictionaryChanged();
		}

		#endregion Load methods

		#region Culture control

		/// <summary>
		/// Gets all culture names currently available in the global dictionary.
		/// </summary>
		public static string[] AvailableCultureNames
		{
			get
			{
				using (new ReadLock(rwlock))
				{
					string[] names = new string[languages.Count];
					int i = 0;
					foreach (string name in languages.Keys)
					{
						names[i++] = name;
					}
					return names;
				}
			}
		}

		/// <summary>
		/// Gets all culture instances currently available in the global dictionary.
		/// </summary>
		public static CultureInfo[] AvailableCultures
		{
			get
			{
				string[] names = AvailableCultureNames;
				CultureInfo[] cis = new CultureInfo[names.Length];
				for (int i = 0; i < names.Length; i++)
				{
					cis[i] = CultureInfo.GetCultureInfo(names[i]);
				}
				return cis;
			}
		}

		/// <summary>
		/// Gets the RFC 4646 name of the thread's current culture. It is not guaranteed that this
		/// culture is available in the global dictionary.
		/// </summary>
		public static string CurrentThreadCulture
		{
			get { return CultureInfo.CurrentCulture.Name; }
		}

		/// <summary>
		/// Gets the two-letter ISO 639-1 language code of the thread's current culture. It is not
		/// guaranteed that this language is available in the global dictionary.
		/// </summary>
		public static string CurrentThreadLanguage
		{
			get { return CultureInfo.CurrentCulture.TwoLetterISOLanguageName; }
		}

		/// <summary>
		/// Gets the native full name of the thread's current culture. It is not guaranteed that
		/// this culture is available in the global dictionary.
		/// </summary>
		public static string CurrentThreadCultureNativeName
		{
			get { return CultureInfo.CurrentCulture.NativeName; }
		}

		/// <summary>
		/// Gets or sets the primary culture name that serves as fallback for incomplete
		/// translations. If this is null, it will not be considered when translating texts. It is
		/// not guaranteed that this culture is available in the global dictionary.
		/// </summary>
		public static string PrimaryCulture
		{
			get
			{
				using (new ReadLock(rwlock))
				{
					return primaryCulture;
				}
			}
			set
			{
				using (new UpgradeableReadLock(rwlock))
				{
					if (value != primaryCulture)
					{
						using (new WriteLock(rwlock))
						{
							primaryCulture = value;
						}
						if (!rwlock.IsWriteLockHeld)
						{
							// Raise the changed event only if no write lock is held, because then,
							// nothing could be read from the dictionary by others. In a situation
							// where this property is set with a write lock, the caller must raise
							// the changed event after releasing the lock.
							RaiseDictionaryChanged();
						}
					}
				}
			}
		}

		/// <summary>
		/// Sets the current thread culture.
		/// </summary>
		/// <param name="culture">Culture name as supported by the CultureInfo class. This culture does not have to be available in the global dictionary.</param>
		public static void SetCulture(string culture)
		{
			CultureInfo ci = new CultureInfo(culture);
			if (ci.Name != CultureInfo.CurrentCulture.Name)
			{
				Thread.CurrentThread.CurrentCulture = ci;
				RaiseDictionaryChanged();
			}
		}

		/// <summary>
		/// Gets the best supported current culture name for the thread. This can be
		/// CurrentThreadCulture, CurrentThreadLanguage, one of the browser's supported cultures
		/// (if specified), PrimaryCulture or, if none of them are available, any one of the
		/// available (loaded) cultures. This culture will be tried first to find translated texts.
		/// </summary>
		/// <returns></returns>
		public static string GetCultureName()
		{
			using (new ReadLock(rwlock))
			{
				foreach (string culture in GetCulturesToTry(3))
				{
					if (languages.ContainsKey(culture)) return culture;
				}
				// The dictionary is empty
				return null;
			}
		}

		/// <summary>
		/// Returns all culture names to search, sorted by descending preference.
		/// </summary>
		/// <param name="stages">Stages to try, combination of 1 or 2.</param>
		/// <returns></returns>
		private static IEnumerable<string> GetCulturesToTry(int stages)
		{
			using (new ReadLock(rwlock))
			{
				if ((stages & 1) != 0)
				{
					// First try with the current culture, if set.
					string cc = CultureInfo.CurrentCulture.Name;
					yield return cc;

					// If the culture name has a region set, try without it.
					if (cc.Length == 5)
					{
						yield return cc.Substring(0, 2);
					}
				}
				if ((stages & 2) != 0)
				{
					// Try other cultures, according to the preference set from the HTTP request header (cf. GetCultureName method)
					if (httpPreferredCultures != null)
					{
						for (int i = 0; i < httpPreferredCultures.Length; i++)
						{
							string cc2 = httpPreferredCultures[i];
							yield return cc2;

							// If the culture name has a region set AND the culture without a region is
							// not contained in the list of preferred cultures AND no other culture
							// with this language part is yet to come in the list of preferred
							// cultures, THEN try without the region now.
							if (cc2.Length == 5 &&
								Array.IndexOf(httpPreferredCultures, cc2.Substring(0, 2)) == -1)
							{
								bool found = false;
								for (int j = i + 1; j < httpPreferredCultures.Length; j++)
								{
									if (httpPreferredCultures[j].Substring(0, 2) == cc2.Substring(0, 2))
									{
										found = true;
										break;
									}
								}
								if (!found)
								{
									yield return cc2.Substring(0, 2);
								}
							}
						}
					}

					// Then try with the primary culture, if set.
					if (PrimaryCulture != null)
					{
						yield return PrimaryCulture;

						// If the culture name has a region set, try without it.
						if (PrimaryCulture.Length == 5)
						{
							yield return PrimaryCulture.Substring(0, 2);
						}
					}

					// Finally try every available language
					foreach (string cc2 in languages.Keys)
					{
						yield return cc2;
					}
				}
			}
		}

		#endregion Culture control

		#region Web culture control

		/// <summary>
		/// Sets the best supported culture for a web page from the browser's language preference
		/// and updates the preference list for the current thread. Only available (loaded)
		/// cultures will be regarded, so all languages must be loaded before calling this method
		/// and it must be called again after loading or reloading the translation files.
		/// </summary>
		/// <param name="httpAcceptLanguage">Value of the HTTP_ACCEPT_LANGUAGE request header.</param>
		/// <remarks>
		/// Tx.SetWebCulture(HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"] as string);
		/// </remarks>
		public static void SetWebCulture(string httpAcceptLanguage)
		{
			if (string.IsNullOrEmpty(httpAcceptLanguage))
			{
				httpPreferredCultures = new string[0];
			}

			// Parse every item from the list
			List<CulturePriority> cpList = new List<CulturePriority>();
			float priority = 1;
			int index = 0;
			using (new ReadLock(rwlock))
			{
				foreach (string item in httpAcceptLanguage.Split(','))
				{
					string[] parts = item.Split(';');

					// Parse, validate and normalise culture name
					string culture = parts[0].Trim().Replace('_', '-');
					try
					{
						CultureInfo ci = CultureInfo.GetCultureInfo(culture);
						culture = ci.Name;
					}
					catch (ArgumentException)
					{
						// .NET 4.0 will throw a CultureNotFoundException, which is inherited from ArgumentException
						// Culture not found. Skip it, we couldn't support it anyway.
						continue;
					}
					if (!languages.ContainsKey(culture))
					{
						// This locale is currently not loaded, so it cannot be supported.
						continue;
					}

					// Parse the priority value for this culture name, if set.
					// If no valid q value was found, the previous item's will be kept.
					if (parts.Length > 1)
					{
						Match m = Regex.Match(parts[1].Trim(), @"q\s*=\s*([0-9.]+)");
						if (m.Success)
						{
							priority = float.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
						}
					}

					cpList.Add(new CulturePriority(culture, priority, index++));
				}
			}

			httpPreferredCultures = cpList
				.OrderByDescending(cp => cp.Priority)
				.ThenBy(cp => cp.Index)
				.Select(cp => cp.Culture)
				.ToArray();

			if (httpPreferredCultures.Length > 0)
			{
				// Set the current thread's culture to the most preferred one
				SetCulture(httpPreferredCultures[0]);
			}
			else
			{
				// At least raise this event because the culture preferences have changed
				RaiseDictionaryChanged();
			}
		}

		#region Helper data structures

		/// <summary>
		/// Contains all data fields from an item of the browser's accept-language header list.
		/// </summary>
		private struct CulturePriority
		{
			public string Culture;
			public float Priority;
			public int Index;

			public CulturePriority(string culture, float priority, int index)
			{
				Culture = culture;
				Priority = priority;
				Index = index;
			}
		}

		#endregion Helper data structures

		#endregion Web culture control

		#region Public translation and formatting methods

		#region Text overloads

		/// <summary>
		/// Returns a text from the dictionary.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		public static string Text(string key)
		{
			return GetText(key, -1) ?? NotFound(key);
		}

		/// <summary>
		/// Returns a text from the dictionary.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="count">Count value to consider when selecting the text value.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		public static string Text(string key, int count)
		{
			using (new ReadLock(rwlock))
			{
				return ResolveData(GetText(key, count), key, count, (Dictionary<string, string>) null) ?? NotFound(key);
			}
		}

		/// <summary>
		/// Returns a text from the dictionary.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="count">Count value to consider when selecting the text value.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		public static string Text(string key, decimal count)
		{
			int icount = count == (int) count ? (int) count : -1;
			using (new ReadLock(rwlock))
			{
				return ResolveData(GetText(key, icount), key, icount, (Dictionary<string, string>) null) ?? NotFound(key);
			}
		}

		/// <summary>
		/// Returns a text from the dictionary and replaces placeholders with the specified values.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="data">Consecutive pairs of placeholder names and values.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		public static string Text(string key, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return ResolveData(GetText(key, -1), key, -1, data) ?? NotFound(key);
			}
		}

		/// <summary>
		/// Returns a text from the dictionary and replaces placeholders with the specified values.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="data">Dictionary of placeholder names and values.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		public static string Text(string key, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return ResolveData(GetText(key, -1), key, -1, data) ?? NotFound(key);
			}
		}

		/// <summary>
		/// Returns a text from the dictionary and replaces placeholders with the specified values.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="count">Count value to consider when selecting the text value.</param>
		/// <param name="data">Consecutive pairs of placeholder names and values.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		public static string Text(string key, int count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return ResolveData(GetText(key, count), key, count, data) ?? NotFound(key);
			}
		}

		/// <summary>
		/// Returns a text from the dictionary and replaces placeholders with the specified values.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="count">Count value to consider when selecting the text value.</param>
		/// <param name="data">Dictionary of placeholder names and values.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		public static string Text(string key, int count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return ResolveData(GetText(key, count), key, count, data) ?? NotFound(key);
			}
		}

		/// <summary>
		/// Returns a text from the dictionary and replaces placeholders with the specified values.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="count">Count value to consider when selecting the text value.</param>
		/// <param name="data">Consecutive pairs of placeholder names and values.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		public static string Text(string key, decimal count, params string[] data)
		{
			int icount = count == (int) count ? (int) count : -1;
			using (new ReadLock(rwlock))
			{
				return ResolveData(GetText(key, icount), key, icount, data) ?? NotFound(key);
			}
		}

		/// <summary>
		/// Returns a text from the dictionary and replaces placeholders with the specified values.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="count">Count value to consider when selecting the text value.</param>
		/// <param name="data">Dictionary of placeholder names and values.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		public static string Text(string key, decimal count, Dictionary<string, string> data)
		{
			int icount = count == (int) count ? (int) count : -1;
			using (new ReadLock(rwlock))
			{
				return ResolveData(GetText(key, icount), key, icount, data) ?? NotFound(key);
			}
		}

		#endregion Text overloads

		#region SafeText

		/// <summary>
		/// Returns a text from the dictionary if it exists, or the text key without its namespace
		/// specification otherwise.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <returns></returns>
		public static string SafeText(string key)
		{
			string text = GetText(key, -1, false, false);
			if (text == null)
			{
				// Text not found in the current language, use the key instead
				int nsPos = text.IndexOf(':');
				if (nsPos > -1)
				{
					// Strip off the namespace part
					text = text.Substring(nsPos + 1);
				}
			}
			return text;
		}

		#endregion SafeText

		#region Text conversion and decoration

		/// <summary>
		/// Encloses a text in normal quotation marks.
		/// </summary>
		/// <param name="text">Text to quote.</param>
		/// <returns></returns>
		public static string Quote(string text)
		{
			using (new ReadLock(rwlock))
			{
				string quoteBegin = GetText(SystemKeys.QuoteBegin, -1, false, false);
				string quoteEnd = GetText(SystemKeys.QuoteEnd, -1, false, false);
				if (string.IsNullOrEmpty(quoteBegin) || string.IsNullOrEmpty(quoteEnd))
				{
					// Use ASCII quotes as fallback
					quoteBegin = quoteEnd = "\"";
				}
				return quoteBegin + text + quoteEnd;
			}
		}

		/// <summary>
		/// Encloses a text in nested quotation marks. These marks are used within normal quotation marks.
		/// </summary>
		/// <param name="text">Text to quote.</param>
		/// <returns></returns>
		public static string QuoteNested(string text)
		{
			using (new ReadLock(rwlock))
			{
				string quoteBegin = GetText(SystemKeys.QuoteNestedBegin, -1, false, false);
				string quoteEnd = GetText(SystemKeys.QuoteNestedEnd, -1, false, false);
				if (string.IsNullOrEmpty(quoteBegin) || string.IsNullOrEmpty(quoteEnd))
				{
					// Use ASCII quotes as fallback
					quoteBegin = quoteEnd = "'";
				}
				return quoteBegin + text + quoteEnd;
			}
		}

		/// <summary>
		/// Encloses a text in parentheses (round brackets).
		/// </summary>
		/// <param name="text">Text to enclose.</param>
		/// <returns></returns>
		public static string Parentheses(string text)
		{
			using (new ReadLock(rwlock))
			{
				string parBegin = GetText(SystemKeys.ParenthesisBegin, -1, false, false);
				string parEnd = GetText(SystemKeys.ParenthesisEnd, -1, false, false);
				if (string.IsNullOrEmpty(parBegin) || string.IsNullOrEmpty(parEnd))
				{
					// Use ASCII parentheses as fallback
					parBegin = "(";
					parEnd = ")";
				}
				return parBegin + text + parEnd;
			}
		}

		/// <summary>
		/// Returns a colon string to put after another text.
		/// </summary>
		/// <returns></returns>
		public static string Colon()
		{
			string colon = GetText(SystemKeys.Colon, -1, false, false);
			if (string.IsNullOrEmpty(colon))
			{
				// Use normal colon as fallback
				colon = ":";
			}
			return colon;
		}

		/// <summary>
		/// Transforms the first character of a text to upper case.
		/// </summary>
		/// <param name="text">Text to transform.</param>
		/// <returns></returns>
		public static string UpperCase(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			return char.ToUpper(text[0]) + text.Substring(1);
		}

		#endregion Text conversion and decoration

		#region Number formatting

		/// <summary>
		/// Formats an integer number.
		/// </summary>
		/// <param name="number">Number value to format.</param>
		/// <returns></returns>
		public static string Number(long number)
		{
			using (new ReadLock(rwlock))
			{
				NumberFormatInfo nfi = (NumberFormatInfo) CultureInfo.CurrentCulture.NumberFormat.Clone();
				nfi.NegativeSign = GetText(SystemKeys.NumberNegative, false, nfi.NegativeSign);
				string sepThreshold = GetText(SystemKeys.NumberGroupSeparatorThreshold, -1, false, false);
				if (sepThreshold == null || Math.Abs(number) >= int.Parse(sepThreshold))
				{
					nfi.NumberGroupSeparator = GetText(SystemKeys.NumberGroupSeparator, false, nfi.NumberGroupSeparator);
				}
				else
				{
					nfi.NumberGroupSeparator = "";
				}
				return number.ToString("N0", nfi);
			}
		}

		/// <summary>
		/// Formats a real number.
		/// </summary>
		/// <param name="number">Number value to format with as many decimal digits as are non-zero.</param>
		/// <returns></returns>
		public static string Number(decimal number)
		{
			string numStr = Number(number, 29);   // decimal has at most 29 siginificant digits
			numStr = numStr.TrimEnd('0');
			string decSep = GetText(SystemKeys.NumberDecimalSeparator, false, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
			if (numStr.EndsWith(decSep))
			{
				numStr = numStr.Substring(0, numStr.Length - decSep.Length);
			}
			return numStr;
		}

		/// <summary>
		/// Formats a real number.
		/// </summary>
		/// <param name="number">Number value to format.</param>
		/// <param name="decimals">Fixed number of decimal digits to use.</param>
		/// <returns></returns>
		public static string Number(decimal number, int decimals)
		{
			if (decimals < 0) throw new ArgumentOutOfRangeException("decimals", "Decimals must be greater than or equal to 0.");

			using (new ReadLock(rwlock))
			{
				NumberFormatInfo nfi = (NumberFormatInfo) CultureInfo.CurrentCulture.NumberFormat.Clone();
				nfi.NegativeSign = GetText(SystemKeys.NumberNegative, false, nfi.NegativeSign);
				nfi.NumberDecimalSeparator = GetText(SystemKeys.NumberDecimalSeparator, false, nfi.NumberDecimalSeparator);
				string sepThreshold = GetText(SystemKeys.NumberGroupSeparatorThreshold, -1, false, false);
				if (sepThreshold == null || Math.Abs(number) >= int.Parse(sepThreshold))
				{
					nfi.NumberGroupSeparator = GetText(SystemKeys.NumberGroupSeparator, false, nfi.NumberGroupSeparator);
				}
				else
				{
					nfi.NumberGroupSeparator = "";
				}
				return number.ToString("N" + decimals, nfi);
			}
		}

		/// <summary>
		/// Combines a formatted number and a unit string with a separator.
		/// </summary>
		/// <param name="number">Formatted number string.</param>
		/// <param name="unit">Unit string.</param>
		/// <returns></returns>
		public static string NumberUnit(string number, string unit)
		{
			if (string.IsNullOrEmpty(unit))
			{
				return number;
			}
			// Certain unit characters have no spacing
			// Source: http://de.wikipedia.org/wiki/Schreibweise_von_Zahlen#Ma.C3.9Feinheit
			if (unit[0] == '°' || unit[0] == '′' || unit[0] == '″' || unit[0] == '"')
			{
				return number + unit;
			}
			return number + GetText(SystemKeys.NumberUnitSeparator, false, " ") + unit;
		}

		/// <summary>
		/// Formats a data size in bytes to a shorter and less precise presentation using the
		/// IEC prefixes to base 2.
		/// </summary>
		/// <param name="bytes">Number of bytes to format.</param>
		/// <returns></returns>
		public static string DataSize(long bytes)
		{
			string byteUnit = GetText(SystemKeys.ByteUnit, false, "B");

			long absBytes = Math.Abs(bytes);
			if (absBytes < 0.9 * 1024)
				return NumberUnit(Number(bytes, 0), byteUnit);
			if (absBytes < 50 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024, 1), "Ki" + byteUnit);
			if (absBytes < 0.9 * 1024 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024, 0), "Ki" + byteUnit);
			if (absBytes < 50 * 1024 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024 / 1024, 1), "Mi" + byteUnit);
			if (absBytes < 0.9 * 1024 * 1024 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024 / 1024, 0), "Mi" + byteUnit);
			if (absBytes < 50L * 1024 * 1024 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024 / 1024 / 1024, 1), "Gi" + byteUnit);
			if (absBytes < 0.9 * 1024 * 1024 * 1024 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024 / 1024 / 1024, 0), "Gi" + byteUnit);
			if (absBytes < 50L * 1024 * 1024 * 1024 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024 / 1024 / 1024 / 1024, 1), "Ti" + byteUnit);
			if (absBytes < 0.9 * 1024 * 1024 * 1024 * 1024 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024 / 1024 / 1024 / 1024, 0), "Ti" + byteUnit);
			if (absBytes < 50L * 1024 * 1024 * 1024 * 1024 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024 / 1024 / 1024 / 1024 / 1024, 1), "Pi" + byteUnit);
			if (absBytes < 0.9 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024)
				return NumberUnit(Number((decimal) bytes / 1024 / 1024 / 1024 / 1024 / 1024, 0), "Pi" + byteUnit);
			return NumberUnit(Number((decimal) bytes / 1024 / 1024 / 1024 / 1024 / 1024 / 1024, 1), "Ei" + byteUnit);
			// A long (Int64) value cannot get greater than this
		}

		/// <summary>
		/// Formats an ordinal number.
		/// </summary>
		/// <param name="number">Number to format.</param>
		/// <returns></returns>
		public static string Ordinal(int number)
		{
			return Ordinal(number, false);
		}

		/// <summary>
		/// Formats an ordinal number with the specified grammatical gender (masculine or feminine).
		/// </summary>
		/// <param name="number">Number to format.</param>
		/// <param name="femininGender">true to specify the feminine form, false for the masculine form.</param>
		/// <returns></returns>
		public static string Ordinal(int number, bool femininGender)
		{
			string text = null;
			if (femininGender)
			{
				text = ResolveData(GetText(SystemKeys.NumberOrdinalFeminin, number, false, false), SystemKeys.NumberOrdinalFeminin, number, (Dictionary<string, string>) null);
			}
			if (text == null)
			{
				text = ResolveData(GetText(SystemKeys.NumberOrdinal, number, false, true), SystemKeys.NumberOrdinal, number, (Dictionary<string, string>) null);
			}
			if (text == null)
			{
				text = number.ToString() + ".";
			}
			return text;
		}

		#endregion Number formatting

		#region Date and time formatting

		/// <summary>
		/// Formats a date with the specified level of detail.
		/// </summary>
		/// <param name="time">DateTime value to format.</param>
		/// <param name="details">Details to include in the formatted string.</param>
		/// <returns></returns>
		public static string Time(DateTime time, TxTime details)
		{
			string dowStr = null;
			StringBuilder sb = new StringBuilder();

			// Day of week
			if ((details & TxTime.Dow) != 0)
			{
				dowStr = time.ToString("ddd");
			}
			if ((details & TxTime.DowLong) != 0)
			{
				dowStr = time.ToString("dddd");
			}

			// Date combinations
			Dictionary<string, string> data;
			switch (details & TxTime.AnyDate)
			{
				case TxTime.Year:
					sb.Append(time.ToString(GetText(SystemKeys.DateYear, false, "yyyy")));
					break;
				case TxTime.YearMonth:
					sb.Append(time.ToString(GetText(SystemKeys.DateYearMonth, false, "yyyy-MM")));
					break;
				case TxTime.YearMonthAbbr:
					sb.Append(time.ToString(GetText(SystemKeys.DateYearMonthAbbr, false, "MMM yyyy")));
					break;
				case TxTime.YearMonthLong:
					sb.Append(time.ToString(GetText(SystemKeys.DateYearMonthLong, false, "Y")));
					break;
				case TxTime.YearMonthDay:
					sb.Append(time.ToString(GetText(SystemKeys.DateYearMonthDay, false, "d")));
					break;
				case TxTime.YearMonthDayAbbr:
					sb.Append(time.ToString(GetText(SystemKeys.DateYearMonthDayAbbr, false, "d MMM yyyy")));
					break;
				case TxTime.YearMonthDayLong:
					sb.Append(time.ToString(GetText(SystemKeys.DateYearMonthDayLong, false, "d MMMM yyyy")));
					break;
				case TxTime.Month:
					sb.Append(time.ToString(GetText(SystemKeys.DateMonth, false, "%M")));
					break;
				case TxTime.MonthAbbr:
					sb.Append(time.ToString(GetText(SystemKeys.DateMonthAbbr, false, "MMM")));
					break;
				case TxTime.MonthLong:
					sb.Append(time.ToString(GetText(SystemKeys.DateMonthLong, false, "MMMM")));
					break;
				case TxTime.MonthDay:
					sb.Append(time.ToString(GetText(SystemKeys.DateMonthDay, false, "MM-dd")));
					break;
				case TxTime.MonthDayAbbr:
					sb.Append(time.ToString(GetText(SystemKeys.DateMonthDayAbbr, false, "dd MMM")));
					break;
				case TxTime.MonthDayLong:
					sb.Append(time.ToString(GetText(SystemKeys.DateMonthDayLong, false, "M")));
					break;
				case TxTime.Day:
					sb.Append(time.ToString(GetText(SystemKeys.DateDay, false, "%d")));
					break;
				case TxTime.YearQuarter:
					data = new Dictionary<string, string>();
					data["year"] = time.Year.ToString();
					data["quarter"] = ((time.Month - 1) / 3 + 1).ToString();
					sb.Append(ResolveData(GetText(SystemKeys.DateYearQuarter, false, "Q{quarter}'/'{year}"), "", -1, data));
					break;
				case TxTime.Quarter:
					data = new Dictionary<string, string>();
					data["quarter"] = ((time.Month - 1) / 3 + 1).ToString();
					sb.Append(ResolveData(GetText(SystemKeys.DateQuarter, false, "Q{quarter}"), "", -1, data));
					break;
			}

			if (dowStr != null && sb.Length > 0)
			{
				// Day of week and date are used together, combine them
				data = new Dictionary<string, string>();
				data["dow"] = dowStr;
				data["date"] = sb.ToString();
				string dowAndDate = ResolveData(GetText(SystemKeys.DateDowWithDate, false, "{dow}, {date}"), SystemKeys.DateDowWithDate, -1, data);
				sb.Clear();
				sb.Append(dowAndDate);
			}

			if ((details & TxTime.AnyDate) != 0 && (details | TxTime.AnyTime) != 0)
			{
				sb.Append("\u2007");   // Non-breaking figure space
			}

			if ((details | TxTime.AnyTime) != 0)
			{
				DateTimeFormatInfo fi = (DateTimeFormatInfo) CultureInfo.CurrentCulture.DateTimeFormat.Clone();
				fi.AMDesignator = GetText(SystemKeys.TimeAM, false, fi.AMDesignator);
				fi.PMDesignator = GetText(SystemKeys.TimePM, false, fi.PMDesignator);

				// Time combinations
				switch (details & TxTime.AnyTime)
				{
					case TxTime.HourMinuteSecondMs:
						sb.Append(time.ToString(GetText(SystemKeys.TimeHourMinuteSecondMs, false, "HH:mm:ss.fff"), fi));
						break;
					case TxTime.HourMinuteSecond:
						sb.Append(time.ToString(GetText(SystemKeys.TimeHourMinuteSecond, false, "T"), fi));
						break;
					case TxTime.HourMinute:
						sb.Append(time.ToString(GetText(SystemKeys.TimeHourMinute, false, "t"), fi));
						break;
					case TxTime.Hour:
						sb.Append(time.ToString(GetText(SystemKeys.TimeHour, false, "%H"), fi));
						break;
				}
			}

			if ((details & TxTime.Zone) != 0)
			{
				if (details != TxTime.Zone)
				{
					sb.Append("\xA0");   // Non-breaking space
				}
				sb.Append(time.ToString("zzz"));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Returns a verbose description of a relative point in time.
		/// </summary>
		/// <param name="time">Time to compare with now. This can be in the future or past. If Kind is not Utc, the value is converted to UTC for comparison.</param>
		/// <returns></returns>
		public static string RelativeTime(DateTime time)
		{
			if (time == DateTime.MinValue)
			{
				return GetText(SystemKeys.TimeNever, -1) ?? NotFound(SystemKeys.TimeNever);
			}
			
			// Calculate time span between specified time and now
			if (time.Kind != DateTimeKind.Utc)
			{
				time = time.ToUniversalTime();
			}
			DateTimeInterval interval = GetRoundedInterval(DateTime.UtcNow, time);

			if (Math.Abs(interval.TimeSpan.TotalSeconds) <= 3)
			{
				return GetText(SystemKeys.TimeNow, -1) ?? NotFound(SystemKeys.TimeNow);
			}

			string[] keys = new string[6];
			keys[0] = !interval.Negative ? SystemKeys.TimeRelativeYears : SystemKeys.TimeRelativeNegYears;
			keys[1] = !interval.Negative ? SystemKeys.TimeRelativeMonths : SystemKeys.TimeRelativeNegMonths;
			keys[2] = !interval.Negative ? SystemKeys.TimeRelativeDays : SystemKeys.TimeRelativeNegDays;
			keys[3] = !interval.Negative ? SystemKeys.TimeRelativeHours : SystemKeys.TimeRelativeNegHours;
			keys[4] = !interval.Negative ? SystemKeys.TimeRelativeMinutes : SystemKeys.TimeRelativeNegMinutes;
			keys[5] = !interval.Negative ? SystemKeys.TimeRelativeSeconds : SystemKeys.TimeRelativeNegSeconds;
			string intervalStr = FormatTimeInterval(interval, keys);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data["interval"] = intervalStr;
			string key = !interval.Negative ? SystemKeys.TimeRelative : SystemKeys.TimeRelativeNeg;
			return ResolveData(GetText(key, -1), key, -1, data);
		}

		/// <summary>
		/// Returns a verbose description of a time span from now to the specified point in time.
		/// </summary>
		/// <param name="time">The other end of the time span. This can be in the future or past.</param>
		/// <returns></returns>
		public static string TimeSpan(DateTime time)
		{
			if (time == DateTime.MinValue)   // TODO: Actually this should not be allowed
			{
				return GetText(SystemKeys.TimeNever, -1) ?? NotFound(SystemKeys.TimeNever);
			}

			// Calculate time span between specified time and now
			if (time.Kind != DateTimeKind.Utc)
			{
				time = time.ToUniversalTime();
			}
			DateTimeInterval interval = GetRoundedInterval(DateTime.UtcNow, time);

			string[] keys = new string[6];
			keys[0] = !interval.Negative ? SystemKeys.TimeSpanRelativeYears : SystemKeys.TimeSpanRelativeNegYears;
			keys[1] = !interval.Negative ? SystemKeys.TimeSpanRelativeMonths : SystemKeys.TimeSpanRelativeNegMonths;
			keys[2] = !interval.Negative ? SystemKeys.TimeSpanRelativeDays : SystemKeys.TimeSpanRelativeNegDays;
			keys[3] = !interval.Negative ? SystemKeys.TimeSpanRelativeHours : SystemKeys.TimeSpanRelativeNegHours;
			keys[4] = !interval.Negative ? SystemKeys.TimeSpanRelativeMinutes : SystemKeys.TimeSpanRelativeNegMinutes;
			keys[5] = !interval.Negative ? SystemKeys.TimeSpanRelativeSeconds : SystemKeys.TimeSpanRelativeNegSeconds;
			string intervalStr = FormatTimeInterval(interval, keys);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data["interval"] = intervalStr;
			string key = !interval.Negative ? SystemKeys.TimeSpanRelative : SystemKeys.TimeSpanRelativeNeg;
			return ResolveData(GetText(key, -1), key, -1, data);
		}

		/// <summary>
		/// Returns a verbose description of a time span that is not related to the current time,
		/// including introductory wording.
		/// </summary>
		/// <param name="span">Time span to describe. This can be positive or negative.</param>
		/// <returns></returns>
		public static string TimeSpan(TimeSpan span)
		{
			string intervalStr = TimeSpanRaw(span);
			Dictionary<string, string> data = new Dictionary<string, string>();
			data["interval"] = intervalStr;
			string key = span.Ticks >= 0 ? SystemKeys.TimeSpan : SystemKeys.TimeSpanNeg;
			return ResolveData(GetText(key, -1), key, -1, data);
		}

		/// <summary>
		/// Returns a verbose description of a time span that is not related to the current time,
		/// without introductory wording.
		/// </summary>
		/// <param name="span">Time span to describe. This can be positive or negative.</param>
		/// <returns></returns>
		public static string TimeSpanRaw(TimeSpan span)
		{
			// Calculate time span between specified time and now
			DateTime now = DateTime.UtcNow;
			DateTimeInterval interval = GetRoundedInterval(now, now + span);

			string[] keys = new string[6];
			keys[0] = !interval.Negative ? SystemKeys.TimeSpanYears : SystemKeys.TimeSpanNegYears;
			keys[1] = !interval.Negative ? SystemKeys.TimeSpanMonths : SystemKeys.TimeSpanNegMonths;
			keys[2] = !interval.Negative ? SystemKeys.TimeSpanDays : SystemKeys.TimeSpanNegDays;
			keys[3] = !interval.Negative ? SystemKeys.TimeSpanHours : SystemKeys.TimeSpanNegHours;
			keys[4] = !interval.Negative ? SystemKeys.TimeSpanMinutes : SystemKeys.TimeSpanNegMinutes;
			keys[5] = !interval.Negative ? SystemKeys.TimeSpanSeconds : SystemKeys.TimeSpanNegSeconds;
			string intervalStr = FormatTimeInterval(interval, keys);
			return intervalStr;
		}

		/// <summary>
		/// Calculates a DateTimeInterval from two DateTime values and rounds it to a resolution of
		/// not more than 2 levels.
		/// </summary>
		/// <param name="time">Start time of the interval.</param>
		/// <param name="end">End time of the interval.</param>
		/// <returns></returns>
		private static DateTimeInterval GetRoundedInterval(DateTime start, DateTime end)
		{
			// Round difference to seconds, update end time
			const long ticksPerSecond = 10000000;
			long newEnd = (long) Math.Round((double) (end.Ticks - start.Ticks) / ticksPerSecond) * ticksPerSecond;
			end = new DateTime(start.Ticks + newEnd);

			DateTimeInterval interval = new DateTimeInterval(start, end);

			// Snap to grid
			// Only snap a unit downwards if there is a greater unit.
			if ((interval.Years != 0 || interval.Months != 0 || interval.Days != 0 || interval.Hours != 0 || interval.Minutes != 0) && interval.Seconds <= 3)
			{
				interval.Seconds = 0;
			}
			else if (interval.Seconds >= 57)
			{
				interval.Minutes++;
				interval.Seconds = 0;
			}
			if ((interval.Years != 0 || interval.Months != 0 || interval.Days != 0 || interval.Hours != 0) && interval.Minutes <= 3)
			{
				interval.Minutes = 0;
			}
			else if (interval.Minutes >= 57)
			{
				interval.Hours++;
				interval.Minutes = 0;
			}
			if ((interval.Years != 0 || interval.Months != 0 || interval.Days != 0) && interval.Hours <= 1)
			{
				interval.Hours = 0;
			}
			else if (interval.Hours >= 23)
			{
				interval.Days++;
				interval.Hours = 0;
			}
			if ((interval.Years != 0 || interval.Months != 0) && interval.Days <= 2)
			{
				interval.Days = 0;
			}
			else if (interval.Days >= 28)
			{
				interval.Months++;
				interval.Days = 0;
			}
			// (Months are not snapped)

			// Decrease resolution over time, also ensure that only one smaller unit is set
			if (interval.Years >= 5)
			{
				if (interval.Months >= 7)
					interval.Years++;
				interval.Months = 0;
			}
			if (interval.Years >= 1)
			{
				interval.Days = 0;
				interval.Hours = 0;
				interval.Minutes = 0;
				interval.Seconds = 0;
			}
			if (interval.Months >= 5)
			{
				if (interval.Days >= 16)
					interval.Months++;
				interval.Days = 0;
			}
			if (interval.Months >= 1)
			{
				interval.Hours = 0;
				interval.Minutes = 0;
				interval.Seconds = 0;
			}
			if (interval.Days >= 5)
			{
				if (interval.Hours >= 12)
					interval.Days++;
				interval.Hours = 0;
			}
			if (interval.Days >= 1)
			{
				interval.Minutes = 0;
				interval.Seconds = 0;
			}
			if (interval.Hours >= 5)
			{
				if (interval.Minutes >= 30)
					interval.Hours++;
				interval.Minutes = 0;
			}
			if (interval.Hours >= 1)
			{
				interval.Seconds = 0;
			}
			if (interval.Minutes >= 5)
			{
				if (interval.Seconds >= 30)
					interval.Minutes++;
				interval.Seconds = 0;
			}

			return interval;
		}

		/// <summary>
		/// Formats a DateTimeInterval value with the specified set of text keys for a specific
		/// grammatical situation.
		/// </summary>
		/// <param name="interval">Interval data.</param>
		/// <param name="keys">Text keys for years, months, days, hours, minutes and seconds.</param>
		/// <returns></returns>
		private static string FormatTimeInterval(DateTimeInterval interval, string[] keys)
		{
			List<string> levels = new List<string>();
			if (interval.Years > 0 && levels.Count < 2)
				levels.Add(ResolveData(GetText(keys[0], interval.Years), keys[0], interval.Years, (Dictionary<string, string>) null));
			if (interval.Months > 0 && levels.Count < 2)
				levels.Add(ResolveData(GetText(keys[1], interval.Months), keys[1], interval.Months, (Dictionary<string, string>) null));
			if (interval.Days > 0 && levels.Count < 2)
				levels.Add(ResolveData(GetText(keys[2], interval.Days), keys[2], interval.Days, (Dictionary<string, string>) null));
			if (interval.Hours > 0 && levels.Count < 2)
				levels.Add(ResolveData(GetText(keys[3], interval.Hours), keys[3], interval.Hours, (Dictionary<string, string>) null));
			if (interval.Minutes > 0 && levels.Count < 2)
				levels.Add(ResolveData(GetText(keys[4], interval.Minutes), keys[4], interval.Minutes, (Dictionary<string, string>) null));
			if (interval.Seconds > 0 && levels.Count < 2)
				levels.Add(ResolveData(GetText(keys[5], interval.Seconds), keys[5], interval.Seconds, (Dictionary<string, string>) null));

			return string.Join(GetText(SystemKeys.TimeRelativeSeparator, " "), levels.ToArray());
		}

		#endregion Date and time formatting

		#endregion Public translation and formatting methods

		#region Abbreviated public methods

		#region T overloads

		/// <summary>
		/// Abbreviation for the Text method.
		/// </summary>
		public static string T(string key)
		{
			return Text(key);
		}

		/// <summary>
		/// Abbreviation for the Text method.
		/// </summary>
		public static string T(string key, int count)
		{
			return Text(key, count);
		}

		/// <summary>
		/// Abbreviation for the Text method.
		/// </summary>
		public static string T(string key, decimal count)
		{
			return Text(key, count);
		}

		/// <summary>
		/// Abbreviation for the Text method.
		/// </summary>
		public static string T(string key, params string[] data)
		{
			return Text(key, data);
		}

		/// <summary>
		/// Abbreviation for the Text method.
		/// </summary>
		public static string T(string key, Dictionary<string, string> data)
		{
			return Text(key, data);
		}

		/// <summary>
		/// Abbreviation for the Text method.
		/// </summary>
		public static string T(string key, int count, params string[] data)
		{
			return Text(key, count, data);
		}

		/// <summary>
		/// Abbreviation for the Text method.
		/// </summary>
		public static string T(string key, int count, Dictionary<string, string> data)
		{
			return Text(key, count, data);
		}

		/// <summary>
		/// Abbreviation for the Text method.
		/// </summary>
		public static string T(string key, decimal count, params string[] data)
		{
			return Text(key, count, data);
		}

		/// <summary>
		/// Abbreviation for the Text method.
		/// </summary>
		public static string T(string key, decimal count, Dictionary<string, string> data)
		{
			return Text(key, count, data);
		}

		#endregion T overloads

		/// <summary>
		/// Abbreviation for the Quote method.
		/// </summary>
		public static string Q(string text)
		{
			return Quote(text);
		}

		/// <summary>
		/// Abbreviation for the Parentheses method.
		/// </summary>
		public static string P(string text)
		{
			return Parentheses(text);
		}

		/// <summary>
		/// Abbreviation for the Colon method.
		/// </summary>
		public static string C()
		{
			return Colon();
		}

		/// <summary>
		/// Abbreviation for the UpperCase method.
		/// </summary>
		public static string U(string text)
		{
			return UpperCase(text);
		}

		#region N overloads

		/// <summary>
		/// Abbreviation for the Number method.
		/// </summary>
		public static string N(long number)
		{
			return Number(number);
		}

		/// <summary>
		/// Abbreviation for the Number method.
		/// </summary>
		public static string N(decimal number)
		{
			return Number(number);
		}

		/// <summary>
		/// Abbreviation for the Number method.
		/// </summary>
		public static string N(decimal number, int decimals)
		{
			return Number(number, decimals);
		}

		#endregion N overloads

		#endregion Abbreviated public methods

		#region Combined abbreviated public methods

		#region UT overloads

		/// <summary>
		/// Combined abbreviation for the UpperCase and Text methods.
		/// </summary>
		public static string UT(string key)
		{
			return U(T(key));
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase and Text methods.
		/// </summary>
		public static string UT(string key, int count)
		{
			return U(T(key, count));
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase and Text methods.
		/// </summary>
		public static string UT(string key, decimal count)
		{
			return U(T(key, count));
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase and Text methods.
		/// </summary>
		public static string UT(string key, params string[] data)
		{
			return U(T(key, data));
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase and Text methods.
		/// </summary>
		public static string UT(string key, Dictionary<string, string> data)
		{
			return U(T(key, data));
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase and Text methods.
		/// </summary>
		public static string UT(string key, int count, params string[] data)
		{
			return U(T(key, count, data));
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase and Text methods.
		/// </summary>
		public static string UT(string key, int count, Dictionary<string, string> data)
		{
			return U(T(key, count, data));
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase and Text methods.
		/// </summary>
		public static string UT(string key, decimal count, params string[] data)
		{
			return U(T(key, count, data));
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase and Text methods.
		/// </summary>
		public static string UT(string key, decimal count, Dictionary<string, string> data)
		{
			return U(T(key, count, data));
		}

		#endregion UT overloads

		#region TC overloads

		/// <summary>
		/// Combined abbreviation for the Text and Colon methods.
		/// </summary>
		public static string TC(string key)
		{
			using (new ReadLock(rwlock))
			{
				return T(key) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Text and Colon methods.
		/// </summary>
		public static string TC(string key, int count)
		{
			using (new ReadLock(rwlock))
			{
				return T(key, count) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Text and Colon methods.
		/// </summary>
		public static string TC(string key, decimal count)
		{
			using (new ReadLock(rwlock))
			{
				return T(key, count) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Text and Colon methods.
		/// </summary>
		public static string TC(string key, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return T(key, data) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Text and Colon methods.
		/// </summary>
		public static string TC(string key, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return T(key, data) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Text and Colon methods.
		/// </summary>
		public static string TC(string key, int count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return T(key, count, data) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Text and Colon methods.
		/// </summary>
		public static string TC(string key, int count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return T(key, count, data) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Text and Colon methods.
		/// </summary>
		public static string TC(string key, decimal count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return T(key, count, data) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Text and Colon methods.
		/// </summary>
		public static string TC(string key, decimal count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return T(key, count, data) + C();
			}
		}

		#endregion TC overloads

		#region UTC overloads

		/// <summary>
		/// Combined abbreviation for the UpperCase, Text and Colon methods.
		/// </summary>
		public static string UTC(string key)
		{
			using (new ReadLock(rwlock))
			{
				return U(T(key)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase, Text and Colon methods.
		/// </summary>
		public static string UTC(string key, int count)
		{
			using (new ReadLock(rwlock))
			{
				return U(T(key, count)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase, Text and Colon methods.
		/// </summary>
		public static string UTC(string key, decimal count)
		{
			using (new ReadLock(rwlock))
			{
				return U(T(key, count)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase, Text and Colon methods.
		/// </summary>
		public static string UTC(string key, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return U(T(key, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase, Text and Colon methods.
		/// </summary>
		public static string UTC(string key, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return U(T(key, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase, Text and Colon methods.
		/// </summary>
		public static string UTC(string key, int count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return U(T(key, count, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase, Text and Colon methods.
		/// </summary>
		public static string UTC(string key, int count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return U(T(key, count, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase, Text and Colon methods.
		/// </summary>
		public static string UTC(string key, decimal count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return U(T(key, count, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the UpperCase, Text and Colon methods.
		/// </summary>
		public static string UTC(string key, decimal count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return U(T(key, count, data)) + C();
			}
		}

		#endregion UTC overloads

		#region QT overloads

		/// <summary>
		/// Combined abbreviation for the Quote and Text methods.
		/// </summary>
		public static string QT(string key)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote and Text methods.
		/// </summary>
		public static string QT(string key, int count)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote and Text methods.
		/// </summary>
		public static string QT(string key, decimal count)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote and Text methods.
		/// </summary>
		public static string QT(string key, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, data));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote and Text methods.
		/// </summary>
		public static string QT(string key, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, data));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote and Text methods.
		/// </summary>
		public static string QT(string key, int count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count, data));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote and Text methods.
		/// </summary>
		public static string QT(string key, int count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count, data));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote and Text methods.
		/// </summary>
		public static string QT(string key, decimal count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count, data));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote and Text methods.
		/// </summary>
		public static string QT(string key, decimal count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count, data));
			}
		}

		#endregion QT overloads

		#region QTC overloads

		/// <summary>
		/// Combined abbreviation for the Quote, Text and Colon methods.
		/// </summary>
		public static string QTC(string key)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, Text and Colon methods.
		/// </summary>
		public static string QTC(string key, int count)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, Text and Colon methods.
		/// </summary>
		public static string QTC(string key, decimal count)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, Text and Colon methods.
		/// </summary>
		public static string QTC(string key, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, Text and Colon methods.
		/// </summary>
		public static string QTC(string key, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, Text and Colon methods.
		/// </summary>
		public static string QTC(string key, int count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, Text and Colon methods.
		/// </summary>
		public static string QTC(string key, int count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, Text and Colon methods.
		/// </summary>
		public static string QTC(string key, decimal count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count, data)) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, Text and Colon methods.
		/// </summary>
		public static string QTC(string key, decimal count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(T(key, count, data)) + C();
			}
		}

		#endregion QTC overloads

		#region QUT overloads

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase and Text methods.
		/// </summary>
		public static string QUT(string key)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key)));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase and Text methods.
		/// </summary>
		public static string QUT(string key, int count)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count)));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase and Text methods.
		/// </summary>
		public static string QUT(string key, decimal count)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count)));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase and Text methods.
		/// </summary>
		public static string QUT(string key, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, data)));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase and Text methods.
		/// </summary>
		public static string QUT(string key, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, data)));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase and Text methods.
		/// </summary>
		public static string QUT(string key, int count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count, data)));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase and Text methods.
		/// </summary>
		public static string QUT(string key, int count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count, data)));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase and Text methods.
		/// </summary>
		public static string QUT(string key, decimal count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count, data)));
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase and Text methods.
		/// </summary>
		public static string QUT(string key, decimal count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count, data)));
			}
		}

		#endregion QUT overloads

		#region QUTC overloads

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase, Text and Colon methods.
		/// </summary>
		public static string QUTC(string key)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key))) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase, Text and Colon methods.
		/// </summary>
		public static string QUTC(string key, int count)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count))) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase, Text and Colon methods.
		/// </summary>
		public static string QUTC(string key, decimal count)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count))) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase, Text and Colon methods.
		/// </summary>
		public static string QUTC(string key, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, data))) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase, Text and Colon methods.
		/// </summary>
		public static string QUTC(string key, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, data))) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase, Text and Colon methods.
		/// </summary>
		public static string QUTC(string key, int count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count, data))) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase, Text and Colon methods.
		/// </summary>
		public static string QUTC(string key, int count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count, data))) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase, Text and Colon methods.
		/// </summary>
		public static string QUTC(string key, decimal count, params string[] data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count, data))) + C();
			}
		}

		/// <summary>
		/// Combined abbreviation for the Quote, UpperCase, Text and Colon methods.
		/// </summary>
		public static string QUTC(string key, decimal count, Dictionary<string, string> data)
		{
			using (new ReadLock(rwlock))
			{
				return Q(U(T(key, count, data))) + C();
			}
		}

		#endregion QUTC overloads

		#endregion Combined abbreviated public methods

		#region Private placeholder resolution methods

		/// <summary>
		/// Resolves placeholders in the text with the specified data values.
		/// </summary>
		/// <param name="text">Text value to resolve placeholders in. Placeholder names are enclosed in curly braces.</param>
		/// <param name="key">Text key that has originally provided the text value.</param>
		/// <param name="count">Count value, if available, to replace the count placeholder.</param>
		/// <param name="data">Consecutive pairs of placeholder names and values.</param>
		/// <returns>Resolved text value.</returns>
		private static string ResolveData(string text, string key, int count, params string[] data)
		{
			Dictionary<string, string> dataDict = null;
			if (data != null)
			{
				dataDict = new Dictionary<string, string>();
				int i = 0;
				while (i + 1 < data.Length)
				{
					string name = data[i];
					string value = data[i + 1];
					if (dataDict.ContainsKey(name))
					{
						LogStack("Resolve data: Duplicate placeholder name {0}.", name);
					}
					dataDict[name] = value;
					i += 2;
				}
			}
			return ResolveData(text, key, count, dataDict);
		}

		/// <summary>
		/// Resolves placeholders in the text with the specified data values.
		/// </summary>
		/// <param name="text">Text value to resolve placeholders in. Placeholder names are enclosed in curly braces.</param>
		/// <param name="key">Text key that has originally provided the text value.</param>
		/// <param name="count">Count value, if available, to replace the count placeholder.</param>
		/// <param name="data">Dictionary of placeholder names and values.</param>
		/// <returns>Resolved text value.</returns>
		private static string ResolveData(string text, string key, int count, Dictionary<string, string> data)
		{
			return ResolveData(text, key, count, data, null);
		}

		/// <summary>
		/// Resolves placeholders in the text with the specified data values.
		/// </summary>
		/// <param name="text">Text value to resolve placeholders in. Placeholder names are enclosed in curly braces.</param>
		/// <param name="key">Text key that has originally provided the text value.</param>
		/// <param name="count">Count value, if available, to replace the count placeholder.</param>
		/// <param name="data">Dictionary of placeholder names and values.</param>
		/// <param name="usedPlaceholderNames">Set of used placeholder names from a parent ResolveData level.</param>
		/// <returns>Resolved text value.</returns>
		private static string ResolveData(string text, string key, int count, Dictionary<string, string> data, HashSet<string> usedPlaceholderNames)
		{
			bool newUnusedSet = false;
			if (usedKeys != null && usedPlaceholderNames == null)
			{
				usedPlaceholderNames = new HashSet<string>();
				newUnusedSet = true;
			}
			
			if (string.IsNullOrEmpty(text))
			{
				if (newUnusedSet && data != null)
					LogUnusedPlaceholderNames(key, data, usedPlaceholderNames);
				return text;
			}

			int openBracePos = text.IndexOf('{');
			if (openBracePos == -1)
			{
				// No placeholder found, nothing to resolve
				if (newUnusedSet)
					LogUnusedPlaceholderNames(key, data, usedPlaceholderNames);
				return text;
			}
			StringBuilder result = new StringBuilder();
			// Copy first plain text chunk if any
			if (openBracePos > 0)
			{
				result.Append(text.Substring(0, openBracePos));
			}
			using (new ReadLock(rwlock))
			{
				while (true)
				{
					// Skip if escape sequence
					if (text.Length > openBracePos + 1 && text[openBracePos + 1] == '{')
					{
						openBracePos++;
						result.Append('{');
					}
					else
					{
						int closeBracePos = text.IndexOf('}', openBracePos + 1);
						if (closeBracePos == -1)
						{
							// No matching brace found, leave the rest as-is
							result.Append(text.Substring(openBracePos));
							break;
						}
						int braceLength = closeBracePos - openBracePos - 1;
						if (braceLength == 0)
						{
							// Empty braces, leave as-is
							result.Append("{}");
						}
						else if (braceLength == 1 && text[openBracePos + 1] == '#')
						{
							// Found a {#} placeholder, insert the count value
							if (count == -1)
							{
								LogStack("Resolve data: No count value or -1 specified for {{#}} placeholder in key \"{0}\".", key);
							}
							result.Append(count.ToString());
						}
						else if (braceLength > 1 && text[openBracePos + 1] == '=')
						{
							// Found another text key inclusion {=...}, resolve that value
							string subkey;
							int subcount = -1;
							// Check for a count part
							int countPos = text.IndexOf('#', openBracePos + 2, braceLength - 1);
							if (countPos == -1)
							{
								// No count value found, interpret everything as text key
								subkey = text.Substring(openBracePos + 2, braceLength - 1);
							}
							else
							{
								// Count value {=...#...} found
								subkey = text.Substring(openBracePos + 2, countPos - (openBracePos + 2));
								string countName = text.Substring(countPos + 1, closeBracePos - (countPos + 1));
								if (countName != "")
								{
									if (usedPlaceholderNames != null)
										usedPlaceholderNames.Add(countName);
									string countValue;
									if (data != null && data.TryGetValue(countName, out countValue))
									{
										if (!int.TryParse(countValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out subcount))
										{
											// Reset value
											subcount = -1;
											LogStack("Resolve data: Subcount placeholder name \"{0}\" resolves to value \"{1}\" which is not an integer. Ignoring count.", countName, countValue);
										}
									}
									else
									{
										LogStack("Resolve data: Subcount placeholder name \"{0}\" is unset. Ignoring count.", countName);
									}
								}
								else
								{
									// Pass through the current count value
									subcount = count;
								}
							}
							// Resolve the subtext
							// (It doesn't matter which key is passed here as second argument
							// because it's not used in that call level anyway.)
							string subtext = ResolveData(GetText(subkey, subcount), key, subcount, data, usedPlaceholderNames);
							if (subtext != null)
							{
								result.Append(subtext);
							}
							else
							{
								LogStack("Resolve data: Subkey text \"{0}\" is unset.", subkey);
								result.Append(text.Substring(openBracePos, braceLength + 2));
							}
						}
						else
						{
							// Anything else is a placeholder name, insert its value
							string varName = text.Substring(openBracePos + 1, braceLength);
							if (usedPlaceholderNames != null)
								usedPlaceholderNames.Add(varName);
							string varValue;
							if (data != null && data.TryGetValue(varName, out varValue))
							{
								result.Append(varValue);
							}
							else
							{
								LogStack("Resolve data: Placeholder name \"{0}\" is unset.", varName);
								result.Append(text.Substring(openBracePos, braceLength + 2));
							}
						}
						openBracePos = closeBracePos;
					}
					int nextBracePos = text.IndexOf('{', openBracePos + 1);
					if (nextBracePos == -1)
					{
						// Nothing more found, append the remaining text
						result.Append(text.Substring(openBracePos + 1));
						break;
					}
					else if (nextBracePos > openBracePos + 1)
					{
						// Copy text between this and the next brace
						result.Append(text.Substring(openBracePos + 1, nextBracePos - (openBracePos + 1)));
					}
					openBracePos = nextBracePos;
				}
			}
			if (newUnusedSet)
				LogUnusedPlaceholderNames(key, data, usedPlaceholderNames);
			return result.ToString();
		}

		#endregion Private placeholder resolution methods

		#region Private text retrieval methods

		/// <summary>
		/// Searches a text key in the preferred language dictionaries.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="defaultText">Text to return if the key was not found.</param>
		/// <returns>Text value if found, defaultText otherwise.</returns>
		private static string GetText(string key, string defaultText)
		{
			return GetText(key, -1, true, false) ?? defaultText;
		}

		/// <summary>
		/// Searches a text key in the preferred language dictionaries.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="useFallback">Sepcifies whether other languages than the current language shall be searched as fallback.</param>
		/// <param name="defaultText">Text to return if the key was not found.</param>
		/// <returns>Text value if found, defaultText otherwise.</returns>
		private static string GetText(string key, bool useFallback, string defaultText)
		{
			return GetText(key, -1, useFallback, false) ?? defaultText;
		}

		/// <summary>
		/// Searches a text key in the preferred language dictionaries.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="count">Count value to consider when selecting the text value.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		private static string GetText(string key, int count)
		{
			return GetText(key, count, true, true);
		}

		/// <summary>
		/// Searches a text key in the preferred language dictionaries.
		/// </summary>
		/// <param name="key">Text key to search.</param>
		/// <param name="count">Count value to consider when selecting the text value.</param>
		/// <param name="useFallback">Sepcifies whether other languages than the current language shall be searched as fallback.</param>
		/// <param name="logMissing">true to log missing keys, false to ignore the error.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		private static string GetText(string key, int count, bool useFallback, bool logMissing)
		{
			// Log used key if the option is enabled
			lock (logLock)
			{
				if (usedKeys != null)
				{
					usedKeys.Add(key);
				}
			}

			using (new ReadLock(rwlock))
			{
				string firstCulture = null;
				foreach (string culture in GetCulturesToTry(1))
				{
					string text = GetCultureText(culture, key, count);
					if (text != null) return text;
					if (firstCulture == null) firstCulture = culture;
				}
				if (!useFallback)
				{
					if (logMissing)
					{
						Log("Get text: Text key \"{0}\" is unset for culture {1}. Fallback cultures will NOT be used.",
							key,
							firstCulture.Length == 5 ? firstCulture + " and " + firstCulture.Substring(0, 2) : firstCulture);
					}
					return null;
				}

				foreach (string culture in GetCulturesToTry(2))
				{
					string text = GetCultureText(culture, key, count);
					if (text != null) return text;
				}
				// Nothing found, return null
				Log("Get text: Text key \"{0}\" is unset for ALL cultures.", key);
				return null;
			}
		}

		/// <summary>
		/// Searches a text key in the specified language dictionary.
		/// </summary>
		/// <param name="culture">Culture name of the language dictionary to search.</param>
		/// <param name="key">Text key to search.</param>
		/// <param name="count">Count value to consider when selecting the text value.</param>
		/// <returns>Text value if found, null otherwise.</returns>
		private static string GetCultureText(string culture, string key, int count)
		{
			// This method is only called from GetText which already holds a read lock,
			// so no additional locking is needed here.

			// Find the specified culture dictionary
			Dictionary<string, Dictionary<int, string>> language;
			if (languages.TryGetValue(culture, out language))
			{
				// Find the text key within that dictionary
				Dictionary<int, string> textItem;
				if (language.TryGetValue(key, out textItem))
				{
					string text;
					if (count != -1)
					{
						// A count value is specified, search for a matching text value.
						// First try a direct match
						if (textItem.TryGetValue(count, out text))
						{
							return text;
						}
						// Then test all available quantifiers (except -1).
						// First collect all defined quantifiers
						List<int> quantifiers = new List<int>();
						foreach (KeyValuePair<int, string> kvp in textItem)
						{
							if (kvp.Key != -1)
							{
								quantifiers.Add(kvp.Key);
							}
						}
						// Then sort them by value
						quantifiers.Sort();
						// And evaluate them, the greatest value first, then decreasing.
						// (Greatest modulo values are most specific and need to be considered first.
						// Since the modulo value is encoded in the higher bits, this can be compared
						// normally.)
						for (int i = quantifiers.Count - 1; i >= 0; i--)
						{
							int q = quantifiers[i];
							int c = q & 0xFFFF;
							int mod = (q >> 16) & 0xFFFF;
							if (mod >= 2 && c == count % mod)
							{
								return textItem[q];
							}
						}
					}
					else if (usedKeys != null)
					{
						// No count value is specified and unused logging is active.
						// Check whether the text key has quantifiers defined.
						if (textItem.Count > 1 || !textItem.ContainsKey(-1))
						{
							LogStack("Get text: Text key \"{0}\" with quantifier in culture {1} was requested without a count value.", key, culture);
						}
					}
					// Try the generic form if nothing was found by now
					if (textItem.TryGetValue(-1, out text))
					{
						return text;
					}
				}
			}

			// Nothing found, return null
			return null;
		}

		/// <summary>
		/// Returns a replacement text for text keys that were not found in the dictionary.
		/// </summary>
		/// <param name="key">Text key was was not found. This is included in the return value for further analysis.</param>
		/// <returns></returns>
		private static string NotFound(string key)
		{
			return "[" + key + "]";
		}

		#endregion Private text retrieval methods

		#region Logging

		/// <summary>
		/// Writes a message to the current logging target, if any, and includes a call stack if
		/// the target is a file.
		/// </summary>
		/// <param name="message">Message to write. May contain placeholders like {0}, {1} like for String.Format.</param>
		/// <param name="args">Placeholder arguments for String.Format.</param>
		private static void LogStack(string message, params object[] args)
		{
			lock (logLock)
			{
				if (logFileName != null)
				{
					Log(message, args);
					if (logWriter != null)
					{
						string stackTrace = Environment.StackTrace;
						// Shorten the stack trace to the effective entry point of WPF applications
						stackTrace = Regex.Replace(stackTrace, @"\r?\n[^\r\n]+System\.RuntimeTypeHandle\.CreateInstance\(.*?$", "", RegexOptions.Singleline);
						logWriter.WriteLine(stackTrace);
						logWriter.Flush();
					}
				}
			}
		}

		/// <summary>
		/// Writes a message to the current logging target, if any.
		/// </summary>
		/// <param name="message">Message to write. May contain placeholders like {0}, {1} like for String.Format.</param>
		/// <param name="args">Placeholder arguments for String.Format.</param>
		private static void Log(string message, params object[] args)
		{
			lock (logLock)
			{
				if (logFileName != null)
				{
					message = string.Format(message, args);
					if (logWriter != null)
					{
						logWriter.WriteLine(DateTime.Now.ToString(@"yyyy-MM-dd HH\:mm\:ss", CultureInfo.InvariantCulture) + " " + message);
						logWriter.Flush();
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("Tx: " + message);
					}
				}
			}
		}

		/// <summary>
		/// Logs placeholder names provided by the application but not used by a text key. This is
		/// only called if the logging of unused text keys option is enabled.
		/// </summary>
		/// <param name="key">Text key that has originally been resolved.</param>
		/// <param name="data">Data dictionary with placeholder names and values as provided by the application.</param>
		/// <param name="usedPlaceholderNames">Set of placeholder names that were used while resolving the text.</param>
		private static void LogUnusedPlaceholderNames(string key, Dictionary<string, string> data, HashSet<string> usedPlaceholderNames)
		{
			if (data != null && data.Count > 0)
			{
				HashSet<string> dataNames = new HashSet<string>(data.Keys);
				dataNames.ExceptWith(usedPlaceholderNames);
				if (dataNames.Count > 0)
				{
					LogStack(
						"Placeholder names provided but unused by text key \"{0}\": {1}",
						key,
						string.Join(", ", dataNames));
				}
			}
		}

		/// <summary>
		/// Gets or sets the name of the file to log to. If this is an empty string, log messages
		/// are written to Trace with the prefix "Tx: ". If this is null, logging is disabled.
		/// Existing log files will be appended to. The directory must already exist.
		/// </summary>
		public static string LogFileName
		{
			get
			{
				lock (logLock)
				{
					return logFileName;
				}
			}
			set
			{
				lock (logLock)
				{
					if (value != logFileName)
					{
						if (logWriter != null)
						{
							// Log writer was open, close it and remove the event handler
							logWriter.Close();
							logWriter = null;
							AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
						}
						logFileName = value;
						if (logFileName != null)
						{
							if (logFileName.Length > 0)
							{
								// Open the log file now and remember to close it when the process exits
								logWriter = new StreamWriter(logFileName, true, Encoding.UTF8);
								AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
							}
							// Otherwise log to Trace
						}
					}
				}
			}
		}

		/// <summary>
		/// Called when the current process exits.
		/// </summary>
		/// <remarks>
		/// The processing time in this event is limited. All handlers of this event together must
		/// not take more than ca. 3 seconds. The processing will then be terminated.
		/// </remarks>
		private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			lock (logLock)
			{
				// Close the log file if it is open
				if (logWriter != null)
				{
					if (usedKeys != null && !string.IsNullOrEmpty(primaryCulture))
					{
						// Write the unused keys of the primary culture to the log file
						try
						{
							HashSet<string> primaryKeys = new HashSet<string>(languages[primaryCulture].Keys);
							primaryKeys.ExceptWith(usedKeys);
							// Don't report system keys, most of them remain unused most of the time
							primaryKeys.RemoveWhere(s => s.StartsWith("Tx:"));
							if (primaryKeys.Count == 0)
							{
								Log("No text keys from the culture {0} were not used.", primaryCulture);
							}
							else
							{
								Log("The following {0} text keys from the culture {1} were not used:", primaryKeys.Count, primaryCulture);
								foreach (string key in primaryKeys)
								{
									logWriter.Write("    ");
									logWriter.WriteLine(key);
								}
							}
						}
						catch (KeyNotFoundException)
						{
							// No culture, no unused keys. Nothing to do.
						}
					}
					
					logWriter.Close();
					logWriter = null;
				}
			}
		}

		#endregion Logging

		#region Synchronisation helper classes

		/// <summary>
		/// Provides a simple disposable write lock adapter for a ReaderWriterLockSlim object.
		/// </summary>
		private struct WriteLock : IDisposable
		{
			private ReaderWriterLockSlim rwlock;
			private bool wasLocked;

			/// <summary>
			/// Acquires a write lock on a lock object. If the lock object already holds a write
			/// lock, no new lock is acquired and no lock will be released.
			/// </summary>
			/// <param name="rwlock">Lock object to work with. If this is null, no lock will be acquired.</param>
			public WriteLock(ReaderWriterLockSlim rwlock)
			{
				this.rwlock = rwlock;
				if (rwlock == null)
					wasLocked = true;
				else
					wasLocked = rwlock.IsWriteLockHeld;
				if (!wasLocked && !rwlock.TryEnterWriteLock(WriteLockTimeout))
				{
					throw new InvalidOperationException("The writer lock could not be acquired.");
				}
			}

			/// <summary>
			/// Releases the lock if one was acquired. It is safe to call this method multiple times.
			/// </summary>
			public void Dispose()
			{
				if (!wasLocked)
				{
					rwlock.ExitWriteLock();
					wasLocked = true;
				}
			}
		}

		/// <summary>
		/// Provides a simple disposable upgradeable read lock adapter for a ReaderWriterLockSlim object.
		/// </summary>
		private struct UpgradeableReadLock : IDisposable
		{
			private ReaderWriterLockSlim rwlock;
			private bool wasLocked;

			/// <summary>
			/// Acquires an upgradeable read lock on a lock object. If the lock object already
			/// holds a read lock, no new lock is acquired and no lock will be released.
			/// </summary>
			/// <param name="rwlock">Lock object to work with. If this is null, no lock will be acquired.</param>
			public UpgradeableReadLock(ReaderWriterLockSlim rwlock)
			{
				this.rwlock = rwlock;
				if (rwlock == null)
					wasLocked = true;
				else
					wasLocked = rwlock.IsUpgradeableReadLockHeld || rwlock.IsWriteLockHeld;
				if (!wasLocked && !rwlock.TryEnterUpgradeableReadLock((ReadLockTimeout + WriteLockTimeout) / 2))
				{
					throw new InvalidOperationException("The upgradeable reader lock could not be acquired.");
				}
			}

			/// <summary>
			/// Releases the lock if one was acquired. It is safe to call this method multiple times.
			/// </summary>
			public void Dispose()
			{
				if (!wasLocked)
				{
					rwlock.ExitUpgradeableReadLock();
					wasLocked = true;
				}
			}
		}

		/// <summary>
		/// Provides a simple disposable read lock adapter for a ReaderWriterLockSlim object.
		/// </summary>
		private struct ReadLock : IDisposable
		{
			private ReaderWriterLockSlim rwlock;
			private bool wasLocked;

			/// <summary>
			/// Acquires a read lock on a lock object. If the lock object already holds a read
			/// lock, no new lock is acquired and no lock will be released.
			/// </summary>
			/// <param name="rwlock">Lock object to work with. If this is null, no lock will be acquired.</param>
			public ReadLock(ReaderWriterLockSlim rwlock)
			{
				this.rwlock = rwlock;
				if (rwlock == null)
					wasLocked = true;
				else
					wasLocked = rwlock.IsReadLockHeld || rwlock.IsUpgradeableReadLockHeld || rwlock.IsWriteLockHeld;
				if (!wasLocked && !rwlock.TryEnterReadLock(ReadLockTimeout))
				{
					throw new InvalidOperationException("The reader lock could not be acquired.");
				}
			}

			/// <summary>
			/// Releases the lock if one was acquired. It is safe to call this method multiple times.
			/// </summary>
			public void Dispose()
			{
				if (!wasLocked)
				{
					rwlock.ExitReadLock();
					wasLocked = true;
				}
			}
		}

		#endregion Synchronisation helper classes
	}

	#region Date and time format enumeration

	/// <summary>
	/// Defines date and time components to be used with the Tx.Time method. Some of these values
	/// can be combined, other values should not be used.
	/// </summary>
	[Flags]
	public enum TxTime
	{
		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentYear = 0x1,
		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentMonth = 0x2,
		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentMonthAbbr = 0x4,
		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentMonthLong = 0x8,
		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentDay = 0x10,
		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentQuarter = 0x20,

		/// <summary>
		/// The day of the week in abbreviated form.
		/// </summary>
		Dow = 0x40,
		/// <summary>
		/// The day of the week in full.
		/// </summary>
		DowLong = 0x80,

		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentHour = 0x100,
		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentMinute = 0x200,
		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentSecond = 0x400,
		/// <summary>
		/// Internal value, do not use.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		FragmentMs = 0x800,

		/// <summary>
		/// The time zone.
		/// </summary>
		Zone = 0x1000,

		/// <summary>
		/// Internal value, do not use. Combines all date-related fragment values.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		AnyDate = FragmentYear | FragmentMonth | FragmentMonthAbbr | FragmentMonthLong | FragmentDay | FragmentQuarter,
		/// <summary>
		/// Internal value, do not use. Combines all time-related fragment values.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		AnyTime = FragmentHour | FragmentMinute | FragmentSecond | FragmentMs,

		/// <summary>
		/// The year alone.
		/// </summary>
		Year = FragmentYear,
		/// <summary>
		/// The year and month in numeric form.
		/// </summary>
		YearMonth = FragmentYear | FragmentMonth,
		/// <summary>
		/// The year and month in abbreviated form.
		/// </summary>
		YearMonthAbbr = FragmentYear | FragmentMonthAbbr,
		/// <summary>
		/// The year and month in full.
		/// </summary>
		YearMonthLong = FragmentYear | FragmentMonthLong,
		/// <summary>
		/// The year, month and day in numeric form.
		/// </summary>
		YearMonthDay = FragmentYear | FragmentMonth | FragmentDay,
		/// <summary>
		/// The year, month and day in abbreviated form.
		/// </summary>
		YearMonthDayAbbr = FragmentYear | FragmentMonthAbbr | FragmentDay,
		/// <summary>
		/// The year, month and day in full.
		/// </summary>
		YearMonthDayLong = FragmentYear | FragmentMonthLong | FragmentDay,
		/// <summary>
		/// The month alone in numeric form.
		/// </summary>
		Month = FragmentMonth,
		/// <summary>
		/// The month alone in abbreviated form.
		/// </summary>
		MonthAbbr = FragmentMonthAbbr,
		/// <summary>
		/// The month alone in full.
		/// </summary>
		MonthLong = FragmentMonthLong,
		/// <summary>
		/// The month and day in numeric form.
		/// </summary>
		MonthDay = FragmentMonth | FragmentDay,
		/// <summary>
		/// The month and day in abbreviated form.
		/// </summary>
		MonthDayAbbr = FragmentMonthAbbr | FragmentDay,
		/// <summary>
		/// The month and day in full.
		/// </summary>
		MonthDayLong = FragmentMonthLong | FragmentDay,
		/// <summary>
		/// The day alone.
		/// </summary>
		Day = FragmentDay,
		/// <summary>
		/// The year and quarter.
		/// </summary>
		YearQuarter = FragmentYear | FragmentQuarter,
		/// <summary>
		/// The quarter alone.
		/// </summary>
		Quarter = FragmentQuarter,

		/// <summary>
		/// The time of day with hour, minute, second and millisecond.
		/// </summary>
		HourMinuteSecondMs = FragmentHour | FragmentMinute | FragmentSecond | FragmentMs,
		/// <summary>
		/// The time of day with hour, minute and second.
		/// </summary>
		HourMinuteSecond = FragmentHour | FragmentMinute | FragmentSecond,
		/// <summary>
		/// The time of day with hour and minute.
		/// </summary>
		HourMinute = FragmentHour | FragmentMinute,
		/// <summary>
		/// The time of day with the hour alone.
		/// </summary>
		Hour = FragmentHour,
	}

	#endregion Date and time format enumeration
}