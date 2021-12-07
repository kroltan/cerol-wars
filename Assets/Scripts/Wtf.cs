using System.ComponentModel;

// https://stackoverflow.com/a/62656145/1045510
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once UnusedType.Global
    public record IsExternalInit;
}