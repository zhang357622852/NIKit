using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class JsonValue {

	public static readonly int INVOID_LENGTH = -1;
	
	public object value_;

	public static JsonValue Object(){
		JsonValue result = new JsonValue();
		return result;
	}

	public static JsonValue Array(){
		JsonValue result = new JsonValue(new List<object>());
		return result;
	}

	public JsonValue(){
		value_ = new Dictionary<string, object>();
	}

	public JsonValue(object value){
		value_ = value;
	}
	
	public virtual System.Type GetValueType(){
		return value_.GetType();
	}

	public virtual bool HasKey(string key){
		if ( !IsObject()){
			return false;
		}

		Dictionary<string, object> m_val = value_ as Dictionary<string, object>;

		return m_val.ContainsKey(key);
	}

	public virtual JsonValue Get(string key){
		if ( !IsObject()){
			return JsonNull.GetInstance();
		}
		
		Dictionary<string, object> m_val = value_ as Dictionary<string, object>;

		if (!m_val.ContainsKey(key)){
			return JsonNull.GetInstance();
		}

		return new JsonValue(m_val[key]);
		
	}

	public virtual JsonValue Get(int index){
		if ( !IsArray()){
			return null;
		}

		List<object> m_val = value_ as List<object>;
		return new JsonValue(m_val[index]);
	}

	public virtual string[] GetKeys(){
		if (!IsObject()){
			return null;
		}

		Dictionary<string, object> m_val = value_ as Dictionary<string, object>;
		
		string[] keys = new string[m_val.Count];

		int index = 0;
		foreach (string key in m_val.Keys) {
			keys[index] = key;
			++index;
		}

		return keys;
	}

	public virtual int GetLength() {
		if (IsObject()) {
			Dictionary<string, object> m_val = value_ as Dictionary<string, object>;
			return m_val.Count;
		}
		if (IsArray()) {
			List<object> l_val = value_ as List<object>;
			return l_val.Count;
		}

		return INVOID_LENGTH;
	}

	public virtual UInt64 GetULong(){
		if(value_.GetType() != typeof(UInt64)){
			return Convert.ToUInt64(value_);
		}
		return (UInt64)value_;
	}

	public virtual long GetLong(){
		if(value_.GetType() != typeof(long)){
			return Convert.ToInt64(value_);
		}
		return (long)value_;
	}

	public virtual int GetInt(){
		if(value_.GetType() != typeof(int)){
			return Convert.ToInt32(value_);
		}
		return (int)value_;
	}

	public virtual float GetFloat(){
		if(value_.GetType() != typeof(float)){
			return Convert.ToSingle(value_);
		}
		return (float)value_;
	}

	public virtual double GetDouble(){
		if(value_.GetType() != typeof(double)){
			return Convert.ToDouble(value_);
		}
		return (double)value_;
	}

	public virtual bool GetBoolean(){
		return (bool)value_;
	}

	public virtual string GetString(){
		if (IsNull()){
			return "";
		}
		return value_.ToString();
	}

    public HashSet<T> GetHashSet<T>() {
        HashSet<T> result = new HashSet<T>();
        if (IsNull()) {
            return result;
        }
        for (int i = 0; i < GetLength(); i++)
        {
            result.Add((T)Get(i).value_);
        }
        return result;
    }

	public virtual Vector3 GetVector3 () {
        if (value_.GetType() != typeof(UnityEngine.Vector3))
        {
            string _value = value_.ToString();
            string[] _vas = _value.Replace("(", "").Replace(")", "").Split(',');
            return new UnityEngine.Vector3(Convert.ToSingle(_vas[0]), Convert.ToSingle(_vas[1]), Convert.ToSingle(_vas[2]));
        }
        return (UnityEngine.Vector3)value_;
    }

	public virtual bool IsNull(){
		if (value_ == null) {
			return true;
		}

		return false;
	}

	public virtual bool IsObject(){
		if (value_ == null){
			return false;
		}
		if (value_.GetType() == typeof(Dictionary<string, object>)) {
			return true;
		}

		return false;
	}

	public virtual bool IsArray(){
		if (value_ == null){
			return false;
		}
		if (value_.GetType() == typeof(List<object>)) {
			return true;
		}

		return false;
	}

	public override string ToString(){
		return value_.ToString();
	}

	public virtual JsonValue Add(object value){
		if (!IsArray()){
			return this;
		}

		if (value.GetType() == typeof(JsonValue)){
			JsonValue val = value as JsonValue;
			value = val.value_;
		}
		
		List<object> m_val = value_ as List<object>;
		m_val.Add(value);
		return this;
	}
	
	public virtual JsonValue Remove(string key){
		if ( !IsObject()){
			return this;
		}
		
		Dictionary<string, object> m_val = value_ as Dictionary<string, object>;
		if (m_val.ContainsKey(key)) {
			m_val.Remove(key);
		}
		return this;
	}

	public virtual JsonValue Add(string key, object value){
		if ( !IsObject()){
			return this;
		}
		
		Dictionary<string, object> m_val = value_ as Dictionary<string, object>;
		
		if (m_val.ContainsKey(key)) {
			m_val.Remove(key);
		}

		if (value.GetType() == typeof(JsonValue)){
			JsonValue val = value as JsonValue;
			value = val.value_;
		}

		m_val.Add(key, value);
		return this;
	}
	
}

public class JsonNull : JsonValue {
	
	private static JsonNull instance_;

	public static JsonNull GetInstance(){
		if(instance_ == null){
			instance_ = new JsonNull();
		}
		return instance_;
	}

	public JsonNull(){
		value_ = null;
	}

	public override System.Type GetValueType(){
		return null;
	}

	public virtual bool HasKey(string key){
		return false;
	}

	public override JsonValue Get(string key){
		return null;
	}

	public override JsonValue Get(int index){
		return null;
	}

	public override string[] GetKeys(){
		return null;
	}

	public override int GetLength() {
		return 0;
	}
	
	public override UInt64 GetULong(){
		return 0;
	}
	
	public override long GetLong(){
		return 0;
	}

	public override int GetInt(){
		return 0;
	}

	public override float GetFloat(){
		return 0;
	}

	public override double GetDouble(){
		return 0;
	}

	public override bool GetBoolean(){
		return false;
	}

	public override string GetString(){
		return "";
	}

	public override bool IsNull(){
		return true;
	}

	public override bool IsObject(){
		return false;
	}

	public override bool IsArray(){
		return false;
	}

	public override string ToString(){
		return "";
	}

	public override JsonValue Add(object value){
		return this;
	}
	
	public override JsonValue Remove(string key){
		return this;
	}

	public override JsonValue Add(string key, object value){
		return this;
	}
	
}

