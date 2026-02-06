using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

[Prototype]
public sealed partial class DepartmentConfigPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public HashSet<ProtoId<DepartmentPrototype>> EnabledDepartments = new();
}
