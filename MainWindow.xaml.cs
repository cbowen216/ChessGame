using BoardModel2;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/*
 * Todo
 * castle move research the rules 
 * 3rd check case take the peice that has the king in check with another peice
 * list moves
 * back up through moves this will help with the check testing and also building a machine opponent
 * 
 */

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // reference the class board that defines the board values 
        static Board myBoard = new Board(8);

        // create a 2d array of button objects the values will be determined by myboard
        public Button[,] btnGrid = new Button[myBoard.Size, myBoard.Size];

        //current player flag
        private bool player1Turn = true;
        //game play control
        bool hasSelectedAPeice = false;
        bool check = false;
        bool mate = false;
        int selectedPeiceRow;
        int selectedPeiceCol;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            PopulateGrid();
            NewGame();
        }

        /// <summary>
        /// click one of the cells on the grid to place a peice and calculate the next legal moves
        /// this is the core of the application interaction with the user 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            //set row and column to relate the btn grid to the board object grid     
            int col = Grid.GetColumn(button);
            int row = Grid.GetRow(button);
            string peiceSelected = myBoard.TheGrid[row, col].Peice;

            // retreive what is in the cell and valdate if it is a legal selection by player
            // set the peice in the current cell
            Cell currentCell = myBoard.TheGrid[row, col];

            // move if a peice already selected peice
            if (hasSelectedAPeice)
            {
                // if player clicks on same peice a second time clear the selection
                // so player can choose another peice
                if (currentCell.Selected)
                {
                    // unselect the peice and clear potential moves
                    myBoard.ClearNextMoves();

                    // redraw board
                    RefreshBoard();

                    hasSelectedAPeice = false;
                    return;
                }

                // if player clicks on a legal move attempt to move the peice
                if (currentCell.LegalNextMove || currentCell.Attack)
                {
                    //hold the values of the next move cell to write back after if invalid move test
                    string tempPeice = myBoard.TheGrid[row, col].Peice;
                    var tempPlayer = myBoard.TheGrid[row, col].Occupied;

                    //move selcted peice info in to the newly selected move cell
                    myBoard.TheGrid[row, col].Peice = myBoard.TheGrid[selectedPeiceRow, selectedPeiceCol].Peice;
                    myBoard.TheGrid[row, col].Occupied = myBoard.TheGrid[selectedPeiceRow, selectedPeiceCol].Occupied;

                    // erase peice from the originally selected peices cell
                    myBoard.TheGrid[selectedPeiceRow, selectedPeiceCol].Occupied = CellOccupiedBy.Unoccupied;
                    myBoard.TheGrid[selectedPeiceRow, selectedPeiceCol].Peice = "";

                    // Clear legal moves
                    myBoard.ClearNextMoves();

                    // see if the move puts the current players king in check. If so, reject the move
                    PlayerKingEval(player1Turn);

                    // if check = Invalid move
                    if (check) 
                    {
                        // set the peice back to the original space and erase from attempted move space
                        MessageBox.Show("invalid Move: This puts your King in check.");

                        // move the selected peice back to 
                        myBoard.TheGrid[selectedPeiceRow, selectedPeiceCol].Occupied = myBoard.TheGrid[row, col].Occupied;
                        myBoard.TheGrid[selectedPeiceRow, selectedPeiceCol].Peice = myBoard.TheGrid[row, col].Peice;

                        //move the info from temp peice back to it's original cell
                        myBoard.TheGrid[row, col].Peice = tempPeice;
                        myBoard.TheGrid[row, col].Occupied = tempPlayer;

                        // redraw board
                        myBoard.ClearNextMoves();
                        RefreshBoard();
                        check = false;
                        hasSelectedAPeice = false;
                    }

                    // valid move
                    else
                    {
                        // if a pawn made it to the other side of the board swap it for a queen
                        if (myBoard.TheGrid[row, col].Peice == "Pawn"
                            && ((player1Turn && currentCell.RowNumber == 0) || (!player1Turn && currentCell.RowNumber == 7)))
                        {
                            myBoard.TheGrid[row, col].Peice = "Queen";
                        }

                        // see if other players king is in check
                        PlayerKingEval(!player1Turn);

                        // if in check see if it is Mate
                        if (check)
                        {
                            RefreshBoard();

                            // notify of check
                            MessageBox.Show("Check!");

                            //mark peice that has king in check
                            myBoard.TheGrid[row, col].HasKingInCheck = true;
                            myBoard.ClearNextMoves();

                            // see if the king is in checkmate
                            EndTurnCheckMateEval(!player1Turn);

                            // if mate end game
                            if (mate)
                            {
                                GameEnd(player1Turn);
                            }
                        }

                        //end turn reset for next player
                        //toggle palyer
                        ChangePlayers();
                        hasSelectedAPeice = false;
                        check = false;
                        mate = false;
                        myBoard.ClearNextMoves();
                        myBoard.ClearHasKingInCheck();
                        RefreshBoard();
                    }

                    //click was not a legal move cell or an attack cell
                    //wait for a legal input from player 
                    return;
                }
            }

            // if a peice has not been selected and a player clicks on one of their peices
            // show legal moves for that peice
            if (!hasSelectedAPeice && ((player1Turn && currentCell.Occupied == CellOccupiedBy.PlayerOne) 
                ||(!player1Turn && currentCell.Occupied == CellOccupiedBy.PlayerTwo))) 
            {
                // get legal next moves and attacks
                myBoard.MarkNextLegalMoves(currentCell, peiceSelected, player1Turn);

                // mark that cell as selected for the board refresh
                currentCell.Selected = true;
                //remember the position of the selected peice to use when moving the selected peice 
                selectedPeiceRow = row;
                selectedPeiceCol = col;

                hasSelectedAPeice = true;

                // redraw board
                RefreshBoard();

                return;
            }

        }

        #region Private Helpers

        #region Game start and setup

        /// <summary>
        /// Reset to start a new game
        /// </summary>
        private void NewGame()
        {
            myBoard.ClearNextMoves();
            myBoard.ClearOccupation();
            myBoard.StartingBoard();

            player1Turn = true;
            check = false;
            mate = false;

            //draw the board
            RefreshBoard();
        }

        /// <summary>
        /// Create the buttons in the play space and sets up the board
        /// </summary>
        private void PopulateGrid()
        {
            // row
            for (int i = 0; i < myBoard.Size; i++)
            {
                // col
                for (int j = 0; j < myBoard.Size; j++)
                {
                    // create a new button
                    btnGrid[i, j] = new Button();

                    // Placemnt in Playspace Grid
                    Grid.SetColumn(btnGrid[i, j], j);
                    Grid.SetRow(btnGrid[i, j], i);

                    // set properties
                    btnGrid[i, j].Margin = new Thickness(2);
                    btnGrid[i, j].BorderThickness = new Thickness(3);
                    btnGrid[i, j].Click += ButtonClick;

                    //print row & col coordinates (uncomment for troubleshooting)
                    //btnGrid[i, j].Content = $"{i} | {j}";

                    //Add the button to the playspace grid
                    PlaySpace.Children.Add(btnGrid[i, j]);
                }
            }

        }

        #endregion

        #region Geme play helpers

        /// <summary>
        /// change player and update ui elements to indicate which players turn it is
        /// </summary>
        private void ChangePlayers()
        {
            player1Turn ^= true;

            // mark the current player at the top of the play space 
            if (player1Turn)
            {
                Player1Label.Content = "Player 1: Playing.";
                Player1Label.FontWeight = FontWeights.Bold;
                Player1Label.Foreground = Brushes.Green;

                Player2Label.Content = "Player 2:";
                Player2Label.FontWeight = FontWeights.Normal;
                Player2Label.Foreground = Brushes.Red;
            }
            else
            {
                Player1Label.Content = "Player 1:";
                Player1Label.FontWeight = FontWeights.Normal;
                Player1Label.Foreground = Brushes.Red;

                Player2Label.Content = "Player 2: Playing.";
                Player2Label.FontWeight = FontWeights.Bold;
                Player2Label.Foreground = Brushes.Green;
            }

        }

        /// <summary>
        /// set which player is being evaluated enum to string
        /// </summary>
        /// <param name="player1peice"></param>
        private string WhatPlayer(bool player1peice)
        {
            string player = (player1peice) ? CellOccupiedBy.PlayerOne.ToString() : CellOccupiedBy.PlayerTwo.ToString();
            return player;
        }

        /// <summary>
        /// finding the king for a player to eval check or mate
        /// </summary>
        /// <param name="player"></param>
        /// <returns> Cell of the kings position </returns>
        private Cell FindKing(string player)
        {
            int row = 0;
            int col = 0;
            Cell returnPosition = myBoard.TheGrid[row, col];

            //find the current players king 
            for (row = 0; row < myBoard.Size; row++)
            {
                for (col = 0; col < myBoard.Size; col++)
                {
                    if (myBoard.TheGrid[row, col].Peice == "King"
                        && player == myBoard.TheGrid[row, col].Occupied.ToString())
                    {
                        returnPosition = myBoard.TheGrid[row, col];
                        return returnPosition;
                    }
                }
            }
            return returnPosition;
        }

        /// <summary>
        /// See if a king is in check by any of the other players peices 
        /// </summary>
        /// <param name="evaluatingPlayer1"></param>
        private void PlayerKingEval(bool evaluatingPlayer1)
        {
            string player = WhatPlayer(evaluatingPlayer1);
            Cell kingPosition = FindKing(player);

            //change to opposite palyer
            player = WhatPlayer(!evaluatingPlayer1);

            // go through all the cells on the board
            for (int row = 0; row < myBoard.Size; row++)
            {
                for (int col = 0; col < myBoard.Size; col++)
                {
                    // does this cell contain the opposite players peice 
                    if (player == myBoard.TheGrid[row, col].Occupied.ToString())
                    {
                        // mark the legal moves for this peice
                        Cell currentCell = myBoard.TheGrid[row, col];
                        myBoard.MarkNextLegalMoves(currentCell, currentCell.Peice, !evaluatingPlayer1);

                        // if the peice tested mark the king's cell as attack, set check
                        if (kingPosition.Attack)
                        {
                            check = true;
                            myBoard.ClearNextMoves();
                            return;
                        }
                        myBoard.ClearNextMoves();
                    }
                }
            }
        }

        /// <summary>
        /// test for checkmate
        /// </summary>
        /// <param name="evalPlayer1"> player we are testing if their king is checkmate </param>
        private void EndTurnCheckMateEval(bool evalPlayer1)
        {
            // first test if the king has a valid move out

            // find the king and store that position
            string player = WhatPlayer(evalPlayer1);
            Cell kingPosition = FindKing(player);

            //Mark the grid with the kings legal moves
            myBoard.MarkNextLegalMoves(kingPosition, "King", evalPlayer1);

            //create a list of x,y point to store kings legal moves from the grid
            // get a list of possible moves from the list creator thingy
            List<IntPoint> KingLegalMoves = MoveListCreator();

            myBoard.ClearNextMoves();

            //change player string to a enum
            CellOccupiedBy getParse;
            bool result = Enum.TryParse(player, out getParse);

            //change to opposite player to test all opposite player peaces agains each possible king move
            player = WhatPlayer(!evalPlayer1);

            // take the king out of the original king position cell
            myBoard.TheGrid[kingPosition.RowNumber, kingPosition.ColumnNumber].Occupied = CellOccupiedBy.Unoccupied;
            myBoard.TheGrid[kingPosition.RowNumber, kingPosition.ColumnNumber].Peice = "";

            int row = 0;
            int col = 0;

            //for each possible legal move for the players king ...
            for (int i = 0; i < KingLegalMoves.Count; i++)
            {
                //hold the values of the kings test to write back after we test the cell
                string tempPeice = myBoard.TheGrid[KingLegalMoves[i].row, KingLegalMoves[i].col].Peice;
                var tempPlayer = myBoard.TheGrid[KingLegalMoves[i].row, KingLegalMoves[i].col].Occupied;

                // create a test cell for the kings moves from the list
                Cell testKingPosition = myBoard.TheGrid[KingLegalMoves[i].row, KingLegalMoves[i].col];

                // move the king to new position
                testKingPosition.Occupied = getParse;
                testKingPosition.Peice = "King";
                testKingPosition.Attack = false;
                //use Next test cell flag to break the inner for next loops and move directy to the next test cell
                bool nextTestCell = false;

                // if the one of the other players peices marks the king's new move as attack then the king
                // cannot move to this space. go to the next possilbe move.
                // if we can find one space where none of the other players peices can attack the king on
                // their next move then there is at least 1 legal move out of check; end the test and return
                //to the calling function

                // go through all the cells on the board ...
                for (row = 0; row < myBoard.Size; row++)
                {
                    for (col = 0; col < myBoard.Size; col++)
                    {
                        // does this cell contain the opposite players peice 
                        if (player == myBoard.TheGrid[row, col].Occupied.ToString())
                        {
                            myBoard.ClearNextMoves();

                            // mark the legal moves for this peice
                            Cell currentCell = myBoard.TheGrid[row, col];
                            string peiceSelected = currentCell.Peice;
                            myBoard.MarkNextLegalMoves(currentCell, peiceSelected, !evalPlayer1);

                            //if king test position has been marked as attack not a valid move
                            if (testKingPosition.Attack)
                            {
                                // not a valid move set test position cell back to original value
                                testKingPosition.Peice = tempPeice;
                                testKingPosition.Occupied = tempPlayer;
                                nextTestCell = true;
                                myBoard.ClearNextMoves();
                                break;
                            }

                            // if you have gotten to cell 7,7 with out the king test position being marked as
                            // attack then there is at least 1 legal move for the king and it is not check mate
                            // end test and return

                        }
                        else if (row == 7 && col == 7 && !kingPosition.Attack)
                        {
                            //MessageBox.Show("at least 1 valid move.");
                            //set test position cell back to hold cell property values
                            testKingPosition.Peice = tempPeice;
                            testKingPosition.Occupied = tempPlayer;

                            // put the king back in the original position
                            kingPosition.Peice = "King";
                            kingPosition.Occupied = getParse;

                            myBoard.ClearNextMoves();
                            return;
                        }
                    }

                    if (nextTestCell)
                    {
                        // if not a legal move break the loop and go on to next possible move for the king
                        break;
                    }
                }
            }

            // if the code reaches here then there are no legal moves for the king
            // put the king back in the original position
            kingPosition.Peice = "King";
            kingPosition.Occupied = getParse;

            //reset the bord for next test
            myBoard.ClearNextMoves();
            //MessageBox.Show("king has no valid moves.");

            // if there is not a legal move for the king then find the peice that put king in check is it a rook, bishop or Queen?
            // can the path be blocked?

            //find the peice that has the king in check
            Cell checkPosition = myBoard.TheGrid[0, 0];

            for (row = 0; row < myBoard.Size; row++)
            {
                for (col = 0; col < myBoard.Size; col++)
                {
                    if (myBoard.TheGrid[row, col].HasKingInCheck)
                    {
                        checkPosition = myBoard.TheGrid[row, col];
                        break;
                    }
                }
            }

            // can the peice that has the king in check be taken by another peice
            // need to write

            if (checkPosition.Peice == "Pawn"
                || checkPosition.Peice == "Knight"
                || checkPosition.Peice == "King")
            {
                // if the peice is a knight or Pawn a peice can not be moved between the king and that peice to block.
                // since we already evalueate that the king has no legal moves left... checkmate
                mate = true;
            }

            // see if the player that their king is in check can block the peice that has it in check
            else
            {
                //mark the spaces between the king and the peice that has the king in check as legal moves
                if ((checkPosition.Peice == "Rook"
                || checkPosition.Peice == "Queen")
                && (kingPosition.RowNumber == checkPosition.RowNumber
                || kingPosition.ColumnNumber == checkPosition.ColumnNumber))
                {
                    //if king above peice that has king in check
                    if (kingPosition.RowNumber < checkPosition.RowNumber)
                    {
                        for (row = kingPosition.RowNumber + 1; row > checkPosition.RowNumber; row++)
                        {
                            col = kingPosition.ColumnNumber;
                            myBoard.TheGrid[row, col].LegalNextMove = true;
                        }
                    }

                    //if king below peice that has king in check
                    if (kingPosition.RowNumber > checkPosition.RowNumber)
                    {
                        for (row = kingPosition.RowNumber - 1; row < checkPosition.RowNumber; row--)
                        {
                            col = kingPosition.ColumnNumber;
                            myBoard.TheGrid[row, col].LegalNextMove = true;
                        }
                    }

                    //if king to the right of peice that has king in check
                    if (kingPosition.ColumnNumber > checkPosition.ColumnNumber)
                    {
                        for (col = kingPosition.ColumnNumber - 1; col > checkPosition.ColumnNumber; col--)
                        {
                            row = kingPosition.RowNumber;
                            myBoard.TheGrid[row, col].LegalNextMove = true;
                        }
                    }

                    //if king to the left of peice that has king in check
                    if (kingPosition.ColumnNumber < checkPosition.ColumnNumber)
                    {
                        for (col = kingPosition.ColumnNumber + 1; col < checkPosition.ColumnNumber; col++)
                        {
                            row = kingPosition.RowNumber;
                            myBoard.TheGrid[row, col].LegalNextMove = true;
                        }
                    }
                }

                else if ((checkPosition.Peice == "Bishop" 
                        || checkPosition.Peice == "Queen")
                        && kingPosition.RowNumber != checkPosition.RowNumber
                        && kingPosition.ColumnNumber != checkPosition.ColumnNumber)
                {
                    //Set the starting column
                    col = kingPosition.ColumnNumber;

                    //if king above and right of peice that has king in check
                    if (kingPosition.RowNumber < checkPosition.RowNumber
                        && kingPosition.ColumnNumber > checkPosition.ColumnNumber)
                    {
                        for (row = kingPosition.RowNumber + 1; row < checkPosition.RowNumber; row++)
                        {
                            col--;
                            myBoard.TheGrid[row, col].LegalNextMove = true;
                        }
                    }

                    //if king above and left of peice that has king in check
                    if (kingPosition.RowNumber < checkPosition.RowNumber
                        && kingPosition.ColumnNumber < checkPosition.ColumnNumber)
                    {
                        for (row = kingPosition.RowNumber + 1; row < checkPosition.RowNumber; row++)
                        {

                            col++;
                            myBoard.TheGrid[row, col].LegalNextMove = true;
                        }
                    }

                    //if king below and right of peice that has king in check
                    if (kingPosition.RowNumber > checkPosition.RowNumber
                        && kingPosition.ColumnNumber > checkPosition.ColumnNumber)
                    {
                        for (row = kingPosition.RowNumber - 1; row > checkPosition.RowNumber; row--)
                        {
                            col--;
                            myBoard.TheGrid[row, col].LegalNextMove = true;
                        }
                    }

                    //if king below and left of peice that has king in check
                    if (kingPosition.RowNumber > checkPosition.RowNumber
                        && kingPosition.ColumnNumber < checkPosition.ColumnNumber)
                    {
                        for (row = kingPosition.RowNumber - 1; row > checkPosition.RowNumber; row--)
                        {
                            col++;
                            myBoard.TheGrid[row, col].LegalNextMove = true;
                        }
                    }
                }

                //create a list to store the checkpeice legal moves
                List<IntPoint> LegalMoves = new List<IntPoint>();

                // find all legal moves of the peice that has the king in check
                // on the grid write location to list of legal moves to work through.
                LegalMoves = MoveListCreator();

                myBoard.ClearNextMoves();

                //change to opposite palyer
                player = WhatPlayer(evalPlayer1);

                //for each possible legal move for the peice in check ...
                for (int i = 0; i < LegalMoves.Count; i++)
                {
                    // go through all the cells on the board ...
                    for (row = 0; row < myBoard.Size; row++)
                    {
                        for (col = 0; col < myBoard.Size; col++)
                        {
                            // does this cell contain the opposite players peice 
                            if (player == myBoard.TheGrid[row, col].Occupied.ToString() &&
                                myBoard.TheGrid[row, col].Peice != "King")
                            {
                                // mark the legal moves for this peice
                                Cell currentCell = myBoard.TheGrid[row, col];
                                string peiceSelected = currentCell.Peice;
                                myBoard.MarkNextLegalMoves(currentCell, peiceSelected, !evalPlayer1);

                                // if the the legal moves for players peice tested = the check peice proposed move cell
                                // gotten from the list of legal moves; then it can block the king from being in check.
                                if (myBoard.TheGrid[LegalMoves[i].row, LegalMoves[i].col].LegalNextMove)
                                {
                                    // found a legal blocking move break
                                    myBoard.ClearHasKingInCheck();
                                    myBoard.ClearNextMoves();
                                    check = false;
                                    // there is a possible blocking move to replase check & 
                                    return;
                                }
                                myBoard.ClearNextMoves();
                            }
                        }
                    }
                }
            }

            // if we have mad it here then the king does not have a legal move out of check
            // and there is no possible blocking move... checkmate
            mate = true;
            return;
        }

        /// <summary>
        /// Create a list of points that are valid moves to pass back to the calling method
        /// </summary>
        /// <returns></returns>
        private List<IntPoint> MoveListCreator()
        {
            int row = 0;
            int col = 0;
            List<IntPoint> validMoveList = new List<IntPoint>();

            // find all marked legal moves on the grid, write location as x,y point to list.
            for (row = 0; row < myBoard.Size; row++)
            {
                for (col = 0; col < myBoard.Size; col++)
                {
                    if (myBoard.TheGrid[row, col].LegalNextMove || myBoard.TheGrid[row, col].Attack)
                    {
                        IntPoint Move = new IntPoint(row, col);
                        Move.row = row;
                        Move.col = col;
                        validMoveList.Add(Move);
                    }
                }
            }
            return validMoveList;
        }

        /// <summary>
        /// End game on checkmate 
        /// </summary>
        /// <param name="player1Turn"></param>
        private void GameEnd(bool player1Turn)
        {
            string messageText;

            //output a winner messagebox
            if (player1Turn)
            {
                messageText = "Check Mate!  Player 1 Wins! \n Play Again?";
            }
            else
            {
                messageText = "Check Mate!  Player 2 Wins! \n Play Again?";
            }
            MessageBoxResult messageBoxResult = MessageBox.Show(messageText, "End Game", MessageBoxButton.YesNo);

            // start new game
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                NewGame();
                return;
            }
            //Exit game
            else if (messageBoxResult == MessageBoxResult.No)
            {
                Close();
            }
        }

        #endregion

        #region Board Drawing

        /// <summary>
        /// Draw the board, using the info from the board class
        /// </summary>
        private void RefreshBoard()
        {
            bool whiteBackground = false;
            for (int row = 0; row < myBoard.Size; row++)
            {
                whiteBackground = !whiteBackground;
                for (int col = 0; col < myBoard.Size; col++)
                {
                    btnGrid[row, col].Content = "";
                    string cellPlayer = myBoard.TheGrid[row, col].Occupied.ToString();
                    bool cellHasPeice = cellPlayer.Length != 0 && cellPlayer != "Unoccupied";
                    string peice = myBoard.TheGrid[row, col].Peice;

                    if (myBoard.TheGrid[row, col].LegalNextMove)
                    {
                        //btnGrid[row, col].Content = "Legal";
                        btnGrid[row, col].Background = Brushes.LightGreen;
                        whiteBackground = !whiteBackground;
                    }

                    // if space is occupied
                    else if (cellHasPeice)
                    {
                        // format attacking cells
                        if (myBoard.TheGrid[row, col].Attack)
                        {
                            btnGrid[row, col].Background = Brushes.Red;
                            btnGrid[row, col].BorderBrush = Brushes.Red;
                            btnGrid[row, col].BorderThickness = new Thickness(10);

                            //get the path to the image from get button image method
                            string imgSource = GetButtonImage(peice, cellPlayer);

                            //place peice image in thoccupied square
                            btnGrid[row, col].Content = new Image
                            {
                                //test string that hard codes the image to the wight knight: 
                                //Source = new BitmapImage(new Uri("Images/WH_knight.png", UriKind.RelativeOrAbsolute))
                                Source = new BitmapImage(new Uri(imgSource, UriKind.RelativeOrAbsolute)),
                            };
                            // toggle main background
                            whiteBackground = !whiteBackground;
                        }

                        else if (myBoard.TheGrid[row, col].Selected)
                        {
                            btnGrid[row, col].Background = Brushes.Blue;
                            btnGrid[row, col].BorderBrush = Brushes.Blue;
                            btnGrid[row, col].BorderThickness = new Thickness(10);

                            //get the path to the image from get button image method
                            string imgSource = GetButtonImage(peice, cellPlayer);

                            //place peice image in thoccupied square
                            btnGrid[row, col].Content = new Image
                            {
                                //test string that hard codes the image to the wight knight: 
                                //Source = new BitmapImage(new Uri("Images/WH_knight.png", UriKind.RelativeOrAbsolute))
                                Source = new BitmapImage(new Uri(imgSource, UriKind.RelativeOrAbsolute)),
                            };
                            // toggle main background
                            whiteBackground = !whiteBackground;
                        }

                        else
                        {
                            //get the path to the image from get button image method
                            string imgSource = GetButtonImage(peice, cellPlayer);

                            //place peice image in thoccupied square
                            btnGrid[row, col].Content = new Image
                            {
                                //test string that hard codes the image to the wight knight: 
                                //Source = new BitmapImage(new Uri("Images/WH_knight.png", UriKind.RelativeOrAbsolute))
                                Source = new BitmapImage(new Uri(imgSource, UriKind.RelativeOrAbsolute)),
                            };

                            // background button formatting toggle method
                            whiteBackground = BoardBackgroundToggle(whiteBackground, row, col);

                            // text return of the peice uncomment for troubleshooting 
                            //btnGrid[i, j].Content = peice;
                        }
                    }

                    // if not occupied or next legal move
                    else if (cellPlayer == "Unoccupied")
                    {
                        // background tolor toggle method
                        whiteBackground = BoardBackgroundToggle(whiteBackground, row, col);
                    }
                }
            }
            
        }

        /// <summary>
        /// Toggles the board background color when drawing the board for cells with not marked legal move or attack 
        /// </summary>
        /// <param name="whiteBackground"> boolean color toggle </param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private bool BoardBackgroundToggle(bool whiteBackground, int row, int col)
        {
            if (whiteBackground)
            {
                //format white space
                btnGrid[row, col].Background = Brushes.White;
                btnGrid[row, col].BorderBrush = Brushes.Black;
                btnGrid[row, col].BorderThickness = new Thickness(2);
                whiteBackground = !whiteBackground;
                return whiteBackground;
            }
            else
            {
                //format grey space
                btnGrid[row, col].Background = Brushes.LightGray;
                btnGrid[row, col].BorderBrush = Brushes.Black;
                btnGrid[row, col].BorderThickness = new Thickness(2);
                whiteBackground = !whiteBackground;
                return whiteBackground;
            }
        }

        /// <summary>
        /// Return the URI string for the image based on peice selection
        /// </summary>
        /// <param name="peice"> </param>
        /// <returns> image URI string for the peice </returns>
        private string GetButtonImage(string peice, string cellPlayer)
        {
            string peiceImage = null;
            switch (peice)
            {
                case "Knight":
                    {
                        peiceImage = cellPlayer == "PlayerOne" ? @"/images/WH_knight.png" : @"/images/BL_knight.png";
                        return peiceImage;
                    }

                case "King":
                    {
                        peiceImage = cellPlayer == "PlayerOne" ? @"/images/WH_king.png" : @"/images/BL_king.png";
                        return peiceImage;
                    }

                case "Queen":
                    {
                        peiceImage = cellPlayer != "PlayerOne" ? @"/images/BL_queen.png" : @"/images/WH_queen.png";
                        return peiceImage;
                    }

                case "Bishop":
                    {
                        peiceImage = cellPlayer == "PlayerOne" ? @"/images/WH_bishop.png" : @"/images/BL_bishop.png";
                        return peiceImage;
                    }

                case "Rook":
                    {
                        peiceImage = cellPlayer == "PlayerOne" ? @"/images/WH_rook.png" : @"/images/BL_rook.png";
                        return peiceImage;
                    }

                case "Pawn":
                    {
                        peiceImage = cellPlayer == "PlayerOne" ? @"/images/WH_pawn.png" : @"/images/BL_pawn.png";
                        return peiceImage;
                    }

                default:
                    peiceImage = "";
                    return peiceImage;
            }
        }

        #endregion

        #region Troubleshooting and testing
        /// <summary>
        /// Troubleshooting set up boards for testing specific situations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestBoardSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // clear the board and any peice selection
            myBoard.ClearNextMoves();
            myBoard.ClearOccupation();
            hasSelectedAPeice = false;

            //get combobox selection
            string testBoard = (e.AddedItems[0] as ComboBoxItem).Content as string; ;

            // load the board
            switch (testBoard)
            {
                case "New Board":
                    {
                        myBoard.StartingBoard();
                        RefreshBoard();
                        return;
                    }
                case "King":
                    {
                        myBoard.KingTestBoard();
                        RefreshBoard();
                        return;
                    }

                case "Queen":
                    {
                        myBoard.QueenTestBoard();
                        RefreshBoard();
                        return;
                    }

                case "Knight":
                    {
                        myBoard.KnightTestBoard();
                        RefreshBoard();
                        return;
                    }

                case "Bishop":
                    {
                        myBoard.BishopTestBoard();
                        RefreshBoard();
                        return;
                    }

                case "Rook":
                    {
                        myBoard.RookTestBoard();
                        RefreshBoard();
                        return;
                    }

                case "Pawn":
                    {
                        myBoard.PawnTestBoard();
                        RefreshBoard();
                        return;
                    }

                case "Mate Test":
                    {
                        myBoard.MateTestBoard();
                        RefreshBoard();
                        return;
                    }
                case "Mate Test 2":
                    {
                        myBoard.MateTestTwoBoard();
                        RefreshBoard();
                        return;
                    }
                case "Mate Test 3":
                    {
                        myBoard.MateTest3Board();
                        RefreshBoard();
                        return;
                    }
                default:
                    break;
            }
        }

        #endregion

        #endregion

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Height = Width + 40;
        }
    }
}
