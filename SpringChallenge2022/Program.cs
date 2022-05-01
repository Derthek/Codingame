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
        State state = new State();
        Map.MyBase = Map.CoordinatesToInteger(baseX, baseY);
        Map.OpponentBase = Map.CoordinatesToInteger(Map.WIDTH - 1 - baseX, Map.HEIGHT - 1 - baseY);
        Map.GetCoverPoints();
        IEnumerable<Entity> threatMonsters = new List<Entity>();

        while (true)
        {
            state = Update(state, ACTIONS.READ_INPUT);
            threatMonsters = state.Monsters.Where((entity) => entity.ThreatFor == Target.Me).OrderBy((entity) => Map.Distance(entity.Pos, Map.MyBase));
            int closestHeroToThreat = 0;
            if (threatMonsters.Any())
            {
                Entity closestThreat = threatMonsters.First();
                closestHeroToThreat = state.MyHeros.OrderBy(hero => Map.Distance(hero.Pos, closestThreat.Pos)).First().Id;
            }
            for (int i = 0; i < heroesPerPlayer; i++)
            {
                if (threatMonsters.Any() && state.MyHeros[i].Id == closestHeroToThreat)
                {
                    state.MyHeros[i].state = new DefendState(i, threatMonsters);
                }
                else
                {
                    state.MyHeros[i].state = new CoverState(i);
                }
                state = state.MyHeros[i].state.Act(state);
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
                ThreatFor = (Target)threatFor,
            });
        }
        newState.MyHeros = entities.Where(entity => entity.Type == EntityType.MyHero).ToList();
        newState.Monsters = entities.Where(entity => entity.Type == EntityType.Monster).ToList();
        newState.OpponentHeroes = entities.Where(entity => entity.Type == EntityType.OpponentHero).ToList();
        return newState;
    }
    static State Wait(State state, ACTIONS action, Parameters parameters = null)
    {
        string cmd = $"WAIT";
        PendingCommands.Add(cmd);
        return state;
    }
    static State Move(State state, ACTIONS action, Parameters parameters = null)
    {
        Coordinate coordinate = Map.IntegerToCoordinates(parameters.Pos);
        string cmd = $"MOVE {coordinate.X} {coordinate.Y}";
        PendingCommands.Add(cmd);
        return state;
    }
    static State Wind(State state, ACTIONS action, Parameters parameters = null)
    {
        Coordinate baseCoord = Map.IntegerToCoordinates(Map.OpponentBase);
        string cmd = $"SPELL WIND {baseCoord.X} {baseCoord.Y}";
        PendingCommands.Add(cmd);
        state.MyMana -= Game.SpellCost;
        return state;
    }
    static State Control(State state, ACTIONS action, Parameters parameters = null)
    {
        Coordinate coordinate = Map.IntegerToCoordinates(parameters.Pos);
        string cmd = $"SPELL CONTROL {parameters.Id} {coordinate.X} {coordinate.Y}";
        PendingCommands.Add(cmd);
        state.MyMana -= Game.SpellCost;
        return state;
    }
    static State Shield(State state, ACTIONS action, Parameters parameters = null)
    {
        string cmd = $"SPELL SHIELD {parameters.Id}";
        PendingCommands.Add(cmd);
        state.MyMana -= Game.SpellCost;
        return state;
    }
    static State Send(State state, ACTIONS action, Parameters parameters = null)
    {
        foreach (string cmd in PendingCommands)
        {
            Console.WriteLine(cmd);
        }
        PendingCommands = new List<string>();
        state.Turn++;
        return state;
    }
    public static State Update(State state, ACTIONS action, Parameters parameters = null)
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
            case ACTIONS.CONTROL:
                return Control(state, action, parameters);
            case ACTIONS.SHIELD:
                return Shield(state, action, parameters);
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

    public int MyHealth { get; set; }
    public int OpponentHealth { get; set; }
    public int MyMana { get; set; }
    public int OpponentMana { get; set; }
    public List<Entity> MyHeros { get; set; } = new List<Entity>();
    public List<Entity> Monsters { get; set; } = new List<Entity>();
    public List<Entity> OpponentHeroes { get; set; } = new List<Entity>();
    public override string ToString()
    {
        return $"Turn:{Turn},MyHealth:{MyHealth},OpponentHealth:{OpponentHealth},MyMana:{MyMana},OpponentMana:{OpponentMana},MyHeros:[{string.Join(",", MyHeros.Select(p => "{" + p.ToString() + "}"))}],Monsters:[{string.Join(",", Monsters.Select(p => "{" + p.ToString() + "}"))}],OpponentHeroes:[{string.Join(",", OpponentHeroes.Select(p => "{" + p.ToString() + "}"))}]";
    }

    public State Clone()
    {
        return new State()
        {
            Turn = Turn,
            MyHealth = MyHealth,
            OpponentHealth = OpponentHealth,
            MyMana = MyMana,
            OpponentMana = OpponentMana,
            MyHeros = MyHeros.ConvertAll(p => p.Clone()),
            Monsters = Monsters.ConvertAll(p => p.Clone()),
            OpponentHeroes = OpponentHeroes.ConvertAll(p => p.Clone()),
        };
    }
}

