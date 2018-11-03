using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodControl
{
    class GamePiece
    {
        public static string[] PieceTypes =
        {
            "Left,Right",
            "Top,Bottom",
            "Left,Top",
            "Top,Right",
            "Right,Bottom",
            "Bottom,Left",
            "Empty"
        };
        public const int
            PieceHeight = 40,
            PieceWidth = 40,
            MaxPlayablePieceIndex = 5,
            EmptyPieceIndex = 6;

        private const int
            textureOffsetX = 1,
            textureOffsetY = 1,
            texturePaddingX = 1,
            texturePaddingY = 1;

        private string
            pieceType = "",
            pieceSuffix = "";

        public string PieceType { get { return pieceType; } }
        public string PieceSuffix { get { return pieceSuffix; } }
        public GamePiece(string type, string suffix)
        {
            pieceType = type;
            pieceSuffix = suffix;
        }
        public GamePiece(string type)
        {
            pieceType = type;
            pieceSuffix = "";
        }
        public void SetPiece(string type, string suffix)
        {
            pieceType = type;
            pieceSuffix = suffix;
        }
        public void SetPiece(string type)
        {
            SetPiece(type, "");
        }
        public void AddSuffix(string suffix)
        {
            if (!pieceSuffix.Contains(suffix))
                pieceSuffix += suffix;
        }
        public void RemoveSuffix(string suffix)
        {
            pieceSuffix = pieceSuffix.Replace(suffix, "");
        }
        public void RotatePiece(bool Clockwise)
        {
            switch (pieceType)
            {
                case "Top,Bottom":
                    pieceType = "Left,Right";
                    break;
                case "Left,Right":
                    pieceType = "Top,Bottom";
                    break;
                case "Left,Top":
                    if (Clockwise)
                        pieceType = "Top,Right";
                    else
                        pieceType = "Bottom,Left";
                    break;
                case "Top,Right":
                    if (Clockwise)
                        pieceType = "Right,Bottom";
                    else
                        pieceType = "Left,Top";
                    break;
                case "Right,Bottom":
                    if (Clockwise)
                        pieceType = "Bottom,Left";
                    else
                        pieceType = "Top,Right";
                    break;
                case "Bottom,Left":
                    if (Clockwise)
                        pieceType = "Left,Top";
                    else
                        pieceType = "Right,Bottom";
                    break;
                case "Empty":
                    break;
            }
        }
        public string[] GetOtherEnds(string startingEnd)
        {
            List<string> opposites = new List<string>();
            foreach(string end in pieceType.Split(','))
                if (end != startingEnd)
                    opposites.Add(end);
            return opposites.ToArray();
        }
        public bool HasConnector(string direction)
        {
            return pieceType.Contains(direction);
        }
        public Rectangle GetSouceRect()
        {
            int x = textureOffsetX,
                y = textureOffsetY;
            if (pieceSuffix.Contains("W"))
                x += PieceWidth + texturePaddingX;
            y += (Array.IndexOf(PieceTypes, pieceType) *
                (PieceHeight + texturePaddingY));
            return new Rectangle(x, y, PieceWidth, PieceHeight);
        }
    }
}