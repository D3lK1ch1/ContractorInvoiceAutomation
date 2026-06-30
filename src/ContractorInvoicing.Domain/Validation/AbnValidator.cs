namespace ContractorInvoicing.Domain.Validation;

public static class AbnValidator
{
    private static readonly int[] Weights = [10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19];

    public static bool IsValid(string abn)
    {
        if (string.IsNullOrWhiteSpace(abn))
            return false;

        var digits = abn.Replace(" ", "");

        if (digits.Length != 11)
            return false;

        if (!digits.All(char.IsDigit))
            return false;

        var nums = digits.Select(c => c - '0').ToArray();
        nums[0] -= 1;

        var sum = nums.Zip(Weights, (d, w) => d * w).Sum();

        return sum % 89 == 0;
    }
}
