using WMS.SharedKernel;

namespace WMS.Domain.Entities.MasterData;

public class Unit : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
