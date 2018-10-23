using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


	public static class JsonWriter {
	
		public static string Write(JsonValue json){
			if(json == null){
				return "";
			}
			return new Writer().Write(json);
		}
	
		sealed class Writer {
			private static readonly int BUILDER_CAPACITY = 2000;
			private System.Text.StringBuilder document_;
			
	
			static string QuoteString(string val){
				return '"' + val + '"';
			}
	
			public string Write(JsonValue value){
				document_ = new System.Text.StringBuilder(BUILDER_CAPACITY);
				WriteValue(value);
				return document_.ToString();
			}
	
			public void WriteValue(JsonValue value) {
				if (value.IsNull()){
					return;
				}
				if (value.GetValueType() == typeof(List<object>)) {
					document_.Append('[');
					int size = value.GetLength();
					for (int ArrayIndex = 0; ArrayIndex < size; ++ArrayIndex) {
						if ( ArrayIndex > 0) {
							document_.Append(',');
						}
	
						WriteValue((value.Get(ArrayIndex)));
					}
					document_.Append(']');
				
				}else if (value.GetValueType() == typeof(Dictionary<String, object>)) {
					string[] keys = value.GetKeys();
					
					document_.Append('{');
	
					for (int DicIndex = 0; DicIndex != keys.Length; ++DicIndex) {
						if (DicIndex != 0) {
							document_.Append(',');
						}
	
						document_.Append(QuoteString(keys[DicIndex]));
						document_.Append(':');
	
						WriteValue((value.Get(keys[DicIndex])));
					}
					document_.Append('}');
	
				}else if (value.GetValueType() == typeof(string)) {
					document_.Append(QuoteString(value.GetString()));
				}
				else if(value.GetValueType() == typeof(bool)) {
					
					if(value.GetBoolean()){
						
						document_.Append("true");
					}else {
					
						document_.Append("false");
					}
				}
				else {
					document_.Append(value.ToString());
				}
			}
	
		}
	}