public interface HeroState
{
    public State Act(State state);
}
public class CoverState : HeroState
{
    private readonly int Id;
    private const int MoveRange = 3000;
    public CoverState(int id)
    {
        Id = id;
    }
    public State Act(State state)
    {
        IEnumerable<Entity> monstersInRange = state.Monsters.Where(entity => Map.Distance(state.MyHeros[Id].Pos, entity.Pos) < MoveRange).OrderBy(entity => Map.Distance(state.MyHeros[Id].Pos, entity.Pos));
        if (monstersInRange.Any() && Map.Distance(state.MyHeros[Id].Pos, Map.CoverPoints[Id]) <= MoveRange)
        {
            state = MoveToMonster(state, monstersInRange.First());
        }
        else
        {
            state = MoveToCover(state);
        }
        return state;
    }
    private State MoveToCover(State state)
    {

        state = Player.Update(state, ACTIONS.MOVE, new Parameters()
        {
            MyTurn = true,
            Pos = Map.CoverPoints[Id]
        });
        return state;
    }
    private State MoveToMonster(State state, Entity monster)
    {
        state = Player.Update(state, ACTIONS.MOVE, new Parameters()
        {
            MyTurn = true,
            Pos = monster.Pos
        });
        return state;
    }
}

public class DefendState : HeroState
{
    private readonly int Id;
    private readonly IEnumerable<Entity> Threats;
    public DefendState(int id, IEnumerable<Entity> threats)
    {
        Id = id;
        Threats = threats;
    }
    public State Act(State state)
    {
        Entity nearestThreat = Threats.First();
        int enemiesInWindRange = Threats.Where(entity => Map.IsInRange(state.MyHeros[Id].Pos, entity.Pos, Game.WindRange)).Count();
        int safetyDistance = enemiesInWindRange > 6 ? 4000 : 2000;
        if (state.MyMana >= Game.SpellCost && Map.Distance(nearestThreat.Pos, Map.MyBase) <= safetyDistance && Map.IsInRange(state.MyHeros[Id].Pos, nearestThreat.Pos, Game.WindRange))
        {
            state = Player.Update(state, ACTIONS.WIND, new Parameters()
            {
                MyTurn = true,
                Pos = state.MyHeros[Id].Pos
            });
        }
        else
        {
            state = Player.Update(state, ACTIONS.MOVE, new Parameters()
            {
                MyTurn = true,
                Pos = nearestThreat.NextPos
            });
        }
        return state;
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
    public int NextPos => Map.CoordinatesToInteger(Map.IntegerToCoordinates(Pos) + Velocity);
    public HeroState state { get; set; }

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
public static class Game
{
    public const int BaseVision = 6000;
    public const int HeroVision = 2200;
    public const int SpellCost = 10;
    public const int AttackRange = 800;
    public const int AttackDamage = 2;
    public const int MonsterBaseAttractRange = 5000;
    public const int MonsterAttackRange = 300;
    public const int WindRange = 1280;
    public const int WindPush = 2200;
    public const int ShieldRange = 2200;
    public const int Control = 2200;
    public const int MaxTurn = 220;

}
public class Map
{
    public static int WIDTH = 17631;
    public static int HEIGHT = 9001;
    public static int MyBase { get; set; }
    public static int OpponentBase { get; set; }
    public static bool IsTopBase => MyBase == 0;
    private Coordinate SymmetryOrigin = new Coordinate((WIDTH - 1) / 2, (HEIGHT - 1) / 2);
    private const int coverDistance = Game.BaseVision + Game.HeroVision / 2;
    private const double coverAngle = 22.5 * Math.PI / 180;
    public static int[] CoverPoints = new int[3];
    public static void GetCoverPoints()
    {
        for (int i = 0; i < 3; i++)
        {
            int x = (int)(coverDistance * Math.Cos((i + 1) * coverAngle));
            int y = (int)(coverDistance * Math.Sin((i + 1) * coverAngle));
            Coordinate myBase = IntegerToCoordinates(MyBase);
            int sign = IsTopBase ? 1 : -1;
            CoverPoints[i] = CoordinatesToInteger(new Coordinate(myBase.X + sign * x, myBase.Y + sign * y));
        }
    }
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
    public static bool IsInRange(int center, int target, int range) => Distance(center, target) < range;
    public static int CoordinatesToInteger(int x, int y)
    {
        return y * WIDTH + x;
    }
    public static int CoordinatesToInteger(Coordinate coordinate)
    {
        return CoordinatesToInteger(coordinate.X, coordinate.Y);
    }
    public static Coordinate IntegerToCoordinates(int integer)
    {
        return new Coordinate(integer % WIDTH, integer / WIDTH);
    }
    public static double Distance(int a, int b)
    {
        return Distance(Map.IntegerToCoordinates(a), Map.IntegerToCoordinates(b));
    }
    public static double Distance(Coordinate a, Coordinate b)
    {
        int dx = a.X - b.X;
        int dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
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
    CONTROL,
    SHIELD,
    SEND,
    WAIT,
}

public interface ICloneable<T>
{
    T Clone();
}
