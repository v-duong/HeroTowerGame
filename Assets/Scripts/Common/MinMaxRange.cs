using Newtonsoft.Json;

public class MinMaxRange
{
    [JsonProperty]
    public int min;

    [JsonProperty]
    public int max;

    public MinMaxRange()
    {
        this.min = 0;
        this.max = 0;
    }
    public MinMaxRange(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public void SetMinMax(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public void AddToBoth(int value)
    {
        this.min += value;
        this.max += value;
    }

    public void Clear()
    {
        min = 0;
        max = 0;
    }

    public bool IsZero()
    {
        if (min == 0 && max == 0)
            return true;
        else
            return false;
    }
}
