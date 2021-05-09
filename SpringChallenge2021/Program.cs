using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if DEBUG
using Newtonsoft.Json;
#endif
public class Program
{
    static void Main(string[] args)
    {
        Game.ReadMap();
        Game.PopulateDistances();

        Tree chosenTree;
        while (true)
        {
            Game.ReadInput();
            chosenTree = Game.State.Trees.Values.FirstOrDefault(tree => tree.Owner == Player.Me && Player.CanComplete(Game.State, Game.State.Me, tree));
            if (chosenTree != null && (Game.State.Me.Trees.Count(t => Game.State.Trees[t].Size == TreeSize.Big) > 1 || Game.State.Day >= 5))
            {
                Game.Complete(chosenTree);
                continue;
            }
            chosenTree = Game.State.Trees.Values.FirstOrDefault(tree => tree.Owner == Player.Me && Player.CanGrow(Game.State, Game.State.Me, tree));
            if (chosenTree != null)
            {
                Game.Grow(chosenTree);
                continue;
            }
            Game.State = State.Update(Game.State, new Action() { Type = ActionType.Send });
#if DEBUG
            break;
#endif
        }
    }
}

public static class Game
{
    public static readonly int MAP_SIZE = 37;
    public static readonly int END_GAME_DAY = 24;
    public static Cell[] Map { get; set; } = new Cell[MAP_SIZE];
    public static int[][] Distances { get; set; } = new int[MAP_SIZE][];
    public static State State = new State();
    public static List<Action> Actions = new List<Action>();


    public static void ReadMap()
    {
#if DEBUG
        dynamic data = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(".\\map.json"));
        Map = data.map.ToObject<Cell[]>();
#else
        string[] inputs;
        int numberOfCells = int.Parse(Console.ReadLine()); // 37
        for (int i = 0; i < numberOfCells; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int index = int.Parse(inputs[0]);
            Map[index] = new Cell()
            {
                Index = index,
                Richness = int.Parse(inputs[1]),
                Neighbors = new List<int>() {
                    int.Parse(inputs[2]),
                    int.Parse(inputs[3]),
                    int.Parse(inputs[4]),
                    int.Parse(inputs[5]),
                    int.Parse(inputs[6]),
                    int.Parse(inputs[7])
                }
            };
        }
        Console.Error.WriteLine(Map.ToString<Cell>());

#endif
    }
    public static void ReadInput()
    {
#if DEBUG
        State = JsonConvert.DeserializeObject<State>(File.ReadAllText(".\\state.json"));
#else
        State = State.Update(State, new Action() { Type = ActionType.ReadInput });
        Console.Error.WriteLine(State.ToString());
#endif
    }
    public static void Complete(Tree tree)
    {
        if (tree is null) return;

        State = State.Update(State, new Action() { Source = tree, Type = ActionType.Complete });
    }
    public static void Grow(Tree tree)
    {
        if (tree is null) return;

        State = State.Update(State, new Action() { Source = tree, Type = ActionType.Grow });
    }
    public static void Seed(Tree tree, Tree target)
    {
        if (tree is null) return;

        State = State.Update(State, new Action() { Source = tree, Target = target, Type = ActionType.Grow });
    }

    public static void PopulateDistances()
    {
        for (int i = 0; i < MAP_SIZE; i++)
        {
            Distances[i] = Algorithm.BFSDistances(MAP_SIZE, i);
        }
    }
}
public class State : ICloneable<State>
{
    public int Turn { get; set; }
    public int Day { get; set; }
    public int SunDirection => Day % 6;
    public int Nutrients { get; set; }
    public Dictionary<int,Tree> Trees { get; set; } = new Dictionary<int, Tree>();
    public Player[] Players { get; set; } = new Player[3];
    public Player Me => Players[Player.Me];
    public Player Opponent => Players[Player.Opponent];
    public override string ToString()
    {
        return $"{{Turn:{Turn},Players:{Players.ToString<Player>()},Day:{Day},Nutrients:{Nutrients},Trees:{Trees.ToString<Tree>()}}}";
    }
    public static State Update(State state, Action action)
    {
        switch (action.Type)
        {
            case ActionType.ReadInput:
                return ReadInput(state, action);
            case ActionType.Complete:
                return Complete(state, action);
            case ActionType.Grow:
                return Grow(state, action);
            case ActionType.Seed:
                return Seed(state, action);
            case ActionType.Send:
                return Send(state);
            default:
                throw new Exception("Action not defined: " + action);
        }
    }
    public static State ReadInput(State state, Action action)
    {
        State newState = state.Clone();
        newState.Trees = new Dictionary<int, Tree>();
        Tree tree;
        string[] inputs;
        newState.Day = int.Parse(Console.ReadLine()); // the game lasts 24 days: 0-23
        newState.Nutrients = int.Parse(Console.ReadLine());
        for (int i = 0; i < 2; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int owner = i + 1;
            newState.Players[owner] = new Player()
            {
                Owner = owner,
                Score = int.Parse(inputs[1]),
                Sun = int.Parse(inputs[0]),
                Trees = new List<int>()
            };
            if (inputs.Length > 2) newState.Players[owner].IsWaiting = inputs[2] != "0";
        }
        int numberOfTrees = int.Parse(Console.ReadLine());
        for (int i = 0; i < numberOfTrees; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int playerIndex = inputs[2] != "0" ? Player.Me : Player.Opponent;
            int index = int.Parse(inputs[0]);
            tree = new Tree()
            {
                Index = index,
                Size = (TreeSize)int.Parse(inputs[1]),
                IsDormant = inputs[3] != "0",
            };
            newState.Trees[index] = tree;
            newState.Players[playerIndex].Trees.Add(index);
        }
        int numberOfPossibleMoves = int.Parse(Console.ReadLine());
        for (int i = 0; i < numberOfPossibleMoves; i++)
        {
            string possibleMove = Console.ReadLine();
        }
        return newState;
    }
    public static State Complete(State state, Action action)
    {
        Game.Actions.Add(action);
        //state.Players[action.Player]
        return Send(state);//TODO: Update state
    }
    public static State Grow(State state, Action action)
    {
        Game.Actions.Add(action);
        return Send(state);//TODO: Update state
    }
    public static State Seed(State state, Action action)
    {
        Game.Actions.Add(action);
        return Send(state);//TODO: Update state
    }
    //public static State Simulate(State state)
    //{

