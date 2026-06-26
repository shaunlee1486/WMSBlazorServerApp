using System;

namespace WMS.SharedKernel;

public interface IIdGenerator
{
    Guid Generate();
}
