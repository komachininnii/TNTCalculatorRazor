using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

/// <summary>
/// ProteinCalculator クラスのテスト
/// </summary>
public class ProteinCalculatorTests
{
    // ========================================
    // ProteinBaseCalculator のテスト
    // ========================================

    [Theory]
    [InlineData(0, 10.0, 20.0)]    // 乳児: 2.0 × 10 = 20.0
    [InlineData(0, 8.0, 16.0)]     // 乳児: 2.0 × 8 = 16.0
    public void ProteinBase_乳児_係数2point0( int age, double weight, double expected )
    {
        var result = ProteinBaseCalculator.Calculate(age, weight);
        Assert.Equal(expected, result, precision: 1);
    }

    [Theory]
    [InlineData(1, 10.0, 18.0)]    // 1-3歳: 1.8 × 10 = 18.0
    [InlineData(2, 12.0, 21.6)]    // 1-3歳: 1.8 × 12 = 21.6
    [InlineData(3, 15.0, 27.0)]    // 1-3歳: 1.8 × 15 = 27.0
    public void ProteinBase_1から3歳_係数1point8( int age, double weight, double expected )
    {
        var result = ProteinBaseCalculator.Calculate(age, weight);
        Assert.Equal(expected, result, precision: 1);
    }

    [Theory]
    [InlineData(4, 15.0, 22.5)]    // 4-6歳: 1.5 × 15 = 22.5
    [InlineData(5, 18.0, 27.0)]    // 4-6歳: 1.5 × 18 = 27.0
    [InlineData(6, 20.0, 30.0)]    // 4-6歳: 1.5 × 20 = 30.0
    public void ProteinBase_4から6歳_係数1point5( int age, double weight, double expected )
    {
        var result = ProteinBaseCalculator.Calculate(age, weight);
        Assert.Equal(expected, result, precision: 1);
    }

    [Theory]
    [InlineData(7, 25.0, 30.0)]    // 7-10歳: 1.2 × 25 = 30.0
    [InlineData(8, 30.0, 36.0)]    // 7-10歳: 1.2 × 30 = 36.0
    [InlineData(10, 35.0, 42.0)]   // 7-10歳: 1.2 × 35 = 42.0
    public void ProteinBase_7から10歳_係数1point2( int age, double weight, double expected )
    {
        var result = ProteinBaseCalculator.Calculate(age, weight);
        Assert.Equal(expected, result, precision: 1);
    }

    [Theory]
    [InlineData(11, 40.0, 40.0)]   // 11歳以上: 1.0 × 40 = 40.0
    [InlineData(15, 50.0, 50.0)]   // 11歳以上: 1.0 × 50 = 50.0
    [InlineData(18, 60.0, 60.0)]   // 11歳以上: 1.0 × 60 = 60.0
    [InlineData(30, 65.0, 65.0)]   // 11歳以上: 1.0 × 65 = 65.0
    public void ProteinBase_11歳以上_係数1point0( int age, double weight, double expected )
    {
        var result = ProteinBaseCalculator.Calculate(age, weight);
        Assert.Equal(expected, result, precision: 1);
    }

    // ========================================
    // ProteinCalculator - 基本計算
    // ========================================

    [Fact]
    public void Calculate_成人_標準ケース()
    {
        // 30歳、体重60kg、ストレス1.2、補正なし、疾患なし
        // 60 × 1.0 × 1.2 × 1.0 = 72.0
        var result = ProteinCalculator.Calculate(
            age: 30,
            weightForProtein: 60.0,
            stressFactor: 1.2,
            proteinCorrect: 1.0,
            disease: DiseaseType.None);

        Assert.Equal(72.0, result, precision: 1);
    }

    [Fact]
    public void Calculate_乳児_ストレス係数適用()
    {
        // 0歳、体重8kg、ストレス1.3、補正なし、疾患なし
        // 8 × 2.0 × 1.3 × 1.0 = 20.8
        var result = ProteinCalculator.Calculate(
            age: 0,
            weightForProtein: 8.0,
            stressFactor: 1.3,
            proteinCorrect: 1.0,
            disease: DiseaseType.None);

        Assert.Equal(20.8, result, precision: 1);
    }

    [Fact]
    public void Calculate_小児_ストレス係数適用()
    {
        // 10歳、体重30kg、ストレス1.2、補正なし、疾患なし
        // 30 × 1.2 × 1.2 × 1.0 = 43.2
        var result = ProteinCalculator.Calculate(
            age: 10,
            weightForProtein: 30.0,
            stressFactor: 1.2,
            proteinCorrect: 1.0,
            disease: DiseaseType.None);

        Assert.Equal(43.2, result, precision: 1);
    }

