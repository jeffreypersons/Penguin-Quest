using System;


namespace PQ.Game.Entities
{
    public struct CharacterStatus
    {
        public int   Lives   { get; private set; }
        public float Stamina { get; private set; }
        public float Health  { get; private set; }

        public override string ToString() =>
            $"CharacterStatus(" +
                $"Lives:{Lives}," +
                $"Stamina:{Stamina}%," +
                $"Health:{Health}%," +
            $")";


        public CharacterStatus(int lives, float stamina, float health)
        {
            EnsurePositiveValue(lives);
            EnsurePositiveRatio(stamina);
            EnsurePositiveRatio(health);

            Lives   = lives;
            Stamina = stamina;
            Health  = health;
        }

        private static void EnsurePositiveValue(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException($"Expected positive count - received {count} instead");
            }
        }

        private static void EnsurePositiveRatio(float ratio)
        {
            if (ratio < 0f)
            {
                throw new ArgumentException($"Expected positive value - received {ratio} instead");
            }
        }
    }
}
