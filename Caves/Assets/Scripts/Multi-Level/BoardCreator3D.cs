using System;
using System.Collections;
using UnityEngine;

public class BoardCreator3D : MonoBehaviour
{
    /*Our walls for the dungeon are calculated as follows:
     *                      4
     *                  ---------
     *                  |        |
     *                  |        |
     *                2 |        | 8
     *                  |        |
     *                  |        |
     *                  ----------
     *                       1
     *  Addtional valuse will be intigrated in the next version to include the main enterence, stairs down.
     *  Adtional functions and variables will be introduced to create grunge on the floors and walls, and possibly some chests as well.
     *  Some cleanups are required to remove hard coding of offsets and eventually solve the offsets in a much neater method.
     * TODO: Create Entry Hall and stairs. Value for entry will be 64, 128, 256, 512
     * TODO: Create the ability to have multiple levels. Maybe 10 levels down max
     * TODO: Remove all commented debug statements.
     */

    public int columns = 100;                                 // The number of columns on the board (how wide it will be).
    public int rows = 100;                                    // The number of rows on the board (how tall it will be).
    public IntRange numRooms = new IntRange(10, 35);         // The range of the number of rooms there can be.
    public IntRange roomWidth = new IntRange(3, 15);         // The range of widths rooms can have.
    public IntRange roomHeight = new IntRange(3, 15);        // The range of heights rooms can have.
    public IntRange corridorLength = new IntRange(6, 30);    // The range of lengths corridors between rooms can have.
    public int Depth = 10;
    public GameObject Wall;
    public GameObject Floor;
    public GameObject Torch1;
    public GameObject Torch2;
    public GameObject Torch4;
    public GameObject Torch8;
    public GameObject Stairs;
    public GameObject EntryHall;
    public GameObject player;

   
    private TileType3D[][][] tiles;                               // A jagged array of tile types representing the board, like a grid.
    private int[][][] tiles3D;
    //private float[,] Torches3D;
    //private float[,] Walls3D;
    private Room3D[] rooms;                                     // All the rooms that are created for this board.
    private Corridor3D[] corridors;                             // All the corridors that connect the rooms.
    private GameObject[] boardHolder;                           // GameObject that acts as a container for all other tiles.

    // Float array to hold offsets for walls and torches from base axis of core room.
    private float[,] Walls3D = new float[4, 6] { { -5.47f, -2.94f, -2.88f, 0f, 0f, 0f }, { -2.87f, -2.94f, 5.47f, 0f, 90f, 0f }, { 5.49f, -2.94f, -2.88f, 0f, 0f, 0f }, { -2.87f, -2.94f, -5.55f, 0f, 90f, 0f } };
    private float[,] Torches3d = new float[4, 6] { { -5.18f, .332f, 0f, 0f, 180f, 0f }, { 0f, .332f, -5.259f, 0f, 90f, 0f }, { 5.519f, .332f, 0f, 0f, 0f, 0f }, { 0f, .332f, 5.22f, 0f, 270f, 0f } };
    private float[,] EntryRotation = new float[4, 6] { { 13.39f, 3.23f, -1.7f, 0f, 0f, 0f }, { -1.68f, 3.23f, -13.31f, 0f, 90f, 0f }, { -13.32f, 3.23f, 1.67f, 0f, 180f, 0f }, { 1.66f, 3.23f, 13.41f, 0f, 270f, 0f } };
    private float[,] DepthStairs = new float[4, 6] { { 13.41f, -2.58f, -1.66f, 0f, 0f, 0f }, { -1.67f, -2.6f, -13.43f, 0f, 90f, 0f }, { -13.4f, -2.61f, 1.69f, 0f, 180f, 0f }, { 1.67f, -2.6f, 13.43f, 0f, 270f, 0f } };


    private void Start()
    {
        // Create the board holder.
        boardHolder = new GameObject[10];
        for (int d = 0; d < Depth; d++)
        {
            boardHolder[d] = new GameObject("BoardHolder"+d);
        }
        if (Depth > 10)
        {
            Depth = 10;
        }

        SetupTilesArray();


        CreateRoomsAndCorridors();

        SetTilesValuesForRooms();
        SetTilesValuesForCorridors();
        
        CalculateWalls();
        
        Build3DCave();
    }

