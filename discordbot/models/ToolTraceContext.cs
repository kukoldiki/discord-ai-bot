namespace discordbot.models;

public class ToolTraceContext
{
    public long TotalPromptEvalCount;
    public long TotalEvalCount;

    public long TotalPromptEvalDuration;
    public long TotalEvalDuration;
    public long TotalDuration;

    public int ToolCalls;

    public void Add(ChatApiResponse obj)
    {
        if (obj == null) return;

        TotalPromptEvalCount += obj.PromptEvalCount;
        TotalEvalCount += obj.EvalCount;

        TotalPromptEvalDuration += obj.PromptEvalDuration;
        TotalEvalDuration += obj.EvalDuration;
        TotalDuration += obj.TotalDuration;
    }

    public string Format()
    {
        double seconds = TotalDuration / 1_000_000_000.0;

        double inputTps = TotalPromptEvalDuration > 0
            ? TotalPromptEvalCount / (TotalPromptEvalDuration / 1_000_000_000.0)
            : 0;

        double outputTps = TotalEvalDuration > 0
            ? TotalEvalCount / (TotalEvalDuration / 1_000_000_000.0)
            : 0;

        return $"`TOTAL: In {TotalPromptEvalCount} {inputTps:F1}T/s | Out {TotalEvalCount} {outputTps:F1}T/s | {seconds:F1}s | Tools {ToolCalls}`";
    }
}