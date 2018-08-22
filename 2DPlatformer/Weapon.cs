using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectPlatformer.Items.Weapons.Swords;
using ProjectPlatformer.Character;

namespace ProjectPlatformer.Items.Weapons
{
    public class Weapon : Item
    {
        public float Damage;
        public float AttackSpeed;
        public bool AutoSwing;
        public Vector2 position;
        public Vector2 Pivot;
        public Texture2D Sprite;
        public Player player;

        public bool isUsing;

        protected Weapon()
        {
            isUsing = true;
        }

        public virtual void Use()
        {
            PlayAttackAnimation();
        }

        public virtual void PlayAttackAnimation()
        {

        }

        public virtual void Draw(SpriteBatch batch)
        {

        }

        #region GloblalFunctions
        public static Weapon GetRandomWeapon(Player player)
        {
            return new Sword(player, 10, 5, true, PlatformerGame.Instance.Load<Texture2D>("katana"));
        }
        #endregion
    }
}
