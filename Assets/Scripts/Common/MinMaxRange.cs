using Newtonsoft.Json;

public class MinMaxRange
{
    [JsonProperty]
    public int min;

    [JsonProperty]
    public int max;

    public MinMaxRange()
    {
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
