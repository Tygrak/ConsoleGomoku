using System;
using System.Collections.Generic;

namespace Gomoku{
    class Board{
        public bool blackTurn;
        private bool blackIsPlayer = false;
        private bool whiteIsPlayer = true;
        private int blackBotTimer = 10;
        private int whiteBotTimer = 10;
        private AI ai = new AI();
        public int width;
        public int[] board;

        public int lastSquare;

        public Board(int width, bool blackIsPlayer, bool whiteIsPlayer){
            this.width = width;
            this.board = new int[width*width];
            this.blackIsPlayer = blackIsPlayer;
            this.whiteIsPlayer = whiteIsPlayer;
            blackTurn = true;
        }

        public Board(Board toCopy){
            this.width = toCopy.width;
            this.board = (int[]) toCopy.board.Clone();
            this.blackTurn = toCopy.blackTurn;
            this.lastSquare = toCopy.lastSquare;
        }

        public void StartLoop(){
            lastSquare = 0;
            while(true){
                DrawBoard(lastSquare);
                if((blackTurn && blackIsPlayer) || (!blackTurn && whiteIsPlayer)){
                    lastSquare = UserSelectSquare();
                } else{
                    //lastSquare = ai.GetHighestValueMove(this);
                    if(blackTurn){
                        //lastSquare = ai.RootAlphaBeta(int.MinValue, int.MaxValue, this, 2);
                        //lastSquare = ai.MonteCarloSearch(this, blackBotTimer);
                    } else{
                        //lastSquare = ai.MonteCarloSearch(this, whiteBotTimer);
                        lastSquare = ai.RootAlphaBeta(int.MinValue, int.MaxValue, this, 4);
                    }
                }
                if(board[lastSquare] == 0 || board[lastSquare] == 3){
                    ChangeSquare(lastSquare);
                    ai.EvaluateBoard(this);
                    int win = CheckWin(lastSquare);
                    if(win != 0){
                        DrawBoard(lastSquare);
                        WriteEndText(win);
                        UserPromptEnd();
                    }
                    EndTurn();
                }
            }
        }

