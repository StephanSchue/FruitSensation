using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MAG.Game
{
    public class BoardManager : MonoBehaviour
    {
        public BoardProfile boardProfile;
        public Transform boardOrigin;

        private BoardTile[,] tiles;

        public void Awake()
        {
            CreateBoard(boardProfile);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                ClearBoard();
                CreateBoard(boardProfile);
            }
        }

        public void CreateBoard(BoardProfile profile)
        {
            tiles = new BoardTile[profile.size.x, profile.size.y];

            Vector3 startPosition = boardOrigin.position;

            for(int x = 0; x < profile.size.x; x++)
            {
                for(int y = 0; y < profile.size.y; y++)
                {
                    BoardTile prefab = profile.tilePack.GetTile();
                    BoardTile newTile = Instantiate(prefab, new Vector3(startPosition.x + (prefab.size.x * x), 
                                                                        startPosition.y + (prefab.size.y * y), 0), 
                                                                        prefab.transform.rotation, boardOrigin);
                    tiles[x, y] = newTile;
                }
            }
        }

        public void ClearBoard()
        {
            for(int x = 0; x < tiles.GetLength(0); x++)
            {
                for(int y = 0; y < tiles.GetLength(1); y++)
                {
                    Destroy(tiles[x, y]);
                }
            }
        }

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
                    Vector3 position = new Vector3(startPosition.x + (prefab.size.x * x),
                                                    startPosition.y + (prefab.size.y * y), 0);

                    DrawRect(position, prefab.size);
                }
            }
        }

        private void DrawRect(Vector3 position, Vector2 size)
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
        }

        #endif

        #endregion
    }
}