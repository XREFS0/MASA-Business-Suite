using System;

namespace MASA_Business_Suite
{
    /// <summary>
    /// Defines search capabilities for child forms loaded in the MASA ERP content panel.
    /// Allows the global navbar search bar to propagate text queries directly to the active screen.
    /// </summary>
    public interface ISearchable
    {
        void PerformSearch(string query);
    }
}
