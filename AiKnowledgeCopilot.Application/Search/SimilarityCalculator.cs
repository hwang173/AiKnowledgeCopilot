namespace AiKnowledgeCopilot.Application.Search;

public static class SimilarityCalculator
{
    public static double CosineSimilarity(
        float[] vectorA,
        float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
        {
            throw new InvalidOperationException(
                "Embedding dimensions do not match.");
        }

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct +=
                vectorA[i] * vectorB[i];

            magnitudeA +=
                vectorA[i] * vectorA[i];

            magnitudeB +=
                vectorB[i] * vectorB[i];
        }

        return dotProduct /
               (Math.Sqrt(magnitudeA)
                * Math.Sqrt(magnitudeB));
    }
}