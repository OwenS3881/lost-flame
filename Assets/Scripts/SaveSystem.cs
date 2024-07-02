using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void Save(SavedDataManager sdm)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/SavedData.data";
        FileStream stream = new FileStream(path, FileMode.Create);

        SavedData data = new SavedData(sdm);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SavedData Load()
    {
        string path = Application.persistentDataPath + "/SavedData.data";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SavedData data = formatter.Deserialize(stream) as SavedData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    public static void Delete()
    {
        string path = Application.persistentDataPath + "/SavedData.data";
        File.Delete(path);
    }
}
