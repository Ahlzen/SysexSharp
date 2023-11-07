using System.Collections.Generic;

namespace Ahlzen.SysexSharp.SysexLib;

/// <summary>
/// Indicates that the type of Sysex can be parsed
/// into known individual parameter values.
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
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the parameter name is invalid.
    /// </exception>
    object GetParameterValue(string parameterName);

    /// <summary>
    /// Validates that all parameter values are within
    /// their allowed/expected range.
    /// </summary>
    /// <exception cref="ValidationException">
    /// Thrown if a value is not within the allowable range.
    /// </exception>
    // TODO: Return a summary of all validation errors
    void Validate();

    /// <summary>
    /// Returns the parsed sysex as a dictionary of parameters and values.
    /// </summary>
    Dictionary<string, object> ToDictionary();

    /// <summary>
    /// Returns the parsed sysex as a JSON document.
    /// May include nested structures. Should be parseable
    /// with FromJSON().
    /// </summary>
    string ToJSON();
}
