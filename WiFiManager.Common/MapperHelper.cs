using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using AutoMapper;



namespace WiFiManager.Common
{
    public static class MapperHelper
    {
        //public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>
        //    (this IMappingExpression<TSource, TDestination> expression, bool isUseServices = true)
        //{
        //    Type sourceType = typeof(TSource);
        //    Type destinationType = typeof(TDestination);
        //    TypeMap existingMaps = Mapper.Configuration.GetAllTypeMaps().First(x => x.SourceType == sourceType && x.DestinationType == destinationType);
        //    foreach (string property in existingMaps.GetUnmappedPropertyNames())
        //    {
        //        expression.ForMember(property, opt => opt.Ignore());
        //    }

        //    if (isUseServices)
        //    {
        //        expression = expression.ConstructUsingServiceLocator();
        //    }

        //    return expression;
        //}

        public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>
        (this IMappingExpression<TSource, TDestination> expression)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var sourceType = typeof(TSource);
            var destinationProperties = typeof(TDestination).GetProperties(flags);

            foreach (var property in destinationProperties)
            {
                if (sourceType.GetProperty(property.Name, flags) == null)
                {
                    expression.ForMember(property.Name, opt => opt.Ignore());
                }
            }

            return expression;
        }

        #region New - http://stackoverflow.com/questions/954480/automapper-ignore-the-rest/31182390#31182390
        private static void IgnoreUnmappedProperties(TypeMap map, IMappingExpression expr)
        {
            foreach (string propName in map.GetUnmappedPropertyNames())
            {
                if (map.SourceType.GetProperty(propName) != null)
                {
                    expr.ForSourceMember(propName, opt => opt.Ignore());
                }
                if (map.DestinationType.GetProperty(propName) != null)
                {
                    expr.ForMember(propName, opt => opt.Ignore());
                }
            }
        }

        public static void IgnoreUnmapped(this IProfileExpression profile)
        {
            profile.ForAllMaps(IgnoreUnmappedProperties);
        }

        public static void IgnoreUnmapped(this IProfileExpression profile, Func<TypeMap, bool> filter)
        {
            profile.ForAllMaps((map, expr) =>
            {
                if (filter(map))
                {
                    IgnoreUnmappedProperties(map, expr);
                }
            });
        }

        public static void IgnoreUnmapped(this IProfileExpression profile, Type src, Type dest)
        {
            profile.IgnoreUnmapped((TypeMap map) => map.SourceType == src && map.DestinationType == dest);
        }

        public static void IgnoreUnmapped<TSrc, TDest>(this IProfileExpression profile)
        {
            profile.IgnoreUnmapped(typeof(TSrc), typeof(TDest));
        }
        #endregion

    }
}
