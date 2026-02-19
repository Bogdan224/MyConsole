using System.Xml.Linq;

namespace MyConsole2
{
    public interface ISpecification;
    public class Component(string name)
    {
        public string ComponentName { get; set; } = name;
    }

    public class Detail(string name) : Component(name), ISpecification;

    public class Node(string name) : Component(name), ISpecification
    {
        public ISpecification?[] Specifications { get; private set; } = new ISpecification[2];

        public Node(string name, ISpecification?[] specifications) : this(name)
        {
            Specifications = specifications;
        }
    }

    public class Product(string name): Component(name)
    {
        public ISpecification?[] Specifications { get; private set; } = new ISpecification[2];

        public Product(string name, ISpecification?[] specifications) : this(name)
        {
            Specifications = specifications;
        }
    }

    public enum ComponentType
    {
        Detail, Product, Node
    }

    public static class StringExtentions
    {
        public static ComponentType ToCompanentType(this string str)
        {
            return str.ToLower() switch
            {
                "деталь" => ComponentType.Detail,
                "узел" => ComponentType.Node,
                "изделие" => ComponentType.Product,
                _ => throw new ArgumentException("Компонент не найден!"),
            };
        }
    }
}
