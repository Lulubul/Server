namespace Models
{
    public class Round
    {
        private Turn turn1;
        private Turn turn2;
        private Turn currentTurn;

        public Round(Hero h1, Hero h2)
        {
            turn1 = new Turn(h1, 1);
            turn2 = new Turn(h2, 2);
            currentTurn = turn2;
        }

        public Creature NextCreature()
        {
            if (currentTurn.PlayerOrder == 1)
            {
                currentTurn = turn2;
            }
            else
            if (currentTurn.PlayerOrder == 2)
            {
                currentTurn = turn1;
            }

            Creature creature = currentTurn.CurrentHero.InstanceCreatures[currentTurn.Index];
            bool find = false;
            while (currentTurn.CurrentHero.InstanceCreatures[currentTurn.Index].Status == CreatureStatus.Death)
            {
                currentTurn.Index = (byte)((currentTurn.Index + 1) % currentTurn.MaxCreature);
                find = true;
            }

            if (find == false)
            {
                currentTurn.Index = (byte)((currentTurn.Index + 1) % currentTurn.MaxCreature);
            }
            return creature;
        }
    }
}
