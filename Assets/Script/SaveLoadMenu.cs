using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;

public class SaveLoadMenu : MonoBehaviour
{
    public InputField nameInput;
    public Text menuLabel, actionButtonLabel;
    public HexGrid hexGrid;
    bool saveMode;
    public RectTransform listContent;
    public SaveLoadItem itemPrefab;

    const int fileMapVersion = 4;

    void FillList()
    {
        string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.map");
        Array.Sort(paths);
        for (int i = 0; i < listContent.childCount; i++)
        {
            Destroy(listContent.GetChild(i).gameObject);
        }
        for (int i = 0; i < paths.Length; i++)
        {
            SaveLoadItem item = Instantiate(itemPrefab);
            item.menu = this;
            item.MapName = Path.GetFileNameWithoutExtension(paths[i]);
            item.transform.SetParent(listContent, false);
        }

    }

    public void Open(bool saveMode)
    {
        this.saveMode = saveMode;
        if (saveMode)
        {
            menuLabel.text = "Save Map";
            actionButtonLabel.text = "Save";
        }
        else
        {
            menuLabel.text = "Load Map";
            actionButtonLabel.text = "Load";
        }
        FillList();
        gameObject.SetActive(true);
       // HexMapCamera.Locked = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        //HexMapCamera.Locked = false;
    }

    string GetSelectedPath()
    {
        string mapName = nameInput.text;
        if (mapName.Length == 0)
        {
            return null;
        }
        return Path.Combine(Application.persistentDataPath, mapName + ".map");
    }

    void Save(string path)
    {

        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write(fileMapVersion);
            hexGrid.Save(writer);
        }
    }

    void Load(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist " + path);
            return;
        }

        PlayerPrefs.SetString("FilePath", path);
        PlayerPrefs.SetInt("PerformActions", 1);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }


    public void Delete()
    {
        string path = GetSelectedPath();
        if (path == null)
        {
            return;
        }
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        nameInput.text = "";
        FillList();
    }

    public void Action()
    {
        string path = GetSelectedPath();
        if (path == null)
        {
            return;
        }
        if (saveMode)
        {
            Save(path);
            Close();
        }
        else
        {
            StartCoroutine(LoadNextSceneAsync(path));
        }
        nameInput.text = "";

    }

    IEnumerator LoadNextSceneAsync(string path)
    {
        yield return new WaitForSeconds(3.0f);
        Load(path);
        Close();
    }

    public void SelectItem(string name)
    {
        nameInput.text = name;
    }
}
