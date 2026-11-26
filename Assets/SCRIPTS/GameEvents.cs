using System;
namespace NED2024
{
    // Delegates topic: UI aur logic in events ko subscribe karte hain
    public static class GameEvents
    {
        public static Action<int> OnKeyCollected;
        public static Action<float> OnHealthChanged;
        public static Action OnPlayerDied;
    }
}
