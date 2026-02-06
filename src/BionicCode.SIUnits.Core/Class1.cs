namespace BionicCode.SIUnits.Core
{
    public readonly struct Mass
    {

    }

    public readonly struct SIBaseDimensions
    {
        public SIBaseDimensions IncrementLengthExponent(int increment)
        {
            return new SIBaseDimensions(
                LengthExponent + increment,
                MassExponent,
                TimeExponent,
                ElectricCurrentExponent,
                ThermodynamicTemperatureExponent,
                AmountOfSubstanceExponent,
                LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementMassExponent(int increment)
        {
            return new SIBaseDimensions(
                LengthExponent,
                MassExponent + increment,
                TimeExponent,
                ElectricCurrentExponent,
                ThermodynamicTemperatureExponent,
                AmountOfSubstanceExponent,
                LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementTimeExponent(int increment)
        {
            return new SIBaseDimensions(
                LengthExponent,
                MassExponent,
                TimeExponent + increment,
                ElectricCurrentExponent,
                ThermodynamicTemperatureExponent,
                AmountOfSubstanceExponent,
                LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementElectricCurrentExponent(int increment)
        {
            return new SIBaseDimensions(
                LengthExponent,
                MassExponent,
                TimeExponent,
                ElectricCurrentExponent + increment,
                ThermodynamicTemperatureExponent,
                AmountOfSubstanceExponent,
                LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementThermodynamicTemperatureExponent(int increment)
        {
            return new SIBaseDimensions(
                LengthExponent,
                MassExponent,
                TimeExponent,
                ElectricCurrentExponent,
                ThermodynamicTemperatureExponent + increment,
                AmountOfSubstanceExponent,
                LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementAmountOfSubstanceExponent(int increment)
        {
            return new SIBaseDimensions(
                LengthExponent,
                MassExponent,
                TimeExponent,
                ElectricCurrentExponent,
                ThermodynamicTemperatureExponent,
                AmountOfSubstanceExponent + increment,
                LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementLuminousIntensityExponent(int increment)
        {
            return new SIBaseDimensions(
                LengthExponent,
                MassExponent,
                TimeExponent,
                ElectricCurrentExponent,
                ThermodynamicTemperatureExponent,
                AmountOfSubstanceExponent,
                LuminousIntensityExponent + increment
            );
        }

        public SIBaseDimensions DecrementLengthExponent(int decrement) => IncrementLengthExponent(-decrement);
        public SIBaseDimensions DecrementMassExponent(int decrement) => IncrementMassExponent(-decrement);
        public SIBaseDimensions DecrementTimeExponent(int decrement) => IncrementTimeExponent(-decrement);
        public SIBaseDimensions DecrementElectricCurrentExponent(int decrement) => IncrementElectricCurrentExponent(-decrement);
        public SIBaseDimensions DecrementThermodynamicTemperatureExponent(int decrement) => IncrementThermodynamicTemperatureExponent(-decrement);
        public SIBaseDimensions DecrementAmountOfSubstanceExponent(int decrement) => IncrementAmountOfSubstanceExponent(-decrement);
        public SIBaseDimensions DecrementLuminousIntensityExponent(int decrement) => IncrementLuminousIntensityExponent(-decrement);

        public SIBaseDimensions MoveLengthToDenominator() => this with { LengthExponent = -Math.Abs(LengthExponent) };
        public SIBaseDimensions MoveMassToDenominator() => this with { MassExponent = -Math.Abs(MassExponent) };
        public SIBaseDimensions MoveTimeToDenominator() => this with { TimeExponent = -Math.Abs(TimeExponent) };
        public SIBaseDimensions MoveElectricCurrentToDenominator() => this with { ElectricCurrentExponent = -Math.Abs(ElectricCurrentExponent) };
        public SIBaseDimensions MoveThermodynamicTemperatureToDenominator() => this with { ThermodynamicTemperatureExponent = -Math.Abs(ThermodynamicTemperatureExponent) };
        public SIBaseDimensions MoveAmountOfSubstanceToDenominator() => this with { AmountOfSubstanceExponent = -Math.Abs(AmountOfSubstanceExponent) };
        public SIBaseDimensions MoveLuminousIntensityToDenominator() => this with { LuminousIntensityExponent = -Math.Abs(LuminousIntensityExponent) };

        public SIBaseDimensions MoveAllToDenominator()
        {
            return new SIBaseDimensions(
                -Math.Abs(LengthExponent),
                -Math.Abs(MassExponent),
                -Math.Abs(TimeExponent),
                -Math.Abs(ElectricCurrentExponent),
                -Math.Abs(ThermodynamicTemperatureExponent),
                -Math.Abs(AmountOfSubstanceExponent),
                -Math.Abs(LuminousIntensityExponent)
            );
        }

        public SIBaseDimensions MoveLengthToNumerator() => this with { LengthExponent = Math.Abs(LengthExponent) };
        public SIBaseDimensions MoveMassToNumerator() => this with { MassExponent = Math.Abs(MassExponent) };
        public SIBaseDimensions MoveTimeToNumerator() => this with { TimeExponent = Math.Abs(TimeExponent) };
        public SIBaseDimensions MoveElectricCurrentToNumerator() => this with { ElectricCurrentExponent = Math.Abs(ElectricCurrentExponent) };
        public SIBaseDimensions MoveThermodynamicTemperatureToNumerator() => this with { ThermodynamicTemperatureExponent = Math.Abs(ThermodynamicTemperatureExponent) };
        public SIBaseDimensions MoveAmountOfSubstanceToNumerator() => this with { AmountOfSubstanceExponent = Math.Abs(AmountOfSubstanceExponent) };
        public SIBaseDimensions MoveLuminousIntensityToNumerator() => this with { LuminousIntensityExponent = Math.Abs(LuminousIntensityExponent) };
        public SIBaseDimensions MoveAllToNumerator()
        {
            return new SIBaseDimensions(
                Math.Abs(LengthExponent),
                Math.Abs(MassExponent),
                Math.Abs(TimeExponent),
                Math.Abs(ElectricCurrentExponent),
                Math.Abs(ThermodynamicTemperatureExponent),
                Math.Abs(AmountOfSubstanceExponent),
                Math.Abs(LuminousIntensityExponent)
            );
        }

        public int LengthExponent { get; init; }
        public int MassExponent { get; init; }
        public int TimeExponent { get; init; }
        public int ElectricCurrentExponent { get; init; }
        public int ThermodynamicTemperatureExponent { get; init; }
        public int AmountOfSubstanceExponent { get; init; }
        public int LuminousIntensityExponent { get; init; }

        public SIBaseDimensions(
            int lengthExponent,
            int massExponent,
            int timeExponent,
            int electricCurrentExponent,
            int thermodynamicTemperatureExponent,
            int amountOfSubstanceExponent,
            int luminousIntensityExponent)
        {
            LengthExponent = lengthExponent;
            MassExponent = massExponent;
            TimeExponent = timeExponent;
            ElectricCurrentExponent = electricCurrentExponent;
            ThermodynamicTemperatureExponent = thermodynamicTemperatureExponent;
            AmountOfSubstanceExponent = amountOfSubstanceExponent;
            LuminousIntensityExponent = luminousIntensityExponent;
        }
    }

    internal class UnitExpressionBuilder
    {
    }

    public class UnitExpression
    {
        public NominatorDimensions? Nominator { get; }
    }
}
