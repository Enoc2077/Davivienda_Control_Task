using Dapper;
using System.Reflection;

namespace Davivienda.GraphQL.Utils
{
    public static class SqlBuilderHelper
    {
        // Construye un UPDATE dinámico basado solo en los campos provistos
        public static (string Sql, DynamicParameters Params) BuildPartialUpdate<T>(
            string tableName,
            T input,
            string keyField)
        {
            var updates = new List<string>();
            var parameters = new DynamicParameters();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                var value = prop.GetValue(input);

                // Saltar el ID principal (llave primaria)w
                if (prop.Name.Equals(keyField, StringComparison.OrdinalIgnoreCase))
                {
                    parameters.Add($"@{prop.Name}", value);
                    continue;
                }

                // Lógica para detectar si el campo es opcional y tiene valor
                var isOptional = prop.PropertyType.IsGenericType &&
                                 prop.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>);

                if (isOptional)
                {
                    var hasValueProp = prop.PropertyType.GetProperty("HasValue");
                    var hasValue = (bool)(hasValueProp?.GetValue(value) ?? false);

                    if (hasValue)
                    {
                        var valueProp = prop.PropertyType.GetProperty("Value");
                        var realValue = valueProp?.GetValue(value);

                        updates.Add($"{prop.Name} = @{prop.Name}");
                        parameters.Add($"@{prop.Name}", realValue);
                    }
                }
            }

            if (updates.Count == 0)
                throw new Exception("No fields provided to update.");

            // Construcción del SQL final
            var sql = $"UPDATE {tableName} SET {string.Join(", ", updates)}, modify_at = @ModifyAt WHERE {keyField} = @{keyField}";
            parameters.Add("@ModifyAt", DateTimeOffset.UtcNow);

            return (sql, parameters);
        }
    }

    // Clase auxiliar para manejar valores opcionales en el patch
    public class Optional<T>
    {
        public T? Value { get; set; }
        public bool HasValue { get; set; }
    }
}