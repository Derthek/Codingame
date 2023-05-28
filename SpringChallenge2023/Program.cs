using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public static class Map
{
    public static Cell[] Cells { get; set; }
    public static int MyBase { get; set; }
    public static int EnemyBase { get; set; }
    public static int NumberOfCells { get; set; }
    public static int[][] Distances { get; set; }

    public static void PopulateMap()
    {
        string[] inputs;
        NumberOfCells = int.Parse(Console.ReadLine());
        Cells = new Cell[NumberOfCells];
        Distances = new int[NumberOfCells][];
        for (int i = 0; i < NumberOfCells; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            Cells[i] = new Cell()
            {
                Index = i,
                Type = (CellType)int.Parse(inputs[0]),
                InitialResources = int.Parse(inputs[1]),
                Neighbors = new int[]{
                    int.Parse(inputs[2]),
                    int.Parse(inputs[3]),
                    int.Parse(inputs[4]),
                    int.Parse(inputs[5]),
                    int.Parse(inputs[6]),
                    int.Parse(inputs[7])
                }
            };
            Console.Error.WriteLine(Cells[i].ToString());
        }

        int numberOfBases = int.Parse(Console.ReadLine());
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            Map.MyBase = int.Parse(inputs[i]);
        }
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            Map.EnemyBase = int.Parse(inputs[i]);
        }
    }
    public static void PopulateDistances()
    {
        for (int i = 0; i < NumberOfCells; i++)
        {
            Distances[i] = Algorithm.BFSDistances(NumberOfCells, i);
        }
    }
}
public class Cell
{
    public int Index { get; set; }
    public CellType Type { get; set; }
    public int InitialResources { get; set; }
    public int Resources { get; set; }
    public int[] Neighbors { get; set; }
    public override string ToString()
    {
        return $"{{Index:{Index},Type:{Type},InitialResources:{InitialResources},Resources:{Resources},Neighbors:{Neighbors?.ToString<int>() ?? "[]"}}}";
    }
}
public enum CellType
{
    Normal = 0,
    Egg = 1,
    Crystal = 2
}
class Player
{
    static void Main(string[] args)
    {
        Map.PopulateMap();
        Map.PopulateDistances();

        // game loop
        string[] inputs;
        List<string> actions = new List<string>();
        while (true)
        {
            for (int i = 0; i < Map.NumberOfCells; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int resources = int.Parse(inputs[0]); // the current amount of eggs/crystals on this cell
                int myAnts = int.Parse(inputs[1]); // the amount of your ants on this cell
                int oppAnts = int.Parse(inputs[2]); // the amount of opponent ants on this cell
                Map.Cells[i].Resources = resources;
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            var resourcessToGo = Map.Cells.Where(cell => cell.Resources > 0 && cell.Type == CellType.Crystal).OrderByDescending(cell => cell.InitialResources).ThenBy(cell => Map.Distances[Map.MyBase][cell.Index]).ToList();
            var eggsToGo = Map.Cells.Where(cell => cell.Resources > 0 && cell.Type == CellType.Egg && Map.Distances[Map.MyBase][cell.Index] < 4).OrderBy(cell => Map.Distances[Map.MyBase][cell.Index]).ToList();
            actions.Add($"LINE {Map.MyBase} {resourcessToGo[0].Index} 1");
            if (eggsToGo.Count() > 0)
            {
                actions.Add($"LINE {Map.MyBase} {eggsToGo[0].Index} 1");
            }

            Console.WriteLine(string.Join(";", actions));
            actions = new List<string>();
        }
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
            foreach (int neighbor in Map.Cells[current].Neighbors)
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
    public static string ToString<T>(this Dictionary<int, T> element)
    {
        return $"{{{string.Join(",", element.Select(kv => $"'{kv.Key}':{kv.Value}"))}}}";
    }
    public static int[] DeepCopy(this int[] original)
    {
        return original.Select(elem => elem).ToArray();
    }

    public static T[] Clone<T>(this T[] original) where T : class, ICloneable<T>
    {
        return original.Select(elem => elem?.Clone()).ToArray();
    }
    public static Dictionary<int, T> Clone<T>(this Dictionary<int, T> original) where T : class, ICloneable<T>
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