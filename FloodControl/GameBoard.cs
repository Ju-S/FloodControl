using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodControl
{
    class GameBoard
    {
        Random rand = new Random();
        public const int
            GameBoardWidth = 8,
            GameBoardHeight = 10;
        private GamePiece[,] boardSquare =
            new GamePiece[GameBoardWidth, GameBoardHeight];
        private List<Vector2> WaterTracker = new List<Vector2>();
        public Dictionary<string, FallingPiece> fallingPieces =
            new Dictionary<string, FallingPiece>();
        public Dictionary<string, RotatingPiece> rotatingPieces =
            new Dictionary<string, RotatingPiece>();
        public Dictionary<string, FadingPiece> fadingPieces =
            new Dictionary<string, FadingPiece>();

        public GameBoard()
        {
            ClearBoard();
        }
        public void ClearBoard()
        {
            for (int x = 0; x < GameBoardWidth; x++)
                for (int y = 0; y < GameBoardHeight; y++)
                    boardSquare[x, y] = new GamePiece("Empty");
        }
        public void RotatePiece(int x,int y, bool clockwise)
        {
            boardSquare[x, y].RotatePiece(clockwise);
        }
        public Rectangle GetSourceRect(int x, int Y)
        {
            return boardSquare[x, Y].GetSouceRect();
        }
        public string GetSquare(int x, int y)
        {
            return boardSquare[x, y].PieceType;
        }
        public void SetSquare(int x, int y, string pieceName)
        {
            boardSquare[x, y].SetPiece(pieceName);
        }
        public bool HasConnector(int x, int y, string direction)
        {
            return boardSquare[x, y].HasConnector(direction);
        }
        public void RandomPiece(int x, int y)
        {
            boardSquare[x, y].SetPiece(
                GamePiece.PieceTypes[rand.Next(
                    0, GamePiece.MaxPlayablePieceIndex + 1)]);
        }
        public void FillFromAbove(int x, int y)
        {
            int rowLookup = y - 1;
            while (rowLookup >= 0)
            {
                if (GetSquare(x, rowLookup) != "Empty")
                {
                    SetSquare(x, y, GetSquare(x, rowLookup));
                    SetSquare(x, rowLookup, "Empty");
                    AddFallingPiece(x, y, GetSquare(x, y),
                        GamePiece.PieceHeight * (y - rowLookup));
                    rowLookup = -1;
                }
                rowLookup--;
            }
        }
        public void GenerateNewPieces(bool dropSquares)
        {
            if (dropSquares)
                for(int x = 0; x < GameBoard.GameBoardWidth; x++)
                    for(int y = GameBoard.GameBoardHeight - 1; y >= 0; y--)
                        if (GetSquare(x, y) == "Empty")
                            FillFromAbove(x, y);
            for (int y = 0; y < GameBoard.GameBoardHeight; y++)
                for (int x = 0; x < GameBoard.GameBoardWidth; x++)
                    if (GetSquare(x, y) == "Empty")
                    {
                        AddFallingPiece(x, y, GetSquare(x, y),
                            GamePiece.PieceHeight * GameBoardHeight);
                        RandomPiece(x, y);
                    }
        }
        public void ResetWater()
        {
            for (int y = 0; y < GameBoard.GameBoardHeight; y++)
                for (int x = 0; x < GameBoard.GameBoardWidth; x++)
                    boardSquare[x, y].RemoveSuffix("W");
        }
        public void FillPiece(int x, int y)
        {
            boardSquare[x, y].AddSuffix("W");
        }
        public void PropagateWater(int x, int y, string fromDirection)
        {
            if ((y >= 0) && (y < GameBoardHeight) && 
                (x >= 0) && x < GameBoardWidth)
            {
                if(boardSquare[x,y].HasConnector(fromDirection)&&
                    !boardSquare[x, y].PieceSuffix.Contains("W"))
                {
                    FillPiece(x, y);
                    WaterTracker.Add(new Vector2(x, y));
                    foreach(string end in
                        boardSquare[x, y].GetOtherEnds(fromDirection))
                    {
                        switch(end)
                        {
                            case "Left":
                                PropagateWater(x - 1, y, "Right");
                                break;
                            case "Right":
                                PropagateWater(x + 1, y, "Left");
                                break;
                            case "Top":
                                PropagateWater(x, y - 1, "Bottom");
                                break;
                            case "Bottom":
                                PropagateWater(x, y + 1, "Top");
                                break;
                        }
                    }
                }
            }
        }
        public List<Vector2> GetWaterChain(int y)
        {
            WaterTracker.Clear();
            PropagateWater(0, y, "Left");
            return WaterTracker;
        }
        public void AddFallingPiece(int x, int y,
            string PieceName, int VerticalOffset)
        {
            fallingPieces[x.ToString() + "_" + y.ToString()] =
                new FallingPiece(PieceName, VerticalOffset);
        }
        public void AddRotatingPiece(int x, int y,
            string PieceName, bool clockwise)
        {
            rotatingPieces[x.ToString() + "_" + y.ToString()] =
                new RotatingPiece(PieceName, clockwise);
        }
        public void AddFadingPiece(int x, int y,
            string PieceName)
        {
            fadingPieces[x.ToString() + "_" + y.ToString()] =
                new FadingPiece(PieceName, "W");
        }
        public bool ArePiecesAnimating()
        {
            if ((fallingPieces.Count == 0) &&
                (rotatingPieces.Count == 0) &&
                (fadingPieces.Count == 0))
                return false;
            else
                return true;
        }
        private void UpdateFadingPieces()
        {
            Queue<string> RemoveKeys = new Queue<string>();

            foreach(string thisKey in fadingPieces.Keys)
            {
                fadingPieces[thisKey].UpdatePiece();

                if (fadingPieces[thisKey].alphaLevel == 0.0f)
                    RemoveKeys.Enqueue(thisKey.ToString());
            }
            while (RemoveKeys.Count > 0)
                fadingPieces.Remove(RemoveKeys.Dequeue());
        }
        private void UpdateFallingPieces()
        {
            Queue<string> RemoveKeys = new Queue<string>();

            foreach (string thisKey in fallingPieces.Keys)
            {
                fallingPieces[thisKey].UpdatePiece();

                if (fallingPieces[thisKey].VerticalOffset == 0)
                    RemoveKeys.Enqueue(thisKey.ToString());
            }
            while (RemoveKeys.Count > 0)
                fallingPieces.Remove(RemoveKeys.Dequeue());
        }
        private void UpdateRotationPieces()
        {
            Queue<string> RemoveKeys = new Queue<string>();

            foreach (string thisKey in rotatingPieces.Keys)
            {
                rotatingPieces[thisKey].UpdatePiece();

                if (rotatingPieces[thisKey].rotationTicksRemaining == 0)
                    RemoveKeys.Enqueue(thisKey.ToString());
            }
            while (RemoveKeys.Count > 0)
                rotatingPieces.Remove(RemoveKeys.Dequeue());
        }
        public void UpdateAnimatedPieces()
        {
            if(fadingPieces.Count == 0)
            {
                UpdateFallingPieces();
                UpdateRotationPieces();
            }
            else
            {
                UpdateFadingPieces();
            }
        }
    }
}