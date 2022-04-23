using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    static List<string> PendingCommands = new List<string>();
    static void Main(string[] args)
    {

        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int baseX = int.Parse(inputs[0]);
        int baseY = int.Parse(inputs[1]);
        int heroesPerPlayer = int.Parse(Console.ReadLine());
        State state = new State()
        {
            MyBase = Map.CoordinatesToInteger(baseX, baseY)
        };
        IEnumerable<Entity> myHeros = new List<Entity>();
        IEnumerable<Entity> threatMonsters = new List<Entity>();

        while (true)
        {
            state = Update(state, ACTIONS.READ_INPUT);
            myHeros = state.Entities.Where((entity) => entity.Type == EntityType.MyHero);
            threatMonsters = state.Entities.Where((entity) => entity.Type == EntityType.Monster && entity.ThreatFor == Target.Me).OrderBy((entity) => Map.Distance(entity.Pos, state.MyBase));
            Console.Error.WriteLine(string.Join(",",threatMonsters));
            foreach (Entity hero in myHeros)
            {
                if (!threatMonsters.Any())
                {
                    state = Wait(state, ACTIONS.WAIT);
                }
                else
                {
                    Entity threat = threatMonsters.First();
                    if (state.MyMana >= 10 && Map.Distance(threat.Pos,state.MyBase) <= 1000)
                    {
                        state = Wind(state, ACTIONS.WIND, new Parameters()
                        {
                            MyTurn = true,
                            Pos = hero.Pos
                        });
                    }
                    state = Move(state, ACTIONS.MOVE, new Parameters()
                    {
                        MyTurn = true,
                        Pos = threat.Pos
                    });
                }
            }
            state = Send(state, ACTIONS.SEND);
        }
    }
    static State ReadInput(State state, ACTIONS action, Parameters parameters = null)
    {
        string[] inputs;
        State newState = state.Clone();
        for (int i = 0; i < 2; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int health = int.Parse(inputs[0]);
            int mana = int.Parse(inputs[1]); 
            if (i == 0)
            {
                newState.MyHealth = health;
                newState.MyMana = mana;

            }
            else
            {
                newState.OpponentHealth = health;
                newState.OpponentMana = mana;
            }
        }
        int entityCount = int.Parse(Console.ReadLine());
        List<Entity> entities = new List<Entity>();
        for (int i = 0; i < entityCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int id = int.Parse(inputs[0]);
            int type = int.Parse(inputs[1]);
            int x = int.Parse(inputs[2]);
            int y = int.Parse(inputs[3]);
            int shieldLife = int.Parse(inputs[4]);
            int isControlled = int.Parse(inputs[5]);
            int health = int.Parse(inputs[6]);
            int vx = int.Parse(inputs[7]);
            int vy = int.Parse(inputs[8]);
            int nearBase = int.Parse(inputs[9]); 
            int threatFor = int.Parse(inputs[10]); 
            entities.Add(new Entity()
            {
                Id = id,
                Type = (EntityType)type,
                Pos = Map.CoordinatesToInteger(x, y),
                ShieldLife = shieldLife,
                IsControlled = isControlled,
                Health = health,
                Velocity = new Coordinate(vx, vy),
                NearBase = (NearBase)nearBase,
                ThreatFor = (Target)threatFor
            });
        }
        newState.Entities = entities;
        return newState;
    }
    static State Wait(State state, ACTIONS action, Parameters parameters = null)
    {
        string cmd = $"WAIT";
        Console.Error.WriteLine(cmd);
        PendingCommands.Add(cmd);
        return state; // TODO: Update state
    }
    static State Move(State state, ACTIONS action, Parameters parameters = null)
    {
        Coordinate coordinate = Map.IntegerToCoordinates(parameters.Pos);
        string cmd = $"MOVE {coordinate.X} {coordinate.Y}";
        Console.Error.WriteLine(cmd);
        PendingCommands.Add(cmd);
        return state; // TODO: Update state
    }
    static State Wind(State state, ACTIONS action, Parameters parameters = null)
    {
        Coordinate coordinate = Map.IntegerToCoordinates(parameters.Pos);
        Coordinate baseCoord = Map.IntegerToCoordinates(state.MyBase);
        Coordinate dir = coordinate - baseCoord;
        Coordinate goal = coordinate + dir;
        string cmd = $"SPELL WIND {goal.X} {goal.Y}";
        Console.Error.WriteLine(cmd);
        PendingCommands.Add(cmd);
        return state;
    }
    static State Send(State state, ACTIONS action, Parameters parameters = null)
    {
        foreach(string cmd in PendingCommands)
        {
            Console.WriteLine(cmd);
        }
        PendingCommands = new List<string>();
        state.Turn++;
        return state;
    }
    static State Update(State state, ACTIONS action, Parameters parameters = null)
    {
        switch (action)
        {
            case ACTIONS.READ_INPUT:
                return ReadInput(state, action, parameters);
            case ACTIONS.MOVE:
                return Move(state, action, parameters);
            //case ACTIONS.SIMULATE_MOVE:
            //    return SimulateMove(state, action, parameters);
            case ACTIONS.WIND:
                return Wind(state, action, parameters);
            case ACTIONS.SEND:
                return Send(state, action, parameters);
            default:
                throw new Exception("Action not defined: " + action);
        }
    }
}
public class Parameters
{
    public bool MyTurn { get; set; }
    public int Pos { get; set; }
    public int Id { get; set; }

}

