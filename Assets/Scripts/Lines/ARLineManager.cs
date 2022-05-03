using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ARApp.Lines
{
    public  static class ARLineManager
    {

        private const string PLAYERS_PREF_STRING = "AR_LINES";

        // public variables
        public static List<ARLine> lines = new List<ARLine>();

        //Public Methods
        public static void Add(ARLine pLine)
        {
            if (!lines.Contains(pLine))
            {
                lines.Add(pLine);
            }
        }

        public static void Save()
        {
            ARLineManifest manifest = new ARLineManifest();

            for(int i = 0; i < lines.Count; i++)
            {
                manifest.lines.Add(new ARLineData() {
                    //uid = lines[i].definition.uid,
                    tag = lines[i].definition.tag,
                    color = lines[i].definition.color,
                    visible = lines[i].gameObject.activeSelf,
                    //startPosition = lines[i].definition.startPosition,
                    //endPosition = lines[i].definition.endPosition
                }) ;
            }


            if (!Directory.Exists(Application.persistentDataPath + "/Scans"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Scans");
            }

            File.WriteAllText(Application.dataPath + "/Scans/Data.txt", JsonUtility.ToJson(manifest));
            _ShowAndroidToastMessage("Save Has Done Successfully.");
            SceneManager.LoadScene("MainMenu");
        }

        private static void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }

        /* public static void Load()
         {
             lines = new List<ARLine>();

             if (PlayerPrefs.HasKey(PLAYERS_PREF_STRING))
             {
                 // Get the JSON
                 string dataString = PlayerPrefs.GetString(PLAYERS_PREF_STRING);

                 // Parse the JSON
                 ARLineManifest manifest = JsonUtility.FromJson<ARLineManifest>(dataString);

                 // Set the Lines
                 for (int i = 0; i < manifest.lines.Count; i++)
                 {
                     ARLineData data = manifest.lines.Find(thingToFind => thingToFind.uid == lines[i].definition.uid);
                     if (data != null) {
                         //ARManager.

                     }
                 }

             }
         }*/


        // reset method? 



    }
}
