using System;
using System.Collections.Generic;
using System.Globalization;
using Database;
using SDE.Tools.DatabaseEditor.Engines.Parsers;
using SDE.Tools.DatabaseEditor.Generic.Lists;

namespace SDE.Tools.DatabaseEditor.Generic.Core {
	public static class ValueConverters {
		public static IValueConverter GetSetZeroString = new StringZeroDefaultConverter();
		public static IValueConverter StringTrimEmptyDefault = new StringTrimEmptyConverter();
		public static IValueConverter GetBooleanSetYesNoString = new BaseBooleanConverter("yes", "no");
		public static IValueConverter GetBooleanSetTrueFalseString = new BaseBooleanConverter("true", "false");
		public static IValueConverter GetBooleanSetIntString = new BaseBooleanConverter("1", "0", "0");
		public static IValueConverter GetIntSetSkillAttackString = new MappedValuesConverter(new string[] { "none", "weapon", "magic", "misc" }, null);
		public static IValueConverter GetIntSetConditionTypeString = new MappedValuesConverter(new string[] { "always", "onspawn", "myhpltmaxrate", "myhpinrate", "mystatuson", "mystatusoff", "friendhpltmaxrate", "friendhpinrate", "friendstatuson", "friendstatusoff", "attackpcgt", "attackpcge", "slavelt", "slavele", "closedattacked", "longrangeattacked", "skillused", "afterskill", "casttargeted", "rudeattacked" }, null);
		public static IValueConverter GetIntSetTargetString = new MappedValuesConverter(new string[] { "target", "self", "friend", "master", "randomtarget" }, null);
		public static IValueConverter GetIntSetRequiredStateString = new MappedValuesConverter(new string[] { "none", "move_enable", "recover_weight_rate", "water", "cart", "riding", "falcon", "sight", "hiding", "cloaking", "explosionspirits", "cartboost", "shield", "warg", "dragon", "ridingwarg", "mado", "poisonweapon", "rollingcutter", "elementalspirit", "mh_fighting", "mh_grappling", "peco" }, null);
		public static IValueConverter GetIntSetStateTypeString = new MappedValuesConverter(new string[] { "any", "idle", "walk", "dead", "loot", "attack", "angry", "chase", "follow", "anytarget" }, null);
		public static IValueConverter GetIntSetZeroString = new IntZeroDefaultConverter();
		public static IValueConverter GetIntSetZeroStringType = new IntZeroDefaultTypeConverter();
		public static IValueConverter GetIntSetEmptyString = new IntMinus1DefaultConverter();
		public static IValueConverter GetScriptNoBracketsSetScriptWithBrackets = new RoundBracketsConverter();
		public static IValueConverter GetScriptNoBracketsSetScriptWithBracketsSqure = new SquareBracketsConverter();
		public static IValueConverter GetNoHexJobSetHexJob = new ApplicableJobConverter();
		public static IValueConverter GetHexToIntSetInt = new HexToIntConverter();
		public static IValueConverter GetBooleanSetRefinableString = new RefineableConverter();
		public static IValueConverter GetSetTypeTrade = new GetSetTypeConverter<Trade>(28);
		public static IValueConverter GetSetTypeNouse = new GetSetTypeConverter<NoUse>(29);

		public static T ParseToInt<T>(string val) {
			if ((val.StartsWith("0x") || val.StartsWith("0X"))) {
				if (val.Length > 2)
					return (T)(object)Convert.ToInt32(val, 16);
				
				return default(T);
			}

			return (T)(object)Int32.Parse(val);
		}

		#region Nested type: ApplicableJobConverter

		public class ApplicableJobConverter : IValueConverter {
			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					return "0xFFFFFFFF";
				}

				string val = (string)value;

				if (val.StartsWith("0x") || val.StartsWith("0X"))
					val = val.Substring(2);

				if (val.Length == 0)
					return "";

				return "0x" + val.ToUpper();
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				if (value == null)
					return (T)(object)"";

				string val = (string)value;

				return (T)(object)val.Replace("0x", "").Replace("0X", "");
			}

