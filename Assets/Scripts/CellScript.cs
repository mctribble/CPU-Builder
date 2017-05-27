using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vexe.Runtime.Types;

public class CellScript : BaseBehaviour
{
    public RawImage image;

    [OnChanged("connectionUpdated")] public bool connectedUp;
    [OnChanged("connectionUpdated")] public bool connectedRight;
    [OnChanged("connectionUpdated")] public bool connectedDown;
    [OnChanged("connectionUpdated")] public bool connectedLeft;

    public void connectionUpdated(bool foobar) { updateSprite(); }

    //updates the sprite to show the connections this cell has to its neighbors
    public void updateSprite()
    {
        //the sprite sheet is laid out based on the connections to other cells.  Find the UV coordinates of the target sprite

        //column
        float uvx = 0.0f;
        if (connectedUp)    { uvx += 0.5f;  } //all sprites that connect upwards are on the right half of the sheet
        if (connectedLeft)  { uvx += 0.25f; } //if the sprite connects to the left, move over another column

        //row
        float uvy = 0.0f;
        if (connectedDown)  { uvy += 0.5f;  } //all sprites that connect downwards are on the top half of the sheet
        if (connectedRight) { uvy += 0.25f; } //if the sprite connects to the right, move up another row

        //set the uvrect to show the sprite we need
        image.uvRect = new Rect(uvx, uvy, 0.25f, 0.25f);
    }

    //Use this for initialization
    void Start ()
    {
		
	}
	
	//Update is called once per frame
	void Update ()
    {
		
	}
}
