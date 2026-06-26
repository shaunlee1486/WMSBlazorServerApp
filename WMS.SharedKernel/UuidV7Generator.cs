using System;

namespace WMS.SharedKernel;

public class UuidV7Generator : IIdGenerator
{
    public Guid Generate() => Guid.CreateVersion7();
}
