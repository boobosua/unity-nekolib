namespace NekoLib.Core
{
    internal enum LoopType
    {
        None,           // No looping
        Count,          // Loop a specific number of times
        Infinite,       // Loop infinitely
        Condition       // Loop until condition is met
    }
}