    // ========================================
    // ProteinCalculator - 補正係数
    // ========================================

    [Fact]
    public void Calculate_成人_CKD補正_0point7()
    {
        // 50歳、体重60kg、ストレス1.0、CKD補正0.7、疾患なし
        // 60 × 1.0 × 1.0 × 0.7 = 42.0
        var result = ProteinCalculator.Calculate(
            age: 50,
            weightForProtein: 60.0,
            stressFactor: 1.0,
            proteinCorrect: 0.7,
            disease: DiseaseType.None);

        Assert.Equal(42.0, result, precision: 1);
    }

    [Fact]
    public void Calculate_成人_肝硬変蛋白不耐_0point5()
    {
        // 60歳、体重55kg、ストレス1.0、肝硬変補正0.5、疾患なし
        // 55 × 1.0 × 1.0 × 0.5 = 27.5
        var result = ProteinCalculator.Calculate(
            age: 60,
            weightForProtein: 55.0,
            stressFactor: 1.0,
            proteinCorrect: 0.5,
            disease: DiseaseType.None);

        Assert.Equal(27.5, result, precision: 1);
    }

    [Fact]
    public void Calculate_小児_手動補正は可能()
    {
        // UIで実際に起こりうるケース
        // 10歳、体重30kg、ストレス1.2、CKD補正0.7、疾患なし（強制）
        // 30 × 1.2 × 1.2 × 0.7 = 30.24
        var result = ProteinCalculator.Calculate(
            age: 10,
            weightForProtein: 30.0,
            stressFactor: 1.2,
            proteinCorrect: 0.7,
            disease: DiseaseType.None);

        Assert.Equal(30.2, result, precision: 1);
    }

    // ========================================
    // ProteinCalculator - 疾患別（ストレス係数無視）
    // ========================================

    [Fact]
    public void Calculate_成人_腎不全_ストレス係数無視()
    {
        // 40歳、体重60kg、ストレス1.5、補正なし、腎不全
        // 60 × 1.0 × 1.0 × 1.0 = 60.0（ストレス1.5は無視される）
        var result = ProteinCalculator.Calculate(
            age: 40,
            weightForProtein: 60.0,
            stressFactor: 1.5,
            proteinCorrect: 1.0,
            disease: DiseaseType.RenalFailure);

        Assert.Equal(60.0, result, precision: 1);
    }

    [Fact]
    public void Calculate_成人_透析_ストレス係数無視()
    {
        // 50歳、体重65kg、ストレス1.3、補正なし、透析
        // 65 × 1.0 × 1.0 × 1.0 = 65.0（ストレス1.3は無視される）
        var result = ProteinCalculator.Calculate(
            age: 50,
            weightForProtein: 65.0,
            stressFactor: 1.3,
            proteinCorrect: 1.0,
            disease: DiseaseType.Hemodialysis);

        Assert.Equal(65.0, result, precision: 1);
    }

    [Fact]
    public void Calculate_成人_肝硬変_ストレス係数無視()
    {
        // 55歳、体重60kg、ストレス1.4、補正なし、肝硬変
        // 60 × 1.0 × 1.0 × 1.0 = 60.0（ストレス1.4は無視される）
        var result = ProteinCalculator.Calculate(
            age: 55,
            weightForProtein: 60.0,
            stressFactor: 1.4,
            proteinCorrect: 1.0,
            disease: DiseaseType.LiverCirrhosis);

        Assert.Equal(60.0, result, precision: 1);
    }

    [Fact]
    public void Calculate_小児_例外疾患でもストレス係数適用()
    {
        // 注: 現在のUIでは小児（<18歳）は例外設定（疾患）を選択できない
        // しかし、ドメインロジックとしては正しく動作することを確認
        // 将来、仕様変更があった場合に備えたテスト

        // 10歳、体重30kg、ストレス1.2、補正なし、腎不全
        // 小児なのでストレス係数は適用される
        // 30 × 1.2 × 1.2 × 1.0 = 43.2
        var result = ProteinCalculator.Calculate(
            age: 10,
            weightForProtein: 30.0,
            stressFactor: 1.2,
            proteinCorrect: 1.0,
            disease: DiseaseType.RenalFailure);

        Assert.Equal(43.2, result, precision: 1);
    }

    // ========================================
    // ProteinCalculator - 複合パターン
    // ========================================

    [Fact]
    public void Calculate_成人_腎不全_CKD補正_ストレス無視()
    {
        // 45歳、体重60kg、ストレス1.5、CKD補正0.7、腎不全
        // 60 × 1.0 × 1.0 × 0.7 = 42.0（ストレスは無視）
        var result = ProteinCalculator.Calculate(
            age: 45,
            weightForProtein: 60.0,
            stressFactor: 1.5,
            proteinCorrect: 0.7,
            disease: DiseaseType.RenalFailure);

        Assert.Equal(42.0, result, precision: 1);
    }

