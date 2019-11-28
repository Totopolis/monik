namespace Monik.Service
{
    public interface IWindowCalculator
    {
        void OnNewValue(double value);
        void OnNextSecond();
        double GetValue();
    }
}