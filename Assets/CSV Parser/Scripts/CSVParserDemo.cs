using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using CSVToolKit;

namespace CSVToolKit {
	public class CSVParserDemo : MonoBehaviour 
	{
		public string fileName;
		public InputField rollNoInputField;
		public InputField nameInputField;
		public Text contentArea;

		void Start () 
		{
			ReadCSVFile();
		}

		void ReadCSVFile() {
			List<List<string>> data = CSVParser.Instance.ReadData(GetPath() + "/CSV Parser/Resources", "Data.csv");
			contentArea.text = "";
			foreach(List<string> row in data){
				foreach(string field in row){
					contentArea.text += field + "\t";
				}
				contentArea.text += "\n";
			}
		}

		public void AddContents() {
			CSVParser.Instance.AddData(GetPath() + "/CSV Parser/Resources", "Data.csv", new List<string>{rollNoInputField.text, nameInputField.text});
			ReadCSVFile();
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