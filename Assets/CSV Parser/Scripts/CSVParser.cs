using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace CSVToolKit {
	public class CSVParser : MonoBehaviour 
	{
		private CSVParser() {}
		private static CSVParser instance = null;
		public static CSVParser Instance {
			get {
				if (instance == null) {
					instance = new CSVParser();
				}
				return instance;
			}
		}

		private char lineSeperater = '\n';
		private char fieldSeperator = ',';

		void SetLineSeparator(char seperator) {
			lineSeperater = seperator;
		}

		void SetFieldSeperator(char seperator) {
			fieldSeperator = seperator;
		}

		public List<List<string>> ReadData(string path, string filename)
		{
			List<List<string>> result = new List<List<string>>();
			try {
				var source = new StreamReader((path  == "" ? GetPath() : path) + "/" + filename);
				var fileContents = source.ReadToEnd();
				source.Close();
				var records = fileContents.Split(lineSeperater);
				
				
				// TextAsset csvFile = Resources.Load<TextAsset>(filename);
				// string[] records = csvFile.text.Split (lineSeperater);
				foreach (string record in records)
				{
					List<string> row = new List<string>();
					string[] fields = record.Split(fieldSeperator);
					foreach(string field in fields)
						row.Add(field);
				
					result.Add(row);
				}

			} catch(Exception ex) {
				Debug.LogError("Something went wrong while reading content");
			}
			return result;			
		}

		public void AddData(string path, string filename, List<string> values)
		{
			try {
				string data = lineSeperater.ToString();
				foreach(string value in values)
					data += value + fieldSeperator;

				File.AppendAllText((path  == "" ? GetPath() : path) + "/" + filename, data);

				#if UNITY_EDITOR
					UnityEditor.AssetDatabase.Refresh ();
				#endif
			} catch(Exception ex) {
				Debug.LogError("Something went wrong while writing content");
			}
		}

		private static string GetPath(){
			#if UNITY_EDITOR
			return Application.dataPath;
			#elif UNITY_ANDROID
			return Application.persistentDataPath;
			#elif UNITY_IPHONE
			return GetiPhoneDocumentsPath();
			#else
			return Application.dataPath;
			#endif
		}

		private static string GetiPhoneDocumentsPath()
		{
			string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
			path = path.Substring(0, path.LastIndexOf('/'));
			return path + "/Documents";
		}
	}
}