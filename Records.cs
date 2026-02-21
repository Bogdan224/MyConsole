using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;

namespace MyConsole2
{
    /// <summary>
    /// Структура заголовка файла списка изделий
    /// </summary>
    public struct ComponentListHeader
    {
        private const int SignatureSize = 2;
        private const int RecordLengthSize = 2;
        private const int FirstRecordPtrSize = 4;
        private const int FreeAreaPtrSize = 4;

        private const int SpecFilenameSize = 16;

        public const int TotalSize = SignatureSize + RecordLengthSize +
            FirstRecordPtrSize + FreeAreaPtrSize + SpecFilenameSize;

        public ComponentListRecord? FirstRecord { get; set; }

        public byte[] Signature { get; set; } // "PS"
        public ushort DataRecordLength { get; set; }
        public int FirstRecordPtr { get; set; }
        public byte[] FreeAreaPtr { get; set; }
        public char[] SpecFilename { get; set; }

        public ComponentListHeader(ushort dataRecordLength, string specFilename)
        {
            Signature = Encoding.ASCII.GetBytes("PS");
            DataRecordLength = dataRecordLength;
            FirstRecordPtr = -1;
            FreeAreaPtr = new byte[FreeAreaPtrSize];
            SpecFilename = new char[SpecFilenameSize];
            Array.Copy(specFilename.ToCharArray(), SpecFilename, specFilename.Length);
        }

        //Сериализация
        public byte[] ToBytes()
        {
            byte[] buffer = new byte[TotalSize];
            int offset = 0;

            Array.Copy(Signature, 0, buffer, offset, SignatureSize);
            offset += SignatureSize;

            Array.Copy(BitConverter.GetBytes(DataRecordLength), 0, buffer, offset, RecordLengthSize);
            offset += RecordLengthSize;

            Array.Copy(BitConverter.GetBytes(FirstRecordPtr), 0, buffer, offset, FirstRecordPtrSize);
            offset += FirstRecordPtrSize;

            Array.Copy(FreeAreaPtr, 0, buffer, offset, FreeAreaPtrSize);
            offset += FreeAreaPtrSize;

            byte[] nameBytes = Encoding.UTF8.GetBytes(SpecFilename);
            Array.Copy(nameBytes, 0, buffer, offset, Math.Min(nameBytes.Length, SpecFilenameSize));
            offset += SpecFilenameSize;

            if(FirstRecord != null)
                buffer = buffer.Concat(FirstRecord.ToBytes()).ToArray();

            return buffer;
        }
        //Десериализация
        public static ComponentListHeader FromBytes(byte[] buffer, int startIndex = 0)
        {
            var header = new ComponentListHeader();
            int offset = startIndex;

            // Signature
            header.Signature = new byte[SignatureSize];
            Array.Copy(buffer, offset, header.Signature, 0, SignatureSize);
            offset += SignatureSize;

            // DataRecordLength
            header.DataRecordLength = BitConverter.ToUInt16(buffer, offset);
            offset += RecordLengthSize;

            // FirstRecordPtr
            header.FirstRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += FirstRecordPtrSize;

            // FreeAreaPtr
            header.FreeAreaPtr = new byte[FreeAreaPtrSize];
            Array.Copy(buffer, offset, header.FreeAreaPtr, 0, FreeAreaPtrSize);
            offset += FreeAreaPtrSize;

            // SpecFileName
            header.SpecFilename = Encoding.UTF8.GetString(buffer, offset, SpecFilenameSize).ToCharArray(0, SpecFilenameSize);
            offset += SpecFilenameSize;

            return header;
        }
    }

    /// <summary>
    /// Запись файла списка изделий
    /// </summary>
    public class ComponentListRecord
    {
        private const int DeletionBitSize = 1;
        private const int SpecificationRecordPtrSize = 4;
        private const int NextRecordPtrSize = 4;
        private const int ComponentTypeSize = 2;

        public ComponentListRecord? NextRecord { get; set; }
        public SpecificationRecord? SpecificationRecord { get; set; }

