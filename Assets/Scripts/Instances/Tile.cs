using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Tile : MonoBehaviour
{
    [SerializeField] private Piece piece;
    public GameObject highlight;
    public GameObject overlay;
    public int coordinateI;
    public int coordinateJ;

    private void OnMouseEnter()
    {
        if (piece == null || GameplayManager.Instance.faction != piece.GetFaction() ||
            GameplayManager.Instance.gameplayState == GameplayState.DropPiece) return;
        CollectAvailablePositions();
        TileManager.Instance.ToggleAllHighlights(true);
    }

    private void OnMouseExit()
    {
        if (GameplayManager.Instance.gameplayState != GameplayState.DropPiece)
        {
            TileManager.Instance.ToggleAllHighlights(false);
        }
    }

    private bool MovePiece()
    {
        if (!TileManager.Instance.highlightedTiles.Contains(this)) return false;
        var activeTile = TileManager.Instance.activeTile;
        piece = activeTile.piece;
        var position = transform.position;
        piece.transform.position = new Vector3(position.x, position.y, -2);

        TileManager.Instance.ToggleAllHighlights(false);
        activeTile.piece = null;
        TileManager.Instance.activeTile = null;

        if (piece.GetPieceType() == PieceType.King && TileManager.Instance.IsCorner(this))
        {
            GameplayManager.Instance.UpdateGameplayState(GameplayState.BoardCleanup);
            Debug.Log($"white won");
        }

        return true;
    }

    private void OnMouseUpAsButton()
    {
        if (GameplayManager.Instance.gameplayState == GameplayState.DropPiece)
        {
            var faction = TileManager.Instance.activeTile.piece.GetFaction();
            if (!MovePiece()) return;
            if (GameplayManager.Instance.gameplayState == GameplayState.BoardCleanup) return;
            CheckCapture();
            GameplayManager.Instance.ToggleTurn(faction);
            return;
        }

        if (piece == null || !piece.GetFaction().Equals(GameplayManager.Instance.faction)) return;
        TileManager.Instance.activeTile = this;
        TileManager.Instance.ToggleAllHighlights(true);
        GameplayManager.Instance.UpdateGameplayState(GameplayState.DropPiece);
    }

    private void CheckCapture()
    {
        const bool showAttackPattern = false;
        if (CheckKingCapture(showAttackPattern))
        {
            GameplayManager.Instance.UpdateGameplayState(GameplayState.BoardCleanup);
            Debug.Log($"black won");
        }
        else
        {
            CheckSpecialCapture(showAttackPattern);
            CheckDefaultCapture(showAttackPattern);
        }
    }

    private bool CheckKingCapture(bool showAttackPattern)
    {
        if (GetPiece().GetFaction() == Faction.White) return false;
        var boardSize = TileManager.Instance.GetBoardSize();
        var didCapture = false;
        if (coordinateI > 1)
        {
            var l1 = TileManager.Instance.GetTileAtPosition(coordinateI - 1, coordinateJ);
            var l2 = TileManager.Instance.GetTileAtPosition(coordinateI - 2, coordinateJ);
            if (showAttackPattern)
            {
                l1.GetComponent<SpriteRenderer>().color = Color.yellow;
                l2.GetComponent<SpriteRenderer>().color = Color.green;
            }

            didCapture = didCapture || CheckKingDestroy(l1);
        }

        if (coordinateI < boardSize - 2)
        {
            var r1 = TileManager.Instance.GetTileAtPosition(coordinateI + 1, coordinateJ);
            var r2 = TileManager.Instance.GetTileAtPosition(coordinateI + 2, coordinateJ);
            if (showAttackPattern)
            {
                r1.GetComponent<SpriteRenderer>().color = Color.yellow;
                r2.GetComponent<SpriteRenderer>().color = Color.green;
            }

            didCapture = didCapture || CheckKingDestroy(r1);
        }

        if (coordinateJ > 1)
        {
            var b1 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ - 1);
            var b2 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ - 2);
            if (showAttackPattern)
            {
                b1.GetComponent<SpriteRenderer>().color = Color.yellow;
                b2.GetComponent<SpriteRenderer>().color = Color.green;
            }

            didCapture = didCapture || CheckKingDestroy(b1);
        }

        if (coordinateJ < boardSize - 2)
        {
            var t1 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ + 1);
            var t2 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ + 2);
            if (showAttackPattern)
            {
                t1.GetComponent<SpriteRenderer>().color = Color.yellow;
                t2.GetComponent<SpriteRenderer>().color = Color.green;
            }

            didCapture = didCapture || CheckKingDestroy(t1);
        }


        return didCapture;
    }

    private bool CheckKingDestroy(Tile enemyTile)
    {
        if (enemyTile.GetPiece() == null) return false;
        if (enemyTile.GetPiece().GetFaction() == Faction.Black) return false;
        if (enemyTile.GetPiece().GetPieceType() == PieceType.Rook) return false;

        var kingI = enemyTile.coordinateI;
        var kingJ = enemyTile.coordinateJ;
        var boardSize = TileManager.Instance.GetBoardSize();
        bool[] isKingSurrounded = { false, false, false, false };
        if (kingI != 0)
        {
            var l = TileManager.Instance.GetTileAtPosition(kingI - 1, kingJ);
            if (l.GetPiece() == null) return false;
            if (l.GetPiece().GetFaction() == Faction.White) return false;
            isKingSurrounded[0] = true;
        }

        if (kingJ != boardSize - 1)
        {
            var t = TileManager.Instance.GetTileAtPosition(kingI, kingJ + 1);
            if (t.GetPiece() == null) return false;
            if (t.GetPiece().GetFaction() == Faction.White) return false;
            isKingSurrounded[1] = true;
        }

        if (kingI != boardSize - 1)
        {
            var r = TileManager.Instance.GetTileAtPosition(kingI + 1, kingJ);
            if (r.GetPiece() == null) return false;
            if (r.GetPiece().GetFaction() == Faction.White) return false;
            isKingSurrounded[2] = true;
        }

        if (kingJ != 0)
        {
            var b = TileManager.Instance.GetTileAtPosition(kingI, kingJ - 1);
            if (b.GetPiece() == null) return false;
            if (b.GetPiece().GetFaction() == Faction.White) return false;
            isKingSurrounded[3] = true;
        }

        var isKingCaptured = Array.TrueForAll(isKingSurrounded, (side) => side);
        if (isKingCaptured) RemovePiece(enemyTile, Faction.Black);
        return isKingCaptured;
    }

    private bool CheckSpecialCapture(bool showAttackPattern)
    {
        var boardSize = TileManager.Instance.GetBoardSize();
        var didCapture = false;
        if (coordinateI == boardSize - 1 || coordinateI == 0)
        {
            if (coordinateJ == boardSize - 3)
            {
                var t1 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ + 1);
                var t2 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ + 2);
                if (showAttackPattern)
                {
                    t1.GetComponent<SpriteRenderer>().color = Color.cyan;
                    t2.GetComponent<SpriteRenderer>().color = Color.green;
                }

                didCapture = didCapture || CheckCornerDestroy(t1, t2, piece.GetFaction());
            }

            if (coordinateJ == 2)
            {
                var b1 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ - 1);
                var b2 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ - 2);
                if (showAttackPattern)
                {
                    b1.GetComponent<SpriteRenderer>().color = Color.yellow;
                    b2.GetComponent<SpriteRenderer>().color = Color.green;
                }

                didCapture = didCapture || CheckCornerDestroy(b1, b2, piece.GetFaction());
            }
        }

        if (coordinateJ == boardSize - 1 || coordinateJ == 0)
        {
            if (coordinateI == 2)
            {
                var l1 = TileManager.Instance.GetTileAtPosition(coordinateI - 1, coordinateJ);
                var l2 = TileManager.Instance.GetTileAtPosition(coordinateI - 2, coordinateJ);
                if (showAttackPattern)
                {
                    l1.GetComponent<SpriteRenderer>().color = Color.magenta;
                    l2.GetComponent<SpriteRenderer>().color = Color.green;
                }

                didCapture = didCapture || CheckCornerDestroy(l1, l2, piece.GetFaction());
            }

            if (coordinateI == boardSize - 3)
            {
                var r1 = TileManager.Instance.GetTileAtPosition(coordinateI + 1, coordinateJ);
                var r2 = TileManager.Instance.GetTileAtPosition(coordinateI + 2, coordinateJ);
                if (showAttackPattern)
                {
                    r1.GetComponent<SpriteRenderer>().color = Color.red;
                    r2.GetComponent<SpriteRenderer>().color = Color.green;
                }

                didCapture = didCapture || CheckCornerDestroy(r1, r2, piece.GetFaction());
            }
        }

        return didCapture;
    }

    private bool CheckCornerDestroy(Tile enemyTile, Tile allyTile, Faction faction)
    {
        if (!TileManager.Instance.IsCorner(allyTile)) return false;
        if (enemyTile.GetPiece() == null) return false;
        if (enemyTile.GetPiece().GetFaction() == faction) return false;
        if (enemyTile.GetPiece().GetPieceType() == PieceType.King) return false;
        return RemovePiece(enemyTile, faction);
    }

    private bool RemovePiece(Tile enemyTile, Faction faction)
    {
        var enemyPiece = enemyTile.piece;
        if (faction == Faction.Black)
        {
            TileManager.Instance.destroyedWhite[TileManager.Instance.destroyedWhiteCount++] = enemyPiece;
            enemyPiece.transform.position = new Vector3(-1, -1 + TileManager.Instance.destroyedWhiteCount);
        }
        else
        {
            TileManager.Instance.destroyedBlack[TileManager.Instance.destroyedBlackCount++] = enemyPiece;
            enemyPiece.transform.position = new Vector3(TileManager.Instance.GetBoardSize(),
                -1 + TileManager.Instance.destroyedBlackCount);
        }

        enemyTile.piece = null;
        return true;
    }

    private bool CheckDefaultCapture(bool showAttackPattern)
    {
        var boardSize = TileManager.Instance.GetBoardSize();
        var didCapture = false;
        if (coordinateI != boardSize - 2 && coordinateI != boardSize - 1)
        {
            if (coordinateJ < boardSize - 2)
            {
                var r1 = TileManager.Instance.GetTileAtPosition(coordinateI + 1, coordinateJ);
                var r2 = TileManager.Instance.GetTileAtPosition(coordinateI + 2, coordinateJ);
                if (showAttackPattern)
                {
                    r1.GetComponent<SpriteRenderer>().color = Color.red;
                    r2.GetComponent<SpriteRenderer>().color = Color.green;
                }

                didCapture = didCapture || CheckDestroy(r1, r2, piece.GetFaction());
            }

            if (coordinateJ > 2)
            {
                var b1 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ - 1);
                var b2 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ - 2);
                if (showAttackPattern)
                {
                    b1.GetComponent<SpriteRenderer>().color = Color.yellow;
                    b2.GetComponent<SpriteRenderer>().color = Color.green;
                }

                didCapture = didCapture || CheckDestroy(b1, b2, piece.GetFaction());
            }
        }

        if (coordinateI != 1 && coordinateI != 0)
        {
            var l1 = TileManager.Instance.GetTileAtPosition(coordinateI - 1, coordinateJ);
            var l2 = TileManager.Instance.GetTileAtPosition(coordinateI - 2, coordinateJ);
            if (showAttackPattern)
            {
                l1.GetComponent<SpriteRenderer>().color = Color.magenta;
                l2.GetComponent<SpriteRenderer>().color = Color.green;
            }

            didCapture = didCapture || CheckDestroy(l1, l2, piece.GetFaction());
        }

        if (coordinateJ != 1 && coordinateJ != 0)
        {
            var b1 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ - 1);
            var b2 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ - 2);
            if (showAttackPattern)
            {
                b1.GetComponent<SpriteRenderer>().color = Color.yellow;
                b2.GetComponent<SpriteRenderer>().color = Color.green;
            }

            didCapture = didCapture || CheckDestroy(b1, b2, piece.GetFaction());
        }

        if (coordinateJ != boardSize - 2 && coordinateJ != boardSize - 1)
        {
            var t1 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ + 1);
            var t2 = TileManager.Instance.GetTileAtPosition(coordinateI, coordinateJ + 2);
            if (showAttackPattern)
            {
                t1.GetComponent<SpriteRenderer>().color = Color.cyan;
                t2.GetComponent<SpriteRenderer>().color = Color.green;
            }

            didCapture = didCapture || CheckDestroy(t1, t2, piece.GetFaction());
        }

        return didCapture;
    }

    private bool CheckDestroy(Tile enemyTile, Tile allyTile, Faction faction)
    {
        if (allyTile.GetPiece() == null) return false;
        if (allyTile.GetPiece().GetFaction() != faction) return false;
        if (enemyTile.GetPiece() == null) return false;
        if (enemyTile.GetPiece().GetFaction() == faction) return false;
        if (enemyTile.GetPiece().GetPieceType() == PieceType.King) return false;
        return RemovePiece(enemyTile, faction);
    }

    public void SetPiece(Piece piece)
    {
        this.piece = piece;
    }

    public Piece GetPiece()
    {
        return piece;
    }

    public void CollectAvailablePositions()
    {
        var counter = 0;
        if (coordinateI != TileManager.Instance.GetBoardSize() - 1)
        {
            for (int i = coordinateI + 1; i <= TileManager.Instance.GetBoardSize() - 1; i++)
            {
                Tile tile = TileManager.Instance.GetTileAtPosition(i, coordinateJ);
                Piece tilePiece = tile.GetPiece();
                var isPieceRook = piece.GetPieceType() == PieceType.Rook;
                var isTileCorner = TileManager.Instance.IsCorner(tile);
                if (tilePiece != null || (isPieceRook && isTileCorner))
                {
                    break;
                }

                TileManager.Instance.highlightedTiles[counter] = tile;
                counter++;
            }
        }

        if (coordinateI != 0)
        {
            for (int i = coordinateI - 1; i >= 0; i--)
            {
                Tile tile = TileManager.Instance.GetTileAtPosition(i, coordinateJ);
                Piece tilePiece = tile.GetPiece();
                var isPieceRook = piece.GetPieceType() == PieceType.Rook;
                var isTileCorner = TileManager.Instance.IsCorner(tile);
                if (tilePiece != null || (isPieceRook && isTileCorner))
                {
                    break;
                }

                TileManager.Instance.highlightedTiles[counter] = tile;
                counter++;
            }
        }

        if (coordinateJ != TileManager.Instance.GetBoardSize() - 1)
        {
            for (int j = coordinateJ + 1; j <= TileManager.Instance.GetBoardSize() - 1; j++)
            {
                Tile tile = TileManager.Instance.GetTileAtPosition(coordinateI, j);
                Piece tilePiece = tile.GetPiece();
                var isPieceRook = piece.GetPieceType() == PieceType.Rook;
                var isTileCorner = TileManager.Instance.IsCorner(tile);
                if (tilePiece != null || (isPieceRook && isTileCorner))
                {
                    break;
                }

                TileManager.Instance.highlightedTiles[counter] = tile;
                counter++;
            }
        }

        if (coordinateJ != 0)
        {
            for (int j = coordinateJ - 1; j >= 0; j--)
            {
                Tile tile = TileManager.Instance.GetTileAtPosition(coordinateI, j);
                Piece tilePiece = tile.GetPiece();
                var isPieceRook = piece.GetPieceType() == PieceType.Rook;
                var isTileCorner = TileManager.Instance.IsCorner(tile);
                if (tilePiece != null || (isPieceRook && isTileCorner))
                {
                    break;
                }

                TileManager.Instance.highlightedTiles[counter] = tile;
                counter++;
            }
        }
    }
}