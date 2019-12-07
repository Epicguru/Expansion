using Engine.Sprites;

namespace Engine.Items
{
    /// <summary>
    /// Items are in-game elements that can be stored, transported and used.
    /// This item definition defines behaviour and data that items have.
    /// </summary>
    public class ItemDef
    {
        public static ItemDef Get(ushort id)
        {
            if (id == 0)
                return null;

            return allItems[id];
        }

        public static void Register(ItemDef def)
        {
            if(def == null)
            {
                Debug.Error("Cannot register null item definition.");
                return;
            }

            if(def.ID != 0)
            {
                Debug.Error($"Item definition '{def}' is already registered under ID {def.ID}.");
                return;
            }
            
            ushort newID = (ushort)(maxIDUsed + 1);
            def.ID = newID;
            allItems[def.ID] = def;

            Debug.Trace($"Registered new item definition '{def}'");
        }

        private static ushort maxIDUsed = 0;
        private static ItemDef[] allItems = new ItemDef[ushort.MaxValue + 1];

        public string Name { get; protected set; }
        public ushort ID { get; internal set; }
        public Sprite Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                if(value == null)
                {
                    _icon = JEngine.MissingTextureSprite;
                }
                else
                {
                    _icon = value;
                }
            }
        }

        private Sprite _icon;

        public ItemDef(string name, Sprite icon = null)
        {
            this.Name = name;
            this.Icon = icon;
        }

        public override string ToString()
        {
            return $"[{ID}] {Name}";
        }
    }
}
