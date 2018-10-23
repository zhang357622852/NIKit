using System;
using System.Collections;
using System.Collections.Generic;

public static class JsonUtility {

    public static bool IsNull(JsonValue jsonData){
        return (jsonData == null || jsonData.IsNull());
    }

	public static JsonValue ToObject(string val){
		
		return new JsonValue((new JsonParser(val)).Decode());
	}

    public static JsonValue ToObjectFromJS(string val){
        
        return ToObject(UnEscapeJavascriptString(val));
    }

	public static string ToString(JsonValue json){
		
        return JsonWriter.Write(json);
	}

	public static string ToStringForJS(JsonValue json){
		return EscapeJavascriptString(ToString(json));
	}

	public static string EscapeJavascriptString(string jsonString){
		if (String.IsNullOrEmpty(jsonString))
            return jsonString;

        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        // builder.Append("\"");
        char[] charArray = jsonString.ToCharArray();
        for (int i = 0; i < charArray.Length; i++){
            char c = charArray[i];
            if (c == '"')
                builder.Append("\\\"");
            else if (c == '\\')
                builder.Append("\\\\");
            else if (c == '\b')
                builder.Append("\\b");
            else if (c == '\f')
                builder.Append("\\f");
            else if (c == '\n')
                builder.Append("\\n");
            else if (c == '\r')
                builder.Append("\\r");
            else if (c == '\t')
                builder.Append("\\t");
            else
                builder.Append(c);
        }
        // builder.Append("\"");

        return builder.ToString();
	}

	public static string UnEscapeJavascriptString(string jsonString){
        
        if (String.IsNullOrEmpty(jsonString))
            return jsonString;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        char c;

        for (int i = 0; i < jsonString.Length;)
        {
            c = jsonString[i++];

            if (c == '\\')
            {
                int remainingLength = jsonString.Length - i;
                if (remainingLength >= 2)
                {
                    char lookahead = jsonString[i];
                    if (lookahead == '\\')
                    {
                        sb.Append('\\');
                        ++i;
                    }
                    else if (lookahead == '"')
                    {
                        sb.Append("\"");
                        ++i;
                    }
                    else if (lookahead == 't')
                    {
                        sb.Append('\t');
                        ++i;
                    }
                    else if (lookahead == 'b')
                    {
                        sb.Append('\b');
                        ++i;
                    }
                    else if (lookahead == 'n')
                    {
                        sb.Append('\n');
                        ++i;
                    }
                    else if (lookahead == 'r')
                    {
                        sb.Append('\r');
                        ++i;
                    }else if (lookahead == 'u'){
                        char[] hex = new char[4];

                        for (int m=0; m< 4; m++) {
                            hex[m] = jsonString[i+m+1];
                        }

                        sb.Append((char) Convert.ToInt32(new string(hex), 16));
                        i++;
                        i += 4;
                    }
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }


    public static int GetInt(JsonValue jsonData, string key, int defaultVal) {
		if(IsNull(jsonData) || jsonData.Get(key).IsNull()){
            return defaultVal;
        }
        return jsonData.Get(key).GetInt();
    }

    public static float GetFloat(JsonValue jsonData, string key, float defaultVal) {
		if(IsNull(jsonData) || jsonData.Get(key).IsNull()){
            return defaultVal;
        }
        return jsonData.Get(key).GetFloat();
    }

    public static long GetLong(JsonValue jsonData, string key, long defaultVal) {
		if( IsNull(jsonData) || jsonData.Get(key).IsNull()){
            return defaultVal;
        }
        return jsonData.Get(key).GetLong();
    }

    public static double GetDouble(JsonValue jsonData, string key, double defaultVal) {
		if (IsNull(jsonData) || jsonData.Get (key).IsNull ()) {
			return defaultVal;
		}
		return jsonData.Get (key).GetDouble ();
	}

    public static string GetString(JsonValue jsonData, string key, string defaultVal) {
		if(IsNull(jsonData) || jsonData.Get(key).IsNull()){
            return defaultVal;
        }
        return jsonData.Get(key).GetString();
    }

    public static bool GetBoolean(JsonValue jsonData, string key, bool defaultVal) {
		if(IsNull(jsonData) || jsonData.Get(key).IsNull()){
            return defaultVal;
        }
        return jsonData.Get(key).GetBoolean();
	}

	public static void CopyJsonToIntList(JsonValue jsonData, List<int> intList){
		intList.Clear ();
		if (IsNull(jsonData) || jsonData.IsNull ()) {
			return;
		}

		for (int index = 0; index < jsonData.GetLength (); index++) {
			intList.Add (jsonData.Get (index).GetInt ());
		}
	}

	public static void CopyJsonToStrList (JsonValue jsonData, List<string> strList){
		strList.Clear ();
		if (IsNull(jsonData) || jsonData.IsNull ()) {
			return;
		}

		for (int index = 0; index < jsonData.GetLength (); index++) {
			strList.Add (jsonData.Get (index).GetString ());
		}
	}

	public static JsonValue CreateJsonByIntList (List<int> intList) {
		if (intList == null)
			return null;
		JsonValue json = JsonValue.Array ();
		for (int index = 0; index < intList.Count; index++) {
			json.Add (intList [index]);
		}
		return json;
	}

    public static JsonValue CreateJsonByStrList(List<string> intList)
    {
        if (intList == null)
            return null;
        JsonValue json = JsonValue.Array();
        for (int index = 0; index < intList.Count; index++)
        {
            json.Add(intList[index]);
        }
        return json;
    }

}