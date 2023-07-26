using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahlzen.SysexSharp.SysexLib
{
    /// <summary>
    /// Indicates that the type of Sysex can be parsed
    /// into known invidual parameter values.
    /// </summary>
    internal interface ICanParse
    {
        /// <summary>
        /// Returns a list of parameter names for this sysex.
        /// </summary>
        IEnumerable<string> ParameterNames { get; }
        
        /// <summary>
        /// Returns the value for the specified parameter.
        /// </summary>
        object GetParameterValue(string parameterName);
        
        /// <summary>
        /// Validates that all parameter values are within
        /// their allowed/expected range.
        /// </summary>
        void Validate();
        
        /// <summary>
        /// Returns the current sysex as a JSON document.
        /// May include nested structures. Should be parseable
        /// with FromJSON().
        /// </summary>
        string ToJSON();

        /// <summary>
        /// Parses the specified JSON as a Sysex.
        /// TODO: make static??
        /// </summary>
        Sysex FromJSON();
    }
}
