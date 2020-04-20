using System;
using System.Collections.Generic;

public static class StencilRegistry
{
    public const int MaxStencilValue = 255;
    private static int nextFree = 2;
    private static Queue<int> freeList = new Queue<int>();

    public static int Register()
    {
        if (freeList.Count > 0)
        {
            return freeList.Dequeue();
        }

        if (nextFree > MaxStencilValue)
        {
            throw new Exception("Out of stencil slots");
        }

        return nextFree++;
    }

    public static void Unregister(int stencil)
    {
        if (stencil > 0 && stencil <= MaxStencilValue)
        {
            freeList.Enqueue(stencil);
        }
        else
        {
            throw new ArgumentException("Invalid stencil value");
        }
    }
}
