using Engine.Entities;

namespace Engine.Tasks
{
    public class Task_Wait : Task
    {
        public float TotalTime;
        public float TimeRemaining;
        public bool UseUnscaledTime = false;

        public Task_Wait(float time) : base("Wait")
        {
            Description = $"Waiting for {time:F1} seconds.";

            if(time <= 0f)
            {
                Debug.Warn($"Tried to create a wait task with time {time}. Time must be greater than or equal to zero.");
                Cancel(null);
            }
            else
            {
                TotalTime = time;
                TimeRemaining = time;
            }
        }

        protected override void Update(ActiveEntity e)
        {
            TimeRemaining -= UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            Progress = 1f - (TimeRemaining / TotalTime);
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                Progress = 1f;

                // Done!
                Complete();
            }
        }

        // No need to do anything in pause, unpause or cancel.
    }
}
