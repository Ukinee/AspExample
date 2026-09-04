namespace Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

public interface ISynchronizationOrderByPriority
{
    public static abstract int Value { get; }
}

public struct SynchronizationOrderByPriority10 : ISynchronizationOrderByPriority { public static int Value => 10; }
public struct SynchronizationOrderByPriority20 : ISynchronizationOrderByPriority { public static int Value => 20; }
public struct SynchronizationOrderByPriority30 : ISynchronizationOrderByPriority { public static int Value => 30; }
public struct SynchronizationOrderByPriority40 : ISynchronizationOrderByPriority { public static int Value => 40; }
public struct SynchronizationOrderByPriority50 : ISynchronizationOrderByPriority { public static int Value => 50; }
public struct SynchronizationOrderByPriority60 : ISynchronizationOrderByPriority { public static int Value => 60; }
public struct SynchronizationOrderByPriority70 : ISynchronizationOrderByPriority { public static int Value => 70; }
public struct SynchronizationOrderByPriority80 : ISynchronizationOrderByPriority { public static int Value => 80; }
public struct SynchronizationOrderByPriority90 : ISynchronizationOrderByPriority { public static int Value => 90; }
public struct SynchronizationOrderByPriority999 : ISynchronizationOrderByPriority { public static int Value => 999; }
