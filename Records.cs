using MyConsole2.Components;
using MyConsole2.Headers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace MyConsole2.Records
{
    public abstract class Record
    {
        protected const int _deletionBitSize = 1;
        protected const int _nextRecordPtrSize = 4;

        public int NextRecordPtr { get; set; } // Указатель на следующую запись
        public bool IsDeleted { get; set; } // 0 - активно, 1 - удалено

        public Record()
        {
            NextRecordPtr = -1;
            IsDeleted = false;
        }

        public virtual int GetTotalSize()
        {
            return _deletionBitSize + _nextRecordPtrSize;
        }

        public virtual byte[] ToBytes()
        {
            byte[] buffer = new byte[_deletionBitSize + _nextRecordPtrSize];
            int offset = 0;

            Array.Copy(BitConverter.GetBytes(NextRecordPtr), 0, buffer, offset, _nextRecordPtrSize);
            offset += _nextRecordPtrSize;

            buffer[offset] = BitConverter.GetBytes(IsDeleted)[0];
            offset += _deletionBitSize;

            return buffer;
        }
    }

    public abstract class Record<T> : Record where T : Record
    {
        public T? NextRecord { get; set; }
    }

    /// <summary>
    /// Запись файла списка изделий
    /// </summary>
    public class ComponentRecord : Record<ComponentRecord>
    {
        private const int _specificationRecordPtrSize = 4;
        private const int _componentTypeSize = 2;

        public SpecificationRecord? SpecificationRecord { get; set; }

        public MyComponent DataArea { get; private set; } // Компонент

        public int SpecificationRecordPtr { get; set; } // Указатель на первую запись в спецификации

        public ComponentRecord(MyComponent component) : base()
        {
            SpecificationRecordPtr = -1;

            if (component.ComponentName.Length > ComponentHeader.DataRecordLength * 2)
                throw new Exception("Название компонента слишком длинное");
            DataArea = component;
        }

        public override int GetTotalSize()
        {
            return base.GetTotalSize() + _specificationRecordPtrSize + ComponentHeader.DataRecordLength * 2 + _componentTypeSize;
        }

        /// <summary>
        /// Сериализует этот объект и все связанные с ним объекты
        /// </summary>
        public override byte[] ToBytes()
        {
            byte[] buffer = new byte[_specificationRecordPtrSize + _componentTypeSize + ComponentHeader.DataRecordLength * 2];
            int offset = 0;

            Array.Copy(BitConverter.GetBytes(SpecificationRecordPtr), 0, buffer, offset, _specificationRecordPtrSize);
            offset += _specificationRecordPtrSize;

            Array.Copy(BitConverter.GetBytes(Convert.ToInt16(DataArea.ComponentType)), 0, buffer, offset, _componentTypeSize);
            offset += _componentTypeSize;

            byte[] nameBytes = new byte[ComponentHeader.DataRecordLength * 2];
            byte[] strBytes = Encoding.Unicode.GetBytes(DataArea.ComponentName);
            Array.Copy(strBytes, nameBytes, strBytes.Length);
            Array.Copy(nameBytes, 0, buffer, offset, nameBytes.Length);
            offset = ComponentHeader.DataRecordLength * 2;

            buffer = base.ToBytes().Concat(buffer).ToArray();

            if (NextRecord != null)
                buffer = buffer.Concat(NextRecord.ToBytes()).ToArray();

            return buffer;
        }

        //Десериализация
        public static ComponentRecord FromBytes(byte[] buffer, int startIndex = 0)
        {
            int offset = startIndex;

            // NextRecordPtr
            var nextRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += _nextRecordPtrSize;

            // Deleted flag
            bool isDeleted = BitConverter.ToBoolean(buffer, offset);
            offset += _deletionBitSize;

            // SpecificationRecordPtr
            var specificationRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += _specificationRecordPtrSize;

            // DataArea
            ComponentType componentType = (ComponentType)BitConverter.ToInt16(buffer, offset);
            offset += _componentTypeSize;

            string name = Encoding.Unicode.GetString(buffer, offset, ComponentHeader.DataRecordLength * 2).Trim('\0');
            offset += ComponentHeader.DataRecordLength * 2;

            MyComponent myComponent = new MyComponent(name, componentType);

            return new ComponentRecord(myComponent)
            {
                IsDeleted = isDeleted,
                SpecificationRecordPtr = specificationRecordPtr,
                NextRecordPtr = nextRecordPtr
            };
        }
    }

    /// <summary>
    /// Запись файла спецификаций
    /// </summary>
    public class SpecificationRecord : Record<SpecificationRecord>
    {
        private const int _componentRecordPtrSize = 4;
        private const int _quantitySize = 2;
        private const int _specificationNextPtrSize = 4;

        public ComponentRecord? ComponentRecord { get; set; }
        public SpecificationRecord? SpecificationNext { get; set; }

        public int ComponentRecordPtr { get; set; } // Указатель на запись в списке изделий
        public ushort Quantity { get; set; } // Кратность вхождения
        public int SpecificationNextPtr { get; set; }


        public SpecificationRecord(ushort quantity = 2) : base()
        {
            ComponentRecordPtr = -1;
            Quantity = quantity;
            SpecificationNextPtr = -1;
        }

        public override int GetTotalSize()
        {
            return base.GetTotalSize() + _componentRecordPtrSize + _quantitySize + _specificationNextPtrSize;
        }

        /// <summary>
        /// Сериализует этот объект и все связанные с ним объекты
        /// </summary>
        public override byte[] ToBytes()
        {
            byte[] buffer = new byte[_componentRecordPtrSize + _quantitySize + _specificationNextPtrSize];
            int offset = 0;

            // ComponentPtr
            Array.Copy(BitConverter.GetBytes(ComponentRecordPtr), 0, buffer, offset, _componentRecordPtrSize);
            offset += _componentRecordPtrSize;

            // Quantity
            Array.Copy(BitConverter.GetBytes(Quantity), 0, buffer, offset, _quantitySize);
            offset += _quantitySize;

            Array.Copy(BitConverter.GetBytes(SpecificationNextPtr), 0, buffer, offset, _specificationNextPtrSize);
            offset += _specificationNextPtrSize;

            buffer = base.ToBytes().Concat(buffer).ToArray();

            if(SpecificationNext != null)
                buffer = buffer.Concat(SpecificationNext.ToBytes()).ToArray();

            if (NextRecord != null)
                buffer = buffer.Concat(NextRecord.ToBytes()).ToArray();

            return buffer;
        }
        //Десериализация
        public static SpecificationRecord FromBytes(byte[] buffer, int startIndex = 0)
        {
            var record = new SpecificationRecord();
            var totalSize = record.GetTotalSize();
            int offset = startIndex;

            // NextRecordPtr
            record.NextRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += _nextRecordPtrSize;

            // Deleted flag
            record.IsDeleted = BitConverter.ToBoolean(buffer, offset);
            offset += _deletionBitSize;

            // ComponentPtr
            record.ComponentRecordPtr = BitConverter.ToInt32(buffer, offset);
            offset += _componentRecordPtrSize;

            // Quantity
            record.Quantity = BitConverter.ToUInt16(buffer, offset);
            offset += _quantitySize;

            record.SpecificationNextPtr = BitConverter.ToInt32(buffer, offset);
            offset += _specificationNextPtrSize;

            return record;
        }
    }

    public class RecordListManager<T> where T : Record<T>
    {
        public static T? GetRecordByPtr(Header<T> header, int ptr)
        {
            if (header.FirstRecord != null)
            {
                if (header.FirstRecordPtr == ptr)
                    return header.FirstRecord;
                for (var tmpRecord = header.FirstRecord; tmpRecord.NextRecord != null; tmpRecord = tmpRecord.NextRecord)
                {
                    if (tmpRecord.NextRecordPtr == ptr)
                        return tmpRecord.NextRecord;
                }
            }
            return null;
        }

        public static void EnumerateRecord(Header<T> header, Action<T> action)
        {
            if (header.FirstRecord != null)
            {
                var tmp = header.FirstRecord;
                while (tmp != null)
                {
                    action.Invoke(tmp);
                    tmp = tmp.NextRecord;
                }
            }
        }

        public static void PushRecord(Header<T> header, T record)
        {
            if (header.FirstRecord != null)
            {
                var tmp = header.FirstRecord;
                while (tmp.NextRecord != null)
                {
                    tmp = tmp.NextRecord;
                }
                tmp.NextRecord = record;
                tmp.NextRecordPtr = record.GetHashCode();
            }
            else
            {
                header.FirstRecord = record;
                header.FirstRecordPtr = record.GetHashCode();
            }
        }
    }

    public class ComponentRecordListManager : RecordListManager<ComponentRecord>
    {
        public static MyComponent? GetMyCompByPtr(ComponentHeader header, int ptr)
        {
            if (header.FirstRecord != null)
            {
                var tmp = header.FirstRecord;
                while (tmp != null)
                {
                    if (tmp.DataArea.GetHashCode() == ptr)
                        return tmp.DataArea;

                    tmp = tmp.NextRecord;
                }
            }
            return null;
        }

        public static int GetCompRecPtr(ComponentHeader header, string compName)
        {
            if (header.FirstRecord != null)
            {
                if (header.FirstRecord.DataArea.ComponentName == compName)
                    return header.FirstRecordPtr;

                var record = header.FirstRecord;
                while (record.NextRecord != null)
                {
                    if (record.NextRecord.DataArea.ComponentName == compName)
                        return record.NextRecordPtr;
                    record = record.NextRecord;
                }
            }
            return -1;
        }

        /// <summary>
        /// Метод ищет запись с названием компонента
        /// </summary>
        /// <param name="name">Название компонента</param>
        /// <returns>Если запись с компонентом найдена, то возвращает запись, иначе null</returns>
        public static ComponentRecord? GetCompRecByName(ComponentHeader header, string name)
        {
            if (header.FirstRecord != null)
            {
                var tmp = header.FirstRecord;
                while (tmp != null)
                {
                    if (tmp.DataArea.ComponentName == name)
                        return tmp;
                    tmp = tmp.NextRecord;
                }
            }
            return null;
        }

        public static IEnumerable<MyComponent> GetComponents(ComponentHeader header)
        {
            var res = new List<MyComponent>();

            var tmp = new Action<ComponentRecord>(x =>
            {
                res.Add(x.DataArea);
            });

            EnumerateRecord(header, tmp);

            return res;
        }
    }

    public class SpecificationRecordListManager : RecordListManager<SpecificationRecord>
    {
        public static void EnumerateSpecification(SpecificationHeader record, Action<SpecificationRecord> action)
        {
            var action1 = new Action<SpecificationRecord>(record =>
            {
                var tmp = record;
                while (tmp != null)
                {
                    action.Invoke(tmp);
                    tmp = tmp.SpecificationNext;
                }
            });

            EnumerateRecord(record, action1);
        }
    }
}
