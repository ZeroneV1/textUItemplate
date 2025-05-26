using System;

namespace TextUITemplate.Management
{
    public enum AdjustableValueType
    {
        Float,
        Int,
        Bool
    }

    public class AdjustableValue
    {
        public string Name { get; set; }
        public AdjustableValueType ValType { get; set; }
        public Func<object> GetValue { get; set; }
        public Action<object> SetValue { get; set; }
        public Action<object> OnValueChanged { get; set; }
        public float MinValue { get; set; } = float.MinValue;
        public float MaxValue { get; set; } = float.MaxValue;
        public float FloatIncrementStep { get; set; } = 0.1f;
        public int IntIncrementStep { get; set; } = 1;
    }
}