using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MAG.General;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MAG.Game
{
    public class BoardManager : MonoBehaviour
    {
        #region Settings/Variables

        // --- References ---
        [Header("References")]
        public InputManager inputManager;

        [Header("Settings")]
        public BoardProfile boardProfile;
        public Transform boardOrigin;
        public bool debug = false;

        // --- Variables ---
        private BoardTile[,] tiles;

        private Vector2Int lastSelected = INVALID_COORDINATE;
        private Vector2 tileSize = new Vector2(1f, 1f);

        public Vector2Int debugCoordinates;

        // --- Properties ---
        private static Vector2Int INVALID_COORDINATE = new Vector2Int(-1, -1); 

        #endregion

        private void Awake()
        {
            CreateBoard(boardProfile);
        }

        private void Start()
        {
            inputManager.OnMouseDown.AddListener(ProccessInput);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
                RecreateBoard();
            else if(Input.GetKeyDown(KeyCode.Escape))
                QuitGame();
        }
       
        #region Create Board

        public void CreateBoard(BoardProfile profile)
        {
            // --- Set Board Variables ---
            BoardTile[] tilesetLibrary = profile.tilePack.boardTiles; // TileSet
            int tilesetLength = tilesetLibrary.Length;

            tiles = new BoardTile[profile.size.x, profile.size.y]; // board array2D
            tileSize = profile.tilePack.boardTiles[0].size;

            Vector3 startPosition = boardOrigin.position;

            // Variables for recognition
            int[] previousLeft = new int[profile.size.y];
            int previousBelow = -1;

            // --- Board Loop ---
            for(int x = 0; x < profile.size.x; x++)
            {
                for(int y = 0; y < profile.size.y; y++)
                {
                    // -- Create Possible TileList --
                    List<int> possibleBoardTiles = new List<int>();

                    for(int i = 0; i < tilesetLength; i++)
                    {
                        if(i != previousBelow && i != previousLeft[y])
                            possibleBoardTiles.Add(i);
                    }

                    // -- Create Tileset --
                    int tileIndex = possibleBoardTiles[Random.Range(0, possibleBoardTiles.Count)];
                    BoardTile prefab = tilesetLibrary[tileIndex];
                    Vector3 position = new Vector3(startPosition.x + (prefab.size.x * 0.5f) + (prefab.size.x * x),
                                                    startPosition.y - (prefab.size.y * 0.5f) - (prefab.size.y * y), 0);

                    BoardTile newTile = Instantiate(prefab, position, prefab.transform.rotation, boardOrigin);
                    tiles[x, y] = newTile;

                    // -- Remember last Tile --
                    previousLeft[y] = tileIndex;
                    previousBelow = tileIndex;
                }
            }
        }

        public void RecreateBoard()
        {
            ClearBoard();
            CreateBoard(boardProfile);
        }

        public void ClearBoard()
        {
            for(int x = 0; x < tiles.GetLength(0); x++)
            {
                for(int y = 0; y < tiles.GetLength(1); y++)
                {
                    Destroy(tiles[x, y].gameObject);
                }
            }
        }

        #endregion

        #region PorcessInput

        private void ProccessInput(Vector3 mouseDownPosition)
        {
            if(FindTileAtPosition(mouseDownPosition, out Vector2Int coordinate))
                SelectTile(coordinate);
        }

        #endregion

        #region Find Tile

        public bool FindTileAtPosition(Vector3 position, out Vector2Int tileCoordinate)
        {
            tileCoordinate = new Vector2Int(-1, -1);
            Rect boardRect = new Rect(boardOrigin.position, GetWorldBoardSize());
            boardRect.position -= new Vector2(0f, boardRect.height);

            if(boardRect.Contains(position))
            {
                float distance = 100f;

                for(int x = 0; x < tiles.GetLength(0); x++)
                {
                    for(int y = 0; y < tiles.GetLength(1); y++)
                    {
                        float newDistance = Vector3.Distance(position, tiles[x, y].Position);

                        if(newDistance < distance)
                        {
                            tileCoordinate = new Vector2Int(x, y);
                            distance = newDistance;
                        }
                    }
                }
            }

            return tileCoordinate.x >= 0;
        }

        public bool SelectTile(Vector2Int coordinate)
        {
            if(tiles[coordinate.x, coordinate.y].IsMoving)
                return false;

            if(lastSelected.x >= 0)
            {
                // Swape Tile
                if(lastSelected != coordinate)
                {
                    if(IsAdjacentNeighbour(lastSelected, coordinate))
                    {
                        SwapeTile(coordinate, lastSelected);

                        // Deselect Tile
                        tiles[lastSelected.x, lastSelected.y].Deselect();

                        foreach(Vector2Int adjacent in GetAdjacent(new Vector2Int(lastSelected.x, lastSelected.y)))
                            tiles[adjacent.x, adjacent.y].Deselect();
                        
                        lastSelected = INVALID_COORDINATE;
                    }
                }
                else
                {
                    // Deselect Tile
                    tiles[lastSelected.x, lastSelected.y].Deselect();

                    foreach(Vector2Int adjacent in GetAdjacent(new Vector2Int(lastSelected.x, lastSelected.y)))
                        tiles[adjacent.x, adjacent.y].Deselect();

                    lastSelected = INVALID_COORDINATE;
                }
            }
            else
            {
                // Select
                tiles[coordinate.x, coordinate.y].Select();

                foreach(Vector2Int adjacent in GetAdjacent(new Vector2Int(coordinate.x, coordinate.y)))
                    tiles[adjacent.x, adjacent.y].Select();

                lastSelected = coordinate;
            }

            debugCoordinates = lastSelected;

            return true;
        }

        public void SwapeTile(Vector2Int tile01, Vector2Int tile02)
        {
            BoardTile boardTile01 = tiles[tile01.x, tile01.y];
            BoardTile boardTile02 = tiles[tile02.x, tile02.y];

            tiles[tile01.x, tile01.y] = boardTile02;
            tiles[tile02.x, tile02.y] = boardTile01;

            Vector3 boardTile01Pos = boardTile01.Position;
            Vector3 boardTile02Pos = boardTile02.Position;

            boardTile02.SetPosition(boardTile01Pos, () => OnSwapeTileComplete(tile01));
            boardTile01.SetPosition(boardTile02Pos, () => OnSwapeTileComplete(tile02));
        }

        private void OnSwapeTileComplete(Vector2Int checkCoordinate)
        {
            if(ValidateMatch(checkCoordinate, out List<Vector2Int> matchList))
            {
                for(int i = 0; i < matchList.Count; i++)
                    tiles[matchList[i].x, matchList[i].y].gameObject.SetActive(false);
            }
        }

        public Vector2 GetWorldBoardSize()
        {
            return new Vector2(tileSize.x * tiles.GetLength(0), tileSize.y * tiles.GetLength(1));
        }

        private List<Vector2Int> GetAdjacent(Vector2Int coordinate)
        {
            List<Vector2Int> adjacentTiles = new List<Vector2Int>();
            float xLength = boardProfile.size.x; // tiles.GetLength(0)
            float yLength = boardProfile.size.y; // tiles.GetLength(1)

            // --- Select Adjacent Tiles ---

            // -- Above --
            if(coordinate.y - 1 > -1) // Above
                adjacentTiles.Add(new Vector2Int(coordinate.x, coordinate.y - 1));

            if(coordinate.x - 1 > -1 && coordinate.y - 1 > -1) // Above/Left
                adjacentTiles.Add(new Vector2Int(coordinate.x - 1, coordinate.y - 1));

            if(coordinate.x + 1 < xLength && coordinate.y - 1 > -1) // Above/Right
                adjacentTiles.Add(new Vector2Int(coordinate.x + 1, coordinate.y - 1));

            // -- Right --
            if(coordinate.x + 1 < xLength) // Right
                adjacentTiles.Add(new Vector2Int(coordinate.x + 1, coordinate.y));

            // -- Below --
            if(coordinate.y + 1 < yLength) // Below
                adjacentTiles.Add(new Vector2Int(coordinate.x, coordinate.y + 1));

            if(coordinate.x + 1 < xLength && coordinate.y + 1 < yLength) // Below/Right
                adjacentTiles.Add(new Vector2Int(coordinate.x + 1, coordinate.y + 1));

            if(coordinate.x - 1 > -1 && coordinate.y + 1 < yLength) // Below/Left
                adjacentTiles.Add(new Vector2Int(coordinate.x - 1, coordinate.y + 1));

            // -- Left --
            if(coordinate.x - 1 > -1) // Left
                adjacentTiles.Add(new Vector2Int(coordinate.x - 1, coordinate.y));

            return adjacentTiles;
        }

        private bool IsAdjacentNeighbour(Vector2Int originTile, Vector2Int newPositionTile)
        {
            return GetAdjacent(originTile).Contains(newPositionTile);
        }

        private bool ValidateMatch(Vector2Int coordinate, out List<Vector2Int> matchList)
        {
            int id = tiles[coordinate.x, coordinate.y].id;
            matchList = new List<Vector2Int>();

            List<Vector2Int> neighbours = GetAdjacent(coordinate);

            foreach(Vector2Int neighbour in neighbours)
            {
                // Neighbour is Match
                if(tiles[neighbour.x, neighbour.y].id == id)
                {
                    //Check Row
                    int count = 2;
                    List<Vector2Int> objectList = new List<Vector2Int>() { coordinate, neighbour };

                    Vector2Int lastPosition = neighbour;
                    Vector2Int heading = neighbour - coordinate;
                    bool neighbourFound = true;

                    while(neighbourFound)
                    {
                        Vector2Int newPosition = lastPosition + heading;

                        if(newPosition.x > -1 && newPosition.x < tiles.GetLength(0) &&
                            newPosition.y > -1 && newPosition.y < tiles.GetLength(1))
                        {
                            if(tiles[lastPosition.x, lastPosition.y].id == tiles[newPosition.x, newPosition.y].id)
                            {
                                objectList.Add(newPosition);
                                ++count;

                                lastPosition = newPosition;
                            }
                            else
                            {
                                neighbourFound = false;
                            }
                        }
                        else
                        {
                            neighbourFound = false;
                        }
                    }

                    if(count > 2)
                    {
                        // --- Draw Line ---
                        for(int i = 1; i < objectList.Count; i++)
                        {
                            Debug.DrawLine(tiles[objectList[i - 1].x, objectList[i - 1].y].Position,
                                tiles[objectList[i].x, objectList[i].y].Position, Color.cyan, 5f);
                        }

                        // --- Destroy ---
                        for(int i = 0; i < objectList.Count; i++)
                        {
                            if(!matchList.Contains(objectList[i]))
                                matchList.Add(objectList[i]);
                        }
                    }
                }
            }

            return matchList.Count > 0;
        }

        #endregion

        #region Gizmos

        #if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if(boardProfile == null)
                return;
            
            DrawBoard(boardOrigin, boardProfile);
        }

        private void DrawBoard(Transform origin, BoardProfile profile)
        {
            if(profile.tilePack == null)
                return;

            Vector3 startPosition = origin.position;
            BoardTile prefab = profile.tilePack.boardTiles[0];

            for(int x = 0; x < profile.size.x; x++)
            {
                for(int y = 0; y < profile.size.y; y++)
                {
                    Vector3 position = new Vector3(startPosition.x + (prefab.size.x * 0.5f) + (prefab.size.x * x),
                                                    startPosition.y - (prefab.size.y * 0.5f) - (prefab.size.y * y), 0);

                    DrawRect(position, prefab.size, x, y);

                    if(debug)
                    {
                        if(tiles != null && tiles.GetLength(0) > 0 && tiles.GetLength(1) > 0)
                            ValidateMatch(new Vector2Int(x, y), out _);

                        // --- Debug Coordinates ---
                        if(debugCoordinates.x == x && debugCoordinates.y == y)
                        {
                            foreach(Vector2Int adjacentItem in GetAdjacent(new Vector2Int(x, y)))
                            {
                                Vector3 position2 = new Vector3(startPosition.x + (prefab.size.x * 0.5f) + (prefab.size.x * adjacentItem.x),
                                                        startPosition.y - (prefab.size.y * 0.5f) - (prefab.size.y * adjacentItem.y), 0);

                                DrawRect(position2, prefab.size, adjacentItem.x, adjacentItem.y);
                            }
                        }
                    }
                }
            }
        }

        private void DrawRect(Vector3 position, Vector2 size, int row, int column)
        {
            Vector3 pos = position;

            Vector3[] verts = new Vector3[]
            {
                new Vector3(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f, 0f),
                new Vector3(pos.x - size.x * 0.5f, pos.y + size.y * 0.5f, 0f),
                new Vector3(pos.x + size.x * 0.5f, pos.y + size.y * 0.5f, 0f),
                new Vector3(pos.x + size.x * 0.5f, pos.y - size.y * 0.5f, 0f)
            };

            Handles.DrawSolidRectangleWithOutline(verts, new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0, 0, 0, 1));
            //Handles.Label(pos, string.Format("{0}:{1}", row, column));
        }

        #endif

        #endregion

        #region Utils

        public void QuitGame()
        {
        // save any game data here
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
             Application.Quit();
        #endif
        }

        #endregion
    }
}