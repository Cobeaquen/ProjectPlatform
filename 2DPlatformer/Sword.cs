using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectPlatformer.Character;

namespace ProjectPlatformer.Items.Weapons.Swords
{
    public class Sword : Weapon
    {
        SwordAnimation anim;

        public Sword(Player player, float damage, float attackSpeed, bool autoSwing, Texture2D sprite)
        {
            this.player = player;
            Damage = damage;
            AttackSpeed = attackSpeed;
            AutoSwing = autoSwing;
            Sprite = sprite;
            Pivot = new Vector2(Sprite.Width/2f, Sprite.Height);

            anim = new SwordAnimation(AttackSpeed);
        }

        public override void Use()
        {
            Console.WriteLine("ATTACKING!!");
            base.Use();
        }
        public override void PlayAttackAnimation()
        {
            anim.PlayAnimation(0, player.flip == SpriteEffects.FlipHorizontally ? MathHelper.PiOver4*3 : -MathHelper.PiOver4*3); // can switch end rotation during swing

            if (anim.rotation == 0)
            {
                isUsing = false;
            }
            Console.WriteLine(anim.rotation);
            base.PlayAttackAnimation();
        }
        public override void Draw(SpriteBatch batch)
        {
            position = player.position.ToVector2();
            SpriteEffects flip = player.flip == SpriteEffects.None ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            batch.Draw(Sprite, position, null, Color.White, anim.rotation, Pivot, .5f, flip, 0f);
            base.Draw(batch);
        }
    }
    public class SwordAnimation
    {
        public float rotation { get; set; }
        public float attackDuration;
        private float speed;

        private float t;

        public SwordAnimation(float speed)
        {
            if (speed > 100f)
                speed = 100f;

            attackDuration = 100f / speed;
            t = 0f;

            this.speed = 1f / attackDuration;

            rotation = 0;
        }

        public void PlayAnimation(float startRotation, float endRotation)
        {
            if (t < 1)
            {
                rotation = MathHelper.Lerp(startRotation, endRotation, t);
                t += speed;
            }
            else
            {
                Console.WriteLine("Animation is done playing");
                rotation = startRotation;
                t = 0;
            }
        }
    }
}