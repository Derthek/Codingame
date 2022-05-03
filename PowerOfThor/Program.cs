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
                if(state.Giants.Count!=giants.Count){ Console.Error.WriteLine("ERR: giant count diff "+state.Giants.Count+" "+giants.Count);}
                else{
                    for(int i = 0; i < state.Giants.Count; i++){
                        if(state.Giants[i].X!=giants[i].X ||state.Giants[i].Y!=giants[i].Y ) Console.Error.WriteLine($"ERR: giant diff:{state.Giants[i].X}!={giants[i].X} || {state.Giants[i].Y}!={giants[i].Y}");
                    }
                }
                state.Giants = giants;
                Console.Error.WriteLine(state.ToString());
#endif
                action = game.PlayAI(state);
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
            return Math.Abs(from.X - X) < Constants.StrikeSize && Math.Abs(from.Y - Y) < Constants.StrikeSize;
        }
        public Giant Clone()
        {
            return new Giant()
            {
                X = X,
                Y = Y
            };
        }
        public new void Move(int x, int y)
        {
            // Giant won't move if position is the same
            if (x == X && y == Y) return;

            double angleToThor = Math.Atan2(y - Y, x - X);
            for (int i = 0; i < Game.Angles.Length; i++)
            {
                if (angleToThor.Between(Game.Angles[i], Game.Angles[i] + Game.AngleRange[i]))
                {
                    Move(Game.AngleDirection[i]);
                    break;
                }
            }
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
        public static bool IsInRange(Coordinate center, Coordinate target, int range) => Distance(center, target) <= range;
        public static int Distance(Coordinate a, Coordinate b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return Math.Abs(dx) + Math.Abs(dy);
        }
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
        public List<Giant> Giants { get; set; } = new List<Giant>();
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
        public static readonly int MaxTimeInMs = 100;
        public static readonly Random Random = new Random();
        public const int MAX_TURNS = 200;
        public static readonly string[] Actions = new string[] { "WAIT", "STRIKE", "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        private static readonly double AngleRangeBase = Math.PI * 27.85 / 180;
        public static readonly double[] AngleRange = new double[] { 2 * AngleRangeBase, Math.PI / 2 - 2 * AngleRangeBase, 2 * AngleRangeBase, Math.PI / 2 - 2 * AngleRangeBase, 2 * AngleRangeBase, Math.PI / 2 - 2 * AngleRangeBase, 2 * AngleRangeBase, Math.PI / 2 - 2 * AngleRangeBase, 2 * AngleRangeBase };
        public static readonly string[] AngleDirection = new string[] { "W", "NW", "N", "NE", "E", "SE", "S", "SW", "W" };
        public static readonly double[] Angles = new double[] { -Math.PI - AngleRangeBase, -Math.PI + AngleRangeBase, -Math.PI / 2 - AngleRangeBase, -Math.PI / 2 + AngleRangeBase, -AngleRangeBase, AngleRangeBase, Math.PI / 2 - AngleRangeBase, Math.PI / 2 + AngleRangeBase, Math.PI - AngleRangeBase };

        public string PlayAI(State state)
        {
            Tree = Algorithm.MCTS.Run(null, state);
            Play(state, Tree.Action);
            return Tree.Action;
        }
        public static State Play(State state, string action)
        {
            if (action != "WAIT" && action != "STRIKE")
            {
                state.Thor.Move(action);
            }
            foreach (Giant giant in state.Giants)
            {
                giant.Move(state.Thor.X, state.Thor.Y);
            }
            if (action == "STRIKE")
            {
                Coordinate thorCoords = new Coordinate(state.Thor.X, state.Thor.Y);
                state.Giants = state.Giants.Where(giant => !giant.IsStrikeable(thorCoords)).ToList();
                state.NumberOfStrikes--;
            }
            state.Turn++;
            return state;
        }
        public static State Simulate(State state, string action)
        {
            State newState = state.Clone();
            return Play(newState, action);
        }
        public static bool IsGameOver(State state)
        {
            if (state.Turn > Game.MAX_TURNS)
            {
                return true;
            }
            if (state.NumberOfStrikes <= 0 && state.Giants.Count != 0)
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
                    if (Game.IsGameOver(state))
                    {
                        Score = Scores.Lose;
                        return Score;
                    }
                    if (state.Giants.Count == 0)
                    {
                        Score = Scores.Win;
                        return Score;
                    }
                    Coordinate centroid = Map.GetCentroid(state.Giants.Select(giant => new Coordinate(giant.X, giant.Y)), state.Giants.Count);
                    double avgDistanceToCentroid = state.Giants.Average(giant => Map.Distance(new Coordinate(giant.X, giant.Y), centroid));
                    int distanceToCentroid = Map.Distance(new Coordinate(state.Thor.X, state.Thor.Y), centroid);
                    Score = 100 - avgDistanceToCentroid + 10 * state.NumberOfStrikes + Game.MAX_TURNS - state.Turn;
                    //Score = 100 - avgDistanceToCentroid + 100 - distanceToCentroid + 10 * state.NumberOfStrikes + Game.MAX_TURNS - state.Turn;
                    //Score = state.NumberOfStrikes;
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
                List<Node> candidates = root.Childs.Where(c => c.Score == Scores.Win).ToList();
                Console.Error.WriteLine(string.Join(",", candidates.Select(c => $"{c.Score}:{c.Action}")));
                if (candidates.Count > 0) return candidates.OrderBy(c => c.Score).Last(); // winning move
                candidates = root.Childs.ToList();
                Console.Error.WriteLine(string.Join(",", candidates.Select(c => $"{c.ToString()}:{c.Score}:{c.Action}")));
                if (candidates.All(c => c.Score == Scores.Lose)) return candidates.OrderBy(c => c.Iterations).Last(); // losing anyway
                List<Node> stillPlaying = candidates.Where(c => c.Score != Scores.Lose).ToList();
                return stillPlaying.OrderBy(c => c.Score).Last();
            }
            public static double Rollout(Node node, int parentIterations, State state)
            {
                double score = node.GetScore(state);
                if (score == Scores.Lose || score == Scores.Win)
                {
                    node.Iterations++;
                    node.Wins += score == Scores.Win ? 1 : 0;
                    return score;
                }

                Node child = node.SelectChild(state, parentIterations);

                State newState = Game.Simulate(state, child.Action);

                double result = Rollout(child, parentIterations, newState);
                node.Iterations++;
                node.Wins += result == Scores.Win ? 1 : result != Scores.Lose ? Scores.Continue : Scores.Lose;

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
        public static readonly double Win = 10000;
    }
    public static class Extensions
    {
        public static bool Between(this int value, int lowerBound, int upperBound)
        {
            return value > lowerBound && value <= upperBound;
        }
        public static bool Between(this double value, double lowerBound, double upperBound)
        {
            return value > lowerBound && value <= upperBound;
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