        public bool IsDeleted { get; set; } // 0 - активно, -1 - удалено
        public int SpecificationRecordPtr { get; set; } // Указатель на первую запись в спецификации
        public int NextRecordPtr { get; set; } // Указатель на следующую запись
        public MyComponent DataArea { get; private set; } // Компонент

        public ComponentListRecord(MyComponent component)
        {
            IsDeleted = false;
            SpecificationRecordPtr = -1;
            NextRecordPtr = -1;
            DataArea = component;
        }

        private int GetTotalSize()
        {
            return DeletionBitSize + SpecificationRecordPtrSize + NextRecordPtrSize + 
                DataArea.ComponentName.Length + ComponentTypeSize;
        }

        //Сериализация
        public byte[] ToBytes()
        {
            byte[] buffer = new byte[GetTotalSize()];
            int offset = 0;

            buffer[offset] = BitConverter.GetBytes(IsDeleted)[0];
            offset += DeletionBitSize;

            Array.Copy(BitConverter.GetBytes(SpecificationRecordPtr), 0, buffer, offset, SpecificationRecordPtrSize);
            offset += SpecificationRecordPtrSize;

            Array.Copy(BitConverter.GetBytes(NextRecordPtr), 0, buffer, offset, NextRecordPtrSize);
            offset += NextRecordPtrSize;

            byte[] nameBytes = Encoding.UTF8.GetBytes(DataArea.ComponentName);
            Array.Copy(nameBytes, 0, buffer, offset, DataArea.ComponentName.Length);
            offset += DataArea.ComponentName.Length;  

            Array.Copy(BitConverter.GetBytes(Convert.ToInt16(DataArea.ComponentType)), 0, buffer, offset, ComponentTypeSize);
            
            if (NextRecord != null)
                buffer = buffer.Concat(NextRecord.ToBytes()).ToArray();

            return buffer;
        }
        //Десериализация
        public static ComponentListRecord FromBytes(byte[] buffer, int startIndex = 0)
        {
            int offset = startIndex;

            // Deleted flag
            bool isDeleted = BitConverter.ToBoolean(buffer, offset);
            offset += DeletionBitSize;

            // FirstComponentPtr
            var specificationRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += SpecificationRecordPtrSize;

            // NextRecordPtr
            var nextRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += NextRecordPtrSize;

            // DataArea
            string name = Encoding.UTF8.GetString(buffer, offset, buffer.Length - offset - ComponentTypeSize);
            offset += buffer.Length - offset - ComponentTypeSize;

            ComponentType componentType = (ComponentType)BitConverter.ToInt16(buffer, offset);

            MyComponent myComponent = new MyComponent(name, componentType);

            return new ComponentListRecord(myComponent)
            {
                IsDeleted = isDeleted,
                SpecificationRecordPtr = specificationRecordPtr,
                NextRecordPtr = nextRecordPtr
            };
        }
    }

    /// <summary>
    /// Структура заголовка файла спецификаций
    /// </summary>
    public struct SpecificationHeader
    {
        private const int FirstRecordPtrSize = 4;
        private const int FreeAreaPtrSize = 4;
        public const int TotalSize = FirstRecordPtrSize + FreeAreaPtrSize;

        public SpecificationRecord? FirstRecord { get; set; }

        public int FirstRecordPtr { get; set; }
        public byte[] FreeAreaPtr { get; set; }

        public SpecificationHeader()
        {
            FirstRecordPtr = -1;
            FreeAreaPtr = new byte[FreeAreaPtrSize];
        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[TotalSize];

            Array.Copy(BitConverter.GetBytes(FirstRecordPtr), 0, buffer, 0, FirstRecordPtrSize);
            Array.Copy(FreeAreaPtr, 0, buffer, FirstRecordPtrSize, FreeAreaPtrSize);

            if (FirstRecord != null)
                buffer = buffer.Concat(FirstRecord.ToBytes()).ToArray();

            return buffer;
        }

