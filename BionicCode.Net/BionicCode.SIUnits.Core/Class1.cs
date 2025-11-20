namespace BionicCode.SIUnits.Core
{
    using System.Collections.Frozen;

    public readonly struct Mass
    {

    }

    public readonly struct SIBaseDimensions
    {
        public SIBaseDimensions IncrementLengthExponent(int increment)
        {
            return new SIBaseDimensions(
                this.LengthExponent + increment,
                this.MassExponent,
                this.TimeExponent,
                this.ElectricCurrentExponent,
                this.ThermodynamicTemperatureExponent,
                this.AmountOfSubstanceExponent,
                this.LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementMassExponent(int increment)
        {
            return new SIBaseDimensions(
                this.LengthExponent,
                this.MassExponent + increment,
                this.TimeExponent,
                this.ElectricCurrentExponent,
                this.ThermodynamicTemperatureExponent,
                this.AmountOfSubstanceExponent,
                this.LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementTimeExponent(int increment)
        {
            return new SIBaseDimensions(
                this.LengthExponent,
                this.MassExponent,
                this.TimeExponent + increment,
                this.ElectricCurrentExponent,
                this.ThermodynamicTemperatureExponent,
                this.AmountOfSubstanceExponent,
                this.LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementElectricCurrentExponent(int increment)
        {
            return new SIBaseDimensions(
                this.LengthExponent,
                this.MassExponent,
                this.TimeExponent,
                this.ElectricCurrentExponent + increment,
                this.ThermodynamicTemperatureExponent,
                this.AmountOfSubstanceExponent,
                this.LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementThermodynamicTemperatureExponent(int increment)
        {
            return new SIBaseDimensions(
                this.LengthExponent,
                this.MassExponent,
                this.TimeExponent,
                this.ElectricCurrentExponent,
                this.ThermodynamicTemperatureExponent + increment,
                this.AmountOfSubstanceExponent,
                this.LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementAmountOfSubstanceExponent(int increment)
        {
            return new SIBaseDimensions(
                this.LengthExponent,
                this.MassExponent,
                this.TimeExponent,
                this.ElectricCurrentExponent,
                this.ThermodynamicTemperatureExponent,
                this.AmountOfSubstanceExponent + increment,
                this.LuminousIntensityExponent
            );
        }

        public SIBaseDimensions IncrementLuminousIntensityExponent(int increment)
        {
            return new SIBaseDimensions(
                this.LengthExponent,
                this.MassExponent,
                this.TimeExponent,
                this.ElectricCurrentExponent,
                this.ThermodynamicTemperatureExponent,
                this.AmountOfSubstanceExponent,
                this.LuminousIntensityExponent + increment
            );
        }

        public SIBaseDimensions DecrementLengthExponent(int decrement) => this.IncrementLengthExponent(-decrement);
        public SIBaseDimensions DecrementMassExponent(int decrement) => this.IncrementMassExponent(-decrement);
        public SIBaseDimensions DecrementTimeExponent(int decrement) => this.IncrementTimeExponent(-decrement);
        public SIBaseDimensions DecrementElectricCurrentExponent(int decrement) => this.IncrementElectricCurrentExponent(-decrement);
        public SIBaseDimensions DecrementThermodynamicTemperatureExponent(int decrement) => this.IncrementThermodynamicTemperatureExponent(-decrement);
        public SIBaseDimensions DecrementAmountOfSubstanceExponent(int decrement) => this.IncrementAmountOfSubstanceExponent(-decrement);
        public SIBaseDimensions DecrementLuminousIntensityExponent(int decrement) => this.IncrementLuminousIntensityExponent(-decrement);

        public SIBaseDimensions MoveLengthToDenominator() => this with { LengthExponent = -Math.Abs(this.LengthExponent) };
        public SIBaseDimensions MoveMassToDenominator() => this with { MassExponent = -Math.Abs(this.MassExponent) };
        public SIBaseDimensions MoveTimeToDenominator() => this with { TimeExponent = -Math.Abs(this.TimeExponent) };
        public SIBaseDimensions MoveElectricCurrentToDenominator() => this with { ElectricCurrentExponent = -Math.Abs(this.ElectricCurrentExponent) };
        public SIBaseDimensions MoveThermodynamicTemperatureToDenominator() => this with { ThermodynamicTemperatureExponent = -Math.Abs(this.ThermodynamicTemperatureExponent) };
        public SIBaseDimensions MoveAmountOfSubstanceToDenominator() => this with { AmountOfSubstanceExponent = -Math.Abs(this.AmountOfSubstanceExponent) };
        public SIBaseDimensions MoveLuminousIntensityToDenominator() => this with { LuminousIntensityExponent = -Math.Abs(this.LuminousIntensityExponent) };

        public SIBaseDimensions MoveAllToDenominator()
        {
            return new SIBaseDimensions(
                -Math.Abs(this.LengthExponent),
                -Math.Abs(this.MassExponent),
                -Math.Abs(this.TimeExponent),
                -Math.Abs(this.ElectricCurrentExponent),
                -Math.Abs(this.ThermodynamicTemperatureExponent),
                -Math.Abs(this.AmountOfSubstanceExponent),
                -Math.Abs(this.LuminousIntensityExponent)
            );
        }

        public SIBaseDimensions MoveLengthToNumerator() => this with { LengthExponent = Math.Abs(this.LengthExponent) };
        public SIBaseDimensions MoveMassToNumerator() => this with { MassExponent = Math.Abs(this.MassExponent) };
        public SIBaseDimensions MoveTimeToNumerator() => this with { TimeExponent = Math.Abs(this.TimeExponent) };
        public SIBaseDimensions MoveElectricCurrentToNumerator() => this with { ElectricCurrentExponent = Math.Abs(this.ElectricCurrentExponent) };
        public SIBaseDimensions MoveThermodynamicTemperatureToNumerator() => this with { ThermodynamicTemperatureExponent = Math.Abs(this.ThermodynamicTemperatureExponent) };
        public SIBaseDimensions MoveAmountOfSubstanceToNumerator() => this with { AmountOfSubstanceExponent = Math.Abs(this.AmountOfSubstanceExponent) };
        public SIBaseDimensions MoveLuminousIntensityToNumerator() => this with { LuminousIntensityExponent = Math.Abs(this.LuminousIntensityExponent) };
        public SIBaseDimensions MoveAllToNumerator()
        {
            return new SIBaseDimensions(
                Math.Abs(this.LengthExponent),
                Math.Abs(this.MassExponent),
                Math.Abs(this.TimeExponent),
                Math.Abs(this.ElectricCurrentExponent),
                Math.Abs(this.ThermodynamicTemperatureExponent),
                Math.Abs(this.AmountOfSubstanceExponent),
                Math.Abs(this.LuminousIntensityExponent)
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
            this.LengthExponent = lengthExponent;
            this.MassExponent = massExponent;
            this.TimeExponent = timeExponent;
            this.ElectricCurrentExponent = electricCurrentExponent;
            this.ThermodynamicTemperatureExponent = thermodynamicTemperatureExponent;
            this.AmountOfSubstanceExponent = amountOfSubstanceExponent;
            this.LuminousIntensityExponent = luminousIntensityExponent;
        }
    }

    internal class UnitExpressionBuilder
    {
        public
    }

    public class UnitExpression
    {
        public NominatorDimensions Nominator { get; }
    }

}
