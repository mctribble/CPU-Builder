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
public class CellScript : BaseBehaviour, IPointerExitHandler
{
    [VisibleWhen("showSettings")] public RawImage image;              //the image used to represent this cell
    [VisibleWhen("showSettings")] public Color    colorNotInSolution; //color to be used if the cell is not in the solution
    [VisibleWhen("showSettings")] public Color    colorWire;          //color to be used if the cell has normal wire connections
    [VisibleWhen("showSettings")] public Color    colorData;          //color to be used if the cell has data wire connections
    [VisibleWhen("showSettings")] public Color    colorEmpty;         //color to be used if the cell is empty, but still in the solution
    [VisibleWhen("showSettings")] public Color    colorDefault;       //color to be used if none of the above colors apply
    [VisibleWhen("showSettings")] public float    deadZone;           //size of the areas in each corner of the cell to ignore mouse drags in because the intended connection would be ambiguous.

    //only show object settings in inspector when the game is not running
    private bool showSettings() { return Application.isPlaying == false; }

    //cell status
    [Show] public CellStatus status;                       //state of this cell
    [Hide] public int gridX = 0;                           //x-coordinate of this cell on the grid
    [Hide] public int gridY = 0;                           //y-coordinate of this cell on the grid

    //convenience accessor of this cell on the level grid so it can be treated as a vector and for cleaner display in the inspector
    [Show] public Vector2 gridCoordinates
    {
        get { return new Vector2(gridX, gridY); }
        set { gridX = (int)value.x; gridY = (int)value.y; }
    } 

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
            throw new InvalidOperationException("invalid connection made: conflicting wire types.");
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