public class State : ICloneable<State>
{
    public int Turn { get; set; }

    public int MyBase { get; set; }
    public int MyHealth { get; set; }
    public int OpponentHealth { get; set; }
    public int MyMana { get; set; }
    public int OpponentMana { get; set; }
    public List<Entity> Entities { get; set; } = new List<Entity>();
    public override string ToString()
    {
        return $"Turn:{Turn},MyBase:{MyBase},MyHealth:{MyHealth},OpponentHealth:{OpponentHealth},MyMana:{MyMana},OpponentMana:{OpponentMana},Entities:[{string.Join(",", Entities.Select(p => "{" + p.ToString() + "}"))}]";
    }

    public State Clone()
    {
        return new State()
        {
            Turn = Turn,
            MyBase = MyBase,
            MyHealth = MyHealth,
            OpponentHealth = OpponentHealth,
            MyMana = MyMana,
            OpponentMana = OpponentMana,
            Entities = Entities.ConvertAll(p => p.Clone()),
        };
    }
}

public class Entity : ICloneable<Entity>
{
    public int Id { get; set; }
    public EntityType Type { get; set; }
    public int Pos { get; set; }
    public int ShieldLife { get; set; }
    public int IsControlled { get; set; }
    public int Health { get; set; }
    public Coordinate Velocity { get; set; }
    public NearBase NearBase { get; set; }
    public Target ThreatFor { get; set; }
    public int Speed => Type == EntityType.Monster ? 400 : 800;
    public static int Vision = 2200;

    public override string ToString()
    {
        return $"Id:{Id},Type:{Type},Pos:{Pos},ShieldLife:{ShieldLife},IsControlled:{IsControlled},Health:{Health},Velocity:{Velocity},NearBase:{NearBase},ThreatFor:{ThreatFor}";
    }

    public Entity Clone()
    {
        return new Entity
        {
            Id = Id,
            Type = Type,
            Pos = Pos,
            ShieldLife = ShieldLife,
            IsControlled = IsControlled,
            Health = Health,
            Velocity = Velocity,
            NearBase = NearBase,
            ThreatFor = ThreatFor
        };
    }
}

public class Map
{
    public static int WIDTH = 17631;
    public static int HEIGHT = 9001;
    public static bool IsInMap(Coordinate position)
    {
        return (
          position.X > 0 &&
          position.X < WIDTH &&
          position.Y > 0 &&
          position.Y < HEIGHT
        );
    }
    public static bool IsInMap(int position)
    {
        return IsInMap(IntegerToCoordinates(position));
    }
    public static int CoordinatesToInteger(int x, int y)
    {
        return y * WIDTH + x;
    }

    public static Coordinate IntegerToCoordinates(int integer)
    {
        return new Coordinate(integer % WIDTH, integer / WIDTH);
    }

    public static int CoordinatesToInteger(Coordinate coordinate)
    {
        return CoordinatesToInteger(coordinate.X, coordinate.Y);
    }
    public static int Distance(int a, int b)
    {
        return Distance(Map.IntegerToCoordinates(a), Map.IntegerToCoordinates(b));
    }
    public static int Distance(Coordinate a, Coordinate b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
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
    public Coordinate Clone()
    {
        return new Coordinate(X, Y);
    }
}

public enum EntityType
{
    Monster,
    MyHero,
    OpponentHero
}

public enum NearBase
{
    NoThreat,
    Targeting
}

public enum Target
{
    Neither,
    Me,
    Opponent
}

public enum ACTIONS
{
    READ_INPUT,
        MOVE,
        SIMULATE_MOVE,
        WIND,
        SEND,
        WAIT,
    }

public interface ICloneable<T>
{
    T Clone();
}
