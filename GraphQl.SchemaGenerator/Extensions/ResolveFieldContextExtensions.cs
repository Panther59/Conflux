using System.Collections.Generic;
using System.Linq;
using GraphQL.SchemaGenerator.Models;
using GraphQL.Types;
using Newtonsoft.Json;

namespace GraphQL.SchemaGenerator.Extensions
{
    /// <summary>
    ///     Extensions for resolve field context.
    /// </summary>
    internal static class ResolveFieldContextExtensions
    {
        /// <summary>
        ///     Generate the parameters for the field.
        /// </summary>
        /// <param name="type">Extension.</param>
        /// <param name="field">Field information.</param>
        /// <returns></returns>
        public static object[] Parameters(this ResolveFieldContext<object> type, FieldInformation field)
        {
            if (field == null)
            {
                return null;
            }

            var routeArguments = new List<object>();
            foreach (var parameter in field.Method.GetParameters())
            {
                if (!type.Arguments.TryGetValue(parameter.Name, out var arg))
                {
                    routeArguments.Add(null);
                    continue;
                }

                if (arg == null)
                {
                    routeArguments.Add(null);
                    continue;
                }

                if (typeof(IDictionary<string, object>).IsAssignableFrom(arg.GetType()) ||
                    typeof(IEnumerable<object>).IsAssignableFrom(arg.GetType())
                    )
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(arg);
                        arg = JsonConvert.DeserializeObject(json, parameter.ParameterType);
                    }
                    catch
                    {
                        var json = JsonConvert.SerializeObject(arg, new DeepDictionaryRequest());
                        arg = JsonConvert.DeserializeObject(json, parameter.ParameterType);
                    }
                }
                else if (parameter.ParameterType == typeof(char))
                {
                    arg = arg.ToString()[0];
                }

                routeArguments.Add(arg);
            }

            return routeArguments.Any() ? routeArguments.ToArray() : null;
        }

    }
}
