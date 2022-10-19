using System;
using UnityEngine;


namespace PQ.Common.Containers
{
    /*
    Wrapper over enums that allows serialization by name.


    Unity has an annoying (but understandable) habit of serializing enums as ints, which is prone to fragile
    ordering issues, as we want to be able to put a new value in say, the middle of the enum without changing
    the value of EVERYTHING that comes after it.

    So the best solution (but still not ideal) is to provide this wrapper to utilize built in string serialization
    via property.
    */
    public sealed class SerializableEnum<TEnum>
        where TEnum : struct, IConvertible
    {
        public TEnum Value
        {
            get { return m_EnumValue; }
            set { m_EnumValue = value; }
        }

        [SerializeField] private string m_EnumValueAsString;
        [SerializeField] private TEnum m_EnumValue;
    }
}