        public static SpecificationHeader FromBytes(byte[] buffer, int startIndex = 0)
        {
            int offset = startIndex;
            var header = new SpecificationHeader();

            header.FirstRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += FirstRecordPtrSize;

            header.FreeAreaPtr = new byte[FreeAreaPtrSize];
            Array.Copy(buffer, offset, header.FreeAreaPtr, 0, FirstRecordPtrSize);

            return header;
        }
    }

    /// <summary>
    /// Запись файла спецификаций
    /// </summary>
    public class SpecificationRecord
    {
        private const int DeletionBitSize = 1;
        private const int ComponentRecordPtrSize = 4;
        private const int QuantitySize = 2;
        private const int NextRecordPtrSize = 4;
        private const int ComponentPtrSize = 4;

        public ComponentListRecord? ComponentRecord { get; set; }
        public SpecificationRecord? NextRecord { get; set; }
        public MyComponent[] Components { get; set; }

        public bool IsDeleted { get; set; } // 0 - активно, -1 - удалено
        public int ComponentRecordPtr { get; set; } // Указатель на запись в списке изделий
        public ushort Quantity { get; set; } // Кратность вхождения
        public int NextRecordPtr { get; set; } // Указатель на следующую запись в спецификации
        public int[] ComponentPtrs { get; set; }

        public SpecificationRecord(ushort quantity = 2)
        {
            IsDeleted = false;
            ComponentRecordPtr = -1;
            Quantity = quantity;
            NextRecordPtr = -1;
            Components = new MyComponent[quantity];
            ComponentPtrs = new int[quantity];
            Array.Fill(ComponentPtrs, -1);
        }

        public int GetTotalSize()
        {
            int totalSize = ComponentPtrSize * Quantity;
            return totalSize + DeletionBitSize + ComponentRecordPtrSize + QuantitySize + NextRecordPtrSize;
        }
        //Сериализация
        public byte[] ToBytes()
        {
            byte[] buffer = new byte[GetTotalSize()];
            int offset = 0;

            // Deleted flag
            buffer[offset] = BitConverter.GetBytes(IsDeleted)[0];
            offset += DeletionBitSize;

            // ComponentPtr
            Array.Copy(BitConverter.GetBytes(ComponentRecordPtr), 0, buffer, offset, ComponentRecordPtrSize);
            offset += ComponentRecordPtrSize;

            // Quantity
            Array.Copy(BitConverter.GetBytes(Quantity), 0, buffer, offset, QuantitySize);
            offset += QuantitySize;

            // NextRecordPtr
            Array.Copy(BitConverter.GetBytes(NextRecordPtr), 0, buffer, offset, NextRecordPtrSize);

            foreach (var component in ComponentPtrs)
            {
                Array.Copy(BitConverter.GetBytes(component), 0, buffer, offset, ComponentPtrSize);
                offset += ComponentPtrSize;
            }

            if (NextRecord != null)
                buffer = buffer.Concat(NextRecord.ToBytes()).ToArray();

            return buffer;
        }
        //Десериализация
        public static SpecificationRecord FromBytes(byte[] buffer, int startIndex = 0)
        {
            var record = new SpecificationRecord();
            int offset = startIndex;

            // Deleted flag
            record.IsDeleted = BitConverter.ToBoolean(buffer, offset);
            offset += DeletionBitSize;

            // ComponentPtr
            record.ComponentRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += ComponentRecordPtrSize;

            // Quantity
            record.Quantity = BitConverter.ToUInt16(buffer, offset);
            offset += QuantitySize;

            // NextRecordPtr
            record.NextRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += NextRecordPtrSize;

            int i = 0;
            while(offset-startIndex < record.GetTotalSize())
            {
                record.ComponentPtrs[i] = BitConverter.ToInt32(buffer, offset);
                offset += ComponentPtrSize;
                i++;
            }

            return record;
        }

        public void AddComponent(MyComponent myComponent)
        {
            int i = 0;
            while (Components[i] != null)
            {
                if (i == Components.Length - 1)
                    throw new Exception("Достигнут лимит компонентов в спецификации!");
                i++;
            }
            Components[i] = myComponent;
            ComponentPtrs[i] = myComponent.GetHashCode();
        }
    }
}
