using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Save humans, destroy zombies!
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        Coordinate action;
        State state = new State();
        List<Human> humans = new List<Human>();
        List<Zombie> zombies = new List<Zombie>();
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            state.Me = new Me()
            {
                Coords = new Coordinate(int.Parse(inputs[0]), int.Parse(inputs[1]))
            };
            state.HumanCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < state.HumanCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                Coordinate coords = new Coordinate(int.Parse(inputs[1]), int.Parse(inputs[2]));
                int id = int.Parse(inputs[0]);
                humans.Add(new Human
                {
                    Id = id,
                    Coords = coords
                });
            }
            state.Humans = humans;
            state.ZombieCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < state.ZombieCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]);
                zombies.Add(new Zombie()
                {
                    Id = id,
                    Coords = new Coordinate(int.Parse(inputs[1]), int.Parse(inputs[2])),
                    NextCoords = new Coordinate(int.Parse(inputs[3]), int.Parse(inputs[4])),
                });
            }
            //if (state.Zombies.Count != zombies.Count) { Console.Error.WriteLine("ERR: zombie count diff " + state.Zombies.Count + " " + zombies.Count); }
            //else
            //{
            //    for (int i = 0; i < state.Zombies.Count; i++)
            //    {
            //        if (state.Zombies[i].Coords.X != zombies[i].Coords.X || state.Zombies[i].Coords.Y != zombies[i].Coords.Y) Console.Error.WriteLine($"ERR: zombie diff:{state.Zombies[i].Coords.X}!={zombies[i].Coords.X} || {state.Zombies[i].Coords.Y}!={zombies[i].Coords.Y}");
            //    }
            //}
            state.Zombies = zombies;
            action = Game.PlayAI(state);
            action = Map.Truncate(action);
            Console.WriteLine($"{action.X} {action.Y}");
            zombies = new List<Zombie>();
            humans = new List<Human>();
        }
    }
}

public static class Algorithm
{
    public static Coordinate MonteCarlo(State state)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int count = 0;
        while (sw.ElapsedMilliseconds < Game.MaxTimeInMs)
        {
            Strategy strategy = GenerateStrategy(state);
            if (strategy.Score > Game.BestStrategy.Score)
            {
                Game.BestStrategy = strategy;
                Console.Error.WriteLine("NewBest");
            }
            count++;
        }
        Console.Error.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
        Console.Error.WriteLine("Iterations: " + count);
        Console.Error.WriteLine("BestStrategy: " + Game.BestStrategy.Score);
        if (Game.BestStrategy.Coordinates.Count == 0)
        {
            return state.Humans[0].Coords;
        }
        return Game.BestStrategy.Coordinates.Dequeue();
    }
    public static Strategy GenerateStrategy(State state)
    {
        Strategy strategy = new Strategy();
        int randomMoves = Game.Random.Next(0, 4);
        Zombie zombie;
        for (int i = 0; i < randomMoves; i++)
        {
            int randomDist = Game.Random.Next(0, 2500);
            double randomAngle = Game.Random.NextDouble() * Math.PI * 2;
            Coordinate newCoord = (new CoordinateDoub(Math.Cos(randomAngle) * randomDist, Math.Sin(randomAngle) * randomDist)).ToInt();
            Coordinate nextCoord = state.Me.Coords + newCoord;
            strategy.Coordinates.Enqueue(nextCoord);
            state = Game.Simulate(state, nextCoord);
        }
        while (state.ZombieCount > 0)
        {
            if (state.HumanCount == 0) break;
            int zombieIndex = Game.Random.Next(0, state.ZombieCount);
            int zombieId = state.Zombies[zombieIndex].Id;
            while (state.Zombies.Any(z => z.Id == zombieId))
            {
                if (state.HumanCount == 0) break;
                zombie = state.Zombies.Find(z => z.Id == zombieId);
                strategy.Coordinates.Enqueue(zombie.NextCoords);
                state = Game.Simulate(state, zombie.NextCoords);
            }
        }
        strategy.Score = state.Score;
        return strategy;
    }
}
public class Strategy
{
    public int Score { get; set; }
    public Queue<Coordinate> Coordinates = new Queue<Coordinate>();
}

public class Action
{
    public Coordinate Target { get; set; }
    public Action(Coordinate target)
    {
        Target = target;
    }
    public override string ToString()
    {
        return $"{Target.X} {Target.Y}";
    }
}

public class Me : Entity, ICloneable<Me>
{
    public const int AttackingDistance = 2000;
    public const int MovingDistance = 1000;
    public void Move(Coordinate toCoord)
    {
        Move(toCoord, MovingDistance);
    }
    public Me Clone()
    {
        return new Me()
        {
            Coords = Coords.Clone(),
        };
    }
}
public abstract class Entity
{
    public Coordinate Coords { get; set; }
    protected void Move(Coordinate toCoord, int distance)
    {
        double distanceToCoord = Coords.Distance(toCoord);
        if (distanceToCoord <= distance)
        {
            Coords = toCoord;
        }
        else
        {
            Coords += (Coordinate.GetUnitaryVector(toCoord - Coords) * distance).ToInt();
        }
        Coords = Map.Truncate(Coords);
    }
}
public class Human : ICloneable<Human>
{
    public int Id { get; set; }
    public Coordinate Coords { get; set; }
    public Human Clone()
    {
        return new Human()
        {
            Id = Id,
            Coords = Coords.Clone()
        };
    }
}

