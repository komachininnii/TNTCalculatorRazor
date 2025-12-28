using System.Collections.Generic;
using System.Linq;

namespace TNTCalculatorRazor.Domain.Services
{
    /// <summary>
    /// 規格割付結果
    /// </summary>
    public sealed record EnteralPackagePlan(
        IReadOnlyDictionary<int, int> CountsByVolume,
        int TotalVolumeMl,
        int RemainderMl )
    {
        public int TotalPackageCount
            => CountsByVolume.Values.Sum();
    }
}
