using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;

namespace XinjingdailyBot.WebAPI.IPC.Middlewares;

/// <summary>
/// 枚举类
/// </summary>
public sealed class EnumSchemaFilter : ISchemaFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (context.Type is not { IsEnum: true })
        {
            return;
        }

        if (context.Type.IsDefined(typeof(FlagsAttribute), false))
        {
            schema.Format = "flags";
        }

        var definition = new OpenApiObject();

        foreach (object? enumValue in context.Type.GetEnumValues())
        {
            if (enumValue == null)
            {
                throw new InvalidOperationException(nameof(enumValue));
            }

            string? enumName = Enum.GetName(context.Type, enumValue);

            if (string.IsNullOrEmpty(enumName))
            {
                // Fallback
                enumName = enumValue.ToString();

                if (string.IsNullOrEmpty(enumName))
                {
                    throw new InvalidOperationException(nameof(enumName));
                }
            }

            if (definition.ContainsKey(enumName))
            {
                // This is possible if we have multiple names for the same enum value, we'll ignore additional ones
                continue;
            }

            IOpenApiPrimitive enumObject;

            if (TryCast(enumValue, out int intValue))
            {
                enumObject = new OpenApiInteger(intValue);
            }
            else if (TryCast(enumValue, out long longValue))
            {
                enumObject = new OpenApiLong(longValue);
            }
            else if (TryCast(enumValue, out ulong ulongValue))
            {
                // OpenApi spec doesn't support ulongs as of now
                enumObject = new OpenApiString(ulongValue.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                throw new InvalidOperationException(nameof(enumValue));
            }

            definition.Add(enumName, enumObject);
        }

        schema.AddExtension("x-definition", definition);
    }

    private static bool TryCast<T>(object value, out T typedValue) where T : struct
    {
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            typedValue = (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);

            return true;
        }
        catch (InvalidCastException)
        {
            typedValue = default;

            return false;
        }
        catch (OverflowException)
        {
            typedValue = default;

            return false;
        }
    }
}
