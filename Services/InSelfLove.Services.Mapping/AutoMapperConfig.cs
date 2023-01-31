namespace InSelfLove.Services.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using AutoMapper;

    public static class AutoMapperConfig
    {
        private static bool initialized;

        public static IMapper MapperInstance { get; set; }

        // Called once in StartUp class
        public static void RegisterMappings(params Assembly[] assemblies)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            // Get a single-dimensional(flattened) sequence of all public types in the assemblies.
            // If we used .Select(), we'd get a Type[][] sequence. SelectMany flattens that array.
            var types = assemblies.SelectMany(a => a.GetExportedTypes()).ToList();

            var config = new MapperConfigurationExpression();
            config.CreateProfile(
                "ReflectionProfile",
                configuration =>
                {
                    // IMapFrom<>
                    foreach (var map in GetFromMaps(types))
                    {
                        configuration.CreateMap(map.Source, map.Destination);
                    }

                    // IMapTo<>
                    foreach (var map in GetToMaps(types))
                    {
                        configuration.CreateMap(map.Source, map.Destination);
                    }

                    // IHaveCustomMappings
                    foreach (var map in GetCustomMappings(types))
                    {
                        // Each type implementing IHaveCustomMappings has the .CreateMappings method
                        // which is called here. It adds our custom mappings to the existing config.
                        map.CreateMappings(configuration);
                    }
                });
            MapperInstance = new Mapper(new MapperConfiguration(config));
        }

        private static IEnumerable<TypesMap> GetFromMaps(IEnumerable<Type> types)
        {
            // Same as below but using nested loops
            //var fromMaps1 = new List<TypesMap>();

            //foreach (var t in types)
            //{
            //    var interfaces = t.GetTypeInfo().GetInterfaces();

            //    foreach (var i in interfaces)
            //    {
            //        if (i.GetTypeInfo().IsGenericType &&
            //                     i.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
            //                     !t.GetTypeInfo().IsAbstract &&
            //                     !t.GetTypeInfo().IsInterface)
            //        {
            //            var typesMap = new TypesMap
            //            {
            //                Source = i.GetTypeInfo().GetGenericArguments()[0],
            //                Destination = t,
            //            };

            //            fromMaps1.Add(typesMap);
            //        }
            //    }
            //}

            // 1. Get all our types
            // 2. Get all interfaces implemented/inherited by them
            //    which are of generic type,
            //    whose type definition matches our IMapFrom<> interface type definition
            //    where the type inheriting/implementing them isn't abstract and isn't an interface
            // 3. Transform them into Source -> Destination objects
            var fromMaps = from t in types
                           from i in t.GetTypeInfo().GetInterfaces()
                           where i.GetTypeInfo().IsGenericType &&
                                 i.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
                                 !t.GetTypeInfo().IsAbstract &&
                                 !t.GetTypeInfo().IsInterface
                           select new TypesMap
                           {
                               Source = i.GetTypeInfo().GetGenericArguments()[0],
                               Destination = t,
                           };

            return fromMaps;
        }

        private static IEnumerable<TypesMap> GetToMaps(IEnumerable<Type> types)
        {
            var toMaps = from t in types
                         from i in t.GetTypeInfo().GetInterfaces()
                         where i.GetTypeInfo().IsGenericType &&
                               i.GetTypeInfo().GetGenericTypeDefinition() == typeof(IMapTo<>) &&
                               !t.GetTypeInfo().IsAbstract &&
                               !t.GetTypeInfo().IsInterface
                         select new TypesMap
                         {
                             Source = t,
                             Destination = i.GetTypeInfo().GetGenericArguments()[0],
                         };

            return toMaps;
        }

        private static IEnumerable<IHaveCustomMappings> GetCustomMappings(IEnumerable<Type> types)
        {
            // IsAssignableFrom - Determines whether the current type implements IHaveCustomMappings
            // Create instances of types implementing IHaveCustomMappings
            var customMaps = from t in types
                             from i in t.GetTypeInfo().GetInterfaces()
                             where typeof(IHaveCustomMappings).GetTypeInfo().IsAssignableFrom(t) &&
                                   !t.GetTypeInfo().IsAbstract &&
                                   !t.GetTypeInfo().IsInterface
                             select (IHaveCustomMappings)Activator.CreateInstance(t);

            return customMaps;
        }

        private class TypesMap
        {
            public Type Source { get; set; }

            public Type Destination { get; set; }
        }
    }
}