        if ( (newValue == ConnectionType.NONE || newValue == ConnectionType.BLOCKED) &&               //if a connection was removed and
             (connectionUp == ConnectionType.NONE || connectionUp == ConnectionType.BLOCKED) &&       //if connectionUp    is empty and 
             (connectionRight == ConnectionType.NONE || connectionRight == ConnectionType.BLOCKED) && //if connectionRight is empty and 
             (connectionDown == ConnectionType.NONE || connectionDown == ConnectionType.BLOCKED) &&   //if connectionDown  is empty and 
             (connectionLeft == ConnectionType.NONE || connectionLeft == ConnectionType.BLOCKED))     //if connectionLeft  is empty  
        {
            status = CellStatus.EMPTY; //then the cell is now empty.  update status accordingly.
            //TODO: remove cells from the solution if the bounding box shrank as a result of this change
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

            case CellStatus.EMPTY:
                image.uvRect = new Rect(0.0f, 0.0f, 0.25f, 0.25f);
                image.color = colorEmpty;
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

            default:
                Debug.LogWarning("CellScript.updateSprite() doesnt know how to update this kind of cell.");
                break;
        }
    }

    /// <summary>
    /// handles the onPointerExit event.  If the mouse button is being held down, calls dragCrossedBorder to handle the user input
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Input.GetMouseButton(LEFT_MOUSE_BUTTON))
            dragCrossedBorder( new Vector2( (eventData.position.x - transform.position.x), (eventData.position.y - transform.position.y) ), false ); //if left button, user wants a normal wire
        else if (Input.GetMouseButton(RIGHT_MOUSE_BUTTON))
            dragCrossedBorder( new Vector2( (eventData.position.x - transform.position.x), (eventData.position.y - transform.position.y) ), true  ); //if right button, user wants a data wire
    }

    /// <summary>
    /// called when a user is holding the left mouse button and drags across the border of this cell.  
    /// Identifies the connection the user wants to change and calls userAttemptedConnection appropriately
    /// </summary>
    /// <param name="eventPos">localPosition of where the mouse crossed the border</param>
    /// <param name="wantsDataWire">Whether or not the user wants a data wire</param>
    /// <seealso cref="userAttemptedConnection"/>
    private void dragCrossedBorder(Vector2 eventPos, bool wantsDataWire)
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
            return;

        //mouse is only near one border.  Attempt to update that connection
        if (nearLeft)
            userAttemptedConnection(ConnectionDirection.LEFT,  ref connectionLeft,  wantsDataWire);
        else if (nearRight)
            userAttemptedConnection(ConnectionDirection.RIGHT, ref connectionRight, wantsDataWire);
        else if (nearTop)
            userAttemptedConnection(ConnectionDirection.UP,    ref connectionUp,    wantsDataWire);
        else
            userAttemptedConnection(ConnectionDirection.DOWN,  ref connectionDown,  wantsDataWire);
    }

    /// <summary>
    /// responsible for handling user requests to update a connection.
    /// </summary>
    /// <param name="direction">direction of the connection to attempt updating</param>
    /// <param name="connection">reference to the connection to be updated</param>
    /// <param name="wantsDataWire">Whether or not the user wants a data wire</param>
    private void userAttemptedConnection(ConnectionDirection direction, ref ConnectionType connection, bool wantsDataWire)
    {
        ConnectionType desiredConnectionType;

        //figure out what kind of connection this cell wants to make
        switch (connection)
        {

            case ConnectionType.BLOCKED:
                return; //ignore attempts to connect on blocked pins

            case ConnectionType.WIRE_PIN:
            case ConnectionType.DATA_PIN:
                return; //ignore attempts to modify pin connections, as this is handled by moving parts around

            //if this connection does not have a type, base it on what the user wants
            case ConnectionType.NONE:
                desiredConnectionType = wantsDataWire ? ConnectionType.DATA : ConnectionType.WIRE; //whether the user wants a WIRE connection or a DATA connection
                break;

            //only break normal wire connections if the user wants a normal wire
            case ConnectionType.WIRE:
                if (wantsDataWire)
                    return;
                else
                    desiredConnectionType = ConnectionType.NONE;

                break;

            //only break data wire connections if the user wants a data wire
            case ConnectionType.DATA:
                if (wantsDataWire)
                    desiredConnectionType = ConnectionType.NONE;
                else
                    return;

                break;

            default:
                Debug.LogWarning("userAttemptedConnection doesnt know how to update a connection of this type!" );
                return;
        }

        //reject if the cell status conflicts with the desired connection type
        switch (status)
        {
            //no conflict if cell is empty
            case CellStatus.EMPTY:
            case CellStatus.NOT_IN_SOLUTION:
                break; 

            //no wire connections if the cell carries data
            case CellStatus.DATA:
                if (desiredConnectionType == ConnectionType.WIRE || desiredConnectionType == ConnectionType.WIRE_PIN)
                    return; 
                break;

            //no data connections if the cell carries normal wires
            case CellStatus.WIRE:
                if (desiredConnectionType == ConnectionType.DATA || desiredConnectionType == ConnectionType.DATA_PIN)
                    return;
                break;

            default:
                Debug.LogWarning("userAttemptedConnection doesnt know how to update a cell of this type!");
                return;
        }

        //we know what kind of connection we want to make, and this cell allows it.  Try to get the target cell to update
        CellScript targetCell = findNeighbor(direction);

        if (targetCell == null)
            return; //the target cell does not exist, so we cannot connect to it

        //we have the target cell and the type of connection we want to make.  Attempt to establish it
        if (targetCell.incomingConnection(desiredConnectionType, direction.Inverse()))
        {
            //connection accepted.  update it here.
            connection = desiredConnectionType;
            connectionUpdated(direction, connection);
        }
        else
        {
            return; //connection refused.  
        }
    }

    /// <summary>
    /// call this if you want to connect something to this cell.  
    /// If the connection can be made, this cell is updated accordingly and true is returned.  
    /// If the connection is refused for any reason, nothing changes and this returns false.
    /// </summary>
    /// <param name="desiredConnectionType">connection type of the incoming connection</param>
    /// <param name="direction">direction the connection is coming from</param>
    /// <returns></returns>
    public bool incomingConnection(ConnectionType desiredConnectionType, ConnectionDirection direction)
    {
        //see if the connection should be rejected based on status of the cell
        switch (status)
        {
            //automatically accept connections if there is nothing in this cell
            case CellStatus.EMPTY:
            case CellStatus.NOT_IN_SOLUTION:
                break;

            //refuse data connections if this cell carries normal wires
            case CellStatus.WIRE:
                if (desiredConnectionType == ConnectionType.DATA || desiredConnectionType == ConnectionType.DATA_PIN)
                    return false;
                break;

            //refuse wire connections if this cell carries data
            case CellStatus.DATA:
                if (desiredConnectionType == ConnectionType.WIRE || desiredConnectionType == ConnectionType.WIRE_PIN)
                    return false;
                break;

            default:
                Debug.LogError("CellScript.incomingConnection() doesnt know how to handle requests of type " + desiredConnectionType);
                return false;
        }

        //status of the cell did not forbid the connection.  Find which connection should be updated and update it, unless that side is blocked
        switch(direction)
        {
            case ConnectionDirection.UP:
                if (connectionUp == ConnectionType.BLOCKED)
                    return false;
                connectionUp = desiredConnectionType;
                connectionUpdated(direction, connectionUp);
                return true;

            case ConnectionDirection.RIGHT:
                if (connectionRight == ConnectionType.BLOCKED)
                    return false;
                connectionRight = desiredConnectionType;
                connectionUpdated(direction, connectionRight);
                return true;

            case ConnectionDirection.LEFT:
                if (connectionLeft == ConnectionType.BLOCKED)
                    return false;
                connectionLeft = desiredConnectionType;
                connectionUpdated(direction, connectionLeft);
                return true;

            case ConnectionDirection.DOWN:
                if (connectionDown == ConnectionType.BLOCKED)
                    return false;
                connectionDown = desiredConnectionType;
                connectionUpdated(direction, connectionDown);
                return true;

            default: Debug.LogError("CellScript.incomingConnection() doesnt recognize direction " + direction); return false;
        }
    }

    /// <summary>
    /// finds the cell the given direction connects to, if any.  If there is no cell in that direction, returns null
    /// </summary>
    /// <param name="direction">the direction to search for a neighbor</param>
    /// <returns></returns>
    public CellScript findNeighbor(ConnectionDirection direction)
    {
        switch (direction)
        {
            case ConnectionDirection.UP:
                if (gridY <= 0)                                             //if there is no cell in this direction
                    return null;                                            //return null
                else                                                        //otherwise
                    return LevelGridScript.instance.grid[gridX, gridY - 1]; //return the cell in this direction

            case ConnectionDirection.RIGHT:
                if (gridX + 1 >= LevelGridScript.instance.levelWidth)       //if there is no cell in this direction
                    return null;                                            //return null
                else                                                        //otherwise
                    return LevelGridScript.instance.grid[gridX + 1, gridY]; //return the cell in this direction

            case ConnectionDirection.DOWN:
                if (gridY + 1 >= LevelGridScript.instance.levelHeight)      //if there is no cell in this direction
                    return null;                                            //return null
                else                                                        //otherwise
                    return LevelGridScript.instance.grid[gridX, gridY + 1]; //return the cell in this direction

            case ConnectionDirection.LEFT:
                if (gridX <= 0)                                             //if there is no cell in this direction
                    return null;                                            //return null
                else                                                        //otherwise
                    return LevelGridScript.instance.grid[gridX - 1, gridY]; //return the cell in this direction

            default:
                Debug.LogError("CellScript.findNeighbor() doesnt recognize the direction " + direction);
                return null;
        }
    }
}
