
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TableMgr
{
    public TableCharacter Character = new TableCharacter();

    public void Init()
    {
#if UNITY_EDITOR
        Character.Init_CSV("Car_spec", 1, 0);
#else
        Character.Init_Binary("Character");
#endif
    }

    public void Save()
    {
        Character.Save_Binary("Car_spec");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
