using UnityEngine;
using System.Collections.Generic;

public interface IPathGenerator
{
    public PathCreator Generate(int pathLength);
}
