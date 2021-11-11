
using System;

namespace BoardModel2
{
    public class Board
    {
        // Normal board is 8x8 
        public int Size { get; set; }

        // 2d array of cells
        public Cell[,] TheGrid { get; set; }

        #region Public class Methods

        /// <summary>
        /// board constructor
        /// </summary>
        /// <param name="s"></param>
        public Board(int s)
        {
            Size = s;

            // new 2d array of cells to make up the grid 
            TheGrid = new Cell[Size, Size];

            // fill the grid with new cells with a unique x,y coordinate
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    TheGrid[i, j] = new Cell(i, j);
                }
            }
        }

        /// <summary>
        /// find next leagal move for a selected peice
        /// </summary>
        /// <param name="CurrentLocation"></param>
        /// <param name="peiceType"></param>
        public void MarkNextLegalMoves(Cell CurrentLocation, string peiceType, bool player1Turn)
        {
            // mark all possible move cells for the selected peice
            switch (peiceType.ToLower())
            {
                #region Knight
                case "knight":
                    int rowMoveTo;
                    int colMoveTo;

                    // 'L' shaped move of 2 spaces in any direction and then 1 space 90 degrees from the original move
                    // There are 2 possible moves in each quadrent

                    // step through all 4 quadrents using for loop
                    for (int move2Spaces = -2; move2Spaces <= 2; move2Spaces += 4)
                    {
                        for (int move1Space = -1; move1Space <= 1; move1Space += 2)
                        {
                            rowMoveTo = CurrentLocation.RowNumber + move2Spaces;
                            colMoveTo = CurrentLocation.ColumnNumber + move1Space;
                            // Move one validation 

                            // validate move in bounds
                            InBoundsValidation(rowMoveTo, colMoveTo, player1Turn);

                            // Move 2 validation (swap the move 1 and move 2 variables on the row and column)
                            rowMoveTo = CurrentLocation.RowNumber + move1Space;
                            colMoveTo = CurrentLocation.ColumnNumber + move2Spaces;

                            // validate move in bounds
                            InBoundsValidation(rowMoveTo, colMoveTo, player1Turn);
                        }
                    }
                    break;

                #endregion

                #region King
                case "king":
                    //move 1 space in any direction
                    //iterate through all moves
                    for (int row = -1; row <= 1; row++)
                    {
                        for (int col = -1; col <= 1; col++)
                        {
                            rowMoveTo = CurrentLocation.RowNumber + row;
                            colMoveTo = CurrentLocation.ColumnNumber + col;
                            //check if cell to mark is not the peice location
                            if (!(rowMoveTo == CurrentLocation.RowNumber
                                && colMoveTo == CurrentLocation.ColumnNumber))
                            {
                                // validate move in bounds
                                InBoundsValidation(rowMoveTo, colMoveTo, player1Turn);
                            }
                        }
                    }
                    break;
                #endregion

                #region Bishop / Rook
                case "bishop":
                    //diagonal run to the edge of the board in any direction
                    FullDiagRun(CurrentLocation, player1Turn);
                    break;

                case "rook":
                    // up/down or Left/right run to the edges of the board in any direction
                    FullVertandHorzRun(CurrentLocation, player1Turn);
                    break;

                #endregion

                #region Queen
                case "queen":
                    //diagonal run to the edge of the board in any direction
                    FullDiagRun(CurrentLocation, player1Turn);
                    // up/down and Left/right run to the edges of the board in any direction
                    FullVertandHorzRun(CurrentLocation, player1Turn);
                    break;
                #endregion

                #region Pawn
                case "pawn":
                    // Pawn can only move forward but can only attack diagonal unique evaluation of move vs attack
                    // for player 1 can only move up the board toward row 0
                    int pawnMove = -1;
                    int maxSpaces = 1;

                    // for player 1 can only move up the board toward row 0
                    if (!player1Turn)
                    {
                        pawnMove = 1;
                    }

                    if((player1Turn && CurrentLocation.RowNumber == 6) || (!player1Turn && CurrentLocation.RowNumber == 1))
                    {
                        maxSpaces = 2;
                    }

                    rowMoveTo = CurrentLocation.RowNumber;
                    colMoveTo = CurrentLocation.ColumnNumber;
                    //use the to stop evaluating legal moves if another peice is in the way
                    bool continueEval = true;

                    for (int i = 1; i <= maxSpaces; i++)
                    {
                        rowMoveTo = rowMoveTo + pawnMove;

                        if (rowMoveTo >= 0 && rowMoveTo < Size && continueEval)
                        {
                            continueEval = MarkLegalOrAttack(rowMoveTo, colMoveTo, player1Turn, true);
                        }
                    }

                    // Check the attack moves
                    rowMoveTo = CurrentLocation.RowNumber + pawnMove;
                    for (colMoveTo = CurrentLocation.ColumnNumber - 1; colMoveTo <= CurrentLocation.ColumnNumber + 1; colMoveTo += 2)

                        // legal attacks
                        if (rowMoveTo >= 0 && rowMoveTo < Size && colMoveTo >= 0 && colMoveTo < Size)
                        {
                            PawnMarkAttack(rowMoveTo, colMoveTo, player1Turn);
                        }
                    break;
                
                #endregion

                default:
                    break;
            }
        }

        #region Clear board methods
        /// <summary>
        /// remove all peices from the board
        /// </summary>
        public void ClearOccupation()
        {
            // clear all peices from the board
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    TheGrid[i, j].Occupied = CellOccupiedBy.Unoccupied;
                    TheGrid[i, j].Peice = null;
                }
            }
        }

        /// <summary>
        /// clear next legal, attack, and selected settings from all cells
        /// </summary>
        public void ClearNextMoves()
        {
            // clear all previous legal moves
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    TheGrid[i, j].LegalNextMove = false;
                    TheGrid[i, j].Attack = false;
                    TheGrid[i, j].Selected = false;
                }
            }
        }

        /// <summary>
        /// clear marked with Check from all cells 
        /// </summary>
        public void ClearHasKingInCheck()
        {
            // clear all previous legal moves
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    TheGrid[i, j].HasKingInCheck = false;
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// is the move in the boundries of the board
        /// </summary>
        /// <param name="rowMoveTo"></param>
        /// <param name="colMoveTo"></param>
        /// <param name="player1Turn"></param>
        private void InBoundsValidation(int rowMoveTo, int colMoveTo, bool player1Turn)
        {
            // validate row in bounds
            if (rowMoveTo < Size && rowMoveTo >= 0)
            {
                // validate column in bounds
                if (colMoveTo < Size && colMoveTo >= 0)
                {
                    // marks cell if inbounds
                    MarkLegalOrAttack(rowMoveTo, colMoveTo, player1Turn, false);
                }
            }
        }

        /// <summary>
        /// Evaluate pawn attack moves
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="player1Turn"></param>
        private void PawnMarkAttack(int row, int col, bool player1Turn)
        {
            //if occupied by other player mark as possible attack
            if ((TheGrid[row, col].Occupied == CellOccupiedBy.PlayerOne && !player1Turn)
                || (TheGrid[row, col].Occupied == CellOccupiedBy.PlayerTwo && player1Turn))
            {
                TheGrid[row, col].Attack = true;
                return;
            }
            else
            {
                TheGrid[row, col].Attack = false;
            }
        }

        /// <summary>
        /// evaluate cell occupation against current player and evaluate if cell is a legal move
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="player1Turn"></param>
        private bool MarkLegalOrAttack(int row, int col, bool player1Turn, bool isPawn)
        {
            // flip to false if we run in to another peice and want to stop the evaluation
            // continueeval used in the Queen, rook and bishop runs
            bool continueEval = true;

            //if space is empty mark as legal move
            if (TheGrid[row, col].Occupied == CellOccupiedBy.Unoccupied)
            {
                TheGrid[row, col].LegalNextMove = true;
                return continueEval;
            }

            // if occupide by current player mark as not a valid move
            else if ((TheGrid[row, col].Occupied == CellOccupiedBy.PlayerOne && player1Turn)
                || (TheGrid[row, col].Occupied == CellOccupiedBy.PlayerTwo && !player1Turn))
            {
                TheGrid[row, col].LegalNextMove = false;
                return !continueEval;
            }

            //If cell is unoccupied and a pawn in moving
            else if ((TheGrid[row, col].Occupied == CellOccupiedBy.Unoccupied) && isPawn)
            {
                TheGrid[row, col].LegalNextMove = true;
                return continueEval;
            }

            //If cell is occupied and a pawn in moving
            else if ((TheGrid[row, col].Occupied != CellOccupiedBy.Unoccupied) && isPawn)
            {
                TheGrid[row, col].LegalNextMove = false;
                return !continueEval;
            }

            //if occupied by other player
            else
            {
                TheGrid[row, col].Attack = true;
                return !continueEval;
            }
        }

        /// <summary>
        /// up/down and Left/right run to the edges of the board in any direction
        /// used for Queen and Rook
        /// </summary>
        /// <param name="CurrentLocation"></param>
        private void FullVertandHorzRun(Cell CurrentLocation, bool player1Turn)
        {
            // up
            int row = CurrentLocation.RowNumber;
            int col = CurrentLocation.ColumnNumber;
            bool continueEval = true;
            while (row > 0 && continueEval)
            {
                row--;
                continueEval = MarkLegalOrAttack(row, col, player1Turn,false);
            }

            // down
            row = CurrentLocation.RowNumber;
            continueEval = true;
            while (row < Size - 1 && continueEval)
            {
                row++;
                continueEval = MarkLegalOrAttack(row, col, player1Turn, false);
            }

            // Left
            row = CurrentLocation.RowNumber;
            col = CurrentLocation.ColumnNumber;
            continueEval = true;
            while (col > 0 && continueEval)
            {
                col--;
                continueEval = MarkLegalOrAttack(row, col, player1Turn, false);
            }

            // right
            col = CurrentLocation.ColumnNumber;
            continueEval = true;
            while (col < Size - 1 && continueEval)
            {
                col++;
                continueEval = MarkLegalOrAttack(row, col, player1Turn, false);
            }
        }

        /// <summary>
        /// diagonal run to the edge of the board in each direction
        /// used for queen and bishop
        /// </summary>
        /// <param name="CurrentLocation"></param>
        private void FullDiagRun(Cell CurrentLocation, bool player1Turn)
        {
            // up and left
            int row = CurrentLocation.RowNumber;
            int col = CurrentLocation.ColumnNumber;
            bool continueEval = true;
            while ((row > 0 && col > 0) && continueEval)
            {
                row--;
                col--;
                continueEval = MarkLegalOrAttack(row, col, player1Turn, false);
            }

            // up and right
            row = CurrentLocation.RowNumber;
            col = CurrentLocation.ColumnNumber;
            continueEval = true;
            while ((row > 0 && col < Size - 1) && continueEval)
            {
                row--;
                col++;
                continueEval = MarkLegalOrAttack(row, col, player1Turn, false);
            }

            // Down and left
            row = CurrentLocation.RowNumber;
            col = CurrentLocation.ColumnNumber;
            continueEval = true;
            while ((row < Size - 1 && col > 0) && continueEval)
            {
                row++;
                col--;
                continueEval = MarkLegalOrAttack(row, col, player1Turn, false);
            }

            // Down and Right
            row = CurrentLocation.RowNumber;
            col = CurrentLocation.ColumnNumber;
            continueEval = true;
            while ((row < Size - 1 && col < Size - 1) && continueEval)
            {
                row++;
                col++;
                continueEval = MarkLegalOrAttack(row, col, player1Turn, false);
            }
        }

        #endregion

        #region Test Boards

        /// <summary>
        /// Standard board set up
        /// </summary>
        public void StartingBoard()
        {
            // Set Pawns
            for (int i = 0; i < Size; i++)
            {
                TheGrid[1, i].Occupied = CellOccupiedBy.PlayerTwo;
                TheGrid[1, i].Peice = "Pawn";
                TheGrid[6, i].Occupied = CellOccupiedBy.PlayerOne;
                TheGrid[6, i].Peice = "Pawn";
            }
            //Set Queens
            TheGrid[0, 3].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[0, 3].Peice = "Queen";
            TheGrid[7, 3].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[7, 3].Peice = "Queen";

            //Set Kings
            TheGrid[0, 4].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[0, 4].Peice = "King";
            TheGrid[7, 4].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[7, 4].Peice = "King";

            // Set Rooks
            for (int i = 0; i < Size; i += 7)
            {
                TheGrid[0, i].Occupied = CellOccupiedBy.PlayerTwo;
                TheGrid[0, i].Peice = "Rook";
                TheGrid[7, i].Occupied = CellOccupiedBy.PlayerOne;
                TheGrid[7, i].Peice = "Rook";
            }

            // Set Knights
            for (int i = 1; i < Size; i += 5)
            {
                TheGrid[0, i].Occupied = CellOccupiedBy.PlayerTwo;
                TheGrid[0, i].Peice = "Knight";
                TheGrid[7, i].Occupied = CellOccupiedBy.PlayerOne;
                TheGrid[7, i].Peice = "Knight";
            }

            // Set Bishop
            for (int i = 2; i < Size; i += 3)
            {
                TheGrid[0, i].Occupied = CellOccupiedBy.PlayerTwo;
                TheGrid[0, i].Peice = "Bishop";
                TheGrid[7, i].Occupied = CellOccupiedBy.PlayerOne;
                TheGrid[7, i].Peice = "Bishop";
            }
        }

        public void KingTestBoard()
        {
            //Set Kings
            TheGrid[4, 4].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[4, 4].Peice = "King";

            TheGrid[4, 3].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[4, 3].Peice = "Pawn";

            TheGrid[3, 3].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[3, 3].Peice = "Pawn";

            TheGrid[3, 4].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[3, 4].Peice = "Rook";

        }

        public void QueenTestBoard()
        {
            TheGrid[5, 5].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[5, 5].Peice = "Queen";

            TheGrid[3, 7].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[3, 7].Peice = "Bishop";
            TheGrid[7, 3].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[7, 3].Peice = "Pawn";
            TheGrid[0, 5].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[0, 5].Peice = "Pawn";
            TheGrid[5, 2].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[5, 2].Peice = "Pawn";
        }

        public void KnightTestBoard()
        {
            TheGrid[3, 3].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[3, 3].Peice = "Knight";

            TheGrid[5, 2].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[5, 2].Peice = "Bishop";
            TheGrid[3, 2].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[3, 2].Peice = "Pawn";
            TheGrid[4, 3].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[4, 3].Peice = "Pawn";
            TheGrid[1, 2].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[1, 2].Peice = "Pawn";
        }

        public void BishopTestBoard()
        {
            TheGrid[5, 5].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[5, 5].Peice = "Bishop";

            TheGrid[3, 7].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[3, 7].Peice = "Bishop";
            TheGrid[7, 3].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[7, 3].Peice = "Pawn";
        }

        public void RookTestBoard()
        {
            for (int i = 0; i < Size; i += 7)
            {
                TheGrid[0, i].Occupied = CellOccupiedBy.PlayerTwo;
                TheGrid[0, i].Peice = "Rook";
                TheGrid[7, i].Occupied = CellOccupiedBy.PlayerOne;
                TheGrid[7, i].Peice = "Rook";
            }

            TheGrid[6, 6].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[6, 6].Peice = "King";
            TheGrid[1, 1].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[1, 1].Peice = "King";

            TheGrid[7, 2].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[7, 2].Peice = "Bishop";
            TheGrid[7, 5].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[7, 5].Peice = "Bishop";
        }

        public void PawnTestBoard()
        {
            for (int i = 0; i < Size; i +=2)
            {
                TheGrid[6, i].Occupied = CellOccupiedBy.PlayerOne;
                TheGrid[6, i].Peice = "Pawn";


            }

            TheGrid[5, 3].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[5, 3].Peice = "Pawn";

            TheGrid[4, 4].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[4, 4].Peice = "Pawn";
            TheGrid[5, 2].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[5, 2].Peice = "Pawn";
            TheGrid[5, 7].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[5, 7].Peice = "Pawn";
        }

        public void MateTestBoard()
        {

            TheGrid[0, 2].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[0, 2].Peice = "King";
            TheGrid[3, 3].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[3, 3].Peice = "Rook";

            TheGrid[7, 7].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[7, 7].Peice = "Rook";
            TheGrid[1, 5].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[1, 5].Peice = "Queen";

        }

        public void MateTestTwoBoard()
        {
            TheGrid[1, 2].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[1, 2].Peice = "King";
            TheGrid[0, 2].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[0, 2].Peice = "Pawn";

            TheGrid[7, 7].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[7, 7].Peice = "Rook";
            TheGrid[2, 6].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[2, 6].Peice = "Queen";
            TheGrid[7, 4].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[7, 4].Peice = "King";

        }

        public void MateTest3Board()
        {
            TheGrid[0, 0].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[0, 0].Peice = "King";
            TheGrid[0, 1].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[0, 1].Peice = "Rook";
            TheGrid[0, 1].Occupied = CellOccupiedBy.PlayerTwo;
            TheGrid[3, 2].Peice = "Pawn";

            TheGrid[1, 7].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[1, 7].Peice = "Rook";
            TheGrid[2, 6].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[2, 6].Peice = "Queen";
            TheGrid[7, 4].Occupied = CellOccupiedBy.PlayerOne;
            TheGrid[7, 4].Peice = "King";

        }

        #endregion
    }
}

