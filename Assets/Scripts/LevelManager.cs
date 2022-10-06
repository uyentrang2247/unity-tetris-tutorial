using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public int Level { get; private set; }
    private int MaxLevel = 15;

    Text levelText;

    // Use this for initialization
    void Awake()
    {
        levelText = GetComponent<Text>();
    }

    void Start()
    {
        Level = 1;
    }

    public void TryLevelUp(int totalLineClear)
    {
        if (Level == MaxLevel) return;

        if (totalLineClear > Level * 10)
        {
            Level++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        levelText.text = Level.ToString();
    }
}