    //}
    public static State Send(State state)
    {
        if (Game.Actions.Count == 0)
        {
            Console.WriteLine("WAIT");
        }
        else
        {
            Console.WriteLine(Game.Actions[0].ToString());
        }
        Game.Actions = new List<Action>();
        //Game.Decisions = new List<Decision>();
        //Game.EnemyDecisions = new List<Decision>();
        //Game.EnemyNextDecision = null;
        state.Turn++;
        return state;
    }
    public static double Score(State state, int playerOwner)
    {
        int scoreMe = state.Me.Score + (int)(state.Me.Sun / 3.0);
        int scoreOpponent = state.Opponent.Score + (int)(state.Opponent.Sun / 3.0);
        if (scoreMe == scoreOpponent)
        {
            scoreMe += state.Me.Trees.Count;
            scoreOpponent += state.Opponent.Trees.Count;
        }
        int multiplier = playerOwner == Player.Me ? 1 : -1;

        return multiplier * (scoreMe - scoreOpponent);
    }
    public static bool IsEndGame(State state)
    {
        return state.Day == Game.END_GAME_DAY;
    }
    public static Winner Evaluation(double score)
    {
        if (score == 0) return Winner.Draw;
        else if (score > 0) return Winner.Me;
        else return Winner.Opponent;
    }
    public enum Winner
    {
        Me = 10,
        Opponent = -10,
        Draw = 0
    }
    public State Clone()
    {
        return new State()
        {
            Turn = Turn,
            Players = Players.Clone<Player>(),
            Day = Day,
            Nutrients = Nutrients,
            Trees = Trees.Clone()
        };
    }
}
public class Player : ICloneable<Player>
{
    public static readonly int Free = 0;
    public static readonly int Me = 1;
    public static readonly int Opponent = 2;
    public static int Toggle(int player)
    {
        return 3 - player;
    }
    public int Score { get; set; }
    public int Owner { get; set; }
    public int Sun { get; set; }
    public bool IsWaiting { get; set; }
    public List<int> Trees { get; set; }
    public override string ToString()
    {
        return $"{{Score:{Score},Sun:{Sun},Owner:{Owner},IsWaiting:{IsWaiting},Trees:{Trees?.ToString<int>() ?? "[]"}}}";
    }

