using System;
using System.Threading;

#nullable enable

namespace Pitchfork.SemanticVersioning.Tests
{
    public record class SemVerTestItem(
        string SemVerString,
        string ExpectedMajor,
        string ExpectedMinor,
        string ExpectedPatch,
        string? ExpectedPrerelease = null,
        string? ExpectedBuild = null)
    {
        private Lazy<SemanticVersion> _semanticVersion = new Lazy<SemanticVersion>(
            valueFactory: () => new SemanticVersion(SemVerString),
            mode: LazyThreadSafetyMode.PublicationOnly);

        public SemanticVersion ToSemanticVersion() => _semanticVersion.Value;
    }
}
