using UnityEngine;
using System.Collections.Generic;

public interface IPathGenerator
{
    public List<Cell> Generate(int pathLength);
}
