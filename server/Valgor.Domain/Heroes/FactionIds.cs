namespace Valgor.Domain.Heroes;

/// <summary>
/// Canonical faction identifiers from heroes.seed.json. Do not invent new values.
/// </summary>
public static class FactionIds
{
    public const string RosaDeSangue = "ROSA_DE_SANGUE";
    public const string AsasDoAmanhecer = "ASAS_DO_AMANHECER";
    public const string GuardaDaOrdem = "GUARDA_DA_ORDEM";

    public static readonly IReadOnlyList<string> All =
    [
        RosaDeSangue,
        AsasDoAmanhecer,
        GuardaDaOrdem
    ];
}
