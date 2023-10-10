using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StartLoading : MonoBehaviour
{
    public HexGrid hexGrid;

    const int fileMapVersion = 3;
    void Start()
    {

        if (PlayerPrefs.GetInt("PerformActions") == 1)
        {
            string path = PlayerPrefs.GetString("FilePath");
            PlayerPrefs.DeleteKey("PerformActions");

            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                int header = reader.ReadInt32();

                if (header <= fileMapVersion)
                {
                    hexGrid.Load(reader, header);
                    //HexMapCamera.ValidatePosition();
                }
                else
                {
                    Debug.LogWarning("Unknown map format " + header);
                }
            }
        }
    }
}
