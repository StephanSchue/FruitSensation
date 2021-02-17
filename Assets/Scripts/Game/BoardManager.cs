using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MAG.General;
using UnityEngine.Events;
using static MAG.General.EventDefinitions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MAG.Game
{
    /// <summary>
    /// Managed the creation, moves and validation of the board
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        #region Settings/Variables

        // --- References ---
        [Header("Settings")]
        public bool diagonalOption = true;
        public bool expicitMatchs = true;

        [Header("Debug")]
        public bool debug = false;
        public Vector2Int debugCoordinates;

        // --- Variables ---
        private BoardProfile boardProfile;

        private BoardTile[,] tiles;
        private Vector2Int tilesDimesions = Vector2Int.zero;
        private int[] previousLeftAbove;

        private Vector2Int lastSelected = INVALID_COORDINATE;
        private Vector2 tileSize = new Vector2(1f, 1f);

        private float tweenMatchTileDelay = 0.25f;
        private float tweenRefillRowDelay = 0.25f;

        // --- Events ---
        public UnityEvent OnSelectTile { get; private set; }
        public UnityEvent OnDeselectTile { get; private set; }
        public UnityEvent OnSwapTile { get; private set; }
        public IntEvent OnMatch { get; private set; }
        public UnityEvent OnNoMatch { get; private set; }
        public UnityEvent OnRefill { get; private set; }
        public BoolEvent OnMoveFinished { get; private set; }

        // --- Properties ---
        public Transform boardOrigin { get; private set; }
        public Vector3 boardOriginPosition { get; private set; }
        public Transform boardExchange { get; private set; }

        private static Vector2Int INVALID_COORDINATE = new Vector2Int(-1, -1);

        private Vector2Int[] lastSwapePair = new Vector2Int[2];
        private bool lastSwaped = false;

        #endregion

        #region Init

        private void Awake()
        {
            OnSelectTile = new UnityEvent();
            OnDeselectTile = new UnityEvent();
            OnSwapTile = new UnityEvent();
            OnMatch = new IntEvent();
            OnNoMatch = new UnityEvent();
            OnRefill = new UnityEvent();
            OnMoveFinished = new BoolEvent();
        }

        #endregion

        #region Create Board

        public void InitializeBoard(SceneSettings sceneSettings)
        {
            this.boardProfile = sceneSettings.boardProfile;
            this.boardOrigin = sceneSettings.boardOrigin;
        }

        public void CreateBoard()
        {
            CreateBoard(this.boardProfile);
        }

        private void CreateBoard(BoardProfile profile)
        {
            // --- Set Board Variables ---
            BoardTile[] tilesetLibrary = profile.tilePack.boardTiles; // TileSet
            int tilesetLength = tilesetLibrary.Length;

            tiles = new BoardTile[profile.size.x, profile.size.y]; // board array2D
            tileSize = profile.tilePack.boardTiles[0].size;
            tilesDimesions = new Vector2Int(profile.size.x, profile.size.y);

            boardOriginPosition = boardOrigin.position +
                new Vector3(-tilesDimesions.x * (tileSize.x * 0.5f), tilesDimesions.y * (tileSize.y * 0.5f), 0f);

            // Variables for recognition
            int[] previousLeft = new int[profile.size.y];
            int previousAbove = -1;
            previousLeftAbove = new int[profile.size.y];

            tweenMatchTileDelay = profile.matchTileDelay;
            tweenRefillRowDelay = profile.refillRowDelay;

            // --- Board Loop ---
            for(int x = 0; x < profile.size.x; x++)
            {
                for(int y = 0; y < profile.size.y; y++)
                {
                    // -- Create Possible TileList --
                    List<int> possibleBoardTiles = new List<int>();

                    for(int i = 0; i < tilesetLength; i++)
                    {
                        if(i != previousAbove && i != previousLeft[y])
                        {
                            if(diagonalOption && x > 0 && y > 0)
                            {
                                bool digAbove = tiles[x - 1, y - 1].id != tilesetLibrary[i].id;

                                if(y < tilesDimesions.y-1)
                                {
                                    bool digBelow = tiles[x - 1, y + 1].id != tilesetLibrary[i].id;

                                    if(digAbove && digBelow)
                                        possibleBoardTiles.Add(i);
                                }
                                else if(digAbove)
                                {
                                    possibleBoardTiles.Add(i);
                                }
                            }
                            else
                            {
                                possibleBoardTiles.Add(i);
                            }
                        }
                    }

                    // -- Create Tileset --
                    int tileIndex = possibleBoardTiles[Random.Range(0, possibleBoardTiles.Count)];
                    BoardTile prefab = tilesetLibrary[tileIndex];
                    Vector3 position = new Vector3(boardOriginPosition.x + (prefab.size.x * 0.5f) + (prefab.size.x * x),
                                                    boardOriginPosition.y - (prefab.size.y * 0.5f) - (prefab.size.y * y), 0);

                    BoardTile newTile = Instantiate(prefab, position, prefab.transform.rotation, boardOrigin);
                    tiles[x, y] = newTile;

                    // -- Remember last Tile --
                    previousLeft[y] = tileIndex;

                    if(y > 0)
                        previousLeftAbove[y-1] = tileIndex;

                    previousAbove = tileIndex;
                }
            }

            // --- Check Playability ---
            if(!CheckBoardPlayable())
                RecreateBoard();
        }

        public void RecreateBoard()
        {
            ClearBoard();
            CreateBoard(boardProfile);
        }

        public void ClearBoard()
        {
            for(int x = 0; x < tilesDimesions.x; x++)
            {
                for(int y = 0; y < tilesDimesions.y; y++)
                {
                    Destroy(tiles[x, y].gameObject);
                }
            }
        }

        #endregion

        #region PorcessInput

        public void ProccessInput(Vector3 mouseDownPosition)
        {
            if(FindTileAtPosition(mouseDownPosition, out Vector2Int coordinate))
                SelectTile(coordinate);
        }

        #endregion

        #region Find Tile

        private bool FindTileAtPosition(Vector3 position, out Vector2Int tileCoordinate)
        {
            tileCoordinate = new Vector2Int(-1, -1);
            Rect boardRect = new Rect(boardOriginPosition, GetWorldBoardSize());
            boardRect.position -= new Vector2(0f, boardRect.height);

            if(boardRect.Contains(position))
            {
                float distance = 100f;

                for(int x = 0; x < tilesDimesions.x; x++)
                {
                    for(int y = 0; y < tilesDimesions.y; y++)
                    {
                        if(tiles[x, y] == null)
                            continue;

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

        private bool SelectTile(Vector2Int coordinate)
        {
            lastSwaped = false; 

            if(tiles[coordinate.x, coordinate.y] == null)
                return false;

            if(tiles[coordinate.x, coordinate.y].IsMoving)
                return false;

            if(lastSelected.x >= 0)
            {
                if(lastSelected != coordinate)
                {
                    // --- If destinaton is selected ---
                    if(IsAdjacentNeighbour(lastSelected, coordinate))
                    {
                        // -- Swape Tile --
                        SwapeTile(coordinate, lastSelected, true);
                        lastSwapePair = new Vector2Int[] { coordinate, lastSelected };
                        lastSwaped = true;

                        // Deselect Tile
                        tiles[lastSelected.x, lastSelected.y].Deselect();

                        foreach(Vector2Int adjacent in GetAdjacent(new Vector2Int(lastSelected.x, lastSelected.y)))
                            tiles[adjacent.x, adjacent.y].Deselect();
                        
                        lastSelected = INVALID_COORDINATE;

                        // Event
                        if(OnSwapTile != null)
                            OnSwapTile.Invoke();
                    }
                }
                else
                {
                    // --- Deselect Tile ---
                    tiles[lastSelected.x, lastSelected.y].Deselect();

                    foreach(Vector2Int adjacent in GetAdjacent(new Vector2Int(lastSelected.x, lastSelected.y)))
                        tiles[adjacent.x, adjacent.y].Deselect();

                    lastSelected = INVALID_COORDINATE;

                    // Event
                    if(OnDeselectTile != null)
                        OnDeselectTile.Invoke();
                }
            }
            else
            {
                // --- Select Tile ---
                tiles[coordinate.x, coordinate.y].Select();

                foreach(Vector2Int adjacent in GetAdjacent(new Vector2Int(coordinate.x, coordinate.y)))
                    tiles[adjacent.x, adjacent.y].Select();

                lastSelected = coordinate;

                // Event
                if(OnSelectTile != null)
                    OnSelectTile.Invoke();
            }

            debugCoordinates = lastSelected;

            return true;
        }

        #endregion

        #region SawpTile

        private void SwapeTile(Vector2Int tile01, Vector2Int tile02, bool counting)
        {
            BoardTile boardTile01 = tiles[tile01.x, tile01.y];
            BoardTile boardTile02 = tiles[tile02.x, tile02.y];

            tiles[tile01.x, tile01.y] = boardTile02;
            tiles[tile02.x, tile02.y] = boardTile01;

            Vector3 boardTile01Pos = boardTile01.Position;
            Vector3 boardTile02Pos = boardTile02.Position;
            
            boardTile02.SetPosition(boardTile01Pos);
            boardTile01.SetPosition(boardTile02Pos, () => OnSwapeTileComplete(tile02, counting));
        }

        private void OnSwapeTileComplete(Vector2Int checkCoordinate, bool counting)
        {
            if(!counting)
                return;

            bool effectiveMove = false;

            if(ValidateBoardSimulation() > 0)
            {
                StartCoroutine(ValidateBoard(() =>
                {
                    effectiveMove = true;

                    if(!CheckBoardPlayable())
                        RecreateBoard();

                    if(OnMoveFinished != null)
                        OnMoveFinished.Invoke(effectiveMove);
                }));
            }
            else if(lastSwaped)
            {
                lastSwaped = false;

                if(expicitMatchs)
                    SwapeTile(lastSwapePair[0], lastSwapePair[1], false); // Swape Back
                else
                    effectiveMove = true;

                if(OnMoveFinished != null)
                    OnMoveFinished.Invoke(effectiveMove);
            }
        }

        #endregion

        #region Neigbour Checks

        private List<Vector2Int> GetAdjacent(Vector2Int coordinate)
        {
            List<Vector2Int> adjacentTiles = new List<Vector2Int>();
            float xLength = boardProfile.size.x; // tilesDimesions.x
            float yLength = boardProfile.size.y; // tilesDimesions.y

            // --- Select Adjacent Tiles ---

            // -- Above --
            if(coordinate.y - 1 > -1 && tiles[coordinate.x, coordinate.y - 1] != null) // Above
                adjacentTiles.Add(new Vector2Int(coordinate.x, coordinate.y - 1));

            if(diagonalOption)
            {
                if(coordinate.x - 1 > -1 && coordinate.y - 1 > -1) // Above/Left
                    adjacentTiles.Add(new Vector2Int(coordinate.x - 1, coordinate.y - 1));

                if(coordinate.x + 1 < xLength && coordinate.y - 1 > -1) // Above/Right
                    adjacentTiles.Add(new Vector2Int(coordinate.x + 1, coordinate.y - 1));
            }

            // -- Right --
            if(coordinate.x + 1 < xLength && tiles[coordinate.x + 1, coordinate.y] != null) // Right
                adjacentTiles.Add(new Vector2Int(coordinate.x + 1, coordinate.y));

            // -- Below --
            if(coordinate.y + 1 < yLength && tiles[coordinate.x, coordinate.y + 1] != null) // Below
                adjacentTiles.Add(new Vector2Int(coordinate.x, coordinate.y + 1));

            if(diagonalOption)
            {
                if(coordinate.x + 1 < xLength && coordinate.y + 1 < yLength) // Below/Right
                    adjacentTiles.Add(new Vector2Int(coordinate.x + 1, coordinate.y + 1));

                if(coordinate.x - 1 > -1 && coordinate.y + 1 < yLength) // Below/Left
                    adjacentTiles.Add(new Vector2Int(coordinate.x - 1, coordinate.y + 1));
            }

            // -- Left --
            if(coordinate.x - 1 > -1 && tiles[coordinate.x - 1, coordinate.y] != null) // Left
                adjacentTiles.Add(new Vector2Int(coordinate.x - 1, coordinate.y));

            return adjacentTiles;
        }

        private bool IsAdjacentNeighbour(Vector2Int originTile, Vector2Int newPositionTile)
        {
            return GetAdjacent(originTile).Contains(newPositionTile);
        }

        #endregion

        #region Validate Board

        private IEnumerator ValidateBoard(UnityAction callback)
        {
            // Check for Matches
            int removedTileCount = 0;

            for(int x = 0; x < tilesDimesions.x; x++)
            {
                for(int y = 0; y < tilesDimesions.y; y++)
                {
                    Vector2Int coordinate = new Vector2Int(x, y);

                    if(ValidateMatch(coordinate, out List<Vector2Int> matchList))
                    {
                        float duration = 0f;

                        // --- Clean Matches ---
                        for(int i = 0; i < matchList.Count; i++)
                        {
                            duration = tiles[matchList[i].x, matchList[i].y].Despawn();
                            tiles[matchList[i].x, matchList[i].y] = null;
                            ++removedTileCount;
                        }

                        // Event
                        if(OnMatch != null)
                            OnMatch.Invoke(matchList.Count);

                        if(removedTileCount > 0)
                            yield return new WaitForSeconds(tweenMatchTileDelay);
                    }
                }
            }

            // --- Shifting/Refill ---
            if(removedTileCount > 0)
            {
                Vector3 startPosition = boardOriginPosition;

                for(int x = 0; x < tilesDimesions.x; x++)
                {
                    for(int y = tilesDimesions.y-1; y >= 0; y--)
                    {
                        if(tiles[x, y] == null)
                        {
                            bool found = false;

                            for(int y2 = y; y2 >= 0; y2--)
                            {
                                if(tiles[x, y2] != null)
                                {
                                    Vector3 position = new Vector3(startPosition.x + (tileSize.x * 0.5f) + (tileSize.x * x),
                                                    startPosition.y - (tileSize.y * 0.5f) - (tileSize.y * y), 0);

                                    tiles[x, y2].SetPosition(position);
                                    tiles[x, y] = tiles[x, y2];
                                    tiles[x, y2] = null;
                                    found = true;
                                    break;
                                }
                            }

                            // Refill
                            if(!found)
                            {
                                // -- Create Tileset --
                                BoardTile[] tilesetLibrary = boardProfile.tilePack.boardTiles; // TileSet
                                //int tileIndex = possibleBoardTiles[Random.Range(0, possibleBoardTiles.Count)];
                                BoardTile prefab = tilesetLibrary[Random.Range(0, tilesetLibrary.Length)];
                                Vector3 position = new Vector3(startPosition.x + (prefab.size.x * 0.5f) + (prefab.size.x * x),
                                                                startPosition.y - (prefab.size.y * 0.5f) - (prefab.size.y * y), 0);
                                
                                BoardTile newTile = Instantiate(prefab, position + new Vector3(0f, 10f), prefab.transform.rotation, boardOrigin);
                                tiles[x, y] = newTile;
                                tiles[x, y].SetPosition(position);
                            }
                        }
                    }

                    yield return new WaitForSeconds(tweenRefillRowDelay);
                }

                // Event
                if(OnRefill != null)
                    OnRefill.Invoke();

                // --- Revalidate Board ---
                StartCoroutine(ValidateBoard(callback));
            }
            else
            {
                if(OnNoMatch != null)
                    OnNoMatch.Invoke();

                callback?.Invoke();
            }
        }

        private int ValidateBoardSimulation()
        {
            int removedTileCount = 0;

            for(int x = 0; x < tilesDimesions.x; x++)
            {
                for(int y = 0; y < tilesDimesions.y; y++)
                {
                    Vector2Int coordinate = new Vector2Int(x, y);

                    if(ValidateMatch(coordinate, out List<Vector2Int> matchList))
                    {
                        // --- Draw Line ---
                        //#if UNITY_EDITOR
                        //if(debug)
                        //{
                        //    for(int i = 1; i < matchList.Count; i++)
                        //    {
                        //        Debug.DrawLine(tiles[matchList[i - 1].x, matchList[i - 1].y].Position,
                        //            tiles[matchList[i].x, matchList[i].y].Position, Color.yellow, 5f);
                        //    }
                        //}
                        //#endif
                        
                        // --- Clean Matches ---
                        for(int i = 0; i < matchList.Count; i++)
                            ++removedTileCount;
                    }
                }
            }
            
            return removedTileCount;
        }

        private bool ValidateMatch(Vector2Int coordinate, out List<Vector2Int> matchList)
        {
            matchList = null;

            BoardTile boardTile = tiles[coordinate.x, coordinate.y];

            if(boardTile == null)
                return false;

            return CheckForMatch(coordinate, boardTile, out matchList);
        }

        private bool CheckForMatch(Vector2Int coordinate, BoardTile boardTile, out List<Vector2Int> matchList)
        {
            matchList = new List<Vector2Int>();
            List<Vector2Int> neighbours = GetAdjacent(coordinate);
            Vector2Int heading = Vector2Int.zero;

            foreach(Vector2Int neighbour in neighbours)
            {
                if(tiles[neighbour.x, neighbour.y] == null)
                    continue;

                if(tiles[neighbour.x, neighbour.y].id == boardTile.id)
                {
                    bool neighbourFound = true;
                    heading = neighbour - coordinate;
                    int matchCount = 2;

                    // Forward Check
                    Vector2Int lastPosition = neighbour;

                    while(neighbourFound)
                    {
                        Vector2Int newPosition = lastPosition + heading;

                        if(newPosition.x > -1 && newPosition.x < tilesDimesions.x &&
                            newPosition.y > -1 && newPosition.y < tilesDimesions.y)
                        {
                            if(tiles[newPosition.x, newPosition.y] == null)
                            {
                                neighbourFound = false;
                                continue;
                            }

                            // Found next Match
                            if(tiles[lastPosition.x, lastPosition.y].id == tiles[newPosition.x, newPosition.y].id)
                            {
                                lastPosition = newPosition;
                                matchList.Add(newPosition);
                                ++matchCount;
                                continue;
                            }
                        }

                        // Fallback if not match
                        neighbourFound = false;
                    }

                    if(matchCount > 2)
                    {
                        if(!matchList.Contains(neighbour))
                            matchList.Add(neighbour);

                        if(!matchList.Contains(coordinate))
                            matchList.Add(coordinate);
                    }
                }
            }

            return matchList.Count > 2;
        }

        #endregion

        #region Check Board Playable

        public bool CheckBoardPlayable()
        {
            int foundPlayable = 0;

            for(int x = 0; x < tilesDimesions.x; x++) // -3
            {
                for(int y = 0; y < tilesDimesions.y; y++) // 3
                {
                    int id = tiles[x, y].id;
                    Vector2Int coordiate = new Vector2Int(x, y);
                    List<Vector2Int> neigbours = GetAdjacent(new Vector2Int(x, y));

                    foreach(var neibour in neigbours)
                    {
                        foreach(var neibour2 in GetAdjacent(neibour))
                        {
                            if(CoordinateInBounds(neibour2) &&
                                id == tiles[neibour2.x, neibour2.y].id)
                            {
                                Vector2Int direction = neibour2 - neibour;
                                Vector2Int neibour3 = neibour2 + direction;

                                if(CoordinateInBounds(neibour3) &&
                                    id == tiles[neibour3.x, neibour3.y].id)
                                {
                                    ++foundPlayable;

                                    #if UNITY_EDITOR
                                    Debug.DrawRay(tiles[coordiate.x, coordiate.y].Position, new Vector3(0f, 0.5f, 0f), Color.blue, 1f);
                                    Debug.DrawLine(tiles[coordiate.x, coordiate.y].Position, tiles[neibour.x, neibour.y].Position, Color.yellow, 1f);
                                    Debug.DrawLine(tiles[neibour.x, neibour.y].Position, tiles[neibour2.x, neibour2.y].Position, Color.yellow, 1f);
                                    Debug.DrawLine(tiles[neibour2.x, neibour2.y].Position, tiles[neibour3.x, neibour3.y].Position, Color.yellow, 1f);
                                    #endif
                                }
                            }
                        }
                    }
                }
            }

            return foundPlayable > 0;
        }

        private bool CoordinateInBounds(Vector2Int coordiante)
        {
            return (coordiante.x > -1 && coordiante.x < tilesDimesions.x
                            && coordiante.y > -1 && coordiante.y < tilesDimesions.y);
        }

        #endregion

        #region Utils

        public Vector2 GetWorldBoardSize() => new Vector2(tileSize.x * tilesDimesions.x, tileSize.y * tilesDimesions.y);

        #endregion

        #region Gizmos

        #if UNITY_EDITOR

        SceneSettings sceneSettings = null;

        private void OnDrawGizmos()
        {
            if(!Application.isPlaying)
            {
                if(sceneSettings == null)
                {
                    sceneSettings = GameObject.FindGameObjectWithTag("SceneSettings")?.
                    GetComponent<SceneSettings>();
                }
                
                if(sceneSettings != null)
                {
                    boardProfile = sceneSettings.boardProfile;

                    if(boardProfile == null || boardProfile.tilePack == null)
                        return;

                    Vector2Int tilesDimesions = sceneSettings.boardProfile.size;
                    Vector2 tileSize = sceneSettings.boardProfile.tilePack.boardTiles[0].size;

                    boardOriginPosition = sceneSettings.boardOrigin.position +
                        new Vector3(-tilesDimesions.x * (tileSize.x * 0.5f), tilesDimesions.y * (tileSize.y * 0.5f), 0f);
                }
            }

            if(boardProfile == null)
                return;
            
            DrawBoard(boardOriginPosition, boardProfile);
        }

        private void DrawBoard(Vector3 originPosition, BoardProfile profile)
        {
            if(originPosition == null || profile.tilePack == null)
                return;

            Vector3 startPosition = originPosition;
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
            //Handles.Label(pos + new Vector3(-0.25f, 0.25f), string.Format("{0}:{1}", row, column));
        }

        #endif

        #endregion
    }
}