    void SetupTilesArray()
    {
        // Set the tiles jagged array to the correct width.
        tiles = new TileType3D[Depth][][];
        tiles3D = new int[Depth][][];

        for (int i=0; i  <tiles.Length; i++)
        {
            tiles[i] = new TileType3D[columns][];
            tiles3D[i] = new int[columns][];

            for(int j=0; j<tiles[i].Length; j++)
            {
                tiles[i][j] = new TileType3D[rows];
                tiles3D[i][j] = new int[rows];
            }
            
        }
        //Debug.Log("tiles.length " + tiles.Length);
        //Debug.Log("tiles[depth].length " + tiles[Depth-1].Length);
        //Debug.Log("tiles[depth][colums].length" + tiles[Depth-1][columns-1].Length);
        
        //SetTilesValuesForCorridors a default value to each cell to show empty.
        for (int i = 0; i < Depth; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                for (int k=0; k < tiles[i][j].Length; k++)
                {
                    tiles3D[i][j][k] = -1;
                }
               
            }
        }
        // Debug.Log("Tiles Array Done");
    }

    void CreateRoomsAndCorridors()
    {
        // Create the rooms array with a random size.
        int TotalRooms;
        int RoomPerLvl;
        int RoomTotalLoop;

        RoomTotalLoop = 0;

        RoomPerLvl = numRooms.Random;
        //TotalRooms = (RoomPerLvl * Depth) + 3 + (1+(2*(Depth-1)));
        TotalRooms = (RoomPerLvl * Depth) + 3;
        Debug.Log("Total Rooms: " + TotalRooms);
        Debug.Log("Total Room per LVL " + RoomPerLvl);

        rooms = new Room3D[TotalRooms];

        // There should be one less corridor than there is rooms.
        corridors = new Corridor3D[TotalRooms - 1];

        //Create the first room and corridor.
        rooms[0] = new Room3D();
        corridors[0] = new Corridor3D();

        //Create Entry Room
        rooms[0].CreateEntry(1, 1, columns, rows);
        corridors[0].BuildEntryCorridor(rooms[0], 5);
        RoomTotalLoop++;
        rooms[1] = new Room3D();
        corridors[1] = new Corridor3D();
        rooms[1].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[0],1,TileType3D.Floor);
        corridors[1].SetupCorridor(rooms[1], corridorLength, roomWidth, roomHeight, columns, rows, false,1);

        RoomTotalLoop++;
        rooms[2] = new Room3D();
        corridors[2] = new Corridor3D();
        rooms[2].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[1],1,TileType3D.Floor);
        corridors[2].SetupCorridor(rooms[2], corridorLength, roomWidth, roomHeight, columns, rows, false,1);

        // Setup the first room, there is no previous corridor so we do not use one.
        //rooms[0].SetupRoom(roomWidth, roomHeight, columns, rows);

        // Setup the first corridor using the first room.
        //corridors[0].SetupCorridor(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);
        for (int i = 0; i < RoomPerLvl; i++)
        {
            // Create a room.
            RoomTotalLoop++;
            rooms[RoomTotalLoop] = new Room3D();
            //Debug.Log("Room " + RoomTotalLoop);

            // Setup the room based on the previous corridor.
            rooms[RoomTotalLoop].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[RoomTotalLoop - 1], 1,TileType3D.Floor);

            // If we haven't reached the end of the corridors array...
            if (RoomTotalLoop < corridors.Length)
            {
                // ... create a corridor.
                corridors[RoomTotalLoop] = new Corridor3D();

                // Setup the corridor based on the room that was just created.
                corridors[RoomTotalLoop].SetupCorridor(rooms[RoomTotalLoop], corridorLength, roomWidth, roomHeight, columns, rows, false, 1);
            }
        }
        // Create an extra room on current level and next level down. Both will be classed as stair undef.

        for (int d = 1; d < Depth; d++)
        {

            for (int i = 0; i < RoomPerLvl; i++)
            {
                // Create a room.
                
                RoomTotalLoop++;
                rooms[RoomTotalLoop] = new Room3D();
                //Debug.Log("Depth " + d + " Room " + RoomTotalLoop);
                // Setup the room based on the previous corridor.
                rooms[RoomTotalLoop].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[RoomTotalLoop - 1],d+1,TileType3D.Floor);

                // If we haven't reached the end of the corridors array...
                if (RoomTotalLoop < corridors.Length)
                {
                    // ... create a corridor.
                    corridors[RoomTotalLoop] = new Corridor3D();

                    // Setup the corridor based on the room that was just created.
                    corridors[RoomTotalLoop].SetupCorridor(rooms[RoomTotalLoop], corridorLength, roomWidth, roomHeight, columns, rows, false,d+1);
                }
            }
            //Create same room at current level and next level for down stairs.

        }

    }

    void SetTilesValuesForRooms()
    {
        // Go through all the rooms...
        for (int i = 0; i < rooms.Length; i++)
        {
            Room3D currentRoom = rooms[i];
            //Debug.Log("Total Rooms " + rooms.Length + " Room # " + i);
            int roomDepth = currentRoom.Depth - 1;
            // ... and for each room go through it's width.
            for (int j = 0; j < currentRoom.roomWidth; j++)
            {
                int xCoord = currentRoom.xPos + j;

                // For each horizontal tile, go up vertically through the room's height.
                for (int k = 0; k < currentRoom.roomHeight; k++)
                {
                    int yCoord = currentRoom.yPos + k;
                    
                    // The coordinates in the jagged array are based on the room's position and it's width and height.
                    tiles[roomDepth][xCoord][yCoord] = currentRoom.FloorType;
                }
            }
        }
        //Redo Room[0] to ensure entry room not destroyed
        Room3D EntryRoom = rooms[0];
        //Debug.Log("Total Rooms " + rooms.Length + " Room # " + i);
        int EntryDepth = EntryRoom.Depth - 1;
        // ... and for each room go through it's width.
        for (int j = 0; j < EntryRoom.roomWidth; j++)
        {
            int xCoord = EntryRoom.xPos + j;

            // For each horizontal tile, go up vertically through the room's height.
            for (int k = 0; k < EntryRoom.roomHeight; k++)
            {
                int yCoord = EntryRoom.yPos + k;

                // The coordinates in the jagged array are based on the room's position and it's width and height.
                tiles[EntryDepth][xCoord][yCoord] = EntryRoom.FloorType;
            }
        }

        //Debug.Log("Rooms created in array.");
    }

    void SetTilesValuesForCorridors()
    {
        // Go through every corridor...
        for (int i = 0; i < corridors.Length; i++)
        {
            Corridor3D currentCorridor = corridors[i];

            // and go through it's length.
            for (int j = 0; j < currentCorridor.corridorLength; j++)
            {
                // Start the coordinates at the start of the corridor.
                int xCoord = currentCorridor.startXPos;
                int yCoord = currentCorridor.startYPos;
                int CorridDepth = currentCorridor.Depth - 1;

                // Depending on the direction, add or subtract from the appropriate
                // coordinate based on how far through the length the loop is.
                switch (currentCorridor.direction)
                {
                    case Direction.North:
                        yCoord += j;
                        break;
                    case Direction.East:
                        xCoord += j;
                        break;
                    case Direction.South:
                        yCoord -= j;
                        break;
                    case Direction.West:
                        xCoord -= j;
                        break;
                }

                // Set the tile at these coordinates to Floor.
                tiles[CorridDepth][xCoord][yCoord] = currentCorridor.FloorType;
            }
        }
        //Redo Room[0] to ensure entry room not destroyed
        Room3D EntryRoom = rooms[0];
        //Debug.Log("Total Rooms " + rooms.Length + " Room # " + i);
        int EntryDepth = EntryRoom.Depth - 1;
        // ... and for each room go through it's width.
        for (int j = 0; j < EntryRoom.roomWidth; j++)
        {
            int xCoord = EntryRoom.xPos + j;

            // For each horizontal tile, go up vertically through the room's height.
            for (int k = 0; k < EntryRoom.roomHeight; k++)
            {
                int yCoord = EntryRoom.yPos + k;

                // The coordinates in the jagged array are based on the room's position and it's width and height.
                tiles[EntryDepth][xCoord][yCoord] = EntryRoom.FloorType;
            }
        }

    }

    void CalculateWalls()
    {
        //This is our wall holder.
        int Walls;

        //Loop through array and set a value of 255 to indicate a room.
        for (int d = 0; d < Depth; d++)
        {
            for (int i = 0; i < tiles[d].Length; i++)
            {
                for (int j = 0; j < tiles[d][i].Length; j++)
                {
                    /*if (tiles[i][j] == TileType.Floor)
                    {
                        tiles3D[i][j] = 255;
                    }*/

                    switch (tiles[d][i][j])
                    {
                        case TileType3D.Floor:
                            tiles3D[d][i][j] = 255;
                            break;
                        case TileType3D.Entry1:
                            tiles3D[d][i][j] = 512;
                            break;
                        case TileType3D.Entry2:
                            tiles3D[d][i][j] = 128;
                            break;
                        case TileType3D.Entry4:
                            tiles3D[d][i][j] = 256;
                            break;
                        case TileType3D.Entry8:
                            tiles3D[d][i][j] = 64;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        //loop through the array to determine what walls are needed
        for (int d = 0; d < Depth; d++)
        {
            for (int i = 0; i < tiles[d].Length; i++)
            {
                for (int j = 0; j < tiles[d][i].Length; j++)
                {
                    if (tiles3D[d][i][j] == 255)
                    {
                        //Function to check the tile 1 square away for each core direction.
                        Walls = CheckForWalls(d,i, j);
                        //assign the results to the array
                        tiles3D[d][i][j] = Walls;
                        //Debug.Log("Wall " + Walls + "placed at (" + d + "," + i + "," + j + ")");
                    }
                }
            }
        }
    }

    void Build3DCave()
    {
        float xpos;
        float ypos;
        float zpos;
        float xoffset;
        float zoffset;
        float yoffset;
        int Walls;

        xpos = 0f;
        ypos = 0f;
        zpos = 0f;
        Walls = 0;
        //this is the offset to move the base room prefab over 1 colum
        //and up 1 row. Y offset is to create a second level of the dungeon.
        xoffset = 11.54f;
        zoffset = 11.62f;
        yoffset = -5.78f;


        //loop through the jagged array
        for (int d = 0; d < Depth; d++)
        {
            //Debug.Log("Doing Depth " + d);
            for (int i = 0; i < columns; i++)
            {
                zpos = 0f;
                //Debug.Log("Doing Column " + i);
                for (int j = 0; j < rows; j++)
                {
                    //Debug.Log("Doing location (" + d + "," + i + "," + j + ")");
                    //check to see if the cell is the default empty or not
                    if (tiles3D[d][i][j] != -1)
                    {
                        //cell contains a wall or room get the value
                        Walls = tiles3D[d][i][j];
                        //0 is an empty room with no wall, 1 to 15 indicates a wall.                  
                        if (Walls == 0)
                        {
                            PlaceRoom(boardHolder[d], xpos, ypos, zpos);
                        }
                        if (Walls > 0)
                        {
                            // do bitwise checks to see what wall needs to be placed
                            if ((Walls & 1) != 0)
                            {
                                //call the wall and torch functions for wall 1
                                PlaceWalls3D(boardHolder[d], xpos, ypos, zpos, "S");
                                PlaceTorch(boardHolder[d], xpos, ypos, zpos, "S");
                                PlaceRoom(boardHolder[d], xpos, ypos, zpos);
                            }
                            if ((Walls & 8) != 0)
                            {
                                //call the wall and torch functions for wall 8
                                PlaceWalls3D(boardHolder[d], xpos, ypos, zpos, "W");
                                PlaceTorch(boardHolder[d], xpos, ypos, zpos, "W");
                                PlaceRoom(boardHolder[d], xpos, ypos, zpos);
                            }
                            if ((Walls & 2) != 0)
                            {
                                //call the wall and torch function for wall 2
                                PlaceWalls3D(boardHolder[d], xpos, ypos, zpos, "E");
                                PlaceTorch(boardHolder[d], xpos, ypos, zpos, "E");
                                PlaceRoom(boardHolder[d], xpos, ypos, zpos);
                            }
                            if ((Walls & 4) != 0)
                            {
                                //call the wall and torch function for wall 4
                                PlaceWalls3D(boardHolder[d], xpos, ypos, zpos, "N");
                                PlaceTorch(boardHolder[d], xpos, ypos, zpos, "N");
                                PlaceRoom(boardHolder[d], xpos, ypos, zpos);
                            }

                            if ((Walls & 64) != 0)
                            {
                                PlaceEntryRoom(boardHolder[d], xpos, ypos, zpos, "S");
                            }
                            if ((Walls & 128) != 0)
                            {
                                PlaceEntryRoom(boardHolder[d], xpos, ypos, zpos, "E");
                            }
                            if ((Walls & 256) != 0)
                            {
                                PlaceEntryRoom(boardHolder[d], xpos, ypos, zpos, "N");
                            }
                            if ((Walls & 512) != 0)
                            {
                                PlaceEntryRoom(boardHolder[d], xpos, ypos, zpos, "W");
                            }
                        }

                    }
                    zpos = zpos + zoffset;
                    Walls = 0;
                }
                xpos = xpos + xoffset;
            }
            ypos = ypos + yoffset;
            xpos = 0f;
            zpos = 0f;
        }
    }

    private void PlaceEntryRoom(GameObject board, float xpos, float ypos, float zpos, string walltype)
    {
        float wallx = 0f;
        float wally = 0f;
        float wallz = 0f;
        float xrotation = 0f;
        float yrotation = 0f;
        float zrotation = 0f;
        //Wall type will be passwed as a core cardinal direction, N, E, S, W.
        if (walltype.Equals("S") == true)
        {
            //for wall 1 lookup values from wall offset array.
            wallx = EntryRotation[0, 0];
            wally = EntryRotation[0, 1];
            wallz = EntryRotation[0, 2];
            xrotation = EntryRotation[0, 3];
            yrotation = EntryRotation[0, 4];
            zrotation = EntryRotation[0, 5];
            Debug.Log("Built S Entry.");
        }
        if (walltype.Equals("N") == true)
        {
            //for wall 4 lookup values from wall offset array
            wallx = EntryRotation[1, 0];
            wally = EntryRotation[1, 1];
            wallz = EntryRotation[1, 2];
            xrotation = EntryRotation[1, 3];
            yrotation = EntryRotation[1, 4];
            zrotation = EntryRotation[1, 5];
            Debug.Log("Built N Entry.");
        }
        if (walltype.Equals("W") == true)
        {
            //for wall 2 lookup values from wall offset array
            wallx = EntryRotation[0, 0];
            wally = EntryRotation[0, 1];
            wallz = EntryRotation[0, 2];
            xrotation = EntryRotation[0, 3];
            yrotation = EntryRotation[0, 4];
            zrotation = EntryRotation[0, 5];
            Debug.Log("Built W Entry.");
        }
        if (walltype.Equals("E") == true)
        {
            //for wall 8 lookup values from wall offset array
            wallx = EntryRotation[2, 0];
            wally = EntryRotation[2, 1];
            wallz = EntryRotation[2, 2];
            xrotation = EntryRotation[2, 3];
            yrotation = EntryRotation[2, 4];
            zrotation = EntryRotation[2, 5];
            Debug.Log("Built E Entry.");
        }
        //create a vector3 position from passed room core cordinate and offsets.
        Vector3 postion = new Vector3(xpos + wallx, ypos + wally, zpos + wallz);
        //create object
        GameObject tileobject = Instantiate(EntryHall, postion, Quaternion.identity) as GameObject;
        //rotate object if needed
        tileobject.transform.Rotate(xrotation, yrotation, zrotation);
        //assign the main gameboard as the wall parent.
        tileobject.transform.parent = board.transform;
        GameObject playerobject = GameObject.Find("DemoCharacter");
        playerobject.transform.position = postion;

    }

    //Function to place wall torches into the scene.
    //TODO: Create a randon chance to put a torch.
    private void PlaceTorch(GameObject board, float xpos, float ypos, float zpos, string walltype)
    {
        float wallx = 0f;
        float wally = 0f;
        float wallz = 0f;
        float xrotation = 0f;
        float yrotation = 0f;
        float zrotation = 0f;
        float TorchPlace = 0f;
        float TorchValue = 0f;
        float ValueLow, ValueHi;

        TorchValue = UnityEngine.Random.Range(0, 99);
        ValueHi = TorchValue + 15;
        ValueLow = TorchValue - 15;
        TorchPlace = UnityEngine.Random.Range(-15, 114);

        if ((TorchPlace > ValueLow) && (TorchPlace < ValueHi))
        {
            //Wall type will be passwed as a core cardinal direction, N, E, S, W.
            if (walltype.Equals("S") == true)
            {
                //Get Torch offsets
                wallx = Torches3d[0, 0];
                wally = Torches3d[0, 1];
                wallz = Torches3d[0, 2];
                xrotation = Torches3d[0, 3];
                yrotation = Torches3d[0, 4];
                zrotation = Torches3d[0, 5];
                //Debug.Log("Built S Torch.");
                //Build torch vector3 from base room cordinate and offsets.
                Vector3 postion = new Vector3(xpos + wallx, ypos + wally, zpos + wallz);
                //create game object
                GameObject tileobject = Instantiate(Torch1, postion, Quaternion.identity) as GameObject;
                //rotate the torch.
                tileobject.transform.Rotate(xrotation, yrotation, zrotation);
                //assign the torch to the main gameboard
                tileobject.transform.parent = board.transform;
            }
            if (walltype.Equals("N") == true)
            {
                //Get torch offsets
                wallx = Torches3d[2, 0];
                wally = Torches3d[2, 1];
                wallz = Torches3d[2, 2];
                xrotation = Torches3d[2, 3];
                yrotation = Torches3d[2, 4];
                zrotation = Torches3d[2, 5];
                //Debug.Log("Built S Torch.");
                //build torch vector3 based on passed cordinate and offsets
                Vector3 postion = new Vector3(xpos + wallx, ypos + wally, zpos + wallz);
                //create gameobject
                GameObject tileobject = Instantiate(Torch1, postion, Quaternion.identity) as GameObject;
                //rotate torch
                tileobject.transform.Rotate(xrotation, yrotation, zrotation);
                //assign the torch to the main gameboard
                tileobject.transform.parent = board.transform;
            }
            if (walltype.Equals("W") == true)
            {
                //get torch offsets
                wallx = Torches3d[3, 0];
                wally = Torches3d[3, 1];
                wallz = Torches3d[3, 2];
                xrotation = Torches3d[3, 3];
                yrotation = Torches3d[3, 4];
                zrotation = Torches3d[3, 5];
                //Debug.Log("Built W Torch.");
                //build vector3 based on passed cordinate and offesets
                Vector3 postion = new Vector3(xpos + wallx, ypos + wally, zpos + wallz);
                //creat game object
                GameObject tileobject = Instantiate(Torch1, postion, Quaternion.identity) as GameObject;
                //rotate torch
                tileobject.transform.Rotate(xrotation, yrotation, zrotation);
                //assign torch to main gameboard
                tileobject.transform.parent = board.transform;
            }
            if (walltype.Equals("E") == true)
            {
                //get torch offsets
                wallx = Torches3d[1, 0];
                wally = Torches3d[1, 1];
                wallz = Torches3d[1, 2];
                xrotation = Torches3d[1, 3];
                yrotation = Torches3d[1, 4];
                zrotation = Torches3d[1, 5];
                //Debug.Log("Built E Torch.");
                //create vector3 based on passed cordinate and offsets
                Vector3 postion = new Vector3(xpos + wallx, ypos + wally, zpos + wallz);
                //create gameobject
                GameObject tileobject = Instantiate(Torch1, postion, Quaternion.identity) as GameObject;
                //rotate torch
                tileobject.transform.Rotate(xrotation, yrotation, zrotation);
                //assign torch to main gameboard
                tileobject.transform.parent = board.transform;
            }
        }

    }

    private int CheckForWalls(int d, int i, int j)
    {
        int WallList;
        WallList = 0;

        if (i - 1 >= 0)//Check to see if we are in bounds.
        {
            if (tiles3D[d][i - 1][j] == -1)// if inbounds check to see if previous room is a wall or not
            {
                WallList = WallList + 1;// previous was a wall assign a wall.
            }
        }
        else
        {
            // we are at edge with a floor so assign a wall to keep things closed in.
            WallList = WallList + 1;
        }

        if (i + 1 < tiles[d].Length)//check to see if inbounds at max outer edge
        {
            if (tiles3D[d][i + 1][j] == -1)// if inbounds check next tile over if it is a wall.
            {
                WallList = WallList + 4;//next tile is a wall, assign a wall
            }
        }
        else
        {
            //we are at max outer edge with a floor, assign a outer wall.
            WallList = WallList + 4;
        }

        if (j - 1 >= 0)//Check to see if we are inbounds
        {
            if (tiles3D[d][i][j - 1] == -1)// if inbounds check room in the previous row for wall
            {
                WallList = WallList + 2;//previous row cell is a wall, assign the wall
            }
        }
        else
        {
            WallList = WallList + 2;// we are at edge with floor, assign a wall
        }

        if (j + 1 < tiles[d][i].Length)// check to see if inbounds at max outer edge
        {
            if (tiles3D[d][i][j + 1] == -1)//if inbounds check next row for wall
            {
                WallList = WallList + 8;//assign a wall
            }

        }
        else
        {
            // we are at outer edge with a floor, assign a wall
            WallList = WallList + 8;
        }

        //Debug.Log("Walls: " + WallList);
        //return the wall list.
        return WallList;

    }

    //function to place the wall.
    void PlaceWalls3D(GameObject board, float xpos, float ypos, float zpos, string walltype)
    {
        float wallx = 0f;
        float wally = 0f;
        float wallz = 0f;
        float xrotation = 0f;
        float yrotation = 0f;
        float zrotation = 0f;
        //Wall type will be passwed as a core cardinal direction, N, E, S, W.
        if (walltype.Equals("S") == true)
        {
            //for wall 1 lookup values from wall offset array.
            wallx = Walls3D[0, 0];
            wally = Walls3D[0, 1];
            wallz = Walls3D[0, 2];
            xrotation = Walls3D[0, 3];
            yrotation = Walls3D[0, 4];
            zrotation = Walls3D[0, 5];
            //Debug.Log("Built S Wall.");
        }
        if (walltype.Equals("N") == true)
        {
            //for wall 4 lookup values from wall offset array
            wallx = Walls3D[2, 0];
            wally = Walls3D[2, 1];
            wallz = Walls3D[2, 2];
            xrotation = Walls3D[2, 3];
            yrotation = Walls3D[2, 4];
            zrotation = Walls3D[2, 5];
            //Debug.Log("Built N Wall.");
        }
        if (walltype.Equals("W") == true)
        {
            //for wall 2 lookup values from wall offset array
            wallx = Walls3D[1, 0];
            wally = Walls3D[1, 1];
            wallz = Walls3D[1, 2];
            xrotation = Walls3D[1, 3];
            yrotation = Walls3D[1, 4];
            zrotation = Walls3D[1, 5];
            //Debug.Log("Built W Wall.");
        }
        if (walltype.Equals("E") == true)
        {
            //for wall 8 lookup values from wall offset array
            wallx = Walls3D[3, 0];
            wally = Walls3D[3, 1];
            wallz = Walls3D[3, 2];
            xrotation = Walls3D[3, 3];
            yrotation = Walls3D[3, 4];
            zrotation = Walls3D[3, 5];
            //Debug.Log("Built E Wall.");
        }
        //create a vector3 position from passed room core cordinate and offsets.
        Vector3 postion = new Vector3(xpos + wallx, ypos + wally, zpos + wallz);
        //create object
        GameObject tileobject = Instantiate(Wall, postion, Quaternion.identity) as GameObject;
        //rotate object if needed
        tileobject.transform.Rotate(xrotation, yrotation, zrotation);
        //assign the main gameboard as the wall parent.
        tileobject.transform.parent = board.transform;
    }

    public void PlaceRoom(GameObject Board, float xpos, float ypos, float zpos)
    {
        // Function to place the base room prefab at a particular x,y,z cordinate.
        Vector3 position = new Vector3(xpos, ypos, zpos);
        GameObject tileobject = Instantiate(Floor, position, Quaternion.identity) as GameObject;
        tileobject.transform.parent = Board.transform;

    }

}