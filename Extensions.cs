using MyConsole2.Components;

namespace MyConsole2.Extensions
{
    public static class Extentions
    {
        public static ComponentType ToComponentType(this string str)
        {
            return str.ToLower() switch
            {
                "деталь" => ComponentType.Detail,
                "узел" => ComponentType.Node,
                "изделие" => ComponentType.Product,
                _ => throw new ArgumentException("Компонент не найден!"),
            };
        }

        public static string ToStr(this ComponentType type)
        {
            return type switch
            {
                ComponentType.Detail => "Деталь",
                ComponentType.Node => "Узел",
                ComponentType.Product => "Изделие",
                _ => throw new ArgumentException("Не существующий тип!")
            };
        }
    }
}
