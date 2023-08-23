using System;

namespace Checkers.Console.Models;

/// <summary>
/// Base class for all Entity's
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    public static bool operator ==(Entity? first, Entity? second)
    {
        return first is not null && second is not null && first.Equals(second);
    }
    
    public static bool operator !=(Entity? first, Entity? second)
    {
        return !(first == second);
    }

    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }
        
        return other.GetType() == GetType();
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Entity)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}