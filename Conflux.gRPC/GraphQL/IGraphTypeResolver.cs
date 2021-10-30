﻿using System;
using GraphQL.Types;

namespace GraphQL.SchemaGenerator
{
    /// <summary>
    ///     Converts an unknown type to a graph type.
    /// </summary>
    public interface IGraphTypeResolver: IServiceProvider
    {
        GraphType ResolveType(Type type);
    }

}
