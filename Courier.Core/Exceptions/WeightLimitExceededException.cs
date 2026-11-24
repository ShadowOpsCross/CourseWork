namespace Courier.Core.Exceptions
{
    /// <summary>Перевищено допустиму вагу посилки.</summary>
    public class WeightLimitExceededException : Exception
    {
        public double LimitKg { get; }
        public double ActualWeightKg { get; }

        public WeightLimitExceededException(double actualWeightKg, double limitKg)
            : base($"Перевищено допустиму вагу: {actualWeightKg} кг (максимум {limitKg} кг).")
        {
            ActualWeightKg = actualWeightKg;
            LimitKg = limitKg;
        }
    }
}
