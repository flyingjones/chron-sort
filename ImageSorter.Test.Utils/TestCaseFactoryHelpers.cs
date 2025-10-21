using System.Linq.Expressions;

namespace ImageSorter.Test.Utils;

public static class TestCaseFactoryHelpers
{
   public static IEnumerable<TTestCase> ConfigureEnum<TTestCase, TEnum>(
        this Func<TTestCase> testCaseFactory,
        Expression<Func<TTestCase, TEnum>> expression,
        string? propertyName = null) 
        where TTestCase : AbstractTestCase
        where TEnum : struct, Enum
    {
        var expr = (MemberExpression)expression.Body;
        if (propertyName == null)
        {
            propertyName = expr.ToString();
        }

        var setter = BuildSetter(expression);

        foreach (var value in Enum.GetValues<TEnum>())
        {
            var testCase = testCaseFactory();
            setter(testCase, value);
            var valueString = value.ToString("G");
            testCase.TestCaseName = $"{propertyName} = {valueString}";
            yield return testCase;
        }
    }
    
    public static TTestCase ConfigureProperty<TTestCase, TProperty>(
        this Func<TTestCase> testCaseFactory,
        Expression<Func<TTestCase, TProperty>> expression,
        TProperty value,
        string? propertyName = null)
        where TTestCase : AbstractTestCase
    {
        var expr = (MemberExpression)expression.Body;
        if (propertyName == null)
        {
            propertyName = expr.ToString();
        }

        var setter = BuildSetter(expression);
        var testCase = testCaseFactory();
        setter(testCase, value);
        var valueString = value?.ToString() ?? "null";
        testCase.TestCaseName = $"{propertyName} = {valueString}";

        return testCase;
    }

    internal static Action<TSource, TProperty> BuildSetter<TSource, TProperty>(
        this Expression<Func<TSource, TProperty>> expression)
    {
        var valueParameter = Expression.Parameter(expression.Body.Type);
        var setterExpression = Expression.Lambda<Action<TSource, TProperty>>(
                Expression.Assign(expression.Body, valueParameter), expression.Parameters.Single(), valueParameter);
        return setterExpression.Compile();
    }
}