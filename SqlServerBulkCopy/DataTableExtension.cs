using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SqlServerBulkCopy;

public static class DataTableExtension
{
    public static DataTable ToDataTable<T>(this IEnumerable<T> items) where T : new()
    {
        var properties = Cache<T>.Properties.AsSpan();

        var dataTable = new DataTable();
        foreach (var p in properties)
        {
            dataTable.Columns.Add(
                CreateDataColumn(p.Name, p.PropertyType));
        }

        if (items == null) { return dataTable; }
        foreach (var item in items)
        {
            var row = dataTable.NewRow();
            if (item != null)
            {
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.Accessor.GetValue(item);
                }
            }
            dataTable.Rows.Add(row);
        }
        return dataTable;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static DataColumn CreateDataColumn(string name, Type? type)
        => new(name)
        {
            AllowDBNull = !IsClass(type),
            Caption = name, //$"{name}({type?.Name ?? ""})",
            DataType = type,
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsClass(Type? type)
    {
        if (type == null) { return false; }
        return !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>)
            ? type.IsClass
            : Nullable.GetUnderlyingType(type)?.IsClass ?? false;
    }
}

internal static class Cache<T>
{
    public static readonly PropCache[] Properties = typeof(T)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
        .Where(x => !x.GetIndexParameters().Any())
        .AsParallel()
        .Select((p, i) => new PropCache(p, i))
        .OrderBy(p => p.Index)
        .ToArray();
}

internal sealed class PropCache
{
    public PropCache(PropertyInfo p, int index)
    {
        Name = p.Name;
        Accessor = p.GetAccessor();
        PropertyType = p.PropertyType;
        Index = index;
    }

    public string Name { get; init; } = "";
    public IAccessor Accessor { get; init; }
    public Type PropertyType { get; init; }
    public int Index { get; init; }
}

public interface IAccessor
{
    object? GetValue(object target);
}

internal sealed class Accessor<TTarget, TProperty> : IAccessor
{
    readonly Func<TTarget, TProperty>? Getter;
    public Accessor(Func<TTarget, TProperty>? getter) => Getter = getter;
    public object? GetValue(object target)
        => Getter == null || typeof(TProperty).IsGenericType ? null : Getter((TTarget)target);
}

public static class AccessorExtension
{
    public static IAccessor GetAccessor(this PropertyInfo property)
    {
        var getterDelegateType = typeof(Func<,>).MakeGenericType(property.DeclaringType!, property.PropertyType);
        var getMethod = property.GetGetMethod();
        return (IAccessor)Activator.CreateInstance(
            typeof(Accessor<,>).MakeGenericType(property.DeclaringType!, property.PropertyType),
            getMethod == null ? null : Delegate.CreateDelegate(getterDelegateType, getMethod))!;
    }
}