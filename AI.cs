using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gomoku{
    class AI{
        public static Random rand = new Random();
        
        public AI(){
        }

        public int GetRandomMove(Board b, int searchRange = 1){
            int[] moves = b.GetInterestingMoves(searchRange);
            if(moves.Length == 0 && b.board[0] == 0){
                return (b.width*b.width)/2;
            } else if(moves.Length == 0){
                return -1;
            }
            return moves[rand.Next(moves.Length)];
        }

        public int GetHighestValueMove(Board b, int searchRange = 1){
            int[] moves = b.GetInterestingMoves(searchRange);
            if(moves.Length == 0){
                return (b.width*b.width)/2;
            }
            int maxVal = int.MinValue;
            int bestI = 0;
            for (int i = 0; i < moves.Length; i++){
                Board copy = new Board(b);
                copy.ChangeSquare(moves[i]);
                int val;
                if(b.blackTurn){
                    val = EvaluateBoard(copy);
                } else{
                    val = -EvaluateBoard(copy); 
                }
                if(val>maxVal){
                    maxVal = val;
                    bestI = i;
                }
            }
            return moves[bestI];
        }

        public int MonteCarloSearch(Board rootState, double searchTimer){
            //Check if we can find a winning move in one move
            string deb = "";
            if(true){
                int winCheck = GetHighestValueMove(rootState, 1);
                if(winCheck == (rootState.width*rootState.width)/2){
                    return (rootState.width*rootState.width)/2;
                }
                Board copy = new Board(rootState);
                copy.ChangeSquare(winCheck);
                int value = rootState.blackTurn ? EvaluateBoard(copy) : -EvaluateBoard(copy);
                deb += "Highest 1 ply value move is: " + winCheck.ToString() + ", value " + value.ToString() + ".\n  ";
                if(value > 9999){
                    rootState.DebugWrite(deb);
                    return winCheck;
                }
            }
            Node root = new Node(rootState.lastSquare, null, rootState, 1);
            Stopwatch s = new Stopwatch();
            s.Start();
            while(s.Elapsed.Seconds < searchTimer){
                Node node = root;
                Board state = new Board(rootState);
                //Select
                while(node.untriedMoves.Count == 0 && node.nodes.Count != 0){
                    node = NodeSelection(node);
                    state.ChangeSquare(node.move);
                    state.EndTurn();
                }
                //Expand
                if(node.untriedMoves.Count != 0){
                    int move = node.GetRandomMove();
                    state.ChangeSquare(move);
                    state.EndTurn();
                    node = node.AddChild(move, state);
                }
                //Rollout
                int lastMove = node.move;
                while(EvaluateBoard(state) == 0){
                    //int move = RootAlphaBeta(int.MinValue, int.MaxValue, state, 2);
                    int move = GetHighestValueMove(state, 1);
                    state.ChangeSquare(move);
                    state.EndTurn();
                    lastMove = move;
                }
                //Backpropagate
                int result = state.CheckWin(lastMove);
                while(node != null){
                    node.games++;
                    if(result == -1){
                        node.wins += 0.4;
                    } else if((result == 1 && rootState.blackTurn) || (result == 2 && !rootState.blackTurn)){
                        node.wins += 1;
                    } else{
                        node.wins += 0;
                    }
                    node = node.parent;
                }
            }
            int bestI = 0;
            double bestValue = double.MinValue;
            for (int i = 0; i < root.nodes.Count; i++){
                double val = root.nodes[i].games*(root.nodes[i].wins/root.nodes[i].games);
                if(val > bestValue){
                    deb += "Move: " + root.nodes[i].move.ToString() + " has Wins " + root.nodes[i].wins.ToString("0.0") + ", games " + root.nodes[i].games.ToString() + ", = " + (root.nodes[i].wins/root.nodes[i].games).ToString("0.000") + ".\n  ";
                    bestI = i;
                    bestValue = val;
                }
            }
            rootState.DebugWrite(deb);
            //rootState.DebugWrite("Done finding move from " + root.nodes.Count.ToString() + " available.");
            return root.nodes[bestI].move;
        }

        public Node NodeSelection(Node n){
            int bestN = 0;
            double bestV = double.MinValue;
            for (int i = 0; i < n.nodes.Count; i++){
                //double value = Math.Sqrt(2*Math.Log(n.games)/n.nodes[i].games);
                double value = (double)n.nodes[i].wins/(double)n.nodes[i].games+2.0*Math.Sqrt(2*Math.Log(n.games)/n.nodes[i].games);
                if(value>bestV){
                    bestN = i;
                    bestV = value;
                }
            }
            return n.nodes[bestN];
        }

        public int AlphaBeta(int alpha, int beta, Board b, int depth, int color){
            Board bCopy = b;
            if(depth == 0){
                return EvaluateBoard(bCopy) * color;
            }
            int[] moves = bCopy.GetInterestingMoves();
            for(int i = 0; i < moves.Length; i++){
                bCopy = new Board(b);
                bCopy.ChangeSquare(moves[i]);
                color = bCopy.blackTurn ? -1 : 1;
                int value = EvaluateBoard(bCopy)*color;
                if(value > 9000){
                    return -value*depth;
                } else if(value < -9000){
                    return -value*depth;
                }
                bCopy.EndTurn();
                int score = -AlphaBeta(-beta, -alpha, bCopy, depth-1, color);
                if(beta != -2147483648 && score >= beta) return beta;
                if(score > alpha) alpha = score;
            }
            return alpha;
        }

        public int RootAlphaBeta(int alpha, int beta, Board b, int depth){
            Board bCopy = b;
            int[] moves = bCopy.GetInterestingMoves();
            if(moves.Length == 0){
                return (b.width*b.width)/2;
            }
            int bestId = 0;
            int color = b.blackTurn ? 1 : -1;
            string deb = "";
            for(int i = 0; i < moves.Length; i++){
                bCopy = new Board(b);
                bCopy.ChangeSquare(moves[i]);
                bCopy.EndTurn();
                int score = -AlphaBeta(-beta, -alpha, bCopy, depth-1, color);
                if(score > alpha){
                    alpha = score; 
                    bestId = i;
                    deb += "Move: " + moves[i].ToString() + " has value " + score.ToString() + ".\n  ";
                }
            }
            b.DebugWrite(deb);
            return moves[bestId];
        }

        public int EvaluateBoard(Board state){
            int value = 0;
            for (int i = 0; i < state.board.Length; i++){
                if(state.board[i] != 0){
                    int color = state.board[i] == 1 ? 1 : -1;
                    int row = InRow(state, i, 1, 0);
                    value += (row)*color;
                    row = InRow(state, i, 0, 1);
                    value += (row)*color;
                    row = InRow(state, i, 1, 1);
                    value += (row)*color;
                    row = InRow(state, i, -1, 1);
                    value += (row)*color;
                }
            }
            //state.DebugWrite("Board value: " + value.ToString());
            return value;
        }

        public int InRow(Board state, int pos, int dirX, int dirY){
            int inRow = 1;
            int blocks = 0;
            int c = state.board[pos];
            while(state.ColumnFromPosition(pos)-dirX >= 0 && state.ColumnFromPosition(pos)-dirX < state.width
                     && state.RowFromPosition(pos)-dirY >= 0 && state.RowFromPosition(pos)-dirY < state.width
                     && state.board[pos-dirX-(dirY*state.width)] == c){
                pos = pos-dirX-(dirY*state.width);            
            }
            if(state.ColumnFromPosition(pos)-dirX < 0 || state.ColumnFromPosition(pos)-dirX >= state.width
                     || state.RowFromPosition(pos)-dirY < 0 || state.RowFromPosition(pos)-dirY >= state.width
                     || (state.board[pos-dirX-(dirY*state.width)] != c && state.board[pos-dirX-(dirY*state.width)] != 0)){
                blocks++;
            }
            while(state.ColumnFromPosition(pos)+dirX >= 0 && state.ColumnFromPosition(pos)+dirX < state.width
                     && state.RowFromPosition(pos)+dirY >= 0 && state.RowFromPosition(pos)+dirY < state.width
                     && state.board[pos+dirX+(dirY*state.width)] == c){
                pos = pos+dirX+(dirY*state.width);
                inRow++;
            }
            if(state.ColumnFromPosition(pos)+dirX < 0 || state.ColumnFromPosition(pos)+dirX >= state.width
                     || state.RowFromPosition(pos)+dirY < 0 || state.RowFromPosition(pos)+dirY >= state.width
                     || (state.board[pos+dirX+(dirY*state.width)] != c && state.board[pos+dirX+(dirY*state.width)] != 0)){
                blocks++;
            }
            if(inRow >= 5){
                return 10000;
            } else{
                if(inRow == 3){
                    inRow = 7;
                } else if(inRow == 4){
                    inRow = 12;
                } else if(inRow == 1){
                    return 0;
                }
                if(blocks == 1){
                    return inRow/2;
                } else if(blocks == 2){
                    return 0;
                } else{
                    return inRow;
                }
            }
        }
    }
}