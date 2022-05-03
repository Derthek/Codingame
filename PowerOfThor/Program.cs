namespace PowerOfThor
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
#if DEBUG
    using Newtonsoft.Json;
#endif
    class Player
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            State state = new State();
#if !DEBUG
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            state.Thor = new Thor(int.Parse(inputs[0]), int.Parse(inputs[1]));
#endif
            List<Giant> giants = new List<Giant>();
            string action = "";

            while (true)
            {
#if DEBUG
                state = JsonConvert.DeserializeObject<State>(File.ReadAllText("state.json"));
#else
                inputs = Console.ReadLine().Split(' ');
                state.NumberOfStrikes = int.Parse(inputs[0]);
                int numberOfGiants = int.Parse(inputs[1]);
                for (int i = 0; i < numberOfGiants; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int x = int.Parse(inputs[0]);
                    int y = int.Parse(inputs[1]);
                    giants.Add(new Giant(x, y));
                }
                state.Giants = giants;
                Console.Error.WriteLine(state.ToString());
#endif
                action = game.PlayAI(state, action);
                Console.WriteLine(action);

                giants = new List<Giant>();
            }
        }

    }
    public abstract class Entity
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Entity(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Entity() { }
        public string Move(int toX, int toY)
        {
            string dir = "";
            if (Y > toY)
            {
                dir += "N";
                Y--;
            }
            else if (Y < toY)
            {
                dir += "S";
                Y++;
            }
            if (X > toX)
            {
                dir += "W";
                X--;
            }
            else if (X < toX)
            {
                dir += "E";
                X++;
            }
            return dir;
        }
        //public string Move(int toPos)
        //{
        //    string dir = "";
        //    Coordinate toCoordinate = Map.IntegerToCoords(toPos);
        //    if (Y > toCoordinate.Y)
        //    {
        //        dir += "N";
        //        Y--;
        //    }
        //    else if (Y < toCoordinate.Y)
        //    {
        //        dir += "S";
        //        Y++;
        //    }
        //    if (X > toCoordinate.X)
        //    {
        //        dir += "W";
        //        X--;
        //    }
        //    else if (X < toCoordinate.X)
        //    {
        //        dir += "E";
        //        X++;
        //    }
        //    return dir;
        //}
        public void Move(string action)
        {
            if (action.Contains("N"))
            {
                Y--;
            }
            else if (action.Contains("S"))
            {
                Y++;
            }
            if (action.Contains("W"))
            {
                X--;
            }
            else if (action.Contains("E"))
            {
                X++;
            }
        }
        public new string ToString()
        {
            return $"X:{X},Y:{Y}";
        }
    }
    public class Thor : Entity, ICloneable<Thor>
    {
        public Thor(int x, int y) : base(x, y) { }
        public Thor() : base() { }
        public Thor Clone()
        {
            return new Thor()
            {
                X = X,
                Y = Y
            };
        }
    }
    public class Giant : Entity, ICloneable<Giant>
    {
        public Giant(int x, int y) : base(x, y) { }
        public Giant() : base() { }
        public bool IsStrikeable(Coordinate from)
        {
            return Map.IsInRange(from, new Coordinate(X, Y), Constants.StrikeSize);
        }
        public Giant Clone()
        {
            return new Giant()
            {
                X = X,
                Y = Y
            };
        }
    }
    public static class Map
    {
        public static readonly int WIDTH = 40;
        public static readonly int HEIGHT = 18;
        public static bool IsInMap(int x, int y)
        {
            return x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;
        }
        public static bool IsInMap(Coordinate coord)
        {
            return IsInMap(coord.X, coord.Y);
        }
        //public static bool IsInMap(int pos)
        //{
        //    return IsInMap(IntegerToCoords(pos));
        //}
        //public static bool IsInRange(int center, int target, int range) => Distance(center, target) <= range;
        public static bool IsInRange(Coordinate center, Coordinate target, int range) => Distance(center, target) <= range;
        //public static int Distance(int a, int b)
        //{
        //    return Distance(Map.IntegerToCoords(a), Map.IntegerToCoords(b));
        //}
        public static int Distance(Coordinate a, Coordinate b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return Math.Abs(dx) + Math.Abs(dy);
        }
        //public static Coordinate IntegerToCoords(int integer)
        //{
        //    return new Coordinate(integer % WIDTH, integer / WIDTH);
        //}
        //public static int CoordsToInteger(int x, int y)
        //{
        //    return x + y * WIDTH;
        //}
        //public static int CoordsToInteger(Coordinate coord)
        //{
        //    return CoordsToInteger(coord.X, coord.Y);
        //}
        public static Coordinate GetCentroid(IEnumerable<Coordinate> coordinates, int length)
        {
            return coordinates.Aggregate(new Coordinate(0, 0), (acc, next) => acc + next, result => result / length);
        }
    }
    public struct Coordinate : IEquatable<Coordinate>
    {
        public int X, Y;
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
        public override string ToString()
        {
            return $"({X},{Y})";
        }
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 29 ^ X.GetHashCode();
            hash = hash * 29 ^ Y.GetHashCode();
            return hash;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Coordinate)) return false;
            return Equals((Coordinate)obj);
        }
        public bool Equals(Coordinate coordinate)
        {
            if (coordinate.Equals(null)) return false;
            return coordinate.X == X && coordinate.Y == Y;
        }
        public static bool operator ==(Coordinate coordinateA, Coordinate coordinateB)
        {
            if (!coordinateA.Equals(null))
            {
                if (!coordinateB.Equals(null))
                {
                    return coordinateA.Equals(coordinateB);
                }
                return false;
            }
            if (!coordinateB.Equals(null)) return false;
            return true;
        }
        public static bool operator !=(Coordinate coordinateA, Coordinate coordinateB)
        {
            return !(coordinateA == coordinateB);
        }
        public static Coordinate operator +(Coordinate coordinateA, Coordinate coordinateB)
        {
            return new Coordinate(coordinateA.X + coordinateB.X, coordinateA.Y + coordinateB.Y);
        }
        public static Coordinate operator -(Coordinate coordinateA, Coordinate coordinateB)
        {
            return new Coordinate(coordinateA.X - coordinateB.X, coordinateA.Y - coordinateB.Y);
        }
        public static Coordinate operator /(Coordinate coordinateA, int number)
        {
            return new Coordinate(coordinateA.X / number, coordinateA.Y / number);
        }
        public static Coordinate operator *(Coordinate coordinateA, int number)
        {
            return new Coordinate(coordinateA.X * number, coordinateA.Y * number);
        }
    }
    public class State : ICloneable<State>
    {
        public int Turn { get; set; }
        public Thor Thor { get; set; }
        public List<Giant> Giants { get; set; }
        public int NumberOfStrikes { get; set; }
        public new string ToString()
        {
            return $"Turn:{Turn},NumberOfStrikes:{NumberOfStrikes},Thor:{{{Thor.ToString()}}},Giants:[{string.Join(",", Giants.Select(g => $"{{{g.ToString()}}}"))}]";
        }
        public State Clone()
        {
            return new State()
            {
                Thor = Thor.Clone(),
                Giants = Giants.ConvertAll(p => p.Clone()),
                NumberOfStrikes = NumberOfStrikes,
                Turn = Turn
            };
        }
    }
    class Game
    {
        public Algorithm.MCTS.Node Tree { get; set; }
        public static readonly int MaxIterations = 128;
        public static readonly int MaxTimeInMs = 80;
        public static readonly Random Random = new Random();
        public const int MAX_TURNS = 200;
        public static readonly string[] Actions = new string[] { "WAIT", "STRIKE", "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        public string PlayAI(State state, string action)
        {
            if (action != "" && Tree != null)
            {
                Tree = Tree.Childs.FirstOrDefault(c => c.Action == action);
                Console.Error.WriteLine(action);
                Console.Error.WriteLine(Tree.Action);
                Console.Error.WriteLine(Tree.Iterations);
                Console.Error.WriteLine(Tree.Childs);
            }
            Tree = Algorithm.MCTS.Run(Tree, state);

            Play(state, Tree.Action);
            Console.Error.WriteLine(Tree.ToString());
            return Tree.Action;
        }
        public State Play(State state, string action)
        {
            // We don't need to update the values for the other actions since we will get updated values in the next cycle
            if (action != "WAIT" && action != "STRIKE")
            {
                state.Thor.Move(action);
            }
            state.Turn++;
            return state;
        }
        public static State Simulate(State state, string action)
        {
            State newState = state.Clone();
            if (action != "WAIT")
            {
                if (action == "STRIKE")
                {
                    Coordinate thorCoords = new Coordinate(state.Thor.X, state.Thor.Y);
                    newState.Giants = newState.Giants.Except(newState.Giants.Where(giant => giant.IsStrikeable(thorCoords))).ToList();
                    newState.NumberOfStrikes--;
                }
                else
                {
                    newState.Thor.Move(action);
                }
            }
            newState.Giants.ForEach(giant => giant.Move(newState.Thor.X, newState.Thor.Y));
            return newState;
        }
        public static bool IsGameOver(State state)
        {
            if (state.Turn > Game.MAX_TURNS)
            {
                return true;
            }
            if (state.NumberOfStrikes == 0 && state.Giants.Count != 0)
            {
                return true;
            }
            if (!Map.IsInMap(state.Thor.X, state.Thor.Y))
            {
                return true;
            }
            if (state.Giants.Any(giant => giant.X == state.Thor.X && giant.Y == state.Thor.Y))
            {
                return true;
            }
            return false;
        }
    }
    public class Algorithm
    {
        public class MCTS
        {
            public class Node
            {
                public List<Node> Childs { get; set; }
                public string Action { get; set; }
                public int Iterations { get; set; }
                public double Wins { get; set; }
                public double Score { get; set; } = Scores.Initial;
                public Node() { }

                public double GetScore(State state)
                {
                    if (Score != Scores.Initial) return Score;

                    if (state.Giants.Count == 0)
                    {
                        Score = Scores.Win;
                        return Score;
                    }
                    if (Game.IsGameOver(state))
                    {
                        Score = Scores.Lose;
                        return Score;
                    }
                    int distanceToCentroid = Map.Distance(new Coordinate(state.Thor.X, state.Thor.Y), Map.GetCentroid(state.Giants.Select(giant => new Coordinate(giant.X, giant.Y)), state.Giants.Count));
                    Score = distanceToCentroid;
                    return Score;
                }
                public Node SelectChild(State state, int parentIterations)
                {
                    //Populate legal actions
                    if (Childs == null)
                    {
                        Childs = new List<Node>(Game.Actions.Length);
                        foreach (string action in Game.Actions)
                        {
                            Childs.Add(new Node() { Action = action });
                        }
                    }
                    //Explore all trees at least once
                    if (Iterations < Childs.Count)
                    {
                        int swap = Game.Random.Next(Iterations, Childs.Count);
                        Node tmp = Childs[swap];
                        Childs[swap] = Childs[Iterations];
                        Childs[Iterations] = tmp;
                        return Childs[Iterations];
                    }
                    //Explore most promising paths
                    double best = 0, utc;
                    Node result = null;
                    foreach (Node child in Childs)
                    {
                        utc = child.Wins / child.Iterations + Math.Sqrt(2 * Math.Log(parentIterations) / child.Iterations);
                        if (utc > best)
                        {
                            best = utc;
                            result = child;
                        }
                    }
                    return result;
                }
                public override string ToString()
                {
                    if (Score == Scores.Win) return "I win!";
                    if (Score == Scores.Lose) return "I lose!";
                    return $"{Wins} / {Iterations}  ({(100 * Wins / Iterations).ToString("0.00")}%)";
                }
            }
            public static Node Run(Node root, State state)
            {
                Stopwatch sw = Stopwatch.StartNew();
                if (root == null) root = new Node();

                while (root.Iterations % Game.MaxIterations != 0 || sw.ElapsedMilliseconds < Game.MaxTimeInMs)
                {
                    Rollout(root, root.Iterations, state.Clone());
                    if (root.Score == Scores.Win) break;
                }
                Console.Error.WriteLine("END");
                List<Node> candidates = root.Childs.Where(c => c.Score == Scores.Win).ToList();
                if (candidates.Count > 0) return candidates.OrderBy(c => c.Iterations).Last(); // winning move
                candidates = root.Childs.ToList();
                if (candidates.All(c => c.Score == Scores.Lose)) return candidates.OrderBy(c => c.Iterations).Last(); // losing anyway
                List<Node> stillPlaying = candidates.Where(c => c.Score != Scores.Lose).ToList();
                return stillPlaying.OrderBy(c => c.Iterations).Last();
            }
            public static double Rollout(Node node, int parentIterations, State state)
            {
                double score = node.GetScore(state);
                if (score == Scores.Lose || score == Scores.Win)
                {
                    node.Iterations++;
                    node.Wins += score;
                    return score;
                }

                Node child = node.SelectChild(state, parentIterations);

                State newState = Game.Simulate(state, child.Action);

                double result = Rollout(child, parentIterations, newState);
                node.Iterations++;
                node.Wins += result;

                if (child.Score == Scores.Win)
                    node.Score = Scores.Win;

                bool win = true;
                foreach (Node ch in node.Childs)
                {
                    win &= ch.Score == Scores.Win;
                    if (!win) break;
                }
                if (win) node.Score = Scores.Win;

                return result;
            }
        }
    }

    internal struct Scores
    {
        public static readonly double Initial = -2;
        public static readonly double Continue = -1;
        public static readonly double Lose = 0;
        public static readonly double Win = 1000;
    }
    public static class Extensions
    {
        public static bool Between(this int value, int lowerBound, int upperBound)
        {
            return value >= lowerBound && value <= upperBound;
        }
    }
    public static class Constants
    {
        public const int StrikeSize = 5;

    }
    public interface ICloneable<T>
    {
        T Clone();
    }
}
