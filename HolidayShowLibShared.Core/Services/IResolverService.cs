using System;

namespace HolidayShowLibShared.Core.Services
{
    public interface IResolverService
    {
        /// <summary>
        /// Used to get types of objects on demand from the host container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>(params object[] param);

        /// <summary>
        /// Resolves a type using Unity.
        /// </summary>
        /// <optional name="t"></optional>
        /// <returns></returns>
        object Resolve(Type t, params object[] optional);

        void Register<TContract, TImplementation>();
        void Register<TContract, TImplementation>(TImplementation instance);
    }
}
