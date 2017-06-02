using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
public class CellScript : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [VisibleWhen("showSettings")] public RawImage image;              //the image used to represent this cell
    [VisibleWhen("showSettings")] public Color    colorNotInSolution; //color to be used if the cell is not in the solution
    [VisibleWhen("showSettings")] public Color    colorWire;          //color to be used if the cell has normal wire connections
    [VisibleWhen("showSettings")] public Color    colorData;          //color to be used if the cell has data wire connections
    [VisibleWhen("showSettings")] public Color    colorDefault;       //color to be used if none of the above colors apply
    [VisibleWhen("showSettings")] public float    deadZone;           //size of the areas in each corner of the cell to ignore mouse drags in because the intended connection would be ambiguous.

    //only show object settings in inspector when the game is not running
    private bool showSettings() { return Application.isPlaying == false; }

    //cell status
    public CellStatus status; //state of this cell

    //connections to neighboring cells
    [OnChanged("UconnectionUpdated")] public ConnectionType connectionUp;
    [OnChanged("RconnectionUpdated")] public ConnectionType connectionRight;
    [OnChanged("DconnectionUpdated")] public ConnectionType connectionDown;
    [OnChanged("LconnectionUpdated")] public ConnectionType connectionLeft;

    //convenience variables for the integer values used by Input.GetMouseButton.  Unity doesnt seem to provide them natively.
    private const int LEFT_MOUSE_BUTTON = 0;
    private const int RIGHT_MOUSE_BUTTON = 1;
    private const int MIDDLE_MOUSE_BUTTON = 2;

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
            Debug.LogWarning("TODO: this cell was not previously in the solution, so the solution area should expand.  This is not yet implemented.");
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

    /// <summary>
    /// updates the sprite to show the connections this cell has to its neighbors
    /// </summary>
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

    /// <summary>
    /// handles the onPointerEnter event.  If the mouse button is being held down, calls dragCrossedBorder to handle the user input
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(LEFT_MOUSE_BUTTON))
            dragCrossedBorder( new Vector2( (eventData.position.x - transform.position.x), (eventData.position.y - transform.position.y) ), true );
    }

    /// <summary>
    /// handles the onPointerExit event.  If the mouse button is being held down, calls dragCrossedBorder to handle the user input
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Input.GetMouseButton(LEFT_MOUSE_BUTTON))
            dragCrossedBorder( new Vector2( (eventData.position.x - transform.position.x), (eventData.position.y - transform.position.y) ), false );
    }

    /// <summary>
    /// called when a user is holding the left mouse button and drags across the border of this cell.  
    /// Identifies the connection the user wants to change and calls userAttemptedConnection appropriately
    /// </summary>
    /// <param name="eventPos">localPosition of where the mouse crossed the border</param>
    /// <param name="isEntering">true if the mouse entered this cell, and false if the mouse left this cell.</param>
    /// <seealso cref="userAttemptedConnection"/>
    private void dragCrossedBorder(Vector2 eventPos, bool isEntering)
    {
        //test which borders the mouse is near
        Rect rect = GetComponent<RectTransform>().rect;
        Vector2 halfSize = new Vector2( rect.width/2.0f, rect.height/2.0f );
        bool nearLeft   = eventPos.x < (deadZone   - halfSize.x);
        bool nearRight  = eventPos.x > (halfSize.x - deadZone  );
        bool nearTop    = eventPos.y > (halfSize.y - deadZone  );
        bool nearBottom = eventPos.y < (deadZone   - halfSize.y);

        //count how many borders we are next to
        ushort bordersNearCursor = 0;
        if (nearLeft)   bordersNearCursor++;
        if (nearRight)  bordersNearCursor++;
        if (nearTop)    bordersNearCursor++;
        if (nearBottom) bordersNearCursor++;

        //if >= 3, something is wrong with this algorithm
        if (bordersNearCursor >= 3) throw new Exception("dragCrossedBorder thinks the mouse is near 3+ sides of the cell!  either deadZone is too large or the algorithm is broken."); 

        //if we are near two edges, this is a "dead zone" and we want to ignore this input as ambiguous
        if (bordersNearCursor == 2)
        {
            Debug.Log("Ambiguous");
            return;
        }

        //mouse is only near one border.  Update that connection
        if (nearLeft)
            userAttemptedConnection(ConnectionDirection.LEFT, ref connectionLeft);
        else if (nearRight)
            userAttemptedConnection(ConnectionDirection.RIGHT, ref connectionRight);
        else if (nearTop)
            userAttemptedConnection(ConnectionDirection.UP, ref connectionUp);
        else
            userAttemptedConnection(ConnectionDirection.DOWN, ref connectionDown);
    }

    /// <summary>
    /// responsible for handling user requests to update a connection.
    /// </summary>
    /// <param name="direction">direction of the connection to attempt updating</param>
    /// <param name="connection">reference to the connection to be updated</param>
    private void userAttemptedConnection(ConnectionDirection direction, ref ConnectionType connection)
    {
        switch (connection)
        {
            case ConnectionType.NONE:
                connection = ConnectionType.WIRE;
                connectionUpdated(direction, connection);
                break;

            case ConnectionType.WIRE:
            case ConnectionType.DATA:
                connection = ConnectionType.NONE;
                connectionUpdated(direction, connection);
                break;

            default:
                Debug.LogWarning("userAttemptedConnection doesnt know how to update a connection of this type!");
                break;
        }
    }
}