			#endregion
		}

		#endregion

		#region Nested type: BaseBooleanConverter

		public class BaseBooleanConverter : IValueConverter {
			private readonly string _default;
			private readonly string _false;
			private readonly string _true;

			public BaseBooleanConverter(string @true, string @false, string @default = "") {
				_true = @true;
				_false = @false;
				_default = @default;
			}

			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					return _default;
				}

				if (value is bool) {
					return (bool)value ? _true : _false;
				}

				string val = (string)value;

				if (val == "true" || val == "yes" || val == "1")
					return _true;
				if (val == "false" || val == "no" || val == "0" || val == "")
					return _false;

				return Boolean.Parse((string)value) ? _true : _false;
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				if (typeof (T) == typeof (bool)) {
					if (value == null)
						return (T) (object) false;

					if (value is bool) {
						return (T) value;
					}

					if (value is string) {
						string val = (string) value;

						if (val == "yes" || val == "true" || val == "1")
							return (T) (object) true;
						if (val == "no" || val == "false" || val == "0")
							return (T) (object) false;

						return (T) (object) (val != "");
					}

					if (value is int) {
						int ival = (int) value;

						return (T) (object) (ival != 0);
					}

					return (T) value;
				}

				if (typeof(T) == typeof(string)) {
					if (value == null)
						return (T)(object)_false;

					if (value is bool) {
						bool bval = (bool) value;
						return (T)(object) (bval ? _true : _false);
					}

					if (value is string) {
						string val = (string)value;

						if (val == "yes" || val == "true" || val == "1")
							return (T)(object)_true;
						if (val == "no" || val == "false" || val == "0")
							return (T)(object)_false;

						return (T)(object)((val != "") ? _true : _false);
					}

					if (value is int) {
						int ival = (int)value;

						return (T)(object)((ival != 0) ? _true : _false);
					}

					return (T)value;
				}

				if (typeof(T) == typeof(int)) {
					string sval = ConvertFrom<string>(source, value);

					if (String.CompareOrdinal(sval, _false) == 0) {
						return (T)(object)0;
					}
					return (T)(object)1;
				}

				return (T)value;
			}

			#endregion
		}

		#endregion

		#region Nested type: BracketsConverter

		public class BracketsConverter : IValueConverter {
			private readonly string _compose;
			private readonly string _left;
			private readonly string _leftSpace;
			private readonly string _rightSpace;

			public BracketsConverter(string left, string right, bool allowSpace = true) {
				_compose = left + right;
				_leftSpace = left + (allowSpace ? " " : "");
				_rightSpace = (allowSpace ? " " : "") + right;
				_left = left;
			}

			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					return _compose;
				}

				string val = (string)value;

				if (val == "" || val == _compose)
					return _compose;

				// Why is this here? The elements in the tuple MUST have the brackets...
				val = val.Trim(' ');

				if (val.StartsWith(_left)) {
					val = val.Substring(1, val.Length - 2).Trim(' ');
				}

				return _leftSpace + val + _rightSpace;
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				if (value == null)
					return (T)(object)"";

				string val = (string)value;
				val = val.Trim(' ');

				if (val.StartsWith(_left)) {
					val = val.Substring(1, val.Length - 2);
				}

				return (T)(object)val.Trim(' ');
			}

			#endregion
		}

		#endregion

		#region Nested type: GetSetTypeConverter

		public class GetSetTypeConverter<TType> : IValueConverter where TType : class, ISettable {
			private readonly int _index;

			public GetSetTypeConverter(int index) {
				_index = index;
			}

			#region IValueConverter Members

			public object ConvertTo(Tuple source, object toReturn) {
				if (toReturn is TType) {
					return toReturn;
				}

				TType type = source.GetRawValue(_index) as TType;

				if (type == null) {
					type = (TType) Activator.CreateInstance(typeof(TType), new object[] { });
				}

				type.Set(toReturn ?? "");

				return type;
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				if (value == null) {
					value = Activator.CreateInstance(typeof(TType), new object[] { });
					source.SetRawValue(_index, value);
				}
				else if (value is string && typeof(T) == typeof(string)) {
					return (T)value;
				}
				else if (value is string) {
					TType ttype = (TType)Activator.CreateInstance(typeof(TType), new object[] { });
					ttype.Set(value);
					source.SetRawValue(_index, ttype);
					value = ttype;
				}

				if (typeof(T) == typeof(TType)) {
					return (T)value;
				}

				if (typeof(T) == typeof(string)) {
					return (T)(object)value.ToString();
				}

				if (!(value is string))
					return (T) (object) value.ToString();

				return (T)value;
			}

			#endregion
		}

		#endregion

		#region Nested type: HexToIntConverter

		public class HexToIntConverter : IValueConverter {
			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					return "0";
				}

				string val = (string)value;

				if ((val.StartsWith("0x") || val.StartsWith("0X"))) {
					if (val.Length > 2) {
						int ival = Convert.ToInt32(val, 16);
						return ival.ToString(CultureInfo.InvariantCulture);
					}

					return "0";
				}

				return val;
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				if (value == null)
					return (T)(object)"";

				string val = (string)value;

				if (val.StartsWith("0x") || val.StartsWith("0X")) {
					int ival = val.Length > 2 ? Convert.ToInt32(val, 16) : 0;

					if (typeof(T) == typeof(int)) {
						return (T)(object)ival;
					}

					if (typeof(T) == typeof(string)) {
						return (T)(object)ival.ToString(CultureInfo.InvariantCulture);
					}
				}

				if (typeof(T) == typeof(int)) {
					return (T)(object)Int32.Parse(val);
				}

				if (typeof(T) == typeof(string)) {
					return (T)(object)val;
				}

				throw new InvalidCastException("Couldn't convert '" + value + "' to type of " + typeof(T) + ".");
			}

			#endregion
		}

		#endregion

		#region Nested type: IntMinus1DefaultConverter

		public class IntMinus1DefaultConverter : IValueConverter {
			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					return "";
				}

				if (value is string && (string) value  == "")
					return "";

				return value.ToString();
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				if (typeof(T) == typeof(string)) {
					if (value == null)
						return (T)(object)"-1";

					string val = (string)value;

					if (val == "")
						return (T)(object)"-1";

					return (T)(object)val;
				}
				if (typeof(T) == typeof(int)) {
					if (value == null)
						return (T)(object)-1;

					if (value is string) {
						string val = (string) value;

						if (val == "")
							return (T) (object) -1;

						return (T) (object) Int32.Parse(val);
					}

					if (value is int) {
						return (T)value;
					}

					return (T)(object)Int32.Parse(value.ToString());
				}
				else {
					if (value == null)
						return (T)(object)-1;

					string val = (string)value;

					if (val == "")
						return (T)(object)-1;

					return (T)(object)Int32.Parse(val);
				}
			}

			#endregion
		}

		#endregion

		#region Nested type: IntZeroDefaultConverter

		public class IntZeroDefaultConverter : IValueConverter {
			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					return "0";
				}

				if (value is string && (string)value == "")
					return "0";

				return value;
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				if (typeof(T) == typeof(string)) {
					if (value == null)
						return (T)(object)"0";

					if (value is int)
						return (T) (object) ((int) value).ToString(CultureInfo.InvariantCulture);

					string val = (string)value;

					if (val == "")
						return (T)(object)"0";

					return (T)(object)val;
				}

				if (typeof(T) == typeof(int)) {
					if (value == null)
						return (T)(object)0;

					if (value is int)
						return (T) value;

					string val = (string)value;

					if (val == "")
						return (T)(object)0;

					return (T)(object) Int32.Parse(val);
				}
				else {
					if (value == null)
						return (T) (object) 0;

					string val = (string) value;

					if (val == "")
						return (T) (object) 0;

					return (T) (object) Int32.Parse(val);
				}
			}

			#endregion
		}

		#endregion

		#region Nested type: IntZeroDefaultTypeConverter

		public class IntZeroDefaultTypeConverter : IValueConverter {
			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					return "0";
				}

				if (value is string && (string)value == "")
					return "0";

				int ival;
				if (Int32.TryParse(value.ToString(), out ival)) {
					bool refineable = source.GetValue<bool>(ServerItemAttributes.Refineable);
					if (!refineable) {
						if (ival == 4 || ival == 5) {
							source.SetRawValue(ServerItemAttributes.Refineable, "0");
						}
						else {
							source.SetRawValue(ServerItemAttributes.Refineable, "");
						}
					}
				}

				return value;
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				if (typeof(T) == typeof(string)) {
					if (value == null)
						return (T)(object)"0";

					if (value is int)
						return (T)(object)((int)value).ToString(CultureInfo.InvariantCulture);

					string val = (string)value;

					if (val == "")
						return (T)(object)"0";

					return (T)(object)val;
				}

				if (typeof(T) == typeof(int)) {
					if (value == null)
						return (T)(object)0;

					if (value is int)
						return (T)value;

					string val = (string)value;

					if (val == "")
						return (T)(object)0;

					return (T)(object)Int32.Parse(val);
				}
				else {
					if (value == null)
						return (T)(object)0;

					string val = (string)value;

					if (val == "")
						return (T)(object)0;

					return (T)(object)Int32.Parse(val);
				}
			}

			#endregion
		}

		#endregion

		#region Nested type: MappedValuesConverter

		public class MappedValuesConverter : IValueConverter {
			public static Type StringType = typeof(string);
			public static Type IntType = typeof(int);
			public static Type BooleanType = typeof(bool);

			public readonly string DefaultString = "";
			private readonly Dictionary<string, int> _stringToInt = new Dictionary<string, int>();
			//private readonly Dictionary<int, string> _intToString = new Dictionary<int, string>();
			private readonly string[] _values;

			public MappedValuesConverter(string[] values, int[] mappedValues) {
				_values = values;

				if (mappedValues == null) {
					for (int i = 0; i < values.Length; i++) {
						_stringToInt[values[i]] = i;
					}
				}
				else {
					for (int i = 0; i < values.Length; i++) {
						_stringToInt[values[i]] = mappedValues[i];
					}
				}
			}

			public bool AllowSetEmpty { get; set; }

			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					if (AllowSetEmpty) {
						return DefaultString;
					}

					return _values[0];
				}

				if (value is bool) {
					return (bool) value ? _values[1] : _values[0];
				}

				if (value is string) {
					int ival;

					if (Int32.TryParse((string)value, out ival)) {
						if (ival < 0) {
							return "";
						}

						return _values[ival];
					}

					bool bval;

					if (Boolean.TryParse((string)value, out bval)) {
						return _values[bval ? 1 : 0];
					}

					if ((string) value == "") {
						return DefaultString;
					}

					return value;
				}

				if (value is int) {
					return _values[(int) value];
				}

				return value;
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				// Returns an int

				if (typeof(T) == StringType) {
					// Destination is string
					if (value is string) {
						int ival;
						string sval = (string)value;

						if (Int32.TryParse(sval, out ival)) {
							return (T)value;
						}

						if (_stringToInt.ContainsKey(sval)) {
							return (T)(object)_stringToInt[sval].ToString(CultureInfo.InvariantCulture);
						}

						return (T)(object)"-1";
					}

					if (value is int) {
						return (T)(object) value.ToString();
					}

					if (value is bool) {
						return (T) (object) ((bool) value ? "1" : "0");
					}
				}

				if (typeof(T) == IntType) {
					// Destination is int
					if (value is string) {
						int ival;
						string sval = (string)value;

						if (Int32.TryParse(sval, out ival)) {
							return (T)(object)ival;
						}

						if (_stringToInt.ContainsKey(sval)) {
							return (T) (object) _stringToInt[sval];
						}

						return (T)(object) -1;
					}

					if (value is int) {
						return (T)value;
					}

					if (value is bool) {
						return (T)(object)((bool)value ? 1 : 0);
					}
				}

				if (typeof(T) == BooleanType) {
					// Destination is boolean
					if (value is string) {
						int ival;
						string sval = (string)value;

						if (Int32.TryParse(sval, out ival)) {
							return (T)(object)(ival != 0);
						}

						if (_stringToInt.ContainsKey(sval)) {
							return (T)(object) (_stringToInt[sval] != 0);
						}

						return (T)(object) false;
					}

					if (value is int) {
						return (T) (object) ((int) value != 0);
					}

					if (value is bool) {
						return (T)value;
					}
				}

				return default(T);
			}

			#endregion
		}

		#endregion

		#region Nested type: RefineableConverter

		public class RefineableConverter : IValueConverter {
			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				string returnedValue;

				if (value == null) {
					returnedValue = "";
				}
				else {
					if (value is bool) {
						returnedValue = (bool)value ? "1" : "0";
					}
					else {
						string val = (string) value;

						if (val == "true" || val == "1")
							returnedValue = "1";
						else if (val == "false" || val == "0" || val == "")
							returnedValue = "0";
						else
							returnedValue = Boolean.Parse((string)value) ? "1" : "0";
					}
				}

				int itemType = source.GetValue<int>(ServerItemAttributes.Type);

				if (_isNull(itemType, returnedValue))
					return "";

				return returnedValue;
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				int itemType = source.GetValue<int>(ServerItemAttributes.Type);

				if (value == null)
					return (T) (object) false;

				string val = (string) value;

				if (typeof (T) == typeof (bool)) {
					if (_isNull(itemType, value))
						return (T)(object)false;
					if (val == "true" || val == "1")
						return (T) (object) true;
					if (val == "false" || val == "0" || val == "")
						return (T) (object) false;

					return (T) (object) (val != "");
				}

				if (typeof (T) == typeof (string)) {
					if (_isNull(itemType, value))
						return (T)(object)"";
					if (val == "true" || val == "1")
						return (T) (object) "true";
					if (val == "false" || val == "" || val == "0")
						return (T) (object) "false";

					return (T) (object) ((val != "") ? "true" : "false");
				}

				if (typeof (T) == typeof (int)) {
					if (_isNull(itemType, value))
						return (T)(object)0;
					if (val == "true" || val == "1")
						return (T) (object) 1;
					if (val == "false" || val == "" || val == "0")
						return (T) (object) 0;

					return (T) (object) (val != "");
				}

				return (T) value;
			}

			#endregion

			private bool _isNull(int itemType, object value) {
				if (itemType != 4 && itemType != 5) {
					string val = (string)value;

					if (val == "false" || val == "" || val == "0")
						return true;
				}

				return false;
			}
		}

		#endregion

		#region Nested type: RoundBracketsConverter

		public class RoundBracketsConverter : BracketsConverter {
			public RoundBracketsConverter() : base("{", "}") { }
		}

		#endregion

		#region Nested type: SquareBracketsConverter

		public class SquareBracketsConverter : BracketsConverter {
			public SquareBracketsConverter() : base("[", "]", false) { }
		}

		#endregion

		#region Nested type: StringTrimEmptyConverter

		public class StringTrimEmptyConverter : IValueConverter {
			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					return "0";
				}

				string val = (string)value;

				return val.Trim(new char[] { '\t', ' ' });
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				if (typeof(T) == typeof(string)) {
					if (value == null)
						return (T)(object)"";

					string val = (string)value;

					if (val == "")
						return (T)(object)"";

					return (T)(object)val.Trim(new char[] { '\t', ' ' });
				}
				else if (typeof(T) == typeof(int)) {
					if (value == null)
						return (T)(object)0;

					string val = (string)value;

					if (val == "")
						return (T)(object)0;

					return (T)(object)Int32.Parse(val);
				}

				return (T)value;
			}

			#endregion
		}

		#endregion

		#region Nested type: StringZeroDefaultConverter

		public class StringZeroDefaultConverter : IValueConverter {
			#region IValueConverter Members

			public object ConvertTo(Tuple source, object value) {
				if (value == null) {
					return "0";
				}

				if (value is int) {
					return value.ToString();
				}

				string val = (string)value;

				if (val == "")
					return "0";

				return value;
			}

			public T ConvertFrom<T>(Tuple source, object value) {
				try {
					if (typeof (T) == typeof (string)) {
						if (value == null)
							return (T) (object) "0";

						if (value is string) {
							string val = (string) value;

							if (val == "")
								return (T) (object) "0";

							return (T) (object) val;
						}

						if (value is int) {
							return (T) (object) ((int) value).ToString(CultureInfo.InvariantCulture);
						}

						return (T) value;
					}

					if (typeof (T) == typeof (int)) {
						if (value == null)
							return (T) (object) 0;

						if (value is int) {
							return (T) value;
						}

						string val = (string) value;

						if (val == "")
							return (T) (object) 0;

						return ParseToInt<T>(val);
					}

					return (T) value;
				}
				catch {
					return default(T);
				}
			}

			#endregion
		}

		#endregion
	}
}