namespace MyConsole2
{
    public interface ISpec;

    public class Detail : ISpec
    {
    }

    public class Node : ISpec
    {
        ISpec[] specs = new ISpec[2];

    }

    public class Product
    {
        ISpec[] spec1 = new ISpec[2];
    }

    public enum ComponentType
    {
        Detail, Product, Node
    }

    public static class StringExtentions
    {
        public static ComponentType ToCompanentType(this string str)
        {
            switch (str.ToLower())
            {
                case "деталь": return ComponentType.Detail;
                case "узел": return ComponentType.Node;
                case "изделие": return ComponentType.Product;
                default:
                    throw new ArgumentException("Компонент не найден!");
            }
        }
    }
}