public class Zombie : Entity, ICloneable<Zombie>
{
    public int Id { get; set; }
    public Coordinate NextCoords { get; set; }
    public const int MovingDistance = 400;
    public const int AttackingDistance = 400;
    public void Move(Coordinate toCoord)
    {
        Move(toCoord, MovingDistance);
    }
    public void GetNextCoords(List<Human> humans, Me me)
    {
        Coordinate targetCoords = me.Coords;
        double minDist = Coords.Distance(targetCoords);
        double tempDist;
        foreach (Human human in humans)
        {
            tempDist = human.Coords.Distance(Coords);
            if (tempDist < minDist)
            {
                minDist = tempDist;
                targetCoords = human.Coords;
            }
        }

        NextCoords = GetNextCoordsFromTarget(minDist, targetCoords);
    }
    private Coordinate GetNextCoordsFromTarget(double distance, Coordinate target)
    {
        if (distance <= MovingDistance)
        {
            return target;
        }
        else
        {
            return Coords + (Coordinate.GetUnitaryVector(target - Coords) * MovingDistance).ToInt();
        }
    }
    public Zombie Clone()
    {
        return new Zombie()
        {
            Id = Id,
            Coords = Coords.Clone(),
            NextCoords = NextCoords.Clone(),
        };
    }
}

public static class Game
{
    public static Random Random = new Random();
    public static Strategy BestStrategy = new Strategy();
    public const int MaxTimeInMs = 100;
    public static readonly List<int> FibonacciMultiplier = new List<int>(new[]{1
                                                                             , 2
                                                                             , 3
                                                                             , 5
                                                                             , 8
                                                                             , 13
                                                                             , 21
                                                                             , 34
                                                                             , 55
                                                                             , 89
                                                                             , 144
                                                                             , 233
                                                                             , 377
                                                                             , 610
                                                                             , 987
                                                                             , 1597
                                                                             , 2584
                                                                             , 4181
                                                                             , 6765
                                                                             ,10946
                                                                             ,17711
                                                                             ,28657
                                                                             ,46368
                                                                             ,75025
                                                                             ,121393
                                                                             ,196418
                                                                             ,317811
                                                                             ,514229
                                                                             ,832040
                                                                             ,1346269});
    public static Coordinate PlayAI(State state)
    {
        Coordinate action = Algorithm.MonteCarlo(state);
        Play(state, action);
        return action;
    }
    public static State Play(State state, Coordinate action)
    {
        state.Zombies.ForEach(zombie => zombie.Move(zombie.NextCoords));
        state.Me.Move(action);
        int killedZombies = KillZombies(state);
        state.Score += state.HumanCount * state.HumanCount * 10 * killedZombies;
        int killedHumanes = KillHumans(state);
        state.Zombies.ForEach(zombie => zombie.GetNextCoords(state.Humans, state.Me));
        if (state.HumanCount == 0) state.Score = -100;
        state.Turn++;
        return state;
    }

    public static State Simulate(State state, Coordinate action)
    {
        State newState = state.Clone();
        return Play(newState, action);
    }

    public static int KillZombies(State state)
    {
        state.Zombies = state.Zombies.Where(zombie => !Map.IsInRange(zombie.Coords, state.Me.Coords, Me.AttackingDistance)).ToList();
        int count = 0;
        int killed = state.ZombieCount - state.Zombies.Count;
        for (int i = 0; i < killed; i++)
        {
            count += FibonacciMultiplier[i];
        }
        state.ZombieCount = state.Zombies.Count;
        return count;
    }
    public static int KillHumans(State state)
    {
        state.Humans = state.Humans.Where(human => !state.Zombies.Any(zombie => Map.IsInRange(human.Coords, zombie.Coords, Zombie.AttackingDistance))).ToList();
        int killed = state.HumanCount - state.Humans.Count;
        state.HumanCount = state.Humans.Count;
        return killed;
    }
}

public class State : ICloneable<State>
{
    public int Turn { get; set; }
    public int HumanCount { get; set; }
    public int ZombieCount { get; set; }
    public Me Me { get; set; }
    public List<Human> Humans { get; set; } = new List<Human>();
    public List<Zombie> Zombies { get; set; } = new List<Zombie>();
    public int Score { get; set; }

