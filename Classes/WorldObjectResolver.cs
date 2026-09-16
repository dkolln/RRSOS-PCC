using RRSOS_PCC.Enums;
using System.ComponentModel;

namespace RRSOS_PCC.Class
{
    public static class WorldObjectResolver
    {
        public static int GetCategoryValue(WorldObjectType type)
        {
            return ((int)type / 1000) * 1000;
        }

        public static WorldObjectCategory GetCategoryEnum(WorldObjectType type)
        {
            int categoryValue = GetCategoryValue(type);

            if (Enum.IsDefined(typeof(WorldObjectCategory), categoryValue))
                return (WorldObjectCategory)categoryValue;

            return WorldObjectCategory.Unknown;
        }

        public static int GetSubCategoryValue(WorldObjectType type)
        {
            int categoryValue = GetCategoryValue(type);
            int hundreds = ((int)type % 1000) / 100;

            return categoryValue + (hundreds * 100);
        }

        public static WorldObjectSubCategory GetSubCategoryEnum(WorldObjectType type)
        {
            int subCategoryValue = GetSubCategoryValue(type);

            if (Enum.IsDefined(typeof(WorldObjectSubCategory), subCategoryValue))
                return (WorldObjectSubCategory)subCategoryValue;

            return WorldObjectSubCategory.Unknown;
        }

        public static string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

            return attr?.Description ?? value.ToString();
        }
    }

}
