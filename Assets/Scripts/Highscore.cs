using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Highscore : MonoBehaviour {

    public static int highscore = 0;
    public static string highScoceFilename = "Highscore.txt";

    private void Awake()
    {
        ReadHighScore();
    }

    public static void Set(int score) {
        if (score > highscore) {
            highscore = score;
            WriteNewHighScore();
        }
    }

    public static void WriteNewHighScore()
    {
        StreamWriter streamWriter = new StreamWriter(Path.Combine(Application.streamingAssetsPath, highScoceFilename));

        try
        {
            streamWriter.Write(highscore);
            Debug.Log("HighScore " + highscore);
        }
        catch (System.Exception)
        {

            throw;
        }
        finally
        {
            streamWriter.Close();
        }
    }

    public static void ReadHighScore()
    {
        StreamReader streamReader = File.OpenText(Path.Combine(Application.streamingAssetsPath, highScoceFilename));

        try
        {
            highscore = int.Parse(streamReader.ReadLine());
            Debug.Log("HighScore " + highscore);
        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            streamReader.Close();
        }
    }

    public static string Get() {
        return System.String.Format("{0:D8}", highscore);
    }

}
