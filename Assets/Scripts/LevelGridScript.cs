using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

public class LevelGridScript : BaseBehaviour
{
    [Hide] public static LevelGridScript instance = null;

    [VisibleWhen("showSettings")] public GameObject cellPrefab;
    [VisibleWhen("showSettings")] public GameObject rowPrefab;
    [VisibleWhen("showSettings")][iSlider(6,30)] public int levelWidth;
    [VisibleWhen("showSettings")][iSlider(6,30)] public int levelHeight;

    //only show object settings in inspector when the game is not running
    private bool showSettings() { return Application.isPlaying == false; }

    public CellScript[,] grid;

    // Use this for initialization
    void Awake ()
    {
        if (instance == null)
            instance = this;
        else
            throw new System.Exception("two instances of singleton LevelGridScript!");

        grid = new CellScript[levelWidth, levelHeight];

        for (uint y = 0; y < levelWidth; y++)
        {
            GameObject row = Instantiate<GameObject>(rowPrefab, transform);
            for (uint x = 0; x < levelHeight; x++)
            {
                grid[x,y] = Instantiate<GameObject>(cellPrefab, row.transform).GetComponent<CellScript>();
                grid[x,y].gridCoordinates = new Vector2(x, y);
            }
        }
	}
}