    public static bool CanComplete(State state, Player player, Tree tree)
    {
        if (tree == null) return false;
        return tree.Size == TreeSize.Big && !tree.IsDormant && state.Nutrients > 0 && player.Sun >= Tree.CompleteCost;
    }
    public static bool CanGrow(State state, Player player, Tree tree)
    {
        if (tree == null) return false;

        return tree.Size != TreeSize.Big && !tree.IsDormant && player.Sun >= Tree.GrowCost(tree, player.Trees, state.Trees);
    }
    public static bool CanSeed(State state, Player player, Tree tree, int target)
    {
        if (tree == null) return false;

        return !tree.IsDormant && Game.Map[target].Richness > 0 && Game.Distances[tree.Index][target] <= (int)tree.Size && state.Trees.ContainsKey(target) && player.Sun >= Tree.SeedCost(player.Trees,state.Trees);
    }
    public Player Clone()
    {
        return new Player()
        {
            Score = Score,
            Owner = Owner,
            Sun = Sun,
            IsWaiting = IsWaiting,
            Trees = Trees?.ToList()
        };
    }
}
public class Tree : ICloneable<Tree>
{
    public static readonly int CompleteCost = 4;
    public int Index { get; set; }
    public TreeSize Size { get; set; }
    public bool IsDormant { get; set; }
    public int Owner { get; set; }
    public override string ToString()
    {
        return $"{{Index:{Index},Size:{(int)Size},IsDormant:{IsDormant},Owner:{Owner}}}";
    }
    public static int GrowCost(Tree tree, List<int> trees, Dictionary<int,Tree> treeCollection)
    {
        switch (tree.Size)
        {
            case TreeSize.Seed:
                return 1 + trees.Count(t => treeCollection[t].Size == TreeSize.Small);
            case TreeSize.Small:
                return 3 + trees.Count(t => treeCollection[t].Size == TreeSize.Medium);
            case TreeSize.Medium:
                return 7 + trees.Count(t => treeCollection[t].Size == TreeSize.Big);
            case TreeSize.Big:
            default:
                return 999;
        }
    }
    public static int SeedCost(List<int> playerTrees, Dictionary<int,Tree> treeCollection)
    {
        return playerTrees.Count(tree => treeCollection[tree].Size == TreeSize.Seed);
    }
    public Tree Clone()
    {
        return new Tree()
        {
            Index = Index,
            Size = Size,
            IsDormant = IsDormant,
            Owner = Owner
        };
    }
}
public class Cell : ICloneable<Cell>
{
    public int Index { get; set; }
    public int Richness { get; set; }
    public List<int> Neighbors { get; set; }
    public override string ToString()
    {
        return $"{{Index:{Index},Richness:{Richness},Neighbors:{Neighbors?.ToString<int>() ?? "[]"}}}";
    }

    public Cell Clone()
    {
        return new Cell
        {
            Index = Index,
            Richness = Richness,
            Neighbors = Neighbors.ToList()
        };
    }
}
public static class Algorithm
{
    public static int[] BFSDistances(int size, int from)
    {
        int[] res = new int[size];
        Queue<int> queue = new Queue<int>();
        Dictionary<int, int> distance = new Dictionary<int, int>();
        int current;
        int currentDistance;
        int neighborDistance;
        queue.Enqueue(from);
        distance.Add(from, 0);
        while (queue.Count > 0)
        {
            current = queue.Dequeue();
            if (!distance.TryGetValue(current, out currentDistance))
            {
                throw new Exception("Pathfinding.BFS: Failed to get path.");
            }
            foreach (int neighbor in Game.Map[current].Neighbors)
            {
                if (!distance.ContainsKey(neighbor) && neighbor >= 0)
                {
                    neighborDistance = currentDistance + 1;
                    queue.Enqueue(neighbor);
                    distance.Add(neighbor, neighborDistance);
                    res[neighbor] = neighborDistance;
                }
            }
        }
        return res;
    }
}
public class Action
{
    public ActionType Type { get; set; }
    public Tree Source { get; set; }
    public Tree Target { get; set; }
    public int Player { get; set; }
    public override string ToString()
    {
        string result = $"{Type.ToString().ToUpper()}";
        switch (Type)
        {
            case ActionType.Complete:
            case ActionType.Grow:
                result += $" {Source.Index}";
                break;
            case ActionType.Seed:
                result += $" {Source.Index} {Target.Index}";
                break;
            default:
                break;
        }
        return result;
    }
}
public static class Extensions
{
    public static string ToString<T>(this T[] element)
    {
        return $"[{string.Join(",", element)}]";
    }
    public static string ToString<T>(this List<T> element)
    {
        return $"[{string.Join(",", element)}]";
    }
    public static string ToString<T>(this Dictionary<int,T> element)
    {
        return $"{{{string.Join(",", element)}}}";
    }
    public static int[] DeepCopy(this int[] original)
    {
        return original.Select(elem => elem).ToArray();
    }

    public static T[] Clone<T>(this T[] original) where T : class, ICloneable<T>
    {
        return original.Select(elem => elem?.Clone()).ToArray();
    }
    public static Dictionary<int,T> Clone<T>(this Dictionary<int,T> original) where T: class, ICloneable<T>
    {
        return new Dictionary<int, T>(original.Select(kv => new KeyValuePair<int, T>(kv.Key, kv.Value.Clone())));
    }
    public static List<T> Clone<T>(this List<T> original) where T : class, ICloneable<T>
    {
        return original.Select(elem => elem?.Clone()).ToList();
    }

    public static bool In<T>(this T elem, params T[] args)
    {
        return args.Contains(elem);
    }
}
public interface ICloneable<T>
{
    T Clone();
}
public enum TreeSize
{
    Seed = 0,
    Small = 1,
    Medium = 2,
    Big = 3
}
public enum ActionType
{
    ReadInput,
    Complete,
    Grow,
    Seed,
    Send
}
