using System;
using System.Collections.Generic;
using System.Text;

namespace SkyFrpPanel.ServiceCore
{
    /// <summary>
    /// 自动注入标记接口
    /// </summary>
    public interface IAutoInject
    {

    }

    public enum ServiceLifetimeType
    {
        Scoped,
        Singleton,
        Transient
    }

    public interface IAutoInjectService : IAutoInject
    {
        ServiceLifetimeType Lifetime { get; }
    }
}
