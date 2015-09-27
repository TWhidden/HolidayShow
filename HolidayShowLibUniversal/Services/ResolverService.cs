using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HolidayShowLibUniversal.Services
{
    public class ResolverService : IResolverService
    {

        private readonly IDictionary<Type, Type> types = new Dictionary<Type, Type>();
        private readonly IDictionary<Type, object> typeInstances = new Dictionary<Type, object>();

        public void Register<TContract, TImplementation>()
        {
            types[typeof(TContract)] = typeof(TImplementation);
        }

        public void Register<TContract, TImplementation>(TImplementation instance)
        {
            typeInstances[typeof(TContract)] = instance;
        }

        public T Resolve<T>(params object[] optional)
        {
            return (T)Resolve(typeof(T), optional);
        }

        public object Resolve(Type contract, params object[] optional)
        {
            if (typeInstances.ContainsKey(contract))
            {
                return typeInstances[contract];
            }

            if (types.ContainsKey(contract))
            {
                var implementation = types[contract];
                var constructor = implementation.GetConstructors()[0];
                var constructorParameters = constructor.GetParameters();
                if (constructorParameters.Length == 0)
                {
                    return Activator.CreateInstance(implementation);
                }
                var parameters = new List<object>(constructorParameters.Length);
                parameters.AddRange(constructorParameters.Select(parameterInfo => (optional.FirstOrDefault(x => x.GetType() == parameterInfo.ParameterType) ?? Resolve(parameterInfo.ParameterType))));
                return constructor.Invoke(parameters.ToArray());
            }

            {
                var constructors = contract.GetConstructors();
                var constructor = constructors[0];
                var constructorParameters = constructor.GetParameters();
                if (constructorParameters.Length == 0)
                {
                    return Activator.CreateInstance(contract);
                }
                var parameters = new List<object>(constructorParameters.Length);
                parameters.AddRange(constructorParameters.Select(parameterInfo => (optional.FirstOrDefault(x => x.GetType() == parameterInfo.ParameterType) ?? Resolve(parameterInfo.ParameterType))));
                return constructor.Invoke(parameters.ToArray());
            }
        }
    }
}
