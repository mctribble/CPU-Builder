using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vexe.Runtime.Types;

public enum CellStatus
{
    NOT_IN_SOLUTION,
    EMPTY,
    WIRE,
    DATA,
    PART,
}

public enum ConnectionDirection
{
    UP,
    RIGHT,
    DOWN,
    LEFT
}

public enum ConnectionType
{
    NONE,
    WIRE,
    WIRE_PIN,
    DATA,
    DATA_PIN,
    BLOCKED,
}

/// <summary>
/// the building block of game levels, this class represents one cell in the level grid
/// </summary>
public class CellScript : BaseBehaviour
{
    [VisibleWhen("showSettings")] public RawImage image;              //the image used to represent this cell
    [VisibleWhen("showSettings")] public Color    colorNotInSolution; //color to be used if the cell is not in the solution
    [VisibleWhen("showSettings")] public Color    colorWire;          //color to be used if the cell has normal wire connections
    [VisibleWhen("showSettings")] public Color    colorData;          //color to be used if the cell has data wire connections
    [VisibleWhen("showSettings")] public Color    colorDefault;       //color to be used if none of the above colors apply

    //only show object settings in inspector when the game is not running
    private bool showSettings() { return Application.isPlaying == false; }

    //cell status
    public CellStatus status; //state of this cell

    //connections to neighboring cells
    [OnChanged("UconnectionUpdated")] public ConnectionType connectionUp;
    [OnChanged("RconnectionUpdated")] public ConnectionType connectionRight;
    [OnChanged("DconnectionUpdated")] public ConnectionType connectionDown;
    [OnChanged("LconnectionUpdated")] public ConnectionType connectionLeft;

    //helper functions to allow the VFW OnChanged attribute to call connectionUpdated()
    private void UconnectionUpdated(ConnectionType v) { connectionUpdated(ConnectionDirection.UP,    v); }
    private void RconnectionUpdated(ConnectionType v) { connectionUpdated(ConnectionDirection.RIGHT, v); }
    private void DconnectionUpdated(ConnectionType v) { connectionUpdated(ConnectionDirection.DOWN,  v); }
    private void LconnectionUpdated(ConnectionType v) { connectionUpdated(ConnectionDirection.LEFT,  v); }

    /// <summary>
    /// called when a connection is updated.
    /// </summary>
    /// <param name="updatedConnection">which connection was changed</param>
    /// <param name="newValue">what the connection was changed to</param>
    public void connectionUpdated(ConnectionDirection Direction, ConnectionType newValue)
    {
        //error if we now have both wire and data in the same cell
        if ((newValue == ConnectionType.WIRE && status == CellStatus.DATA) ||
            (newValue == ConnectionType.DATA && status == CellStatus.WIRE))
        {
            throw new InvalidOperationException("cannot make connection: conflicting wire types.");
        }

        //if this cell was previously not in the solution, expand the solution
        if (status == CellStatus.NOT_IN_SOLUTION)
        {
            //TODO: EXPAND SOLUTION
        }

        //if this cell previously had no connections, set its connection type to that of the new connection
        if (status == CellStatus.NOT_IN_SOLUTION || status == CellStatus.EMPTY)
        {
            switch (newValue)
            {
                case ConnectionType.WIRE:
                case ConnectionType.WIRE_PIN:
                    status = CellStatus.WIRE;
                    break;

                case ConnectionType.DATA:
                case ConnectionType.DATA_PIN:
                    status = CellStatus.DATA;
                    break;
            }
        }

        //update the sprite
        updateSprite();
    } 

    //updates the sprite to show the connections this cell has to its neighbors
    public void updateSprite()
    {
        switch (status)
        {
            case CellStatus.NOT_IN_SOLUTION:
                image.uvRect = new Rect(0.0f, 0.0f, 0.25f, 0.25f);
                image.color = colorNotInSolution;
                break;

            case CellStatus.WIRE:
            case CellStatus.DATA:
                //Find the UV coordinates of the target sprite
                float uvx = 0.0f; //column
                if (connectionUp   == ConnectionType.WIRE || connectionUp     == ConnectionType.DATA) { uvx += 0.5f;   } //all sprites that connect upwards are on the right half of the sheet
                if (connectionLeft == ConnectionType.WIRE || connectionLeft   == ConnectionType.DATA) { uvx += 0.25f;  } //if the sprite connects to the left, move over another column

                float uvy = 0.0f; //row
                if (connectionDown  == ConnectionType.WIRE || connectionDown  == ConnectionType.DATA) { uvy += 0.5f;  } //all sprites that connect downwards are on the top half of the sheet
                if (connectionRight == ConnectionType.WIRE || connectionRight == ConnectionType.DATA) { uvy += 0.25f; } //if the sprite connects to the right, move up another row

                //set the uvrect to show the sprite we need
                image.uvRect = new Rect(uvx, uvy, 0.25f, 0.25f);

                //set the color
                if (status == CellStatus.WIRE)
                    image.color = colorWire;
                else
                    image.color = colorData;

                break;
        }
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
