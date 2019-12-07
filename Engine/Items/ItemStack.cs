using Engine.Sprites;

namespace Engine.Items
{
    public struct ItemStack
    {
        public ushort ItemID { get; set; }
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                if (value <= 0)
                {
                    Debug.Warn("Count in item stack cannot be zero or less (that makes no sense). Count has been adjusted to 1.");
                    _count = 1;
                }
                else
                {
                    _count = value;
                }
            }
        }
        public object Data;

        public ItemDef Def { get { return ItemDef.Get(ItemID); } } // Automatically handles 0 id.
        public string Name { get { return ItemID == 0 ? "???" : Def.Name; } }
        public Sprite Icon { get { return Def?.Icon ?? JEngine.MissingTextureSprite; } }

        private int _count;

        public ItemStack(ushort itemID, int count, object data = null)
        {
            this.ItemID = itemID;
            if(count <= 0)
            {
                Debug.Warn("Count in item stack cannot be zero or less (that makes no sense). Count has been adjusted to 1.");
                count = 1;
            }
            this._count = count;
            this.Data = data;
        }

        public static ItemStack Combine(ItemStack a, ItemStack b)
        {
            if (a.ItemID != b.ItemID)
            {
                Debug.Error($"ItemStack {a} is not combinable with stack {b}! They do not contain the same item type!");
                return default;
            }

            if(b.Data != null)
            {
                Debug.Warn($"Combining itemstacks results in data loss: stack {b} has item data that will be lost in this merge.");
            }

            return new ItemStack(a.ItemID, a.Count + b.Count, a.Data);
        }

        public static ItemStack Split(ref ItemStack baseStack, int count)
        {
            if(count <= 0)
            {
                Debug.Error($"Item count {count} is not a valid amount to split from the stack. Must be greater than zero.");
                return default;
            }

            if(count >= baseStack.Count)
            {
                Debug.Error($"Item count {count} is an invalid amount to split the base stack into. Base stack only has {baseStack.Count} items in it.");
                return default;
            }

            baseStack.Count -= count;
            return new ItemStack(baseStack.ItemID, count, baseStack.Data);
        }
    }
}
