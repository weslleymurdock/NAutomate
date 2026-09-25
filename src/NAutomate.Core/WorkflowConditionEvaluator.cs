using System.Collections;
using System.Globalization;
using NAutomate.Abstractions;
using NAutomate.Parser;

namespace NAutomate.Core;

/// <summary>Evaluates the deliberately small, typed workflow condition language.</summary>
public sealed class WorkflowConditionEvaluator
{
    public bool Evaluate(WorkflowCondition condition, IWorkflowVariableStore variables)
    {
        var left = variables.Get(condition.Variable);
        var right = WorkflowValueResolver.Resolve(condition.Value, variables);

        return condition.Operator switch
        {
            WorkflowConditionOperator.IsNull => left is null,
            WorkflowConditionOperator.IsNotNull => left is not null,
            WorkflowConditionOperator.Equals => Equals(left, right),
            WorkflowConditionOperator.NotEquals => !Equals(left, right),
            WorkflowConditionOperator.GreaterThan => Compare(left, right) > 0,
            WorkflowConditionOperator.GreaterThanOrEqual => Compare(left, right) >= 0,
            WorkflowConditionOperator.LessThan => Compare(left, right) < 0,
            WorkflowConditionOperator.LessThanOrEqual => Compare(left, right) <= 0,
            WorkflowConditionOperator.Contains => Contains(left, right),
            WorkflowConditionOperator.StartsWith => StringOperation(left, right, static (a, b) => a.StartsWith(b, StringComparison.Ordinal)),
            WorkflowConditionOperator.EndsWith => StringOperation(left, right, static (a, b) => a.EndsWith(b, StringComparison.Ordinal)),
            _ => throw new InvalidOperationException($"Unsupported condition operator '{condition.Operator}'.")
        };
    }

    private static int Compare(object? left, object? right)
    {
        if (left is null || right is null)
            throw new InvalidDataException("Relational conditions cannot compare null values.");

        if (left is IComparable comparable)
        {
            try
            {
                return comparable.CompareTo(ConvertTo(right, left.GetType()));
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidCastException or FormatException or OverflowException)
            {
                throw new InvalidDataException($"Values of type '{left.GetType()}' and '{right.GetType()}' are not comparable.", exception);
            }
        }

        throw new InvalidDataException($"Type '{left.GetType()}' does not support relational comparison.");
    }

    private static bool Contains(object? left, object? right)
    {
        if (left is string text && right is string search)
            return text.Contains(search, StringComparison.Ordinal);

        if (left is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
                if (Equals(item, right))
                    return true;
            return false;
        }

        throw new InvalidDataException($"Contains is not supported for type '{left?.GetType()}'.");
    }

    private static bool StringOperation(object? left, object? right, Func<string, string, bool> operation)
    {
        if (left is string text && right is string search)
            return operation(text, search);

        throw new InvalidDataException("String condition operators require two string values.");
    }

    private static object? ConvertTo(object value, Type targetType)
    {
        if (targetType.IsInstanceOfType(value))
            return value;

        if (targetType == typeof(Guid))
            return Guid.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)!);

        if (targetType == typeof(TimeSpan))
            return TimeSpan.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, Convert.ToString(value, CultureInfo.InvariantCulture)!, true);

        return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType, CultureInfo.InvariantCulture);
    }
}
