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
        Tree chosenTree;
        while (true)
        {
            Game.ReadInput();
            chosenTree = Game.State.Me.Trees.FirstOrDefault(tree => Player.CanComplete(Game.State, Game.State.Me, tree));
            if (chosenTree != null && (Game.State.Me.Trees.Count(t => t.Size == TreeSize.Big) > 1 || Game.State.Day >= 5))
            {
                Game.Complete(chosenTree);
                continue;
            }
            chosenTree = Game.State.Me.Trees.FirstOrDefault(tree => Player.CanGrow(Game.State.Me, tree));
            if (chosenTree != null)
            {
                Game.Grow(chosenTree);
                continue;
            }
            // GROW cellIdx | SEED sourceIdx targetIdx | COMPLETE cellIdx | WAIT <message>
            Game.State = State.Update(Game.State, new Action() { Type = ActionType.Send });
#if DEBUG
            break;
#endif
        }
    }
}

public static class Game
{
    public static Cell[] Map { get; set; } = new Cell[37];
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

        State = State.Update(State, new Action() { Index = tree.Index, Type = ActionType.Complete });
    }
    public static void Grow(Tree tree)
    {
        if (tree is null) return;

        State = State.Update(State, new Action() { Index = tree.Index, Type = ActionType.Grow });
    }
}
public class State : ICloneable<State>
{
    public int Turn { get; set; }
    public int Day { get; set; }
    public int Nutrients { get; set; }

    public Player[] Players { get; set; } = new Player[2];

    public Player Me => Players[0];
    public Player Opponent => Players[1];
    public override string ToString()
    {
        return $"{{Turn:{Turn},Players:{Players.ToString<Player>()},Day:{Day},Nutrients:{Nutrients}}}";
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
            case ActionType.Send:
                return Send(state);
            default:
                throw new Exception("Action not defined: " + action);
        }
    }
    public static State ReadInput(State state, Action action)
    {
        State newState = state.Clone();
        string[] inputs;
        newState.Day = int.Parse(Console.ReadLine()); // the game lasts 24 days: 0-23
        newState.Nutrients = int.Parse(Console.ReadLine());
        for (int i = 0; i < 2; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            newState.Players[i] = new Player()
            {
                Owner = i + 1,
                Score = int.Parse(inputs[1]),
                Sun = int.Parse(inputs[0]),
                Trees = new List<Tree>()
            };
            if (inputs.Length > 2) newState.Players[i].IsWaiting = inputs[2] != "0";
        }
        int numberOfTrees = int.Parse(Console.ReadLine());
        for (int i = 0; i < numberOfTrees; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int playerIndex = inputs[2] != "0" ? Player.Me : Player.Opponent;
            newState.Players[playerIndex - 1].Trees.Add(new Tree()
            {
                Index = int.Parse(inputs[0]),
                Size = (TreeSize)int.Parse(inputs[1]),
                IsDormant = inputs[3] != "0",
            });
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
        return Send(state);//TODO: Update state
    }
    public static State Grow(State state, Action action)
    {
        Game.Actions.Add(action);
        return Send(state);//TODO: Update state
    }
    public static State Send(State state)
    {
        if (Game.Actions.Count == 0)
        {
            Console.WriteLine("WAIT");
        }
        else
        {
            Console.WriteLine(string.Join(" | ", Game.Actions.Select(a => a.ToString())));
        }
        Game.Actions = new List<Action>();
        //Game.Decisions = new List<Decision>();
        //Game.EnemyDecisions = new List<Decision>();
        //Game.EnemyNextDecision = null;
        state.Turn++;
        return state;
    }

    public State Clone()
    {
        return new State()
        {
            Turn = Turn,
            Players = Players.Clone<Player>(),
            Day = Day,
            Nutrients = Nutrients
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
    public List<Tree> Trees { get; set; }
    public override string ToString()
    {
        return $"{{Score:{Score},Sun:{Sun},Owner:{Owner},IsWaiting:{IsWaiting},Trees:{Trees?.ToString<Tree>() ?? "[]"}}}";
    }

    public static bool CanComplete(State state, Player player, Tree tree)
    {
        return state.Nutrients > 0 && player.Sun >= Tree.CompleteCost && tree.Size == TreeSize.Big && !tree.IsDormant;
    }
    public static bool CanGrow(Player player, Tree tree)
    {
        return player.Sun >= Tree.GrowCost(tree, player.Trees) && tree.Size != TreeSize.Big && !tree.IsDormant;
    }
    public Player Clone()
    {
        return new Player()
        {
            Score = Score,
            Owner = Owner,
            Sun = Sun,
            IsWaiting = IsWaiting,
            Trees = Trees?.Clone()
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
    public static int GrowCost(Tree tree, List<Tree> trees)
    {
        switch (tree.Size)
        {
            case TreeSize.Small:
                return 3 + trees.Count(t => t.Size == TreeSize.Medium);
            case TreeSize.Medium:
                return 7 + trees.Count(t => t.Size == TreeSize.Big);
            case TreeSize.Big:
            default:
                return 999;
        }
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
public class Action
{
    public ActionType Type { get; set; }
    public int Index { get; set; }
    //public int Times { get; set; }
    public override string ToString()
    {
        string result = $"{Type.ToString().ToUpper()}";
        switch (Type)
        {
            case ActionType.Complete:
            case ActionType.Grow:
                result += $" {Index}";
                break;
            //case ActionType.Cast:
            //    result += $" {Id} {Times}";
            //    break;
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
    public static int[] DeepCopy(this int[] original)
    {
        return original.Select(elem => elem).ToArray();
    }

    public static T[] Clone<T>(this T[] original) where T : class, ICloneable<T>
    {
        return original.Select(elem => elem?.Clone()).ToArray();
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
    Small = 1,
    Medium = 2,
    Big = 3
}
public enum ActionType
{
    ReadInput,
    Complete,
    Grow,
    Send
}