    public State Clone()
    {
        return new State()
        {
            HumanCount = HumanCount,
            ZombieCount = ZombieCount,
            Me = Me.Clone(),
            Humans = Humans.ConvertAll(h => h.Clone()),
            Zombies = Zombies.ConvertAll(g => g.Clone()),
            Score = Score,
            Turn = Turn
        };
    }
}
public class CoordinateDoub
{
    public double X { get; set; }
    public double Y { get; set; }
    public CoordinateDoub(double x, double y)
    {
        X = x;
        Y = y;
    }
    public static CoordinateDoub operator *(CoordinateDoub coordinateA, double doub)
    {
        return new CoordinateDoub(coordinateA.X * doub, coordinateA.Y * doub);
    }
    public static CoordinateDoub operator *(double doub, CoordinateDoub coordinateA)
    {
        return coordinateA * doub;
    }
    public static CoordinateDoub operator *(CoordinateDoub coordinateA, int doub)
    {
        return new CoordinateDoub(coordinateA.X * doub, coordinateA.Y * doub);
    }
    public static CoordinateDoub operator *(int doub, CoordinateDoub coordinateA)
    {
        return coordinateA * doub;
    }
    public Coordinate ToInt()
    {
        return new Coordinate((int)X, (int)Y);
    }
}
public class Coordinate : ICloneable<Coordinate>
{
    public int X { get; set; }
    public int Y { get; set; }
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
    public override bool Equals(object value)
    {
        return Equals(value as Coordinate);
    }
    public bool Equals(Coordinate coordinate)
    {
        // Is null?
        if (ReferenceEquals(null, coordinate))
        {
            return false;
        }

        // Is the same object?
        if (ReferenceEquals(this, coordinate))
        {
            return true;
        }

        return Equals(X, coordinate.X)
            && Equals(Y, coordinate.Y);
    }
    public double Distance(Coordinate coordinate)
    {
        int dx = X - coordinate.X;
        int dy = Y - coordinate.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    public double Modulus()
    {
        return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
    }
    public CoordinateDoub GetUnitaryVector()
    {
        if (X == 0 && Y == 0) throw new Exception("Cannot get unitary vector of (0,0)");
        double modulus = Modulus();
        return new CoordinateDoub(X / modulus, Y / modulus);
    }
    public static double Distance(Coordinate coordinate1, Coordinate coordinate2)
    {
        return coordinate1.Distance(coordinate2);
    }
    public static double Modulus(Coordinate coordinate)
    {
        return coordinate.Modulus();
    }
    public static CoordinateDoub GetUnitaryVector(Coordinate coordinate)
    {
        return coordinate.GetUnitaryVector();
    }
    public static bool operator ==(Coordinate coordinateA, Coordinate coordinateB)
    {
        if (ReferenceEquals(coordinateA, coordinateB))
        {
            return true;
        }

        // Ensure that "coordinateA" isn't null
        if (ReferenceEquals(null, coordinateA))
        {
            return false;
        }

        return (coordinateA.Equals(coordinateB));
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
    public static Coordinate operator *(Coordinate coordinateA, double doub)
    {
        return new Coordinate((int)(coordinateA.X * doub), (int)(coordinateA.Y * doub));
    }
    public static Coordinate operator *(double doub, Coordinate coordinateA)
    {
        return coordinateA * doub;
    }
    public static Coordinate operator *(Coordinate coordinateA, int integer)
    {
        return new Coordinate(coordinateA.X * integer, coordinateA.Y * integer);
    }
    public static Coordinate operator *(int integer, Coordinate coordinateA)
    {
        return coordinateA * integer;
    }
    public static Coordinate operator /(int integer, Coordinate coordinateA)
    {
        return coordinateA / integer;
    }
    public static Coordinate operator /(Coordinate coordinateA, int integer)
    {
        if (integer == 0) throw new Exception("Cannot divide by 0");
        return new Coordinate(coordinateA.X / integer, coordinateA.Y / integer);
    }
    public Coordinate Clone()
    {
        return new Coordinate(X, Y);
    }
}

public static class Map
{
    public static readonly int Width = 16000;
    public static readonly int Height = 9000;

    public static bool IsInMap(Coordinate coord)
    {
        return coord.X.Between(0, Width - 1) && coord.Y.Between(0, Height - 1);
    }
    public static bool IsInRange(Coordinate center, Coordinate target, int range) => center.Distance(target) <= range;
    public static Coordinate Truncate(Coordinate coords)
    {
        return new Coordinate(Math.Max(Math.Min(coords.X, Width - 1), 0), Math.Max(Math.Min(coords.Y, Height - 1), 0));
    }
}

public static class Extensions
{
    public static bool Between(this int i, int lowerValue, int higherValue, bool inclusive = true)
    {
        if (inclusive)
        {
            return i >= lowerValue && i <= higherValue;
        }
        return i > lowerValue && i < higherValue;
    }
    public static Dictionary<K, V> Clone<K, V>(this Dictionary<K, V> original) where K : ICloneable<K> where V : ICloneable<V>
    {
        Dictionary<K, V> dict = new Dictionary<K, V>();
        foreach (var pair in original)
        {
            dict.Add(pair.Key.Clone(), pair.Value.Clone());
        }
        return dict;
    }
    public static Dictionary<int, V> Clone<V>(this Dictionary<int, V> original) where V : ICloneable<V>
    {
        Dictionary<int, V> dict = new Dictionary<int, V>();
        foreach (var pair in original)
        {
            dict.Add(pair.Key, pair.Value.Clone());
        }
        return dict;
    }
}

public interface ICloneable<T>
{
    T Clone();
}