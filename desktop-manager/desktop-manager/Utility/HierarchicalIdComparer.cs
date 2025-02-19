using System;
using System.Collections.Generic;
using desktop_manager.Models;

namespace desktop_manager.Utility;

public class HierarchicalIdComparer : IComparer<Item>
{
    
    public int Compare(Item x, Item y)
    {
        if (x?.Id == null || y?.Id == null)
            return Comparer<string>.Default.Compare(x?.Id, y?.Id);

        var xParts = x.Id.Split('.');
        var yParts = y.Id.Split('.');

        int length = Math.Max(xParts.Length, yParts.Length);
        for (int i = 0; i < length; i++)
        {
            if (i >= xParts.Length) return -1; // x is shorter, comes first
            if (i >= yParts.Length) return 1;  // y is shorter, comes first

            if (int.TryParse(xParts[i], out int xPart) && int.TryParse(yParts[i], out int yPart))
            {
                int comparison = xPart.CompareTo(yPart);
                if (comparison != 0)
                    return comparison;
            }
            else
            {
                int comparison = string.Compare(xParts[i], yParts[i], StringComparison.Ordinal);
                if (comparison != 0)
                    return comparison;
            }
        }

        return 0; // IDs are equal
    }
    
}