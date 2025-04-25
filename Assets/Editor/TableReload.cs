using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TableReload : MonoBehaviour
{
    [MenuItem("CS_Util/Table/CSV &F1", false , 1)]
    static public void ParserTableCSV()
    {
        TableMgr mgr = new TableMgr();
        mgr.Init();
        mgr.Save();
    }
}
