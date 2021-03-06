using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Database;
using ErrorManager;
using SDE.Tools.DatabaseEditor.Engines.Parsers;
using SDE.Tools.DatabaseEditor.Generic.Core;
using SDE.Tools.DatabaseEditor.Generic.Lists;
using Utilities;
using Utilities.Extension;

namespace SDE.Tools.DatabaseEditor.Generic.DbLoaders {
	public static partial class DbLoaderMethods {
		public static void DbItemGroups<TKey>(DbDebugItem<TKey> debug, AbstractDb<TKey> db) {
			foreach (DbAttribute attribute in ServerItemGroupSubAttributes.AttributeList.Attributes) {
				db.Attached[attribute.DisplayName] = false;
			}

			if (debug.FileType == FileType.Txt) {
				if (db.Attached["FromUserRawInput"] != null && (bool) db.Attached["FromUserRawInput"]) {
					_loadItemsGroupdDb(db, debug.FilePath);
					return;
				}

				using (StreamReader reader = new StreamReader(File.OpenRead(debug.FilePath))) {
					string line;

					while (!reader.EndOfStream) {
						line = reader.ReadLine();

						if (line != null && line.StartsWith("import: ")) {
							string dbPath = AllLoaders.DetectPathAll(line.Replace("import: ", ""));

							if (dbPath == null) {
								ErrorHandler.HandleException("Couldn't find the file '" + line.Replace("import: ", "") + "'.");
							}
							else {
								db.Attached[dbPath] = new Tuple<string, HashSet<int>>(line.Replace("import: ", ""), new HashSet<int>());
								_loadItemsGroupdDb(db, dbPath);
							}
						}
					}
				}
			}
			else if (debug.FileType == FileType.Conf) {
				ItemGroupParser itemHelper = new ItemGroupParser();
				Table<int, ReadableTuple<int>> itemsDb = db.GetMeta<int>(ServerDbs.Items);
				int index = ServerItemAttributes.AegisName.Index;
				var table = db.Table;

				var items = itemsDb.FastItems;

				// The reverse table is used for an optimization (~3 seconds to ~50 ms)
				// All the items are stored in a dictionary by their name instead of their ID
				TkDictionary<string, int> reverseTable = new TkDictionary<string, int>();

				foreach (var item in items) {
					reverseTable[item.GetStringValue(index).ToLowerInvariant()] = item.GetKey<int>();
				}

#if SDE_DEBUG
				Z.StopAndRemoveWithoutDisplay(-1);
				Z.StopAndRemoveWithoutDisplay(-2);
				Z.StopAndRemoveWithoutDisplay(-3);
				CLHelper.CR(-2);
#endif
				foreach (string elements in TextFileHelper.GetElementsByParenthesis(File.ReadAllBytes(debug.FilePath))) {
#if SDE_DEBUG
					CLHelper.CS(-2);
					CLHelper.CR(-1);
					CLHelper.CR(-3);
#endif
					if (!itemHelper.Init(debug, elements)) return;
#if SDE_DEBUG
					CLHelper.CS(-3);
#endif

					try {
						Tuple tupleItem = itemsDb.TryGetTuple(reverseTable[itemHelper.Id.ToLowerInvariant()]);

						if (tupleItem == null) {
							if (itemHelper.Id.StartsWith("ID")) {
								int ival;

								if (Int32.TryParse(itemHelper.Id.Substring(2), out ival)) {
									tupleItem = itemsDb.TryGetTuple(ival);
								}

								if (tupleItem == null) {
									debug.ReportIdException("Item ID '" + ival + "' couldn't be found.", itemHelper.Id, ErrorLevel.Critical);
									continue;
								}
							}

							if (tupleItem == null) {
								debug.ReportIdException("Item ID '" + itemHelper.Id + "' couldn't be found.", itemHelper.Id, ErrorLevel.Critical);
								continue;
							}
						}

						TKey itemId = tupleItem.GetKey<TKey>();

						if (!table.ContainsKey(itemId)) {
							ReadableTuple<TKey> tuple = new ReadableTuple<TKey>(itemId, db.AttributeList);
							tuple.SetRawValue(ServerItemGroupAttributes.Table, new Dictionary<int, ReadableTuple<int>>());
							table.Add(itemId, tuple);
						}

						for (int i = 0; i < itemHelper.Quantities.Count; i++) {
							string onameId = itemHelper.Quantities[i].Item1;
							string orate = itemHelper.Quantities[i].Item2;
							int id = 0;
							int rate;

							tupleItem = itemsDb.TryGetTuple(reverseTable[onameId.ToLowerInvariant()]);

							if (tupleItem == null) {
								if (onameId.StartsWith("ID")) {
									if (!Int32.TryParse(onameId.Substring(2), out id)) {
										debug.ReportIdException("Item ID '" + itemHelper.Quantities[i].Item1 + "' couldn't be found in group '" + itemHelper.Id + "'.", itemHelper.Id, ErrorLevel.Critical);
										continue;
									}
								}
								else {
									debug.ReportIdException("Item ID '" + itemHelper.Quantities[i].Item1 + "' couldn't be found in group '" + itemHelper.Id + "'.", itemHelper.Id, ErrorLevel.Critical);
									continue;
								}
							}

							int nameId = tupleItem == null ? id : tupleItem.GetKey<int>();
							Int32.TryParse(orate, out rate);

							Dictionary<int, ReadableTuple<int>> dico = (Dictionary<int, ReadableTuple<int>>)table.GetRaw(itemId, ServerItemGroupAttributes.Table);

							ReadableTuple<int> tuple = new ReadableTuple<int>(nameId, ServerItemGroupSubAttributes.AttributeList);
							tuple.SetRawValue(ServerItemGroupSubAttributes.Rate, rate);
							dico[nameId] = tuple;
						}
					}
					catch {
						if (!debug.ReportIdException(itemHelper.Id)) return;
					}
#if SDE_DEBUG
					CLHelper.CS(-1);
					CLHelper.CR(-2);
#endif
				}
#if SDE_DEBUG
				CLHelper.CS(-2);
				CLHelper.CS(-3);
				CLHelper.WA = ", method core : " + CLHelper.CD(-1) + "ms, loop getter : " + CLHelper.CD(-2) + "ms, internal parser : " + CLHelper.CD(-3);
#endif
			}
		}
		private static void _loadItemsGroupdDb<TKey>(AbstractDb<TKey> db, string file) {
			int numberOfErrors = 3;
			TextFileHelper.LatestFile = file;

			if (String.IsNullOrEmpty(file)) {
				DbLoaderErrorHandler.Handle("File not found " + ServerDbs.ItemGroups + ".", ErrorLevel.NotSpecified);
				return;
			}

			var itemDb1 = db.Database.GetDb<int>(ServerDbs.Items);
			var itemDb2 = db.Database.GetDb<int>(ServerDbs.Items);

			if (!itemDb1.IsLoaded) {
				itemDb1.LoadDb();
			}

			if (!itemDb2.IsLoaded) {
				itemDb2.LoadDb();
			}

			var itemDb = new MetaTable<int>(ServerItemAttributes.AttributeList);
			itemDb.AddTable(itemDb1.Table);
			itemDb.AddTable(itemDb2.Table);

			var table = db.Table;
			HashSet<int> loadedIds = ((Tuple<string, HashSet<int>>)db.Attached[file]).Item2;

			foreach (string[] elements in TextFileHelper.GetElementsByCommas(File.ReadAllBytes(file))) {
				try {
					TKey itemId;
					int iItemId;

					if (Int32.TryParse(elements[0], out iItemId)) {
						itemId = (TKey) (object) iItemId;
					}
					else {
						var constantDb = db.Database.GetDb<string>(ServerDbs.Constants);

						if (!constantDb.IsLoaded) {
							constantDb.LoadDb();
						}

						var tuple = constantDb.Table.TryGetTuple(elements[0]);

						if (tuple == null) {
							if (AllLoaders.GenericErrorHandler(ref numberOfErrors, elements[0])) return;
							continue;
						}

						itemId = (TKey) (object) tuple.GetValue<int>(1);
					}

					string orate = elements[2];

					int nameId;
					int rate;

					if (Int32.TryParse(elements[1], out nameId)) { }
					else {
						var tuple = itemDb.FastItems.FirstOrDefault(p => p.GetStringValue(ServerItemAttributes.AegisName.Index) == elements[1]);

						if (tuple == null) {
							if (AllLoaders.GenericErrorHandler(ref numberOfErrors, elements[0])) return;
							continue;
						}

						nameId = tuple.GetKey<int>();
					}

					Int32.TryParse(orate, out rate);

					var id = (object) itemId;
					loadedIds.Add((int) id);

					if (!table.ContainsKey(itemId)) {
						ReadableTuple<TKey> tuple = new ReadableTuple<TKey>(itemId, db.AttributeList);
						tuple.SetRawValue(ServerItemGroupAttributes.Table, new Dictionary<int, ReadableTuple<int>>());
						table.Add(itemId, tuple);
					}

					Dictionary<int, ReadableTuple<int>> dico = (Dictionary<int, ReadableTuple<int>>)table.GetRaw(itemId, ServerItemGroupAttributes.Table);

					ReadableTuple<int> newTuple = new ReadableTuple<int>(nameId, ServerItemGroupSubAttributes.AttributeList);
					List<DbAttribute> attributes = new List<DbAttribute>(ServerItemGroupSubAttributes.AttributeList.Attributes);

					for (int i = 2; i < elements.Length; i++) {
						db.Attached[attributes[i - 1].DisplayName] = true;
						newTuple.SetRawValue(attributes[i - 1], elements[i]);
					}

					dico[nameId] = newTuple;
				}
				catch {
					if (AllLoaders.GenericErrorHandler(ref numberOfErrors, elements[0])) return;
				}
			}
		}
	}
}