using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVtest : MonoBehaviour
{
    private string csvfileName = "CarList_FilePath";
    private Dictionary<int, Car_Info> dicName = new Dictionary<int, Car_Info>();

    private class Car_Info
    {
        private int carnum;
        private string fileName;
        private string carName;
        private string carColor;
        private string path;
    }

    private void Start()
    {
        ReadCSV();
    }

    private void ReadCSV()
    {
        List<Car_Info> list = new List<Car_Info>();
    }
}
