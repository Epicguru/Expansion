using Engine.IO;
using Engine.Items;
using Engine.Screens;
using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Entities
{
    public class DroppedItem : Entity
    {
        public ItemStack ItemStack;

        public DroppedItem(ItemStack itemStack) : base(itemStack.Name)
        {
            this.ItemStack = itemStack;
            Size = new Vector2(16, 16);
        }

        public DroppedItem() : base("Load-Pending")
        {

        }

        public override void Draw(SpriteBatch spr)
        {
            Sprite icon = ItemStack.Icon;

            if(icon != null)
                spr.Draw(icon, Bounds.ToRectangle(), Color.White);

            //string label = $"{ItemStack.Name} x{ItemStack.Count}";
            //Vector2 size = LoadingScreen.font.MeasureString(label);
            //spr.DrawString(LoadingScreen.font, label, new Vector2(Center.X - size.X * 0.5f, Position.Y - size.Y - 2), Color.Black);
        }

        public override void Serialize(IOWriter writer)
        {
            base.Serialize(writer);

            writer.Write(ItemStack);
        }

        public override void Deserialize(IOReader reader)
        {
            base.Deserialize(reader);

            ItemStack = reader.ReadItemStack();
        }
    }
}
