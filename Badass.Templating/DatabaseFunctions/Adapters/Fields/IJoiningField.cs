using System;
using System.Collections.Generic;
using System.Text;

namespace Badass.Templating.DatabaseFunctions.Adapters.Fields
{
    public interface IJoiningField : IPseudoField
    {
        string PrimaryAlias { get; }
    }
}
