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
        public InputManager inputManager;

        public BoardProfile boardProfile;
        public Transform boardOrigin;

        private BoardTile[,] tiles;

        private Vector2Int lastSelected = INVALID_COORDINATE;
        private Vector2 tileSize = new Vector2(1f, 1f);

        private static Vector2Int INVALID_COORDINATE = new Vector2Int(-1, -1);

        public void Awake()
        {
            inputManager.OnMouseDown.AddListener(ProccessInput);
            
            CreateBoard(boardProfile);
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
                // Deselect Tile
                tiles[lastSelected.x, lastSelected.y].Deselect();

                // Swape Tile
                if(coordinate != lastSelected)
                    SwapeTile(coordinate, lastSelected);

                lastSelected = INVALID_COORDINATE;
            }
            else
            {
                // Select
                tiles[coordinate.x, coordinate.y].Select();
                lastSelected = coordinate;
            }

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

            boardTile02.SetPosition(boardTile01Pos);
            boardTile01.SetPosition(boardTile02Pos);
        }

        public Vector2 GetWorldBoardSize()
        {
            return new Vector2(tileSize.x * tiles.GetLength(0), tileSize.y * tiles.GetLength(1));
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