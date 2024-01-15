using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance;
    [SerializeField] private int boardSize;
    [SerializeField] private Tile tile;
    [SerializeField] private Piece piece;
    [SerializeField] private Transform camera;
    [SerializeField] private Sprite white_rook;
    [SerializeField] private Sprite black_rook;
    [SerializeField] private Sprite white_king;
    [SerializeField] private Sprite overlay_corner;
    [SerializeField] private Sprite overlay_throne;
    [SerializeField] private Tile[,] tiles;
    public Tile activeTile;
    public Tile[] highlightedTiles = new Tile[20];
    public Piece[] destroyedWhite = new Piece[20];
    public int destroyedWhiteCount = 0;
    public Piece[] destroyedBlack = new Piece[20];
    public int destroyedBlackCount = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShiftCameraToView();
        // SetupBoard();
    }

    void ShiftCameraToView()
    {
        camera.transform.position = new Vector3((float)boardSize / 2 - 0.5f, (float)boardSize / 2 - 0.5f, -10);
    }

    public void SetupBoard()
    {
        tiles = new Tile[boardSize, boardSize];
        int boardLength = boardSize - 1;
        int halfBoard = (int)(boardSize - 1) / 2;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                var generatedTile = Instantiate(tile, new Vector3(i, j, 0), Quaternion.identity);
                generatedTile.name = $"Tile [{i}, {j}]";
                generatedTile.coordinateI = i;
                generatedTile.coordinateJ = j;
                tiles[i, j] = generatedTile;
                SpriteRenderer tileSR = generatedTile.GetComponent<SpriteRenderer>();
                tileSR.color = Color.black;
                if ((i % 2 == 0 && j % 2 == 0) || (i % 2 != 0 && j % 2 != 0))
                {
                    tileSR.color = Color.white;
                }

                if (IsCorner(i, j, boardLength))
                {
                    var overlaySR = generatedTile.overlay.GetComponent<SpriteRenderer>();
                    overlaySR.sprite = overlay_corner;
                    var overlayTransform = generatedTile.transform.position;
                    generatedTile.overlay.transform.position = new Vector3(overlayTransform.x,
                        overlayTransform.y, overlayTransform.z - 1);
                    generatedTile.overlay.SetActive(true);
                    continue;
                }

                if (IsBlack(i, j, boardLength, halfBoard))
                {
                    var generatedPiece = Instantiate(piece, new Vector3(i, j, -2), Quaternion.identity);
                    generatedPiece.name = $"Black [{i}, {j}]";
                    generatedPiece.SetFaction(Faction.Black);
                    SpriteRenderer pieceSR = generatedPiece.GetComponent<SpriteRenderer>();
                    pieceSR.sprite = black_rook;
                    generatedTile.SetPiece(generatedPiece);
                    continue;
                }

                if (IsWhite(i, j, boardLength, halfBoard))
                {
                    if (IsCenter(i, j, halfBoard))
                    {
                        var overlaySR = generatedTile.overlay.GetComponent<SpriteRenderer>();
                        overlaySR.sprite = overlay_throne;
                        var overlayTransform = generatedTile.transform.position;
                        generatedTile.overlay.transform.position = new Vector3(overlayTransform.x,
                            overlayTransform.y, overlayTransform.z - 1);
                        generatedTile.overlay.SetActive(true);

                        var generatedPiece = Instantiate(piece, new Vector3(i, j, -2), Quaternion.identity);
                        generatedPiece.name = $"King";
                        generatedPiece.SetFaction(Faction.White);
                        generatedPiece.SetPieceType(PieceType.King);
                        var pieceSR = generatedPiece.GetComponent<SpriteRenderer>();
                        pieceSR.sprite = white_king;
                        generatedTile.SetPiece(generatedPiece);
                    }
                    else
                    {
                        var generatedPiece = Instantiate(piece, new Vector3(i, j, -2), Quaternion.identity);
                        generatedPiece.name = $"White [{i}, {j}]";
                        generatedPiece.SetFaction(Faction.White);
                        SpriteRenderer pieceSR = generatedPiece.GetComponent<SpriteRenderer>();
                        pieceSR.sprite = white_rook;
                        generatedTile.SetPiece(generatedPiece);
                    }
                }
            }
        }

        GameplayManager.Instance.UpdateGameplayState(GameplayState.TurnBlack);
    }

    public void CleanupBoard()
    {
        foreach (var tile in tiles)
        {
            if (tile.GetPiece()) Destroy(tile.GetPiece());
            Destroy(tile);
        }

        SetupBoard();
    }

    public bool IsBlack(int i, int j, int boardLength, int halfBoard)
    {
        return ((j == 0 || j == boardLength) && ((i > 2) && (i < boardLength - 2)))
               || ((i == 0 || i == boardLength) && ((j > 2) && (j < boardLength - 2)))
               || (j == halfBoard && (i == 1 || i == boardLength - 1))
               || (i == halfBoard && (j == 1 || j == boardLength - 1));
    }

    public bool IsWhite(int i, int j, int boardLength, int halfBoard)
    {
        return (i == halfBoard && j == halfBoard)
               || ((i == halfBoard - 1 || i == halfBoard + 1) && (j > 2) && (j < boardLength - 2))
               || ((j == halfBoard - 1 || j == halfBoard + 1) && (i > 2) && (i < boardLength - 2));
    }

    public bool IsCenter(int i, int j, int halfBoard)
    {
        return (i == halfBoard && j == halfBoard);
    }

    public bool IsCorner(Tile tile)
    {
        return IsCorner(tile.coordinateI, tile.coordinateJ, boardSize - 1);
    }

    public bool IsCorner(int i, int j, int boardLength)
    {
        return (i == 0 && j == 0)
               || (i == 0 && j == boardLength)
               || (i == boardLength && j == 0)
               || (i == boardLength && j == boardLength);
    }

    public int GetBoardSize()
    {
        return boardSize;
    }

    public Tile GetTileAtPosition(int i, int j)
    {
        return tiles[i, j];
    }

    public void ToggleAllHighlights(bool isActive)
    {
        if (activeTile != null)
        {
            activeTile.highlight.SetActive(isActive);
        }

        foreach (var hTile in highlightedTiles)
        {
            if (hTile != null)
            {
                hTile.highlight.SetActive(isActive);
            }
        }

        if (!isActive)
        {
            highlightedTiles = new Tile[20];
        }
    }
}