        public void DrawBoard(int cursorPosition = 0){
            ClearBoard();
            string columnLabels = "    ";
            for (int i = 0; i < width; i++){
                columnLabels += (char) ('a' + i);
            }
            Console.WriteLine(columnLabels);
            for (int i = 0; i < width; i++){
                Console.WriteLine();
                Console.Write((i+1).ToString("00") + "  ");
                for (int j = 0; j < width; j++){
                    if(board[i*width+j] == 0){
                        Console.Write("-");
                    } else if(board[i*width+j] == 1){
                        Console.Write("X");
                    } else if(board[i*width+j] == 2){
                        Console.Write("O");
                    } else{
                        Console.Write("T");
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.SetCursorPosition(4+ColumnFromPosition(cursorPosition),
                                         2+RowFromPosition(cursorPosition));
        }

        public int UserSelectSquare(){
            while(Console.KeyAvailable){
                Console.ReadKey(true);
            }
            ConsoleKey key = Console.ReadKey(true).Key;
            if(key == ConsoleKey.UpArrow && Console.CursorTop > 2){
                Console.CursorTop -= 1;
            } else if(key == ConsoleKey.DownArrow && Console.CursorTop < width+1){
                Console.CursorTop += 1;
            } else if(key == ConsoleKey.LeftArrow && Console.CursorLeft > 4){
                Console.CursorLeft -= 1;
            } else if(key == ConsoleKey.RightArrow && Console.CursorLeft < width+3){
                Console.CursorLeft += 1;
            }
            int cLeftPos = Console.CursorLeft;
            int cTopPos  = Console.CursorTop;
            Console.SetCursorPosition(width+6, 0);
            Console.Write((cLeftPos-4+(cTopPos-2)*width).ToString("000"));
            Console.SetCursorPosition(width+6, 1);
            Console.Write((char) ('a' + cLeftPos-4) + (cTopPos-1).ToString("00"));
            Console.SetCursorPosition(cLeftPos, cTopPos);
            if(key == ConsoleKey.Enter){
                return Console.CursorLeft-4+(Console.CursorTop-2)*width;
                //return (Console.CursorTop-2)*width+Console.CursorLeft-4;
            } else{
                return UserSelectSquare();
            }
        }

        ///Returns 0 if noone wins, 1 if black(X) wins and 2 if white(O) wins and -1 for a stalemate.
        public int CheckWin(int position){
            int targetC = board[position];
            int pos2 = position;
            int inRow = 0;
            for(; ColumnFromPosition(pos2)>=0; pos2--){
                if(ColumnFromPosition(pos2)-1 < 0 || board[pos2-1] != targetC){
                    break;
                }
            }
            for(; ColumnFromPosition(pos2)<width; pos2++){
                inRow++;
                if(ColumnFromPosition(pos2)+1 >= width || board[pos2+1] != targetC){
                    if(inRow >= 5){
                        return targetC;
                    }
                    break;
                }
            }
            pos2 = position;
            inRow = 0;
            for(; RowFromPosition(pos2)>=0; pos2 -= width){
                if(RowFromPosition(pos2)-1 < 0 || board[pos2-width] != targetC){
                    break;
                }
            }
            for(; RowFromPosition(pos2)<width; pos2 += width){
                inRow++;
                if(RowFromPosition(pos2)+1 >= width || board[pos2+width] != targetC){
                    if(inRow >= 5){
                        return targetC;
                    }
                    break;
                }
            }
            pos2 = position;
            inRow = 0;
            for(; RowFromPosition(pos2)>=0 && ColumnFromPosition(pos2)>=0; pos2 = pos2-width-1){
                if(RowFromPosition(pos2)-1 < 0 || ColumnFromPosition(pos2)-1 < 0 || board[pos2-width-1] != targetC){
                    break;
                }
            }
            for(; RowFromPosition(pos2)<width && ColumnFromPosition(pos2)<width; pos2 += width+1){
                inRow++;
                if(RowFromPosition(pos2)+1 >= width || ColumnFromPosition(pos2)+1 >= width || board[pos2+width+1] != targetC){
                    if(inRow >= 5){
                        return targetC;
                    }
                    break;
                }
            }
            pos2 = position;
            inRow = 0;
            for(; RowFromPosition(pos2)>=0 && ColumnFromPosition(pos2)<width; pos2 = pos2-width+1){
                if(RowFromPosition(pos2)-1 < 0 || ColumnFromPosition(pos2)+1 >= width || board[pos2-width+1] != targetC){
                    break;
                }
            }
            for(; RowFromPosition(pos2)<width && ColumnFromPosition(pos2)>=0; pos2 += width-1){
                inRow++;
                if(RowFromPosition(pos2)+1 >= width || ColumnFromPosition(pos2)-1 < 0 || board[pos2+width-1] != targetC){
                    if(inRow >= 5){
                        return targetC;
                    }
                    break;
                }
            }
            //DebugWrite(ColumnFromPosition(position).ToString() + ", " + RowFromPosition(position).ToString());
            if(GetInterestingMoves(1).Length == 0 && board[0] != 0){
                return -1;
            }
            return 0;
        }

        public void ChangeSquare(int position){
            board[position] = blackTurn ? 1 : 2;
        }

        public void EndTurn(){
            blackTurn = !blackTurn;
        }

        public void UserPromptEnd(){
            while(Console.KeyAvailable){
                Console.ReadKey(true);
            }
            ConsoleKey key = Console.ReadKey(true).Key;
            if(key == ConsoleKey.Enter){
                board = new int[width*width];
                StartLoop();
            } else if(key == ConsoleKey.Escape){
                Program.ExitGame();
            } else{
                UserPromptEnd();
            }
        }

        public void WriteEndText(int won){
            Console.Clear();
            DrawBoard();
            Console.SetCursorPosition(2, 3+width);
            if(won == 1){
                Console.WriteLine("X won!");
            } else if(won == 2){
                Console.WriteLine("O won!");
            } else if(won == -1){
                Console.WriteLine("Stalemate!");
            }
            Console.WriteLine();
            Console.WriteLine("  Press enter to play another game. Press escape to exit.");
        }

        public int ColumnFromPosition(int position){
            return position%width;
        }
        public int RowFromPosition(int position){
            return (position/width);
        }

        public void ClearBoard(){
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < width+2; i++){
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.SetCursorPosition(0, 0);
        }

        public void DebugWrite(string toWrite){
            Console.Clear();
            DrawBoard();
            Console.SetCursorPosition(2, 3+width);
            Console.WriteLine(toWrite);
        }

        public int[] GetInterestingMoves(int range = 1){
            List<int> moves = new List<int>();
            for (int i = 0; i < this.board.Length; i++){
                if(this.board[i] != 0){
                    if(this.RowFromPosition(i)-1 >= 0 && this.ColumnFromPosition(i)-1 >= 0 && !moves.Contains(i-1-this.width) && this.board[i-1-this.width] == 0){
                        moves.Add(i-1-this.width);
                    }
                    if(this.RowFromPosition(i)-1 >= 0 && this.ColumnFromPosition(i)+1 < this.width && !moves.Contains(i+1-this.width) && this.board[i+1-this.width] == 0){
                        moves.Add(i+1-this.width);
                    }
                    if(this.RowFromPosition(i)+1 < this.width && this.ColumnFromPosition(i)-1 >= 0 && !moves.Contains(i-1+this.width) && this.board[i-1+this.width] == 0){
                        moves.Add(i-1+this.width);
                    }
                    if(this.RowFromPosition(i)+1 < this.width && this.ColumnFromPosition(i)+1 < this.width && !moves.Contains(i+1+this.width) && this.board[i+1+this.width] == 0){
                        moves.Add(i+1+this.width);
                    }
                    if(this.RowFromPosition(i)-1 >= 0 && !moves.Contains(i-this.width) && this.board[i-this.width] == 0){
                        moves.Add(i-this.width);
                    }
                    if(this.RowFromPosition(i)+1 < this.width && !moves.Contains(i+this.width) && this.board[i+this.width] == 0){
                        moves.Add(i+this.width);
                    }
                    if(this.ColumnFromPosition(i)-1 >= 0 && !moves.Contains(i-1) && this.board[i-1] == 0){
                        moves.Add(i-1);
                    }
                    if(this.ColumnFromPosition(i)+1 < this.width && !moves.Contains(i+1) && this.board[i+1] == 0){
                        moves.Add(i+1);
                    }
                }
            }
            return moves.ToArray();
        }
    }
}