using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Models;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

public sealed class EnteralEnergyCalculatorTests
{
    [Fact]
    public void CalculateEnergyFromVolume_volume0は0kcal()
    {
        var comp = new EnteralFormulaComposition(
            volumePerKcal: 1.0,
            proteinPerKcal: 0,
            fatPerKcal: 0,
            carbPerKcal: 0,
            saltPerKcal: 0,
            vitaminKPerKcal: 0,
            waterPerKcal: 0);

        var kcal = EnteralEnergyCalculator.CalculateEnergyFromVolume(0.0, comp);

        Assert.Equal(0.0, kcal, precision: 10);
    }

    [Fact]
    public void CalculateEnergyFromVolume_1mLperKcalなら400mLで400kcal()
    {
        // Meibalance10 等：VolumePerKcal=1.0 mL/kcal を想定
        var comp = new EnteralFormulaComposition(
            volumePerKcal: 1.0,
            proteinPerKcal: 0,
            fatPerKcal: 0,
            carbPerKcal: 0,
            saltPerKcal: 0,
            vitaminKPerKcal: 0,
            waterPerKcal: 0);

        var kcal = EnteralEnergyCalculator.CalculateEnergyFromVolume(400.0, comp);

        Assert.Equal(400.0, kcal, precision: 10);
    }

    [Fact]
    public void CalculateEnergyFromVolume_端数係数_267mLが約400kcal()
    {
        // PeptamenPrebio15 の VolumePerKcal = 267/400 = 0.6675 mL/kcal
        // kcal = volume / (mL/kcal) なので 267 / 0.6675 = 400
        var comp = new EnteralFormulaComposition(
            volumePerKcal: 267.0 / 400.0,
            proteinPerKcal: 0,
            fatPerKcal: 0,
            carbPerKcal: 0,
            saltPerKcal: 0,
            vitaminKPerKcal: 0,
            waterPerKcal: 0);

        var kcal = EnteralEnergyCalculator.CalculateEnergyFromVolume(267.0, comp);

        Assert.Equal(400.0, kcal, precision: 10);
    }

    [Fact]
    public void CalculateEnergyFromVolume_VolumePerKcalが0以下なら例外()
    {
        var comp = new EnteralFormulaComposition(0.0, 0, 0, 0, 0, 0, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EnteralEnergyCalculator.CalculateEnergyFromVolume(100.0, comp));
    }

    [Fact]
    public void CalculateEnergyFromVolume_volumeが負なら0kcal()
    {
        var comp = new EnteralFormulaComposition(1.0, 0, 0, 0, 0, 0, 0);

        var kcal = EnteralEnergyCalculator.CalculateEnergyFromVolume(-10.0, comp);

        Assert.Equal(0.0, kcal, 10);
    }
}
