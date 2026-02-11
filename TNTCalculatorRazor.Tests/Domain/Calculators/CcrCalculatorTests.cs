using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

public class CcrCalculatorTests
{
    [Theory]
    [InlineData(30, 60.0, 0.0, GenderType.Male)]
    [InlineData(30, 60.0, -1.0, GenderType.Female)]
    public void Calculate_Crが0以下なら0を返す( int age, double weight, double cr, GenderType gender )
    {
        var ccr = CcrCalculator.Calculate(age, weight, cr, gender);
        Assert.Equal(0.0, ccr, 6);
    }

    [Fact]
    public void Calculate_男性_CG式が正しく計算される()
    {
        // Arrange
        // base = ((140-30)*60)/(72*1.2) = (110*60)/(86.4) = 6600/86.4 = 76.388...
        int age = 30;
        double weight = 60.0;
        double cr = 1.2;

        // Act
        var ccr = CcrCalculator.Calculate(age, weight, cr, GenderType.Male);

        // Assert
        Assert.Equal(6600.0 / 86.4, ccr, 6);
    }

    [Fact]
    public void Calculate_女性は男性値に0_85を掛ける()
    {
        int age = 30;
        double weight = 60.0;
        double cr = 1.2;

        var male = CcrCalculator.Calculate(age, weight, cr, GenderType.Male);
        var female = CcrCalculator.Calculate(age, weight, cr, GenderType.Female);

        Assert.Equal(male * 0.85, female, 6);
    }
}