    [Fact]
    public void Calculate_成人_肝硬変_蛋白不耐_ストレス無視()
    {
        // 60歳、体重55kg、ストレス1.6、肝硬変補正0.5、肝硬変
        // 55 × 1.0 × 1.0 × 0.5 = 27.5（ストレスは無視）
        var result = ProteinCalculator.Calculate(
            age: 60,
            weightForProtein: 55.0,
            stressFactor: 1.6,
            proteinCorrect: 0.5,
            disease: DiseaseType.LiverCirrhosis);

        Assert.Equal(27.5, result, precision: 1);
    }

    [Fact]
    public void Calculate_成人_高ストレス_補正なし_疾患なし()
    {
        // 35歳、体重70kg、ストレス1.8、補正なし、疾患なし
        // 70 × 1.0 × 1.8 × 1.0 = 126.0
        var result = ProteinCalculator.Calculate(
            age: 35,
            weightForProtein: 70.0,
            stressFactor: 1.8,
            proteinCorrect: 1.0,
            disease: DiseaseType.None);

        Assert.Equal(126.0, result, precision: 1);
    }

    // ========================================
    // 境界値テスト
    // ========================================

    [Fact]
    public void ProteinBase_年齢境界_0から1歳()
    {
        // Age = 0 → 係数 2.0
        var infant = ProteinBaseCalculator.Calculate(0, 10.0);
        Assert.Equal(20.0, infant, precision: 1);

        // Age = 1 → 係数 1.8
        var child = ProteinBaseCalculator.Calculate(1, 10.0);
        Assert.Equal(18.0, child, precision: 1);
    }

    [Fact]
    public void ProteinBase_年齢境界_3から4歳()
    {
        // Age = 3 → 係数 1.8
        var age3 = ProteinBaseCalculator.Calculate(3, 10.0);
        Assert.Equal(18.0, age3, precision: 1);

        // Age = 4 → 係数 1.5
        var age4 = ProteinBaseCalculator.Calculate(4, 10.0);
        Assert.Equal(15.0, age4, precision: 1);
    }

    [Fact]
    public void ProteinBase_年齢境界_6から7歳()
    {
        // Age = 6 → 係数 1.5
        var age6 = ProteinBaseCalculator.Calculate(6, 10.0);
        Assert.Equal(15.0, age6, precision: 1);

        // Age = 7 → 係数 1.2
        var age7 = ProteinBaseCalculator.Calculate(7, 10.0);
        Assert.Equal(12.0, age7, precision: 1);
    }

    [Fact]
    public void ProteinBase_年齢境界_10から11歳()
    {
        // Age = 10 → 係数 1.2
        var age10 = ProteinBaseCalculator.Calculate(10, 10.0);
        Assert.Equal(12.0, age10, precision: 1);

        // Age = 11 → 係数 1.0
        var age11 = ProteinBaseCalculator.Calculate(11, 10.0);
        Assert.Equal(10.0, age11, precision: 1);
    }

    [Fact]
    public void Calculate_Age18の境界_ストレス無視ルール()
    {
        // 注: Age < 18 では例外設定は現在のUIで選択不可
        // しかし、ドメインロジックの境界値として動作を確認

        // Age = 17、腎不全 → ストレス係数適用（小児扱い）
        var age17 = ProteinCalculator.Calculate(17, 50.0, 1.5, 1.0,
            DiseaseType.RenalFailure);
        Assert.Equal(75.0, age17, precision: 1);  // 50 × 1.0 × 1.5 × 1.0

        // Age = 18、腎不全 → ストレス係数無視（成人扱い）
        var age18 = ProteinCalculator.Calculate(18, 50.0, 1.5, 1.0,
            DiseaseType.RenalFailure);
        Assert.Equal(50.0, age18, precision: 1);  // 50 × 1.0 × 1.0 × 1.0（ストレス無視）
    }

    [Fact]
    public void Calculate_Age18の境界_疾患なしの場合()
    {
        // Age = 17、疾患なし → ストレス係数適用
        var age17 = ProteinCalculator.Calculate(17, 50.0, 1.5, 1.0,
            DiseaseType.None);
        Assert.Equal(75.0, age17, precision: 1);  // 50 × 1.0 × 1.5 × 1.0

        // Age = 18、疾患なし → ストレス係数適用
        var age18 = ProteinCalculator.Calculate(18, 50.0, 1.5, 1.0,
            DiseaseType.None);
        Assert.Equal(75.0, age18, precision: 1);  // 50 × 1.0 × 1.5 × 1.0
    }
}
