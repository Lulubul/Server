namespace NetworkTypes
{
    public class Round
    {
        private readonly Turn _turn1;
        private readonly Turn _turn2;
        private Turn _currentTurn;

        public Round(Hero h1, Hero h2)
        {
            _turn1 = new Turn(h1, 1);
            _turn2 = new Turn(h2, 2);
            _currentTurn = _turn2;
        }

        public Creature NextCreature()
        {
            if (_currentTurn.PlayerOrder == 1)
            {
                _currentTurn = _turn2;
            }
            else if (_currentTurn.PlayerOrder == 2)
            {
                _currentTurn = _turn1;
            }

            var creature = _currentTurn.CurrentHero.InstanceCreatures[_currentTurn.Index];
            var find = false;
            while (_currentTurn.CurrentHero.InstanceCreatures[_currentTurn.Index].Status == CreatureStatus.Death)
            {
                _currentTurn.Index = (byte)((_currentTurn.Index + 1) % _currentTurn.MaxCreature);
                find = true;
            }

            if (find == false)
            {
                _currentTurn.Index = (byte)((_currentTurn.Index + 1) % _currentTurn.MaxCreature);
            }
            return creature;
        }
    }
}
